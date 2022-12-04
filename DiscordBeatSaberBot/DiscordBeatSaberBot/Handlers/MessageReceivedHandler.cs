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
            if (message.Author.IsBot) return false;

            if(message.Author.Id != 138439306774577152) MessageDelete.DeleteMessageCheck(message, discordSocketClient);

            if (message.Content.Length <= 3) return false;

            if (message.Content.Contains("<@!504633036902498314>") && message.Content.ToLower().Contains("help")) await message.Channel.SendMessageAsync("For help with commands, type `!bs help`");

            if (message.Content.Substring(0, 3).Contains("!bs"))
            {
                var messageCommand = message.Content.ToLower();

                Console.WriteLine(message.Content);
                try
                {
                    if (messageCommand.Contains(" help"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/help` instead. If slash command don't work. Kick the bot and reinvite it using this invite https://discord.com/api/oauth2/authorize?client_id=504633036902498314&permissions=1402575645776&redirect_uri=http%3A%2F%2Fbeatsaberbot.com%2F&response_type=code&scope=bot%20applications.commands%20messages.read");                        
                        return true;
                    }
                    else if (messageCommand.Contains(" randomgif"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/randomgif` instead");
                    }
                    else if (messageCommand.Contains(" topsongs"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/topsongs` instead");                       
                        return true;
                    }
                    else if (messageCommand.Contains(" search"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/search` instead");                        
                        return true;
                    }
                    else if (messageCommand.Contains(" topsong"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/topsong` instead");                        
                        return true;
                    }
                    else if (messageCommand.Contains(" improve"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/improve` instead");
                        return true;
                    }
                    else if (messageCommand.Contains(" updateroles"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/updateroles` instead");                        
                        return true;
                    }
                    else if (messageCommand.Contains(" recentsongs"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/recentsongs` instead");                        
                        return true;
                    }
                    else if (messageCommand.Contains(" recentsong"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/recentsong` instead");                        
                        return true;
                    }
                    else if (messageCommand.Contains(" ranktracker"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/ranktracker` instead");                        
                        return true;
                    }
                    else if (messageCommand.Contains(" removebg"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/removebg` instead");                        
                        return true;
                    }
                    else if (messageCommand.Contains(" draw"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/draw` instead");                        
                        return true;
                    }
                    else if (messageCommand.Contains(" invite"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/invite` instead");                        
                        return true;
                    }
                    else if (messageCommand.Contains(" tools"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/tools` instead");                       
                        return true;
                    }
                    else if (messageCommand.Contains(" statistics"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/statistics` instead");                        
                        return true;
                    }
                    else if (messageCommand.Contains(" playlist"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Playlist(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" compare"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/compare` instead");                        
                        return true;
                    }
                    else if (messageCommand.Contains(" map") || message.Content.StartsWith("!bsr "))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/map` instead");                        
                        return true;
                    }
                    else if (messageCommand.Contains(" settings"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/settings` instead");                        
                        return true;
                    }
                    else if (messageCommand.Contains(" unlink"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/unlink` instead");                        
                        return true;
                    }
                    else if (messageCommand.Contains(" link"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/link` instead");                        
                        return true;
                    }
                    else if (messageCommand.Contains(" profile"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/profile` instead");
                        return true;
                    }
                    else if (messageCommand.Contains(" eventmanager"))
                    {
                        message.Channel.SendMessageAsync("This command has been switched towards being a 'slash' command. type `/eventmanager` instead");                        
                        return true;
                    }
                    else
                    {

                    }
                }
                catch (Exception ex)
                {
                    await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed($"{ex.Message}", "Command crashed.\nThis could be caused by wrong input statements.\nTry `!bs help [CommandName]` to get more info about using a command").Build());
                    await new Logger(discordSocketClient).Log(Logger.LogCode.error, ex.ToString(), message, "CommandException");
                    return false;
                }
                return false;
            }
            return false;
        }

        public Task HandleTaskException(Task task, SocketMessage message)
        {
            var typingState = message.Channel.EnterTypingState(new RequestOptions
            {
                Timeout = 2000,
            });

            try
            {
                task.Wait();
                typingState.Dispose();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                typingState.Dispose();
                throw new ArgumentException("Error", "failed");
            }
        }
    }
}