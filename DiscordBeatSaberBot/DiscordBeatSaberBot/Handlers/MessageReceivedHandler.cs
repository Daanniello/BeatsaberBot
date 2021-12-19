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

            MessageDelete.DeleteMessageCheck(message, discordSocketClient);

            if (message.Content.Length <= 3) return false;

            if (message.Content.Contains("<@!504633036902498314>") && message.Content.ToLower().Contains("help")) await message.Channel.SendMessageAsync("For help with commands, type `!bs help`");

            if (message.Content.Substring(0, 3).Contains("!bs"))
            {                
                var messageCommand = message.Content.ToLower();

                Console.WriteLine(message.Content);
                try
                {
                    if (messageCommand.Contains(" helplistraw"))
                    {
                        HandleTaskException(GenericCommands.HelpListRaw(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" help"))
                    {
                        HandleTaskException(GenericCommands.Help(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" randomcringe"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.RandomCringe(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" randomgif"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.RandomGif(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" joe"))
                    {
                        message.Channel.SendMessageAsync("mama");
                        return true;
                    }
                    else if (messageCommand.Contains(" topsongs"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.TopSongs(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" search"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.SearchUserCommand(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" topsong"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.NewTopSong(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" improve"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Improve(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" updateroles"))
                    {
                        HandleTaskException(DutchServerCommands.UpdateRoles(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" recentsongs"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Recentsongs(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" recentsong"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.NewRecentSong(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" ranktracker"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.RankTracker(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" removebg"))
                    {
                        HandleTaskException(GenericCommands.RemoveBG(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" poll"))
                    {
                        HandleTaskException(GenericCommands.Poll(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" playing"))
                    {
                        HandleTaskException(GenericCommands.Playing(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" draw"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Draw(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" invite"))
                    {
                        HandleTaskException(GenericCommands.Invite(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" tools"))
                    {
                        HandleTaskException(GenericCommands.Tools(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" leaderboard"))
                    {
                        HandleTaskException(GenericCommands.Leaderboard(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" statistics"))
                    {
                        HandleTaskException(GenericCommands.Statistics(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" playlist"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Playlist(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" compare"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Compare(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" qualifiedmaps"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.QualifiedMaps(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" map") || message.Content.StartsWith("!bsr "))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Map(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" settings"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Settings(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" unlink"))
                    {
                        HandleTaskException(DutchServerCommands.UnLinkScoresaberFromDiscord(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" link"))
                    {
                        HandleTaskException(DutchServerCommands.LinkScoresaberWithDiscord(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" adminlink"))
                    {
                        if(message.Author.Id == 138439306774577152)
                        {
                            var content = message.Content.Substring(14);
                            var elements = content.Split(" ");
                            var discordId = elements[0];
                            var scoresaberId = elements[1];
                            var chnl = message.Channel as SocketGuildChannel;

                            var msg = await message.Channel.SendMessageAsync($"Are you sure you want to add this user to the database? DiscordID: {discordId} | ScoresaberID: {scoresaberId}");
                            await msg.AddReactionAsync(Emote.Parse("<:green_check:671412276594475018>"));
                            discordSocketClient.ReactionAdded += DiscordSocketClient_ReactionAdded;
                            async Task DiscordSocketClient_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
                            {                                
                                if (arg3.UserId == 138439306774577152 && arg3.Emote.Name == "green_check")
                                {
                                    discordSocketClient.ReactionAdded -= DiscordSocketClient_ReactionAdded;
                                    await message.Channel.SendMessageAsync("Done");
                                    await new RoleAssignment(discordSocketClient).LinkAccount(discordId, scoresaberId, chnl.Guild.Id);
                                    return;
                                }                             
                                return;
                            }                            
                        }                        
                    }
                    else if (messageCommand.Contains(" profile"))
                    {
                        HandleTaskException(GlobalScoresaberCommands.Profile(discordSocketClient, message), message);
                        return true;
                    }
                    else if (messageCommand.Contains(" eventmanager"))
                    {
                        HandleTaskException(DutchServerCommands.RandomEvent(discordSocketClient, message), message);
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