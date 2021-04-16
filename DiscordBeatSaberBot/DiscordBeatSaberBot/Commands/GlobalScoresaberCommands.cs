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

            if(parameter == "create")
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
            var keycode = message.Content.Substring(8);
            await BeatSaberInfoExtension.GetAndPostMapInfoWithKey(message, keycode);
        }

        [Help("RecentSong", "Get info from the latest song played", "!bs recentsong [DiscordTag or username]", HelpAttribute.Catergories.General)]
        public static async Task NewRecentSong(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var content = message.Content;
            var r = new RoleAssignment(discordSocketClient);
            var discordId = message.Author.Id.ToString();
            var userId = "";
            if (int.TryParse(message.Content.Split(' ').Last(), out int n)) content = content.Substring(0, content.IndexOf(content.Split(' ').Last()) - 1);
            if (n == 0) n = 1;

            if (message.Content.Contains("@"))
            {
                discordId = message.Content.Split(' ')[2].Replace("<@!", "").Replace(">", "");
                userId = discordId;
            }

            if (await r.CheckIfDiscordIdIsLinked(discordId))
            {
                var scoresaberId = await RoleAssignment.GetScoresaberIdWithDiscordId(discordId);

                await BeatSaberInfoExtension.GetAndPostRecentSongWithScoresaberIdNew(scoresaberId, message, n);
            }
            else
            {
                if (content.Length <= 14)
                {
                    await message.Channel.SendMessageAsync("", false,
                        EmbedBuilderExtension.NullEmbed("Search failed",
                                "You are not linked with the bot yet. Use (!bs link [scoresaberid]) to link", null, null)
                            .Build());
                    return;
                }
                if (userId != "")
                {
                    discordId = await RoleAssignment.GetScoresaberIdWithDiscordId(userId);
                    if (discordId == "")
                    {
                        await message.Channel.SendMessageAsync("", false,
                        EmbedBuilderExtension.NullEmbed("Search failed",
                                "The person you are trying to search on is not linked with the bot", null, null)
                            .Build());
                        return;
                    }
                }

                await BeatSaberInfoExtension.GetAndPostRecentSongWithScoresaberIdNew(discordId, message, n);
            }
        }

        [Help("TopSong", "Get info from the latest song played", "!bs topsong [DiscordTag]", HelpAttribute.Catergories.General)]
        public static async Task NewTopSong(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var content = message.Content;
            var discordId = message.Author.Id.ToString();
            if (int.TryParse(message.Content.Split(' ').Last(), out int n)) content = content.Substring(0, content.IndexOf(content.Split(' ').Last()) - 1);
            if (n == 0) n = 1;

            var userId = "";
            if (message.Content.Contains("@"))
            {
                discordId = message.Content.Split(' ')[2].Replace("<@!", "").Replace(">", "");
                userId = discordId;
            }
            var r = new RoleAssignment(discordSocketClient);
            if (await r.CheckIfDiscordIdIsLinked(discordId) && content.Count() == 11)
            {
                var scoresaberId = await RoleAssignment.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());

                await BeatSaberInfoExtension.GetNewTopSongWithScoresaberIdNew(scoresaberId, message, n);

            }
            else
            {
                if (message.Content.Length <= 11)
                {
                    await message.Channel.SendMessageAsync("", false,
                        EmbedBuilderExtension.NullEmbed("Search failed",
                                "You are not linked with the bot yet. Use (!bs link [scoresaberid]) to link", null, null)
                            .Build());
                    return;
                }

                if (userId != "")
                {
                    discordId = await RoleAssignment.GetScoresaberIdWithDiscordId(userId);
                    if (discordId == "")
                    {
                        await message.Channel.SendMessageAsync("", false,
                        EmbedBuilderExtension.NullEmbed("Search failed",
                                "The person you are trying to search on is not linked with the bot", null, null)
                            .Build());
                        return;
                    }
                }

                await BeatSaberInfoExtension.GetNewTopSongWithScoresaberIdNew(discordId, message, n);

            }
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
        public static async Task Recentsongs(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (await r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()))
            {
                var scoresaberId = await RoleAssignment.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
                //Create UserCard
                await BeatSaberInfoExtension.GetAndCreateUserCardImage(scoresaberId, "Recentsongs");
                await BeatSaberInfoExtension.GetAndCreateRecentsongsCardImage(scoresaberId);
                await message.Channel.SendFileAsync($"../../../Resources/img/UserCard_{scoresaberId}.png");
                await message.Channel.SendFileAsync($"../../../Resources/img/RecentsongsCard_{scoresaberId}.png");
                File.Delete($"../../../Resources/img/RecentsongsCard_{scoresaberId}.png");
                File.Delete($"../../../Resources/img/UserCard_{scoresaberId}.png");
            }
            else
            {
                message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("No Scoresaber linked", "You have not linked your scoresaber with discord. Use '!bs link [ScoresaberId]' to link your account.").Build());
            }
        }

        [Help("Topsongs", "Creates a profile from your linked scoresaber with your 5 topsongs as png", "!bs topsongs", HelpAttribute.Catergories.General)]
        public static async Task TopSongs(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (await r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()))
            {
                var scoresaberId = await RoleAssignment.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
                //Create UserCard
                await BeatSaberInfoExtension.GetAndCreateUserCardImage(scoresaberId, "Topsongs");
                await BeatSaberInfoExtension.GetAndCreateTopsongsCardImage(scoresaberId);
                await message.Channel.SendFileAsync($"../../../Resources/img/UserCard_{scoresaberId}.png");
                await message.Channel.SendFileAsync($"../../../Resources/img/TopsongsCard_{scoresaberId}.png");
                File.Delete($"../../../Resources/img/TopsongsCard_{scoresaberId}.png");
                File.Delete($"../../../Resources/img/UserCard_{scoresaberId}.png");
            }
            else
            {
                message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("No Scoresaber linked", "You have not linked your scoresaber with discord. Use '!bs link [ScoresaberId]' to link your account.").Build());
            }
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