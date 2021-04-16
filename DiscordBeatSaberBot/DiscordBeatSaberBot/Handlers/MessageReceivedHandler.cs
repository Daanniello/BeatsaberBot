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
        private Program _program;
        public async Task HandleMessage(DiscordSocketClient discordSocketClient, SocketMessage message, Program program)
        {
            _program = program;
            if (message.Author.Username == "BeatSaber Bot") return;

            MessageDelete.DeleteMessageCheck(message, discordSocketClient);

            if (message.Content.Length <= 3)
                return;

            if (message.Content.Substring(0, 3).Contains("!bs"))
            {
                _program.commandsEachHour++;
                var messageCommand = message.Content.ToLower();

                var typingState = message.Channel.EnterTypingState(new RequestOptions
                {
                    Timeout = GlobalConfiguration.TypingTimeOut,
                });

                Console.WriteLine(message.Content);
                try
                {


                    if (messageCommand.Contains(" helplistraw"))
                    {
                        HandleTaskException(GenericCommands.HelpListRaw(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" help"))
                    {
                        HandleTaskException(GenericCommands.Help(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" randomcringe"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.RandomCringe(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" randomgif"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.RandomGif(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" joe"))
                    {
                        message.Channel.SendMessageAsync("mama");
                    }
                    else if (messageCommand.Contains(" topsongs"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.TopSongs(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" topsong"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.NewTopSong(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" improve"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Improve(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" updateroles"))
                    {
                        HandleTaskException(DutchServerCommands.UpdateRoles(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" recentsongs"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Recentsongs(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" recentsong"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.NewRecentSong(discordSocketClient, message));                      
                    }
                    else if (messageCommand.Contains(" poll"))
                    {
                        HandleTaskException(GenericCommands.Poll(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" playing"))
                    {
                        HandleTaskException(GenericCommands.Playing(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" draw"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Draw(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" invite"))
                    {
                        HandleTaskException(GenericCommands.Invite(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" statistics"))
                    {
                        HandleTaskException(GenericCommands.Statistics(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" playlist"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Playlist(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" compare"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Compare(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" qualifiedmaps"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.QualifiedMaps(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" map"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Map(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" settings"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Settings(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" unlink"))
                    {
                        HandleTaskException(DutchServerCommands.UnLinkScoresaberFromDiscord(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" link"))
                    {
                        HandleTaskException(DutchServerCommands.LinkScoresaberWithDiscord(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" profile"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Profile(discordSocketClient, message));
                    }                    
                    else if (messageCommand.Contains(" number"))
                    {
                        HandleTaskException(GenericCommands.Number(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" songs"))
                    {
                        await message.Channel.SendMessageAsync(null, false, EmbedBuilderExtension.NullEmbed("Ewh..", "This command is outdated. Blame silverhaze to remake it").Build());
                        //GlobalScoresaberCommands.Songs(discordSocketClient, message);
                    }
                    else if (messageCommand.Contains(" irlevent create"))
                    {
                        HandleTaskException(DutchServerCommands.IRLevent(discordSocketClient, message));
                    }
                    else if (messageCommand.Contains(" randomevent create"))
                    {
                        HandleTaskException(DutchServerCommands.RandomEvent(discordSocketClient, message));
                    }
                    else
                    {
                        if (!messageCommand.Contains("!bsr"))
                        {
                            var embedBuilder = EmbedBuilderExtension.NullEmbed("Oops", "There is no command like that, try something else", null, null);
                            await message.Channel.SendMessageAsync(null, false, embedBuilder.Build());
                        }
                    }
                }
                catch (Exception ex)
                {
                    await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed($"Error... {ex.Message}", "Command crashed QQ \nI am not feeling well... \nAm I dying? pls help").Build());
                    await new Logger(discordSocketClient).Log(Logger.LogCode.error, ex.ToString(), message, "CommandException");
                }

                typingState.Dispose();
            }
        }
        public Task HandleTaskException(Task task)
        {
            try
            {
                task.Wait();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error", "failed");
            }
        }
    }
}