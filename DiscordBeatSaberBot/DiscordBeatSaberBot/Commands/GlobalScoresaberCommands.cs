using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBeatSaberBot.Api.GiphyApi;
using DiscordBeatSaberBot.Api.Spotify;
using DiscordBeatSaberBot.Extensions;
using GiphyDotNet.Model.Parameters;

namespace DiscordBeatSaberBot.Commands
{
    internal class GlobalScoresaberCommands : ICommand
    {
        public static System.IDisposable triggerState = null;

        [Help("Compare", "Compares two player's stats with each other.", "!bs compare (DiscordTag or ID player1) (DiscordTag or ID player2)", HelpAttribute.Catergories.General)]
        public static async Task Compare(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var embedBuilder = await BeatSaberInfoExtension.GetComparedEmbedBuilder(message.Content.Substring(12), message, discordSocketClient);
            await message.Channel.SendMessageAsync("", false, embedBuilder.Build());
        }

        [Help("CompareNew", "Compares two player's stats with each other.", "!bs compare (DiscordTag or ID player1) (DiscordTag or ID player2)", HelpAttribute.Catergories.General)]
        public static async Task CompareNew(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var embedBuilder = await BeatSaberInfoExtension.GetComparedEmbedBuilderNew(message.Content.Substring(12), message, discordSocketClient);
            if (embedBuilder != null) await message.Channel.SendMessageAsync("", false, embedBuilder.Build());
        }

        [Help("Songs", "Searches up the song with the name", "!bs songs", HelpAttribute.Catergories.General)]
        public static async Task Songs(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var builderList = await BeatSaberInfoExtension.GetSongs(message.Content.Substring(10));
            if (builderList.Count > 4)
            {
                await message.Channel.SendMessageAsync("", false,
                    EmbedBuilderExtension.NullEmbed("Search term to wide",
                        "I can not post more as 6 songs. " + "\n Try searching with a more specific word please. \n" +
                        ":rage:", null, null).Build());
            }
            else
            {
                foreach (var builder in builderList)
                {
                    await message.Channel.SendMessageAsync("", false, builder.Build());
                }
            }
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
                var scoresaberId = await r.GetScoresaberIdWithDiscordId(discordId);

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
                    discordId = await r.GetScoresaberIdWithDiscordId(userId);
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
                var scoresaberId = await r.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());

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
                    discordId = await r.GetScoresaberIdWithDiscordId(userId);
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

        [Help("Typing", "Turn on typing in the channel 'On' or 'Off'", "!bs typing (on or off)", HelpAttribute.Catergories.General)]
        public static async Task Typing(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var option = message.Content.Substring(10);
            if (option.Contains("on"))
            {
                GlobalScoresaberCommands.triggerState = message.Channel.EnterTypingState(new Discord.RequestOptions() { RetryMode = Discord.RetryMode.AlwaysFail, Timeout = 1 });
                message.Channel.SendMessageAsync("Let me tell you something...");

            }
            else if (option.Contains("off"))
            {
                if (GlobalScoresaberCommands.triggerState != null)
                {
                    GlobalScoresaberCommands.triggerState.Dispose();

                }
            }
            else
            {


            }
        }

        [Help("CreateRankMapFeed", "Creates a ranmap feed. Topqueue, qualified maps and recent ranked maps as feed", "Do this in a empty channel! no people are allowed to talk in there.", HelpAttribute.Catergories.AdminCommands)]
        public static async Task CreateRankMapFeed(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var chnl = message.Channel as SocketGuildChannel;
            var guild = chnl.Guild;
            if (message.Author.Id == guild.OwnerId)
            {
                await message.DeleteAsync();
                new ScoresaberRankMapsFeed(discordSocketClient).CreateRankmapFeed(guild.Id, chnl.Id);
            }
            else
            {
                var msg = await message.Channel.SendMessageAsync("Only the owner of the server can call this command.");
                await Task.Delay(3000);
                await msg.DeleteAsync();
            }
        }

