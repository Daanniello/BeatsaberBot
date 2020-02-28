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
        public async Task HandleMessage(DiscordSocketClient discordSocketClient, SocketMessage message, Logger _logger,
            BeatSaberHourCounter DutchHourCounter, Program program, IDisposable typingState = null)
        {
            if (message.Author.Username == "BeatSaber Bot") return;

            MessageDelete.DeleteMessageCheck(message, discordSocketClient);

            if (message.Content.Length <= 3)
                return;

            if (message.Content.Substring(0, 3).Contains("!bs"))
            {
                Console.WriteLine(message.Content);

                if (message.Content.Contains(" help"))
                {
                    GenericCommands.Help(discordSocketClient, message);
                }

                if (message.Content.Contains(" top10"))
                {
                    GlobalScoresaberCommands.Top10(discordSocketClient, message);
                }
                else if (message.Content.Contains(" rolecolor"))
                {
                    DutchServerCommands.RoleColor(discordSocketClient, message);
                }
                else if (message.Content.Contains(" topsong"))
                {
                    GlobalScoresaberCommands.TopSong(discordSocketClient, message);
                }
                else if (message.Content.Contains(" search"))
                {
                    GlobalScoresaberCommands.Search(discordSocketClient, message);
                }
                else if (message.Content.Contains(" updateroles"))
                {
                    DutchServerCommands.UpdateRoles(discordSocketClient, message);
                }
                else if (message.Content.Contains(" linkednames"))
                {
                    DutchServerCommands.LinkedNames(discordSocketClient, message);
                }
                else if (message.Content.Contains(" notlinkednames"))
                {
                    DutchServerCommands.NotLinkedNames(discordSocketClient, message);
                }
                else if (message.Content.Contains(" changecolor"))
                {
                    DutchServerCommands.ChangeColor(discordSocketClient, message);
                }
                else if (message.Content.Contains(" createevent"))
                {
                    DutchServerCommands.CreatEvent(discordSocketClient, message);
                }
                else if (message.Content.Contains(" playing"))
                {
                    GenericCommands.Playing(discordSocketClient, message);
                }

                else if (message.Content.Contains(" invite"))
                {
                    GenericCommands.Invite(discordSocketClient, message);
                }
                else if (message.Content.Contains(" poll"))
                {
                    GenericCommands.Poll(discordSocketClient, message);
                }
                else if (message.Content.Contains(" playerbase"))
                {
                    GlobalScoresaberCommands.Playerbase(discordSocketClient, message);
                }
                else if (message.Content.Contains(" compare"))
                {
                    GlobalScoresaberCommands.Comapre(discordSocketClient, message);
                }
                else if (message.Content.Contains(" resetdutchhours"))
                {
                    DutchServerCommands.ResetDutchHours(discordSocketClient, message, DutchHourCounter);
                }
                else if (message.Content.Contains(" requestverification"))
                {
                    DutchServerCommands.RequestVerification(discordSocketClient, message);
                }

                else if (message.Content.Contains(" country"))
                {
                    GlobalScoresaberCommands.Country(discordSocketClient, message);
                }
                else if (message.Content.Contains(" topdutchhours"))
                {
                    DutchServerCommands.TopDutchHours(discordSocketClient, message, DutchHourCounter);
                }
                else if (message.Content.Contains(" recentsong"))
                {
                    GlobalScoresaberCommands.RecentSong(discordSocketClient, message);
                }
                else if (message.Content.Contains(" ranks"))
                {
                    var builderList = await BeatSaberInfoExtension.GetRanks();
                    foreach (var builder in builderList)
                        await message.Channel.SendMessageAsync("", false, builder.Build());
                }
                else if (message.Content.Contains(" senddm"))
                {
                    GenericCommands.SendDM(discordSocketClient, message);
                }
                else if (message.Content.Contains(" approvedutchuser"))
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
                else if (message.Content.Contains(" songs"))
                {
                    DutchServerCommands.IRLevent(discordSocketClient, message);
                }
                else if (message.Content.Contains(" irlevent create"))
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