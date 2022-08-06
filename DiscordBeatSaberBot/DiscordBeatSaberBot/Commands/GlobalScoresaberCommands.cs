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
using DiscordBeatSaberBot.Handlers;
using DiscordBeatSaberBot.Handlers.RankTrackerHandler;
using GiphyDotNet.Model.Parameters;
using Newtonsoft.Json;
using ScoreSaberLib;

namespace DiscordBeatSaberBot.Commands
{
    internal class GlobalScoresaberCommands : ICommand
    {
        public static System.IDisposable triggerState = null;

        [Help("Search", "Get info about a scoresaber user", "/search [DiscordTag]/[ScoresaberID]/[Username]", HelpAttribute.Catergories.General)]
        public static async Task SearchUserCommand(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {
            if (command.Data.Options.Count == 0)
            {
                await new Search(discordSocketClient).CreateUserSearchEmbedWithScoresaberIDAndSend(await RoleAssignment.GetScoresaberIdWithDiscordId(command.User.Id.ToString()), command, discordSocketClient);
            }
            var parameter = command.Data.Options.First();

            Embed embed = null;

            if (parameter.Name == "username")
            {
                var player = await ScoresaberAPI.GetPlayerByName(parameter.Value.ToString());
                if (player == null)
                {
                    await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Username not found.", "Could not find a user with this name on scoresaber.").Build());
                    return;
                }
                await new Search(discordSocketClient).CreateUserSearchEmbedWithScoresaberIDAndSend(player.Players[0].PlayerId, command, discordSocketClient);
            }
            else if (parameter.Name == "scoresaber_id")
            {
                var scoreSaberID = parameter.Value.ToString();

                await new Search(discordSocketClient).CreateUserSearchEmbedWithScoresaberIDAndSend(scoreSaberID, command, discordSocketClient);
            }
            else if (parameter.Name == "discord_tag" || parameter.Name == "discord_id")
            {
                var discordid = "";
                var f = (dynamic)parameter.Value;
                if (parameter.Name == "discord_tag") discordid = f.Id.ToString();
                else discordid = parameter.Value.ToString();
                await new Search(discordSocketClient).CreateUserSearchEmbedWithScoresaberIDAndSend(await RoleAssignment.GetScoresaberIdWithDiscordId(discordid), command, discordSocketClient);
            }


        }

        [Help("Draw", "Draws a random card of someone in the top 50", "!bs draw", HelpAttribute.Catergories.General)]
        public static async Task Draw(SocketSlashCommand command)
        {
            BeatSaberCardCollection.DrawAndSendRandomCard(command);
        }

        [Help("PatternCatalog", "Show the list of all pattern names and shows you a preview of how they look and more details about how to use them for mapping.", "/patterncatalog", HelpAttribute.Catergories.General)]
        public static async Task PatternCatalog(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {
            new PatternCatalog().Function(discordSocketClient, command);
        }

        [Help("Settings", "Show your own or someone else his Beat Saber settings and more", "\n`/settings` shows your own settings page \n`/settings @Silverhaze` shows the settings page from silverhaze \n`/settings 76561198033166451` shows the page from silverhaze. \n`/settings edit` edits a certain setting. \n`/settings create` creates your own settings page. \n`/settings remove` removes your page", HelpAttribute.Catergories.General)]
        public static async Task Settings(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {
            new Settings(discordSocketClient, command);
        }

        [Help("CupOfTheDay", "join the cup of the day", "join", HelpAttribute.Catergories.General)]
        public static async Task CupOfTheDay(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {
            if (command.Data.Options.FirstOrDefault(x => x.Value.ToString() == "MakePublicPrivate") != null)
            {
                if(discordSocketClient.GetGuild((ulong) command.GuildId).OwnerId == command.User.Id || command.User.Id == 138439306774577152)
                {
                    //Make server private / public 
                    var json = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDPublicServers.json");
                    var publicServers = JsonConvert.DeserializeObject<Dictionary<ulong, string>>(json);
                    if (publicServers == null || publicServers.Count == 0)
                    {
                        publicServers = new Dictionary<ulong, string>();
                        var guild = discordSocketClient.GetGuild((ulong)command.GuildId);
                        publicServers.Add((ulong)command.GuildId, guild.Name);
                        var msg = await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Done", "This server has been added to the list and is now public on http://beatsaberbot.com/CupOfTheDay").Build());
                    }
                    else
                    {
                        if (publicServers.Keys.Contains((ulong)command.GuildId))
                        {
                            publicServers.Remove((ulong)command.GuildId);
                            var msg = await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Done", "This server is now private").Build());
                        }
                        else
                        {
                            var guild = discordSocketClient.GetGuild((ulong)command.GuildId);
                            publicServers.Add((ulong)command.GuildId, guild.Name);
                            var msg = await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Done", "This server is now public on http://beatsaberbot.com/CupOfTheDay").Build());
                        }
                    }

                    var serversJson = JsonConvert.SerializeObject(publicServers);
                    System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDPublicServers.json", serversJson);

                    return;
                }
                else
                {
                    var msg = await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("No Permissions", "Only the server owner has access to this command").Build());
                    return;
                }
            }

            var r = new RoleAssignment(discordSocketClient);
            if(await r.CheckIfDiscordIdIsLinked(command.User.Id.ToString()))
            {
                var guild = discordSocketClient.GetGuild((ulong)command.GuildId);
                var scoresaberID = await RoleAssignment.GetScoresaberIdWithDiscordId(command.User.Id.ToString());
                var scoresaberplayer = await new ScoreSaberClient().Api.Players.GetPlayer(Convert.ToInt64(scoresaberID));
                var player = new CupOfTheDayHandler.Player() { ScoreSaberID = scoresaberplayer.Id, Name = scoresaberplayer.Name };
                CupOfTheDayHandler.StorePlayer(player, guild.Id.ToString());

                var msg = await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Done!", "You have joined the cup of the day. for the leaderboard, check out http://beatsaberbot.com/CupOfTheDay").Build());
            }
            else
            {
                var msg = await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Not Linked", "To join the cupoftheday, you need to be linked with your scoresaber account. please use the command /link").Build());
            }
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

        [Help("Compare", "Compares two player's stats with each other.", "/compare (DiscordTag or ID player1) (DiscordTag or ID player2)", HelpAttribute.Catergories.General)]
        public static async Task Compare(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {
            var embedBuilder = await BeatSaberInfoExtension.GetComparedEmbedBuilderNew(command, discordSocketClient);
            if (embedBuilder != null) await command.Channel.SendMessageAsync("", false, embedBuilder.Build());
        }

        [Help("RankTracker", "Gives notifications about changes in ranked stats", "/ranktracker", HelpAttribute.Catergories.General)]
        public static async Task RankTracker(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {
            var rankTracker = new RankTrackerHandler(discordSocketClient);
            var isBeingTracker = await rankTracker.IsBeingTracked(Convert.ToInt64(command.User.Id));
            if (!isBeingTracker)
            {
                var msg = await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("RankTracker", "Do you want to turn on the rank tracker? This will notify you in DM about any of the following stat changes \n ```- Global Rank ```\n\n Type `yes` to activate the rank tracker.").Build());
                var timeNow = DateTime.Now;
                var timeToEnd = timeNow.AddSeconds(20);
                do
                {
                    await Task.Delay(1000);
                    var messages = await msg.Channel.GetMessagesAsync(10).FlattenAsync();
                    if (messages.FirstOrDefault(x => x.Content.ToLower() == "yes") != null && messages.FirstOrDefault(x => x.Content.ToLower() == "yes").CreatedAt > timeNow && messages.FirstOrDefault(x => x.Content == "yes").Author.Id == command.User.Id)
                    {
                        var result = await rankTracker.AddPlayerToRankTracker(Convert.ToInt64(command.User.Id));
                        if (result) msg.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("RankTracker", "You have been succesfully added. You will now be notified about rank changes in DM").Build());
                        else msg.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("RankTracker", "You could not be added to the ranktracker. Make sure you are linked with the bot by typing `!bs link [scoresaberID]`").Build());
                        break;
                    }
                } while (timeNow < timeToEnd);
            }
            else
            {
                var msg = await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("RankTracker", "You are curently being tracked already. Do you wish to disable the notifications? Type `yes` to deactivate the rank tracker.").Build());
                var timeNow = DateTime.Now;
                var timeToEnd = timeNow.AddSeconds(20);
                do
                {
                    await Task.Delay(1000);
                    var messages = await msg.Channel.GetMessagesAsync(10).FlattenAsync();
                    if (messages.FirstOrDefault(x => x.Content.ToLower() == "yes") != null && messages.FirstOrDefault(x => x.Content.ToLower() == "yes").CreatedAt > timeNow && messages.FirstOrDefault(x => x.Content == "yes").Author.Id == command.User.Id)
                    {
                        var result = await rankTracker.DeletePlayerFromRankTracker(Convert.ToInt64(command.User.Id));
                        if (result) msg.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("RankTracker", "You have been succesfully deleted from the rank tracker. You won't be notified anymore about rank changes in DM").Build());
                        else msg.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("RankTracker", "You could not be deleted from the ranktracker. Make sure you are linked with the bot by typing `!bs link [scoresaberID]`").Build());
                        break;
                    }
                } while (timeNow < timeToEnd);
            }
        }