        [Help("Improve", "Gives you a list of scoresaber maps to improve on", "!bs improve", HelpAttribute.Catergories.General)]
        public static async Task Improve(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (await r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()))
            {
                var scoresaberId = await r.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
                var embedBuilder = await BeatSaberInfoExtension.GetImprovableMapsByAccFromToplist(scoresaberId, Convert.ToDouble(message.Content.Substring(12)));
                message.Channel.SendMessageAsync("", false, embedBuilder.Build());
            }



        }

        [Help("Profile", "Creates a profile from your linked scoresaber as png", "!bs profile", HelpAttribute.Catergories.General)]
        public static async Task Profile(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (await r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()))
            {
                var scoresaberId = await r.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
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
                var scoresaberId = await r.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
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
                var scoresaberId = await r.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
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

        [Help("Seal", "Gives a random image of a seal", "!bs seal", HelpAttribute.Catergories.General)]
        public static async Task Seal(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var link = await new Giphy().SearchParameter("seal");
            await message.Channel.SendMessageAsync(link);
        }

        [Help("randomgif", "Gives a random gif from a parameter", "!bs randomgif [parameter]", HelpAttribute.Catergories.General)]
        public static async Task RandomGif(DiscordSocketClient discordSocketClient, SocketMessage message)
        {

            var ch = message.Channel as SocketTextChannel;
            var rating = Rating.G;
            if (ch.IsNsfw) rating = Rating.R;

            var parameter = message.Content.Substring(14);
            var link = await new Giphy().SearchParameter(parameter, rating);
            if (link == null)
            {
                await message.Channel.SendMessageAsync("No results");
                return;
            }
            await message.Channel.SendMessageAsync(link);
        }

        [Help("Search", "Shows new scoresaber information about a player", "!bs search [DiscordTag or username]", HelpAttribute.Catergories.General)]
        public static async Task NewSearch(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (message.Content.Contains("@"))
            {
                var discordId = message.Content.Substring(11).Replace("<", "").Replace(">", "").Replace("@", "")
                    .Replace("!", "");
                if (await r.CheckIfDiscordIdIsLinked(discordId))
                {
                    var scoresaberId = await r.GetScoresaberIdWithDiscordId(discordId);
                    var embedTask = new List<EmbedBuilder>();
                    try
                    {
                        embedTask = await BeatSaberInfoExtension.GetPlayerSearchInfoEmbed(scoresaberId, message);

                    }
                    catch (Exception ex)
                    {
                        await message.Channel.SendMessageAsync("", false,
                        EmbedBuilderExtension.NullEmbed("Error",
                            ex.Message).Build());
                    }

                    if (embedTask.Count() == 0)
                    {
                        await message.Channel.SendMessageAsync("", false,
                        EmbedBuilderExtension.NullEmbed("No results",
                            "Try linking your discord and scoresaber again with like this: https://scoresaber.com/u/76561198033166451").Build());
                    }

                    foreach (var embedBuilder in embedTask)
                    {
                        await message.Channel.SendMessageAsync("", false, embedBuilder.Build());
                    }
                }
                else
                {
                    await message.Channel.SendMessageAsync("", false,
                        EmbedBuilderExtension.NullEmbed("Not linked yet!",
                            "Your discord is not linked yet. Type !bs link [Scoresaberlink] to link it.").Build());
                }
            }
            else if (await r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()) && message.Content.Count() == 10)
            {
                var scoresaberId = await r.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
                var embedTask = await BeatSaberInfoExtension.GetPlayerSearchInfoEmbed(scoresaberId, message);

                foreach (var embedBuilder in embedTask)
                {
                    await message.Channel.SendMessageAsync("", false, embedBuilder.Build());
                }

            }
            else
            {
                if (message.Content.Substring(11).Count() == 0)
                {
                    await message.Channel.SendMessageAsync("", false,
                        EmbedBuilderExtension.NullEmbed("User's discord not linked",
                            "Your discord is not linked yet. Type !bs link [Scoresaberlink] to link it.", null,
                            null).Build());
                    return;
                }

                var playerId = await BeatSaberInfoExtension.GetPlayerId(message.Content.Substring(11));
                var embedTask = await BeatSaberInfoExtension.GetPlayerSearchInfoEmbed(playerId, message);
                foreach (var embedBuilder in embedTask)
                {
                    await message.Channel.SendMessageAsync("", false, embedBuilder.Build());
                }
            }
        }
    }
}