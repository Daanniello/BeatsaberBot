using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBeatSaberBot.Api.BeatSaverApi;
using DiscordBeatSaberBot.Api.GiphyApi;
using DiscordBeatSaberBot.Api.Spotify;
using DiscordBeatSaberBot.Api.TenorApi;
using DiscordBeatSaberBot.Commands.Functions;
using DiscordBeatSaberBot.Extensions;
using GiphyDotNet.Model.Parameters;

namespace DiscordBeatSaberBot.Commands
{
    internal class GlobalScoresaberCommands : ICommand
    {
        public static System.IDisposable triggerState = null;

        [Help("Search", "Get info about a scoresaber user", "!bs search [DiscordTag]/[ScoresaberID]/[Username]", HelpAttribute.Catergories.General)]
        public static async Task SearchUserCommand(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var parameter = message.Content.Substring(10).Trim();
            parameter = parameter.Replace("<@!", "").Replace(">", "");
            if (parameter == "") parameter = message.Author.Id.ToString();

            //Check if the parameter is a name or ID 
            var isUsername = false;
            if (!parameter.All(c => char.IsDigit(c))) isUsername = true;
            else isUsername = false;

            Embed embed = null;
            if (isUsername)
            {
                var player = await ScoresaberAPI.GetPlayerByName(parameter);
                if (player == null)
                {
                    await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Username not found.", "Could not find a user with this name on scoresaber.").Build());
                    return;
                }
                await BeatSaberInfoExtension.CreateUserSearchEmbedWithScoresaberIDAndSend(player.Players[0].PlayerId, message, discordSocketClient);
            }
            else
            {
                var scoreSaberID = parameter;
                if (await new RoleAssignment(discordSocketClient).CheckIfDiscordIdIsLinked(parameter)) scoreSaberID = await RoleAssignment.GetScoresaberIdWithDiscordId(parameter);
                await BeatSaberInfoExtension.CreateUserSearchEmbedWithScoresaberIDAndSend(scoreSaberID, message, discordSocketClient);
            }


        }

        [Help("Draw", "Draws a random card of someone in the top 50", "!bs draw", HelpAttribute.Catergories.General)]
        public static async Task Draw(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            BeatSaberCardCollection.DrawAndSendRandomCard(message);
        }

        [Help("Settings", "Show your own or someone else his Beat Saber settings and more", "\n`!bs settings` shows your own settings page \n`!bs settings @Silverhaze` shows the settings page from silverhaze \n`!bs settings 76561198033166451` shows the page from silverhaze. \n`!bs settings edit` edits a certain setting. \n`!bs settings create` creates your own settings page. \n`!bs settings remove` removes your page", HelpAttribute.Catergories.General)]
        public static async Task Settings(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            new Settings(discordSocketClient, message);
        }

        [Help("Playlist", "Creates a playlist based of key codes as input", "`!bs playlist create`", HelpAttribute.Catergories.General)]
        public static async Task Playlist(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var parameter = message.Content.Substring(13).Trim();

            if (parameter == "create")
            {
                var playlistModel = await Functions.Playlist.AskQuestions(message);
                var path = Functions.Playlist.Create(playlistModel);
                await message.Channel.SendFileAsync(path, text: "Done! You can download your file here");
                File.Delete(path);
            }
        }

        [Help("QualifiedMaps", "Shows all the current qualified maps", "`!bs qualifiedmaps`", HelpAttribute.Catergories.General)]
        public static async Task QualifiedMaps(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var maps = await ScoresaberAPI.GetQualifiedMaps();
            var description = "";
            var qualifiedmaps = maps.Songs.Where(x => x.Ranked == 0);
            foreach (var map in qualifiedmaps)
            {
                if (description.Length > 2000) continue;
                description += $"[link](https://scoresaber.com/leaderboard/{map.Uid}) | {map.SongAuthorName} - {map.Name} by {map.LevelAuthorName} \n";
            }

            await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Qualified Maps", $"{description}").Build());
        }

