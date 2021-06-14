using System;
using System.Threading;
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
        public async Task<bool> HandleMessage(DiscordSocketClient discordSocketClient, SocketMessage message, Program program)
        {
            _program = program;
            if (message.Author.Username == "BeatSaber Bot") return false;

            MessageDelete.DeleteMessageCheck(message, discordSocketClient);

            if (message.Content.Length <= 3) return false;

            if (message.Content.Substring(0, 3).Contains("!bs"))
            {                
                var messageCommand = message.Content.ToLower();

                //command debug channel
                var commandDebugEmbed = EmbedBuilderExtension.NullEmbed("Successfull command", $"**User:** {message.Author.Id} \n\n**Used:** {messageCommand}");
                commandDebugEmbed.Color = Color.Green;
                var commandDebugMessage = await discordSocketClient.GetGuild(731936395223892028).GetTextChannel(853921035669340201).SendMessageAsync("", false, commandDebugEmbed.Build());

                var typingState = message.Channel.EnterTypingState(new RequestOptions
                {
                    Timeout = 2000,                  
                });
                typingState.Dispose();


                Console.WriteLine(message.Content);
                try
                {
                    if (messageCommand.Contains(" helplistraw"))
                    {
                        HandleTaskException(GenericCommands.HelpListRaw(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" help"))
                    {
                        HandleTaskException(GenericCommands.Help(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" randomcringe"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.RandomCringe(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" randomgif"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.RandomGif(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" joe"))
                    {
                        message.Channel.SendMessageAsync("mama");
                        return true;
                    }
                    else if (messageCommand.Contains(" topsongs"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.TopSongs(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" search"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.SearchUserCommand(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" topsong"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.NewTopSong(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" improve"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Improve(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" updateroles"))
                    {
                        HandleTaskException(DutchServerCommands.UpdateRoles(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" recentsongs"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Recentsongs(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" recentsong"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.NewRecentSong(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" poll"))
                    {
                        HandleTaskException(GenericCommands.Poll(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" playing"))
                    {
                        HandleTaskException(GenericCommands.Playing(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" draw"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Draw(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" invite"))
                    {
                        HandleTaskException(GenericCommands.Invite(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" statistics"))
                    {
                        HandleTaskException(GenericCommands.Statistics(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" playlist"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Playlist(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" compare"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Compare(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" qualifiedmaps"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.QualifiedMaps(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" map") || message.Content.StartsWith("!bsr "))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Map(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" settings"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Settings(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" unlink"))
                    {
                        HandleTaskException(DutchServerCommands.UnLinkScoresaberFromDiscord(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" link"))
                    {
                        HandleTaskException(DutchServerCommands.LinkScoresaberWithDiscord(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" profile"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Profile(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" number"))
                    {
                        HandleTaskException(GenericCommands.Number(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" songs"))
                    {
                        await message.Channel.SendMessageAsync(null, false, EmbedBuilderExtension.NullEmbed("Ewh..", "This command is outdated. Blame silverhaze to remake it").Build());
                        //GlobalScoresaberCommands.Songs(discordSocketClient, message);
                        return true;
                    }
                    else if (messageCommand.Contains(" irlevent create"))
                    {
                        HandleTaskException(DutchServerCommands.IRLevent(discordSocketClient, message));
                        return true;
                    }
                    else if (messageCommand.Contains(" randomevent create"))
                    {
                        HandleTaskException(DutchServerCommands.RandomEvent(discordSocketClient, message));
                        return true;
                    }
                    else
                    {
                        if (!messageCommand.Contains("!bsr"))
                        {
                            var embedBuilder = EmbedBuilderExtension.NullEmbed("Oops", "There is no command like that, try something else", null, null);
                            await message.Channel.SendMessageAsync(null, false, embedBuilder.Build());
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed($"Error... {ex.Message}", "Command crashed QQ \nI am not feeling well... \nAm I dying? pls help").Build());
                    commandDebugEmbed.Title = "Failed command";
                    commandDebugEmbed.Color = Color.Red;
                    commandDebugMessage.ModifyAsync(x => x.Embed = commandDebugEmbed.Build());
                    await new Logger(discordSocketClient).Log(Logger.LogCode.error, ex.ToString(), message, "CommandException");
                    return false;
                }
                return false;
            }
            return false;
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