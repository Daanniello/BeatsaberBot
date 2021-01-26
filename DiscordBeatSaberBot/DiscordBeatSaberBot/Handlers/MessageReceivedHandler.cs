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
        public async Task HandleMessage(DiscordSocketClient discordSocketClient, SocketMessage message, Program program)
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

                if (messageCommand.Contains(" helplistraw"))
                {
                    GenericCommands.HelpListRaw(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" help"))
                {
                    GenericCommands.Help(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" rolecolor"))
                {
                    DutchServerCommands.RoleColor(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" seal"))
                {
                    GlobalScoreSaberCommands.Seal(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" randomgif"))
                {
                    GlobalScoreSaberCommands.RandomGif(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" topsongs"))
                {
                    GlobalScoreSaberCommands.TopSongs(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" topsong"))
                {
                    GlobalScoreSaberCommands.NewTopSong(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" search"))
                {
                    GlobalScoreSaberCommands.NewSearch(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" improve"))
                {
                    GlobalScoreSaberCommands.Improve(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" updateroles"))
                {
                    DutchServerCommands.UpdateRoles(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" recentsongs"))
                {
                    GlobalScoreSaberCommands.Recentsongs(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" recentsong"))
                {
                    GlobalScoreSaberCommands.NewRecentSong(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" changecolor"))
                {
                    DutchServerCommands.ChangeColor(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" poll"))
                {
                    GenericCommands.Poll(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" playing"))
                {
                    GenericCommands.Playing(discordSocketClient, message);
                }

                else if (messageCommand.Contains(" invite"))
                {
                    GenericCommands.Invite(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" comparetext"))
                {
                    GlobalScoreSaberCommands.Compare(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" compare"))
                {
                    GlobalScoreSaberCommands.CompareNew(discordSocketClient, message);
                }                      
                else if (messageCommand.Contains(" unlink"))
                {
                    DutchServerCommands.UnLinkScoreSaberFromDiscord(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" link"))
                {
                    DutchServerCommands.LinkScoreSaberWithDiscord(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" profile"))
                {
                    GlobalScoreSaberCommands.Profile(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" interviewtest"))
                {
                    new WelcomeInterviewHandler(discordSocketClient, message.Channel, message.Author.Id).AskForInterview();
                }
                else if (messageCommand.Contains(" number"))
                {
                    GenericCommands.Number(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" createachievement"))
                {
                    GenericCommands.CreateAchievementFeed(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" achievement"))
                {
                    GenericCommands.AchievementFeed(discordSocketClient, message);
                }                
                else if (messageCommand.Contains(" typing"))
                {
                    GlobalScoreSaberCommands.Typing(discordSocketClient, message);
                    message.DeleteAsync();
                }
                else if (messageCommand.Contains(" createrankmapfeed"))
                {
                    GlobalScoreSaberCommands.CreateRankMapFeed(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" ranks"))
                {
                    var builderList = await BeatSaberInfoExtension.GetRanks();
                    foreach (var builder in builderList)
                        await message.Channel.SendMessageAsync("", false, builder.Build());
                }
                else if (messageCommand.Contains(" songs"))
                {
                    await message.Channel.SendMessageAsync(null, false, EmbedBuilderExtension.NullEmbed("Ewh..", "This command is outdated. Blame silverhaze to remake it").Build());
                    //GlobalScoreSaberCommands.Songs(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" irlevent create"))
                {
                    DutchServerCommands.IRLevent(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" randomevent create"))
                {
                    DutchServerCommands.RandomEvent(discordSocketClient, message);
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