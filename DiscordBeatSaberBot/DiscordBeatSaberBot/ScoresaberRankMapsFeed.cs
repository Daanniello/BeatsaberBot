using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    class ScoresaberRankMapsFeed
    {
        private DiscordSocketClient _discord;
        private ulong _guildId;
        private ulong _channelId;

        public ScoresaberRankMapsFeed(DiscordSocketClient discord)
        {
            _discord = discord;
        }

        public void CreateRankmapFeed(ulong guildId, ulong channelId)
        {
            _guildId = guildId;
            _channelId = channelId;



            if (!CheckIfChannelIsEmpty().Result)
            {
                var msg = _discord.GetGuild(guildId).GetTextChannel(channelId).SendMessageAsync("The channel should be empty to start this command. This channel should also be set on read-only").Result;
                Task.Delay(5000).Wait();
                msg.DeleteAsync().Wait();
                return;
            }

            var country = DatabaseContext.ExecuteSelectQuery($"select * from country where guildid = {guildId}").Result;
            if (country == null)
            {
                var msg = _discord.GetGuild(guildId).GetTextChannel(channelId).SendMessageAsync("This Guild ID is not listed in the database. Ask Silverhaze#0001 to add your Beat Saber country Discord").Result;
                Task.Delay(5000).Wait();
                msg.DeleteAsync().Wait();
                return;
            }

            DatabaseContext.ExecuteSelectQuery($"Update Country set RankedMapQueueChannelId = {channelId} where GuildId={guildId}").Wait();

            CreateRankedRequestTopQueueEmbedInChannel().Wait();
            CreateRecentRankedMapsEmbedInChannel().Wait();
            CreateQualifiedMapsEmbedsInChannel(_discord, guildId, channelId);
        }

        private async Task<bool> CheckIfChannelIsEmpty()
        {
            var guild = _discord.GetGuild(_guildId);
            var guildChannel = guild.GetTextChannel(_channelId);
            var messages = guildChannel.GetMessagesAsync(5).Flatten();

            return await messages.CountAsync() == 0;
        }

        //update function 
        public async Task CheckIfRankedRequestTopChanged()
        {
            Console.WriteLine("Updating Rankedmap feed");
            var rankedRequests = await ScoresaberAPI.GetTopRankedRequests();
            var newRequestIds = "";
            var newRequestIdsList = new List<long>();
            foreach (var request in rankedRequests.Requests)
            {
                newRequestIdsList.Add(request.RequestRequest);
                if (request.Id == rankedRequests.Requests[rankedRequests.Requests.Count() - 1].Id)
                {
                    newRequestIds += request.RequestRequest;
                    continue;
                }
                newRequestIds += request.RequestRequest + ",";
            }

            //Check if new map is in the rank queue top  
            var currentRequests = await DatabaseContext.ExecuteSelectQuery($"Select * from ScoresaberRankedQueueTopMaps where RequestID IN ({newRequestIds})");

            foreach (var Id in newRequestIdsList)
            {
                if (currentRequests == null || !currentRequests.SelectMany(x => x).Contains(Id))
                {
                    //new map in the queue
                    //Add to database
                    Console.WriteLine("New map is in the top queue");
                    await DatabaseContext.ExecuteInsertQuery($"Insert into ScoresaberRankedQueueTopMaps(RequestId, Songname) values({Id},'{rankedRequests.Requests.First(x => x.RequestRequest == Id).Name}')");

                }
            }

            //Check if ranked request top map is gone
            var DatabaseRankedRequests = await DatabaseContext.ExecuteSelectQuery($"Select * from ScoresaberRankedQueueTopMaps");

            //update rankmap top queue embed
            var channelsToModify = await DatabaseContext.ExecuteSelectQuery($"select * from Country where RankedMapQueueChannelId IS NOT NULL");
            foreach (var chn in channelsToModify)
            {
                await ModifyRankedRequestTopQueueEmbedInChannel(Convert.ToUInt64(chn[2]), Convert.ToUInt64(chn[6]));
            }

            if (currentRequests == null) return;

            //update all current qualifier embeds 
            foreach (var chn in channelsToModify)
            {
                ModifyAllCurrentQualifiers(Convert.ToUInt64(chn[2]), Convert.ToUInt64(chn[6]));
            }


            //Check map in queue goes to qualified maps 
            foreach (var request in DatabaseRankedRequests)
            {
                if (!newRequestIdsList.Contains(Convert.ToInt64(request[0])) && DatabaseRankedRequests.Where(x => Convert.ToInt64(x[0]) == Convert.ToInt64(request[0])) == null)
                {
                    Console.WriteLine("Map went into qualified maps!");
                    //map is qualified
                    //remove from database
                    await DatabaseContext.ExecuteRemoveQuery($"Delete from ScoresaberRankedQueueTopMaps where RequestId={Convert.ToInt64(request[0])}");
                    //add to database
                    await DatabaseContext.ExecuteInsertQuery($"Insert into ScoresaberRankedQualifiedMaps(RequestId, SongName, DateFromQualified) values({Convert.ToInt64(request[0])}, '{request[1].ToString()}', '{DateTime.Now.ToString()}')");
                    //Create new embed and send
                    foreach (var chn in channelsToModify)
                    {
                        AddQualifiedMapsEmbedsInChannel(_discord, Convert.ToUInt64(chn[2]), Convert.ToUInt64(chn[6]), Convert.ToInt64(request[0]));
                    }
                    //await guildChannel.SendMessageAsync("", false, await CreateQualifiedEmbedinChannel(Convert.ToInt64(request[0])));
                    //-----
                }
            }

            //Check if qualified map should go to ranked maps
            var qualifiedMaps = await DatabaseContext.ExecuteSelectQuery($"Select * from ScoresaberRankedQualifiedMaps");
            foreach (var qualifiedmap in qualifiedMaps)
            {
                var rankedDate = DateTime.Parse(qualifiedmap[2].ToString());
                rankedDate = rankedDate.AddDays(4);
                if (rankedDate < DateTime.Now)
                {
                    //ScoresaberRecentRankedMaps
                    await DatabaseContext.ExecuteRemoveQuery($"Delete from ScoresaberRankedQualifiedMaps where RequestId={qualifiedmap[0]}");
                    await DatabaseContext.ExecuteInsertQuery($"Insert into ScoresaberRecentRankedMaps(RequestId, SongName, datesinceranked) values({qualifiedmap[0]}, '{qualifiedmap[1].ToString()}', '{DateTime.Now.ToString()}')");
                    //Remove embed in channels!
                    foreach (var chn in channelsToModify)
                    {
                        //Get qualified embed 
                        RemoveQualifiedEmbedsInChannels(Convert.ToUInt64(chn[2]), Convert.ToUInt64(chn[6]), qualifiedmap[0].ToString());
                    }
                }
            }

            //recent ranked map remove check 
            var recentRankedmaps = await DatabaseContext.ExecuteSelectQuery($"select * from ScoresaberRecentRankedMaps");
            foreach (var rrm in recentRankedmaps)
            {
                var dateToRemove = DateTime.Parse(rrm[2].ToString());
                dateToRemove = dateToRemove.AddDays(2);
                if(dateToRemove < DateTime.Now)
                {
                    await DatabaseContext.ExecuteRemoveQuery($"Delete from ScoresaberRecentRankedMaps where RequestId={rrm[0]}");
                }
            }

            //update recent ranked maps 
            foreach (var chn in channelsToModify)
            {
                ModifyRecentRankedMapsEmbedInChannel(Convert.ToUInt64(chn[2]), Convert.ToUInt64(chn[6]));
            }

            return;
        }

        public async Task<ulong> CreateRankedRequestTopQueueEmbedInChannel()
        {
            var guild = _discord.GetGuild(_guildId);
            var guildChannel = guild.GetTextChannel(_channelId);
            var rankedRequests = await ScoresaberAPI.GetTopRankedRequests();

            var description = "";
            foreach (var rankedRequest in rankedRequests.Requests)
            {
                description += $"**{rankedRequest.Name} - {rankedRequest.LevelAuthorName}** \n```" +
                    $"Created: {Math.Round((DateTime.Now - rankedRequest.CreatedAt).TotalDays)} days ago \n" +
                    $"RankingTeamVotes: {rankedRequest.RankVotes.Upvotes} upvotes | {rankedRequest.RankVotes.Downvotes} downvotes \n" +
                    $"QualityAssuranceTeamVotes: {rankedRequest.QatVotes.Upvotes} upvotes | {rankedRequest.QatVotes.Downvotes} downvotes```\n";
            }

            var embed = new EmbedBuilder()
            {
                Color = Color.Red,
                Title = "Ranked requests top queue",
                Description = description,
                Footer = new EmbedFooterBuilder() { Text = "Top Queue" }
            };

            var message = await guildChannel.SendMessageAsync("", false, embed.Build());
            return message.Id;
        }

        public async void RemoveQualifiedEmbedsInChannels(ulong guildId, ulong channelId, string id)
        {
            var chn = _discord.GetGuild(guildId).GetTextChannel(channelId);
            var message = chn.GetMessagesAsync(50).Flatten();
            try
            {
                var f = await message.FirstAsync();
                var embedToModify = await message.FirstAsync(x => x.Embeds.First().Footer.Value.Text == id) as RestUserMessage;
                await embedToModify.DeleteAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            //Place map in ranked embeds
        }


        public async Task ModifyAllCurrentQualifiers(ulong guildId, ulong channelId)
        {
            var chn = _discord.GetGuild(guildId).GetTextChannel(channelId);
            var message = chn.GetMessagesAsync(50).Flatten();
            var qualifiedMaps = await DatabaseContext.ExecuteSelectQuery($"select * from Scoresaberrankedqualifiedmaps");

            foreach (var qm in qualifiedMaps)
            {
                var embedToModify = await message.FirstAsync(x => x.Embeds.First().Footer.Value.Text == qm[0].ToString()) as RestUserMessage;

                var d = await DatabaseContext.ExecuteSelectQuery($"select * from ScoresaberRankedQualifiedMaps where RequestId={qm[0]}");
                var timeTillRanked = DateTime.Parse(d[0][2].ToString());
                timeTillRanked = timeTillRanked.AddHours(96);

                var embed = new EmbedBuilder()
                {
                    Color = Color.Orange,
                    Title = embedToModify.Embeds.First().Title,
                    Description = $"{Math.Round((timeTillRanked - DateTime.Now).TotalHours, 1)}h left",
                    Footer = new EmbedFooterBuilder() { Text = Convert.ToInt64(qm[0]).ToString() },
                    ThumbnailUrl = embedToModify.Embeds.First().Thumbnail.ToString(),
                    Url = embedToModify.Embeds.First().Url.ToString()
                };

                embedToModify.ModifyAsync(x => x.Embed = embed.Build());
            }
        }

        public async Task ModifyRankedRequestTopQueueEmbedInChannel(ulong guildId, ulong channelId)
        {
            var guild = _discord.GetGuild(guildId);
            var guildChannel = guild.GetTextChannel(channelId);
            var rankedRequests = await ScoresaberAPI.GetTopRankedRequests();

            var description = "";
            foreach (var rankedRequest in rankedRequests.Requests)
            {
                description += $"**{rankedRequest.Name} - {rankedRequest.LevelAuthorName}** \n```" +
                    $"Created: {Math.Round((DateTime.Now - rankedRequest.CreatedAt).TotalDays)} days ago \n" +
                    $"RankingTeamVotes: {rankedRequest.RankVotes.Upvotes} upvotes | {rankedRequest.RankVotes.Downvotes} downvotes \n" +
                    $"QualityAssuranceTeamVotes: {rankedRequest.QatVotes.Upvotes} upvotes | {rankedRequest.QatVotes.Downvotes} downvotes```\n";
            }

            var embed = new EmbedBuilder()
            {
                Color = Color.Red,
                Title = "Ranked requests top queue",
                Description = description,
                Footer = new EmbedFooterBuilder() { Text = "Top Queue" }
            };

            //get message to modify
            var message = guildChannel.GetMessagesAsync(50).Flatten();
            var embedToModify = await message.FirstAsync(x => x.Embeds.First().Footer.Value.Text == "Top Queue") as RestUserMessage;

            embedToModify.ModifyAsync(x => x.Embed = embed.Build());
        }

        public async Task<ulong> CreateRecentRankedMapsEmbedInChannel()
        {
            var guild = _discord.GetGuild(_guildId);
            var guildChannel = guild.GetTextChannel(_channelId);

            //get latest ranked maps
            var recentRankedMaps = await DatabaseContext.ExecuteSelectQuery($"select * from scoresaberrecentrankedmaps");

            var description = "";
            foreach (var rm in recentRankedMaps)
            {
                description += $"{rm[1]} \n";
            }


            var embed = new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = "Recent Ranked Maps",
                Description = $"Maps that are ranked in the last 2 days\n{description}",
                Footer = new EmbedFooterBuilder() { Text = "Recent Ranked List" }
            };

            var message = await guildChannel.SendMessageAsync("", false, embed.Build());
            return message.Id;
        }

        public async void ModifyRecentRankedMapsEmbedInChannel(ulong guildId, ulong channelId)
        {
            var guild = _discord.GetGuild(guildId);
            var guildChannel = guild.GetTextChannel(channelId);

            //get message to modify
            var message = guildChannel.GetMessagesAsync(50).Flatten();
            var embedToModify = await message.FirstAsync(x => x.Embeds.First().Footer.Value.Text == "Recent Ranked List") as RestUserMessage;

            //get latest ranked maps
            var recentRankedMaps = await DatabaseContext.ExecuteSelectQuery($"select * from scoresaberrecentrankedmaps");

            var description = "";
            foreach (var rm in recentRankedMaps)
            {
                description += $"**{rm[1]}** \n\n";
            }

            var embed = new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = "Recent Ranked Maps",
                Description = $"Maps that are ranked in the last 2 days\n\n{description}",
                Footer = new EmbedFooterBuilder() { Text = "Recent Ranked List" }
            };

            await embedToModify.ModifyAsync(x => x.Embed = embed.Build());

        }

        public async void CreateQualifiedMapsEmbedsInChannel(DiscordSocketClient discord, ulong guildId, ulong channelId)
        {
            var guild = discord.GetGuild(guildId);
            var guildChannel = guild.GetTextChannel(channelId);

            var qualifiedMaps = await DatabaseContext.ExecuteSelectQuery($"select * from ScoresaberRankedQualifiedMaps");

            foreach (var qm in qualifiedMaps)
            {
                var rankedRequest = await ScoresaberAPI.GetRankedRequests(Convert.ToInt64(qm[0]));

                var timeTillRanked = DateTime.Parse(qm[2].ToString());
                timeTillRanked = timeTillRanked.AddDays(4);

                var embed = new EmbedBuilder()
                {
                    Color = Color.Orange,
                    Title = rankedRequest.Request.Info.Name + " - " + rankedRequest.Request.Info.LevelAuthorName,
                    Description = $"{Math.Round((timeTillRanked - DateTime.Now).TotalHours, 1)}h left",
                    Footer = new EmbedFooterBuilder() { Text = Convert.ToInt64(qm[0]).ToString() },
                    ThumbnailUrl = $"https://new.scoresaber.com/api/static/covers/{rankedRequest.Request.Info.Id}.png",
                    Url = $"https://new.scoresaber.com/ranking/request/{Convert.ToInt64(qm[0]).ToString()}"
                };

                var message = await guildChannel.SendMessageAsync("", false, embed.Build());
            }

        }

        public async void AddQualifiedMapsEmbedsInChannel(DiscordSocketClient discord, ulong guildId, ulong channelId, long requestId)
        {
            var guild = discord.GetGuild(guildId);
            var guildChannel = guild.GetTextChannel(channelId);

            var qualifiedMaps = await DatabaseContext.ExecuteSelectQuery($"select * from ScoresaberRankedQualifiedMaps where requestId={requestId}");

            foreach (var qm in qualifiedMaps)
            {
                var rankedRequest = await ScoresaberAPI.GetRankedRequests(Convert.ToInt64(qm[0]));

                var timeTillRanked = DateTime.Parse(qm[2].ToString());
                timeTillRanked = timeTillRanked.AddDays(4);

                var embed = new EmbedBuilder()
                {
                    Color = Color.Orange,
                    Title = rankedRequest.Request.Info.Name + " - " + rankedRequest.Request.Info.LevelAuthorName,
                    Description = $"{Math.Round((timeTillRanked - DateTime.Now).TotalHours, 1)}h left",
                    Footer = new EmbedFooterBuilder() { Text = Convert.ToInt64(qm[0]).ToString() },
                    ThumbnailUrl = $"https://new.scoresaber.com/api/static/covers/{rankedRequest.Request.Info.Id}.png",
                    Url = $"https://new.scoresaber.com/ranking/request/{Convert.ToInt64(qm[0]).ToString()}"
                };

                var message = await guildChannel.SendMessageAsync("", false, embed.Build());
            }

        }
    }
}