        [Help("Compare", "Compares two player's stats with each other.", "!bs compare (DiscordTag or ID player1) (DiscordTag or ID player2)", HelpAttribute.Catergories.General)]
        public static async Task Compare(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var embedBuilder = await BeatSaberInfoExtension.GetComparedEmbedBuilderNew(message.Content.Substring(11).Trim(), message, discordSocketClient);
            if (embedBuilder != null) await message.Channel.SendMessageAsync("", false, embedBuilder.Build());
        }

        [Help("Map", "Displays a maps info by searching it with the key code", "!bs map (Key)", HelpAttribute.Catergories.General)]
        public static async Task Map(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var search = message.Content.Substring(8);
            if(message.Content.StartsWith("!bsr ")) search = message.Content.Substring(5);
            if (!message.Content.Substring(0, 4).Contains("!bsr"))
            {
                var maps = await BeatSaverApi.GetMapsBySearch(search);
                if(maps == null || maps.Docs.Count() == 0)
                {
                    await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Error", $"**Could not find a map with the search value {search}**\n\n**The following type of searches are available:** \n*Scoresaber Hashcode*\n*BeatSaver KeyCode (Use `!bsr [ID]`)*\n*Map Name (could add mappers name for better results, or other elements)*").Build());
                    return;
                }
                search = maps.Docs.First().Id;
            }
            
            await BeatSaberInfoExtension.GetAndPostMapInfoWithKey(message, search);
        }

        [Help("RecentSong", "Get info from the latest song played", "!bs recentsong [DiscordTag or username]", HelpAttribute.Catergories.General)]
        public static async Task NewRecentSong(DiscordSocketClient discordSocketClient, SocketMessage message, bool isTopSong = false)
        {
            var parameters = message.Content.Substring(isTopSong ? 12 : 14).Trim();
            var parameterAmount = parameters.Split(" ").Count();
            var identity = await ValidationExtension.GetIdentityFromData(parameters.Split(" ")[0]);

            //Self function
            if(parameters.Length <= 4)
            {
                var number = 1;
                if (parameters != "") number = Convert.ToInt32(parameters.Trim());
                var scoresaberId = await RoleAssignment.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
                await BeatSaberInfoExtension.GetAndPostPlaythroughStatsWithScoresaberId(scoresaberId, message, number, isTopSong);
                return;
            }
            if(identity.Key == ValidationExtension.IdentityType.None)
            {
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Error", $"Could not find {parameters}").Build());
                return;
            }
            //third person function 
            else if(parameterAmount == 1)
            {
                if(identity.Key == ValidationExtension.IdentityType.DiscordID)
                {
                    var scoresaberId = await RoleAssignment.GetScoresaberIdWithDiscordId(identity.Value);
                    await BeatSaberInfoExtension.GetAndPostPlaythroughStatsWithScoresaberId(scoresaberId, message,isTopSong: isTopSong);
                    return;
                }
                else if(identity.Key == ValidationExtension.IdentityType.ScoresaberID)
                {
                    await BeatSaberInfoExtension.GetAndPostPlaythroughStatsWithScoresaberId(identity.Value, message, isTopSong: isTopSong);
                    return;
                }
                else if (identity.Key == ValidationExtension.IdentityType.Username)
                {
                    var player = await ScoresaberAPI.GetPlayerByName(identity.Value);
                    if(player == null)
                    {
                        await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Error", $"Could not find {identity.Value}").Build());
                        return;
                    }
                    var scoresaberID = player.Players[0].PlayerId;
                    await BeatSaberInfoExtension.GetAndPostPlaythroughStatsWithScoresaberId(scoresaberID, message, isTopSong: isTopSong);
                    return;
                }
            }
            //third person function with parameter
            else if (parameterAmount == 2)
            {
                var number = Convert.ToInt32(parameters.Split(" ")[1]);

                if (identity.Key == ValidationExtension.IdentityType.DiscordID)
                {
                    var scoresaberId = await RoleAssignment.GetScoresaberIdWithDiscordId(identity.Value);
                    await BeatSaberInfoExtension.GetAndPostPlaythroughStatsWithScoresaberId(scoresaberId, message, number, isTopSong);
                    return;
                }
                else if (identity.Key == ValidationExtension.IdentityType.ScoresaberID)
                {
                    await BeatSaberInfoExtension.GetAndPostPlaythroughStatsWithScoresaberId(identity.Value, message, number, isTopSong);
                    return;
                }
                else if (identity.Key == ValidationExtension.IdentityType.Username)
                {
                    var player = await ScoresaberAPI.GetPlayerByName(identity.Value);
                    if (player == null)
                    {
                        await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Error", $"Could not find {identity.Value}").Build());
                        return;
                    }
                    var scoresaberID = player.Players[0].PlayerId;
                    await BeatSaberInfoExtension.GetAndPostPlaythroughStatsWithScoresaberId(scoresaberID, message, number, isTopSong);
                    return;
                }
            }
        }       

