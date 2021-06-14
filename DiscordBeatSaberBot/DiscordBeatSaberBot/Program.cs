using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DiscordBeatSaberBot.Handlers;
using Microsoft.Extensions.DependencyInjection;
using DiscordBeatSaberBot.Config;
using DiscordBeatSaberBot.Api.GiphyApi;
using DiscordBeatSaberBot.Api.Spotify;
using DiscordBeatSaberBot.Api.BeatSaviourApi;
using DiscordBeatSaberBot.Commands.Functions;
using DiscordBeatSaberBot.Security;

namespace DiscordBeatSaberBot
{
    public class Program
    {
        private ILogger _logger;
        private Dictionary<string, string> _reactionWatcher = ReactionRolesConfig.GetReactionRoles();
        private DateTime _startTime;
        private DiscordSocketClient discordSocketClient;
        private bool hasBeenInitialized = false;
        public int commandsEachHour = 0;
        public RateLimit rateLimit = new RateLimit(5);

        public static void Main(string[] args)
        {                  
            try { new Program().MainAsync().GetAwaiter().GetResult(); } catch (Exception ex) { Console.WriteLine(ex); }
        }

        public async void Unhandled_Exception(object sender, dynamic e)
        {
            await _logger.Log(Logger.LogCode.error, e.ExceptionObject.ToString(), null, "Unhandled_Exception");
        }

        public async void Unhandled_TaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            await _logger.Log(Logger.LogCode.error, e.Exception.Message + "\n\n" + e.Exception.InnerException.ToString(), null, "Unhandled_TaskException");
            e.SetObserved();
        }

