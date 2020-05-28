using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBeatSaberBot.Extensions;

namespace DiscordBeatSaberBot.Commands
{
    internal class GlobalScoresaberCommands : ICommand
    {
        public static System.IDisposable triggerState = null;

        [Help("Compare", "Compares two player's stats with each other.", HelpAttribute.Catergories.General)]
        public static async Task Compare(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var embedBuilder = await BeatSaberInfoExtension.GetComparedEmbedBuilder(message.Content.Substring(12), message, discordSocketClient);
            await message.Channel.SendMessageAsync("", false, embedBuilder.Build());
        }

        [Help("Songs", "Searches up the song with the name", HelpAttribute.Catergories.General)]
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

        [Help("RecentSong", "Get info from the latest song played", HelpAttribute.Catergories.General)]
        public static async Task NewRecentSong(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()) && message.Content.Count() == 14)
            {
                var scoresaberId = r.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
                
                var embedTask = await BeatSaberInfoExtension.GetNewRecentSongWithScoresaberId(scoresaberId);
                await message.Channel.SendMessageAsync("", false, embedTask.Build());
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
                var id = await BeatSaberInfoExtension.GetPlayerId(username);
                var embedTask = await BeatSaberInfoExtension.GetNewRecentSongWithScoresaberId(id[0]);
                await message.Channel.SendMessageAsync("", false, embedTask.Build());
            }
        }

        [Help("TopSong", "Get info from the latest song played", HelpAttribute.Catergories.General)]
        public static async Task NewTopSong(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()) && message.Content.Count() == 11)
            {
                var scoresaberId = r.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());

                var embedTask = await BeatSaberInfoExtension.GetNewTopSongWithScoresaberId(scoresaberId);
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
                var embedTask = await BeatSaberInfoExtension.GetNewRecentSongWithScoresaberId(id);
                await message.Channel.SendMessageAsync("", false, embedTask.Build());
            }
        }     

        [Help("Typing", "Turn on typing in the channel 'On' or 'Off'", HelpAttribute.Catergories.General)]
        public static async Task Typing(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var option = message.Content.Substring(10);
            if (option.Contains("on"))
            {
                GlobalScoresaberCommands.triggerState = message.Channel.EnterTypingState(new Discord.RequestOptions() { RetryMode = Discord.RetryMode.AlwaysFail, Timeout = 1});
                message.Channel.SendMessageAsync("Let me tell you something...");

            }
            else if (option.Contains("off"))
            {
                if(GlobalScoresaberCommands.triggerState != null)
                {
                    GlobalScoresaberCommands.triggerState.Dispose();

                }
            }
            else
            {
            

            }
        }

        [Help("Search", "Shows new scoresaber information about a player", HelpAttribute.Catergories.General)]
        public static async Task NewSearch(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (message.Content.Contains("@"))
            {
                var discordId = message.Content.Substring(11).Replace("<", "").Replace(">", "").Replace("@", "")
                    .Replace("!", "");
                if (r.CheckIfDiscordIdIsLinked(discordId))
                {
                    var scoresaberId = r.GetScoresaberIdWithDiscordId(discordId);
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
               
                    if(embedTask.Count() == 0)
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
            else if (r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()) && message.Content.Count() == 10)
            {
                var scoresaberId = r.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
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

                foreach (var embed in await BeatSaberInfoExtension.GetPlayer(message.Content.Substring(11)))
                {
                    var completedMessage = await message.Channel.SendMessageAsync("", false, embed.Build());

                    //await completedMessage.AddReactionAsync(new Emoji("⬅"));
                    //await completedMessage.AddReactionAsync(new Emoji("➡"));
                }
            }
        }
    }
}