        [Help("TopSong", "Get info from the latest song played", "!bs topsong [DiscordTag]", HelpAttribute.Catergories.General)]
        public static async Task NewTopSong(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            NewRecentSong(discordSocketClient, message, true);            
        }

        [Help("Improve", "Gives you a list of scoresaber maps to improve on", "!bs improve", HelpAttribute.Catergories.General)]
        public static async Task Improve(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (await r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()))
            {
                var scoresaberId = await RoleAssignment.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
                var acc = message.Content.Substring(11).Trim();
                double doubleAcc = 0;
                if (acc != "") doubleAcc = Convert.ToDouble(acc);
                var embedBuilder = await BeatSaberInfoExtension.GetImprovableMapsByAccFromToplist(scoresaberId, doubleAcc);
                message.Channel.SendMessageAsync("", false, embedBuilder.Build());
            }



        }

        [Help("Profile", "Creates a profile from your linked scoresaber as png", "!bs profile", HelpAttribute.Catergories.General)]
        public static async Task Profile(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (await r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()))
            {
                var scoresaberId = await RoleAssignment.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
                await BeatSaberInfoExtension.GetAndCreateProfileImage(scoresaberId);
                await message.Channel.SendFileAsync($"../../../Resources/img/RankingCard_{scoresaberId}.png");
                File.Delete($"../../../Resources/img/RankingCard_{scoresaberId}.png");
            }
            else
            {
                message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("No Scoresaber linked", "You have not linked your scoresaber with discord. Use '!bs link [ScoresaberId]' to link your account.").Build());
            }
        }

        [Help("Recentsongs", "Creates a profile from your linked scoresaber with your 5 recentsongs as png", "!bs recentsongs", HelpAttribute.Catergories.General)]
        public static async Task Recentsongs(DiscordSocketClient discordSocketClient, SocketMessage message, bool isTopsongs = false)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (await r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()))
            {
                var scoresaberId = await RoleAssignment.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
                var pageParameter = message.Content.Split(" ").Last().Trim();
                var pageNumber = 1;
                var hasPage = pageParameter.All(x => char.IsDigit(x)) ? pageNumber = Convert.ToInt32(pageParameter) : pageNumber = 1;
                //Create UserCard
                await BeatSaberInfoExtension.GetAndCreateUserCardImage(scoresaberId, isTopsongs ? "Topsongs" : "Recentsongs" + $" {(pageNumber == 1 ? "" : $"p.{ pageNumber}")}");
                await BeatSaberInfoExtension.GetAndCreateRecentsongsCardImage(scoresaberId, pageNumber, isTopsongs);
                if(File.Exists($"../../../Resources/img/UserCard_{scoresaberId}.png") && File.Exists($"../../../Resources/img/RecentsongsCard_{scoresaberId}.png"))
                {
                    await message.Channel.SendFileAsync($"../../../Resources/img/UserCard_{scoresaberId}.png");
                    await message.Channel.SendFileAsync($"../../../Resources/img/RecentsongsCard_{scoresaberId}.png");
                    File.Delete($"../../../Resources/img/RecentsongsCard_{scoresaberId}.png");
                    File.Delete($"../../../Resources/img/UserCard_{scoresaberId}.png");
                }
                else
                {
                    await message.Channel.SendMessageAsync("Couldn't create recentsongs");
                }
                
            }
            else
            {
                message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("No Scoresaber linked", "You have not linked your scoresaber with discord. Use '!bs link [ScoresaberId]' to link your account.").Build());
            }
        }

        [Help("Topsongs", "Creates a profile from your linked scoresaber with your 5 topsongs as png", "!bs topsongs", HelpAttribute.Catergories.General)]
        public static async Task TopSongs(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            await Recentsongs(discordSocketClient, message, true);
            //var r = new RoleAssignment(discordSocketClient);
            //if (await r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()))
            //{
            //    var scoresaberId = await RoleAssignment.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
            //    var pageParameter = message.Content.Split(" ").Last().Trim();
            //    var pageNumber = 1;
            //    var hasPage = pageParameter.All(x => char.IsDigit(x)) ? pageNumber = Convert.ToInt32(pageParameter) : pageNumber = 1;

            //    //Create UserCard
            //    await BeatSaberInfoExtension.GetAndCreateUserCardImage(scoresaberId, $"Topsongs {(pageNumber == 1 ? "" : $"p.{pageNumber}")}");
            //    await BeatSaberInfoExtension.GetAndCreateTopsongsCardImage(scoresaberId, pageNumber);
            //    await message.Channel.SendFileAsync($"../../../Resources/img/UserCard_{scoresaberId}.png");
            //    await message.Channel.SendFileAsync($"../../../Resources/img/TopsongsCard_{scoresaberId}.png");
            //    File.Delete($"../../../Resources/img/TopsongsCard_{scoresaberId}.png");
            //    File.Delete($"../../../Resources/img/UserCard_{scoresaberId}.png");
            //}
            //else
            //{
            //    message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("No Scoresaber linked", "You have not linked your scoresaber with discord. Use '!bs link [ScoresaberId]' to link your account.").Build());
            //}
        }

        [Help("randomcringe", "Gives a random gif from giphy.", "`!bs randomcringe [parameter]` \nShows nsfw if its in a nsfw channel.", HelpAttribute.Catergories.General)]
        public static async Task RandomCringe(DiscordSocketClient discordSocketClient, SocketMessage message)
        {

            var ch = message.Channel as SocketTextChannel;
            var rating = Rating.G;
            if (ch.IsNsfw) rating = Rating.R;

            var parameter = message.Content.Substring(16).Trim();
            var link = await new Giphy().SearchParameter(parameter, rating);
            if (link == null)
            {
                await message.Channel.SendMessageAsync("No results");
                return;
            }
            await message.Channel.SendMessageAsync(link);
        }

        [Help("test", "New function :O test them out SoonTM", "`!bs test`", HelpAttribute.Catergories.General)]
        public static async Task Test(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            Task.Run(async () => {
                var embed = EmbedBuilderExtension.NullEmbed("Warning", "Weeeeeee");
                embed.Color = Color.Red;
                var msg = await message.Channel.SendMessageAsync("", false, embed.Build());

                for (var i = 0; i < 4; i++)
                {
                    await Task.Delay(1000);
                    if (embed.Color == Color.Red)
                    {
                        embed.Description = "Woooooooo";
                        embed.Color = Color.Blue;
                    }
                    else
                    {
                        embed.Description = "Weeeeeeee";
                        embed.Color = Color.Red;
                    }

                    await msg.ModifyAsync(x => x.Embed = embed.Build());
                }
            });           
            
        }

        [Help("randomgif", "Gives a random gif from tenor.", "`!bs randomgif [parameter]` \nShows nsfw if its in a nsfw channel.", HelpAttribute.Catergories.General)]
        public static async Task RandomGif(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var ch = message.Channel as SocketTextChannel;

            var parameter = message.Content.Substring(13).Trim();

            if (parameter == "") parameter = null;

            var filter = Tenor.Schema.ContentFilter.Medium;
            if (ch.IsNsfw) filter = Tenor.Schema.ContentFilter.Off;

            var link = await TenorApi.GetGif(filter, parameter);
            if (link == null)
            {
                await message.Channel.SendMessageAsync("https://media1.tenor.com/images/6a22b36d7658ceb0d6984bf28c759100/tenor.gif?itemid=10902527");
            }
            else
            {
                await message.Channel.SendMessageAsync(link);
            }
        }
    }
}