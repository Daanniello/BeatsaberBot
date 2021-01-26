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
    internal class GlobalScoreSaberCommands : ICommand
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
            if(embedBuilder != null) await message.Channel.SendMessageAsync("", false, embedBuilder.Build());
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
            var r = new RoleAssignment(discordSocketClient);
            var discordId = message.Author.Id.ToString();
            if (message.Content.Split(' ').Count() == 3) discordId = message.Content.Split(' ')[2].Replace("<@!", "").Replace(">", "");
            if (await r.CheckIfDiscordIdIsLinked(discordId))
            {
                var ScoreSaberId = await r.GetScoreSaberIdWithDiscordId(discordId);

                await BeatSaberInfoExtension.GetAndPostRecentSongWithScoreSaberId(ScoreSaberId, message);         
            }
            else
            {
                if (message.Content.Length <= 14)
                {
                    await message.Channel.SendMessageAsync("", false,
                        EmbedBuilderExtension.NullEmbed("Search failed",
                                "Search value is not long enough. it should be larger than 3 characters.", null, null)
                            .Build());
                    return;
                }

                var username = message.Content.Substring(14);
                var id = username.Trim();
                if (id.All(char.IsDigit))
                {

                }
                else
                {
                    id = await BeatSaberInfoExtension.GetPlayerId(username);
                }               
                await BeatSaberInfoExtension.GetAndPostRecentSongWithScoreSaberId(id, message);
            }
        }

        [Help("TopSong", "Get info from the latest song played", "!bs topsong [DiscordTag or username]", HelpAttribute.Catergories.General)]
        public static async Task NewTopSong(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (await r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()) && message.Content.Count() == 11)
            {
                var ScoreSaberId = await r.GetScoreSaberIdWithDiscordId(message.Author.Id.ToString());

                var embedTask = await BeatSaberInfoExtension.GetNewTopSongWithScoreSaberId(ScoreSaberId);
                await message.Channel.SendMessageAsync("", false, embedTask.Build());
            }
            else
            {
                if (message.Content.Length <= 11)
                {
                    await message.Channel.SendMessageAsync("", false,
                        EmbedBuilderExtension.NullEmbed("Search failed",
                                "Search value is not long enough. it should be larger than 3 characters.", null, null)
                            .Build());
                    return;
                }

                var username = message.Content.Substring(11);
                var id = await BeatSaberInfoExtension.GetPlayerIdsWithUsername(username);
                var embedTask = await BeatSaberInfoExtension.GetNewTopSongWithScoreSaberId(id);
                await message.Channel.SendMessageAsync("", false, embedTask.Build());
            }
        }

        [Help("Typing", "Turn on typing in the channel 'On' or 'Off'", "!bs typing (on or off)", HelpAttribute.Catergories.General)]
        public static async Task Typing(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var option = message.Content.Substring(10);
            if (option.Contains("on"))
            {
                GlobalScoreSaberCommands.triggerState = message.Channel.EnterTypingState(new Discord.RequestOptions() { RetryMode = Discord.RetryMode.AlwaysFail, Timeout = 1 });
                message.Channel.SendMessageAsync("Let me tell you something...");

            }
            else if (option.Contains("off"))
            {
                if (GlobalScoreSaberCommands.triggerState != null)
                {
                    GlobalScoreSaberCommands.triggerState.Dispose();

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
                new ScoreSaberRankMapsFeed(discordSocketClient).CreateRankmapFeed(guild.Id, chnl.Id);
            }
            else
            {
                var msg = await message.Channel.SendMessageAsync("Only the owner of the server can call this command.");
                await Task.Delay(3000);
                await msg.DeleteAsync();
            }
        }

        [Help("Improve", "Gives you a list of ScoreSaber maps to improve on", "!bs improve", HelpAttribute.Catergories.General)]
        public static async Task Improve(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (await r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()))
            {
                var ScoreSaberId = await r.GetScoreSaberIdWithDiscordId(message.Author.Id.ToString());
                var embedBuilder = await BeatSaberInfoExtension.GetImprovableMapsByAccFromToplist(ScoreSaberId, Convert.ToDouble(message.Content.Substring(12)));
                message.Channel.SendMessageAsync("", false, embedBuilder.Build());
            }



        }

        [Help("Profile", "Creates a profile from your linked ScoreSaber as png", "!bs profile", HelpAttribute.Catergories.General)]
        public static async Task Profile(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (await r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()))
            {
                var ScoreSaberId = await r.GetScoreSaberIdWithDiscordId(message.Author.Id.ToString());
                await BeatSaberInfoExtension.GetAndCreateProfileImage(ScoreSaberId);
                await message.Channel.SendFileAsync($"../../../Resources/img/RankingCard_{ScoreSaberId}.png");
                File.Delete($"../../../Resources/img/RankingCard_{ScoreSaberId}.png");
            }
            else
            {
                message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("No ScoreSaber linked", "You have not linked your ScoreSaber with discord. Use '!bs link [ScoreSaberId]' to link your account.").Build());
            }
        }

        [Help("Recentsongs", "Creates a profile from your linked ScoreSaber with your 5 recentsongs as png", "!bs recentsongs", HelpAttribute.Catergories.General)]
        public static async Task Recentsongs(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (await r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()))
            {
                var ScoreSaberId = await r.GetScoreSaberIdWithDiscordId(message.Author.Id.ToString());
                //Create UserCard
                await BeatSaberInfoExtension.GetAndCreateUserCardImage(ScoreSaberId, "Recentsongs");
                await BeatSaberInfoExtension.GetAndCreateRecentsongsCardImage(ScoreSaberId);                
                await message.Channel.SendFileAsync($"../../../Resources/img/UserCard_{ScoreSaberId}.png");
                await message.Channel.SendFileAsync($"../../../Resources/img/RecentsongsCard_{ScoreSaberId}.png");
                File.Delete($"../../../Resources/img/RecentsongsCard_{ScoreSaberId}.png");
                File.Delete($"../../../Resources/img/UserCard_{ScoreSaberId}.png");
            }
            else
            {
                message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("No ScoreSaber linked", "You have not linked your ScoreSaber with discord. Use '!bs link [ScoreSaberId]' to link your account.").Build());
            }
        }

        [Help("Topsongs", "Creates a profile from your linked ScoreSaber with your 5 topsongs as png", "!bs topsongs", HelpAttribute.Catergories.General)]
        public static async Task TopSongs(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (await r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()))
            {
                var ScoreSaberId = await r.GetScoreSaberIdWithDiscordId(message.Author.Id.ToString());
                //Create UserCard
                await BeatSaberInfoExtension.GetAndCreateUserCardImage(ScoreSaberId, "Topsongs");
                await BeatSaberInfoExtension.GetAndCreateTopsongsCardImage(ScoreSaberId);
                await message.Channel.SendFileAsync($"../../../Resources/img/UserCard_{ScoreSaberId}.png");
                await message.Channel.SendFileAsync($"../../../Resources/img/TopsongsCard_{ScoreSaberId}.png");
                File.Delete($"../../../Resources/img/TopsongsCard_{ScoreSaberId}.png");
                File.Delete($"../../../Resources/img/UserCard_{ScoreSaberId}.png");
            }
            else
            {
                message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("No ScoreSaber linked", "You have not linked your ScoreSaber with discord. Use '!bs link [ScoreSaberId]' to link your account.").Build());
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

        [Help("Search", "Shows new ScoreSaber information about a player", "!bs search [DiscordTag or username]", HelpAttribute.Catergories.General)]
        public static async Task NewSearch(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (message.Content.Contains("@"))
            {
                var discordId = message.Content.Substring(11).Replace("<", "").Replace(">", "").Replace("@", "")
                    .Replace("!", "");
                if (await r.CheckIfDiscordIdIsLinked(discordId))
                {
                    var ScoreSaberId = await r.GetScoreSaberIdWithDiscordId(discordId);
                    var embedTask = new List<EmbedBuilder>();
                    try
                    {
                        embedTask = await BeatSaberInfoExtension.GetPlayerSearchInfoEmbed(ScoreSaberId, message);

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
                            "Try linking your discord and ScoreSaber again with like this: https://scoresaber.com/u/76561198033166451").Build());
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
                            "Your discord is not linked yet. Type !bs link [ScoreSaberlink] to link it.").Build());
                }
            }
            else if (await r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()) && message.Content.Count() == 10)
            {
                var ScoreSaberId = await r.GetScoreSaberIdWithDiscordId(message.Author.Id.ToString());
                var embedTask = await BeatSaberInfoExtension.GetPlayerSearchInfoEmbed(ScoreSaberId, message);

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
                            "Your discord is not linked yet. Type !bs link [ScoreSaberlink] to link it.", null,
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