        [Help("Map", "Displays a maps info by searching it with the key code", "!bs map (Key)", HelpAttribute.Catergories.General)]
        public static async Task Map(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {
            var search = command.Data.Options.First().Value.ToString().Trim();
            if (!search.All(x => char.IsDigit(x)))
            {
                var maps = await BeatSaverApi.GetMapsBySearch(search);
                if (maps == null || maps.Docs.Count() == 0)
                {
                    await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Error", $"**Could not find a map with the search value {search}**\n\n**The following type of searches are available:** \n*Scoresaber Hashcode*\n*BeatSaver KeyCode (Use `!bsr [ID]`)*\n*Map Name (could add mappers name for better results, or other elements)*").Build());
                    return;
                }
                search = maps.Docs.First().Id;
            }

            await BeatSaberInfoExtension.GetAndPostMapInfoWithKey(command, search);
        }

        [Help("RecentSong", "Get info from the latest song played", "!bs recentsong [DiscordTag or username]", HelpAttribute.Catergories.General)]
        public static async Task NewRecentSong(DiscordSocketClient discordSocketClient, SocketSlashCommand command, bool isTopSong = false)
        {
            var playthroughStats = new PlaythroughStats(discordSocketClient);

            var pageNr = command.Data.Options.Count > 0 ? (command.Data.Options.Where(x => x.Name == "page").Count() > 0 ? Convert.ToInt32(command.Data.Options.First(x => x.Name == "page").Value) : 1) : 1;

            //Self
            if (!(command.Data.Options.Where(x => x.Name != "page").Count() > 0))
            {
                var scoresaberId = await RoleAssignment.GetScoresaberIdWithDiscordId(command.User.Id.ToString());
                await playthroughStats.GetAndPostPlaythroughStatsWithScoresaberId(scoresaberId, command, pageNr, isTopSong);
                return;
            }

            //ScoresaberID
            if (command.Data.Options.FirstOrDefault(x => x.Name == "scoresaber_id") != null)
            {
                await playthroughStats.GetAndPostPlaythroughStatsWithScoresaberId(command.Data.Options.FirstOrDefault(x => x.Name == "scoresaber_id").Value.ToString(), command, pageNr, isTopSong: isTopSong);
                return;
            }

            //DiscordID
            if (command.Data.Options.FirstOrDefault(x => x.Name == "discord_id") != null)
            {
                var scoresaberId = await RoleAssignment.GetScoresaberIdWithDiscordId(command.Data.Options.FirstOrDefault(x => x.Name == "discord_id").Value.ToString());
                await playthroughStats.GetAndPostPlaythroughStatsWithScoresaberId(scoresaberId, command, pageNr, isTopSong: isTopSong);
                return;
            }

            //DiscordTag
            if (command.Data.Options.FirstOrDefault(x => x.Name == "mention") != null)
            {
                var discordID = (dynamic) command.Data.Options.FirstOrDefault(x => x.Name == "mention").Value;
                var scoresaberId = await RoleAssignment.GetScoresaberIdWithDiscordId(discordID.Id.ToString());
                await playthroughStats.GetAndPostPlaythroughStatsWithScoresaberId(scoresaberId, command, pageNr, isTopSong: isTopSong);
                return;
            }
        }

        [Help("TopSong", "Get info from the latest song played", "!bs topsong [DiscordTag]", HelpAttribute.Catergories.General)]
        public static async Task NewTopSong(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {
            NewRecentSong(discordSocketClient, command, true);
        }

        [Help("Improve", "Gives you a list of scoresaber maps to improve on", "`/improve [WishedAcc]`", HelpAttribute.Catergories.General)]
        public static async Task Improve(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (await r.CheckIfDiscordIdIsLinked(command.User.Id.ToString()))
            {
                var scoresaberId = await RoleAssignment.GetScoresaberIdWithDiscordId(command.User.Id.ToString());
                var doubleAcc = (double)command.Data.Options.First().Value;
                var embedBuilder = await BeatSaberInfoExtension.GetImprovableMapsByAccFromToplist(scoresaberId, doubleAcc);
                await command.Channel.SendMessageAsync("", false, embedBuilder.Build());
            }
            else
            {
                await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Discord not linked", "For this command you have to link your discord account with scoresaber. You can do this by using the `/link` command").Build());
            }
        }

        [Help("Profile", "Creates a profile from your linked scoresaber as png", "/profile", HelpAttribute.Catergories.General)]
        public static async Task Profile(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (await r.CheckIfDiscordIdIsLinked(command.User.Id.ToString()))
            {
                var scoresaberId = await RoleAssignment.GetScoresaberIdWithDiscordId(command.User.Id.ToString());
                var cardID = await BeatSaberInfoExtension.GetAndCreateProfileImage(scoresaberId);
                await command.Channel.SendMessageAsync($"{GlobalConfiguration.BotImageStorageLink}RankingCard_{scoresaberId}_{cardID}.png");
            }
            else
            {
                await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("No Scoresaber linked", "You have not linked your scoresaber with discord. Use the '/link' command to link your account.").Build());
            }
        }

        [Help("Recentsongs", "Creates a profile from your linked scoresaber with your 5 recentsongs as png", "!bs recentsongs", HelpAttribute.Catergories.General)]
        public static async Task Recentsongs(DiscordSocketClient discordSocketClient, SocketSlashCommand command, bool isTopsongs = false)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (await r.CheckIfDiscordIdIsLinked(command.User.Id.ToString()))
            {
                var scoresaberId = await RoleAssignment.GetScoresaberIdWithDiscordId(command.User.Id.ToString());
                var pageNumber = (command.Data.Options.Count > 0) ? Convert.ToInt32(command.Data.Options.First().Value) : 1;

                //Create UserCard
                await BeatSaberInfoExtension.GetAndCreateUserCardImage(scoresaberId, isTopsongs ? "Topsongs" : "Recentsongs" + $" {(pageNumber == 1 ? "" : $"p.{ pageNumber}")}");
                var guid = await BeatSaberInfoExtension.GetAndCreateRecentsongsCardImage(scoresaberId, pageNumber, isTopsongs);
                if (File.Exists($"../../../Resources/img/UserCard_{scoresaberId}.png") && guid != Guid.Empty)
                {
                    var embedBuilder = new EmbedBuilder();
                    embedBuilder.ImageUrl = $"{GlobalConfiguration.BotImageStorageLink}{(isTopsongs ? "TopsongsCard" : "RecentsongsCard")}_{scoresaberId}_{guid}.png";
                    await command.Channel.SendFileAsync($"../../../Resources/img/UserCard_{scoresaberId}.png", embed: embedBuilder.Build());
                    File.Delete($"../../../Resources/img/UserCard_{scoresaberId}.png");
                }
                else
                {
                    await command.Channel.SendMessageAsync("Couldn't create recentsongs");
                }

            }
            else
            {
                command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("No Scoresaber linked", "You have not linked your scoresaber with discord. Use '!bs link [ScoresaberId]' to link your account.").Build());
            }
        }

