using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBeatSaberBot.Commands;
using DiscordBeatSaberBot.Extensions;

namespace DiscordBeatSaberBot.Handlers
{
    public class MessageReceivedHandler
    {
        public async Task HandleMessage(DiscordSocketClient discordSocketClient, SocketMessage message, Logger _logger, Program program)
        {
            if (message.Author.Username == "BeatSaber Bot") return;

            MessageDelete.DeleteMessageCheck(message, discordSocketClient);

            if (message.Content.Length <= 3)
                return;

            if (message.Content.Substring(0, 3).Contains("!bs"))
            {
                var messageCommand = message.Content.ToLower();

                var typingState = message.Channel.EnterTypingState(new RequestOptions
                {
                    Timeout = GlobalConfiguration.TypingTimeOut,
                });

                Console.WriteLine(message.Content);

                if (messageCommand.Contains(" help"))
                {
                    GenericCommands.Help(discordSocketClient, message);
                }

                if (messageCommand.Contains(" top10"))
                {
                    GlobalScoresaberCommands.Top10(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" rolecolor"))
                {
                    DutchServerCommands.RoleColor(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" newsearch"))
                {
                    GlobalScoresaberCommands.NewSearch(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" topsong"))
                {
                    GlobalScoresaberCommands.TopSong(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" search"))
                {
                    GlobalScoresaberCommands.Search(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" updateroles"))
                {
                    DutchServerCommands.UpdateRoles(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" linkednames"))
                {
                    DutchServerCommands.LinkedNames(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" notlinkednames"))
                {
                    DutchServerCommands.NotLinkedNames(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" newrecentsong"))
                {
                    GlobalScoresaberCommands.NewRecentSong(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" changecolor"))
                {
                    DutchServerCommands.ChangeColor(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" createevent"))
                {
                    DutchServerCommands.CreatEvent(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" playing"))
                {
                    GenericCommands.Playing(discordSocketClient, message);
                }

                else if (messageCommand.Contains(" invite"))
                {
                    GenericCommands.Invite(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" poll"))
                {
                    GenericCommands.Poll(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" playerbase"))
                {
                    GlobalScoresaberCommands.Playerbase(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" compare"))
                {
                    GlobalScoresaberCommands.Comapre(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" requestverification"))
                {
                    DutchServerCommands.RequestVerification(discordSocketClient, message);
                }

                else if (messageCommand.Contains(" country"))
                {
                    GlobalScoresaberCommands.Country(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" recentsong"))
                {
                    GlobalScoresaberCommands.RecentSong(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" typing"))
                {
                    GlobalScoresaberCommands.Typing(discordSocketClient, message);
                    message.DeleteAsync();
                }
                else if (messageCommand.Contains(" ranks"))
                {
                    var builderList = await BeatSaberInfoExtension.GetRanks();
                    foreach (var builder in builderList)
                        await message.Channel.SendMessageAsync("", false, builder.Build());
                }
                else if (messageCommand.Contains(" senddm"))
                {
                    GenericCommands.SendDM(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" approvedutchuser"))
                {
                    var guildUserSender = message.Author as SocketGuildUser;
                    if (guildUserSender.IsDutchAdmin())
                    {
                        var userId = message.Content.Substring(21).Replace("<@!", "").Replace(">", "").Trim();
                        var user = discordSocketClient.GetUser(ulong.Parse(userId));

                        var guildService = new GuildService(discordSocketClient, 505485680344956928);

                        await guildService.AddRole("Koos Rankloos", user);
                        await program.UserJoinedMessage(user);
                    }

                    await message.DeleteAsync();
                }
                else if (messageCommand.Contains(" songs"))
                {
                    DutchServerCommands.IRLevent(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" irlevent create"))
                {
                    DutchServerCommands.IRLevent(discordSocketClient, message);
                }

                else
                {
                    var embedBuilder = EmbedBuilderExtension.NullEmbed("Oops", "There is no command like that, try something else",
                        null, null);
                    await message.Channel.SendMessageAsync(null, false, embedBuilder.Build());
                }

                typingState.Dispose();
            }
        }
    }
}