        public async Task MainAsync()
        {            
            try
            {                
                discordSocketClient = new DiscordSocketClient();
                Console.WriteLine("Connecting to Discord...");

                var loginCode = await DatabaseContext.ExecuteSelectQuery("Select * from Settings");
                await discordSocketClient.LoginAsync(TokenType.Bot, loginCode[0][0].ToString());
                await discordSocketClient.StartAsync();

                //Events
                AppDomain.CurrentDomain.UnhandledException += Unhandled_Exception;
                TaskScheduler.UnobservedTaskException += Unhandled_TaskException;
                discordSocketClient.MessageReceived += MessageReceived;
                discordSocketClient.ReactionAdded += ReactionAdded;
                discordSocketClient.ReactionRemoved += ReactionRemoved;
                discordSocketClient.UserJoined += OnUserJoined;
                discordSocketClient.Ready += Init;
           
                Console.WriteLine("Connecting to Discord...");                
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task Init()
        {
            if (hasBeenInitialized) return;
            try
            {
                hasBeenInitialized = true;
                //Setup up the depencendy injection
                var serviceCollection = new ServiceCollection();
                serviceCollection.AddSingleton(discordSocketClient);
                serviceCollection.AddSingleton<ILogger, Logger>();
                var serviceProvider = serviceCollection.BuildServiceProvider();

                //Getting needed objects for the program class 
                _logger = serviceProvider.GetService<ILogger>();
                _logger.ConsoleLog("Discord Bot is now Connected, starting the initialization...");

                //Setting up info for the bot
                _startTime = DateTime.Now;
                var playingGame = await DatabaseContext.ExecuteSelectQuery("Select * from Settings");
                await discordSocketClient.SetGameAsync(playingGame[0][1].ToString());


                //Automatic updates
                var updater = new UpdateTimer(discordSocketClient);
                var liveFeed = new DiscordScoreSaberLiveFeed(discordSocketClient);
                //updater.Start(() => liveFeed.Start(), "ScoresaberLiveFeed", 0, 0, 15);
                updater.Start(() => updater.UpdateSilverhazeStatsInDiscordServer(), "UpdateSilverInfoInSilverhazeServer", 5, 0, 0);
                updater.Start(() => UpdateSilverhazeDiscordRank(), "SilverhazeDiscordRankUpdate", 0, 30, 0);
                async Task UpdateSilverhazeDiscordRank()
                {
                    var guild = discordSocketClient.GetGuild(627156958880858113);
                    var stats = await new ScoresaberAPI("76561198033166451").GetPlayerFull();
                    await guild.GetCategoryChannel(780597859527557130).ModifyAsync(x => x.Name = $"Rank: #{stats.playerInfo.rank} | PP: {stats.playerInfo.Pp}");
                }
                updater.Start(() => updateServersAndUsersCount(), "Discord server and user count", 1, 0, 0);
                Task updateServersAndUsersCount()
                {
                    var guild = discordSocketClient.GetGuild(731936395223892028);
                    guild.GetTextChannel(770821423668920321).ModifyAsync(x => x.Name = $"server-count: {discordSocketClient.Guilds.Count}");
                    var userCount = 0;
                    foreach (var g in discordSocketClient.Guilds) userCount += g.MemberCount;
                    guild.GetTextChannel(770821486914437120).ModifyAsync(x => x.Name = $"user-count: {userCount}");

                    guild.GetTextChannel(821918821076959232).ModifyAsync(x => x.Name = $"Call-each-hour: {commandsEachHour}");
                    commandsEachHour = 0;
                    return Task.CompletedTask;
                }
                //updater.Start(() => DutchRankFeed.GetScoresaberLiveFeed(discordSocketClient), "ScoresaberLiveFeed", 0, 0, 20);

                _logger.ConsoleLog("initialization completed.");

                

            }
            catch (Exception ex)
            {
                _logger.Log(Logger.LogCode.error, "Init Exception!! \n" + ex, null, "Init_Exception");
            }           
        }

        private async Task OnUserJoined(SocketGuildUser guildUser)
        {
            var guild = discordSocketClient.Guilds.FirstOrDefault(x => x.Id == (ulong) 505485680344956928);
            var addRole = guild.Roles.FirstOrDefault(x => x.Name == "Nieuwkomer");
            await guildUser.AddRoleAsync(addRole);
        }

        public async Task UserJoinedMessage(IUser user)
        {
            var server = new GuildService(discordSocketClient, 505485680344956928);
            var guildUser = await server.ConvertUserToGuildUser(user);
            await server.UserJoinedMessage(guildUser);
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {           
            try
            {
                new ReactionAddedHandler().HandleReaction(discordSocketClient, reaction, channel, _reactionWatcher, this);
            }
            catch (Exception ex)
            {
                _logger.Log(Logger.LogCode.error, ex.ToString(), null, "ReactionAddedException");
            }
        }

        private async Task ReactionRemoved(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {            
            try
            {
                new ReactionRemovedHandler().HandleReaction(discordSocketClient, reaction, channel, _reactionWatcher, this);
            }
            catch (Exception ex)
            {
                _logger.Log(Logger.LogCode.error, ex.ToString(), null, "ReactionRemovedException");
            }
        }

        private async Task MessageReceived(SocketMessage message)
        {
            try
            {
                if (!rateLimit.IsUserRateLimited(message.Author.Id))
                {
                    var isBotCall = await new MessageReceivedHandler().HandleMessage(discordSocketClient, message, this);
                    if (isBotCall)
                    {
                        commandsEachHour++;
                        var rateLimitCount = rateLimit.AddCall(message.Author.Id);
                        if (rateLimitCount > rateLimit.callsBeforeLimit) message.Channel.SendMessageAsync($"<@!{message.Author.Id}> You are being rate limited from now on. The rate limit is {rateLimit.callsBeforeLimit} calls each minute. No worries, you can use commands soon again.");
                    }
                }                
            }
            catch (Exception ex)
            {
                //If a command fails its job. return a message to the user and log the error.
                Console.WriteLine(ex);
                await _logger.Log(Logger.LogCode.error, ex.ToString());  
            }            
        }
    }
}