        [Help("Topsongs", "Creates a profile from your linked scoresaber with your 5 topsongs as png", "!bs topsongs", HelpAttribute.Catergories.General)]
        public static async Task TopSongs(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {
            await Recentsongs(discordSocketClient, command, true);
        }

        [Help("DiceRoll", "Rolls a dice with an x amount of sides.", "/diceroll", HelpAttribute.Catergories.General)]
        public static async Task DiceRoll(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {
            var randomMaxNumber = 6;
            SocketSlashCommandDataOption type = null;
            if (command.Data.Options.Count > 0) type = command.Data.Options.First();
            if (type == null) randomMaxNumber = 6;
            else
            {
                randomMaxNumber = Convert.ToInt32(type.Value);
            }

            var randomNumber = new Random().Next(0, randomMaxNumber);
            await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed($"{randomNumber}", $"You rolled a {randomNumber} with a {randomMaxNumber} sided dice").Build());
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

        [Help("randomgif", "Gives a random gif from tenor.", "`/randomgif [parameter]` \nShows nsfw if its in a nsfw channel.", HelpAttribute.Catergories.General)]
        public static async Task RandomGif(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {

            var channel = command.Channel as SocketTextChannel;

            string parameter = null;
            if (command.Data.Options.Count > 0) parameter = command.Data.Options.First().Value.ToString();

            var filter = Tenor.Schema.ContentFilter.Medium;
            if (channel.IsNsfw) filter = Tenor.Schema.ContentFilter.Off;

            var link = await TenorApi.GetGif(filter, parameter);
            if (link == null)
            {
                await channel.SendMessageAsync("https://media1.tenor.com/images/6a22b36d7658ceb0d6984bf28c759100/tenor.gif?itemid=10902527");
            }
            else
            {
                await channel.SendMessageAsync(link);
            }
        }
    }
}