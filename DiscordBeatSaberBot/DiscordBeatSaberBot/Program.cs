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
using DiscordBeatSaberBot.Handlers.RankTrackerHandler;
using DiscordBeatSaberBot.Services;
using System.IO;
using Newtonsoft.Json;

namespace DiscordBeatSaberBot
{
    public class Program
    {
        private MessageReceivedHandler _messageReceivedHandler;

        private ILogger _logger;
        private Dictionary<string, string> _reactionWatcher = ReactionRolesConfig.GetReactionRoles();
        private DateTime _startTime;
        private DiscordSocketClient discordSocketClient;
        private bool _hasBeenInitializedBefore = false;
        public int commandsEachHour = 0;
        public RateLimit rateLimit = new RateLimit(5);
        private SlashCommandHandler _slashCommandHandler;
        private AutomaticCountryRankUpdateHandler _countryUpdateHandler;

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
        private Task DiscordSocketClient_Log(LogMessage arg)
        {
            Console.WriteLine(arg.Message);
            return Task.CompletedTask;
        }

        public async Task MainAsync()
        {
            try
            {
                discordSocketClient = new DiscordSocketClient(new DiscordSocketConfig()
                {
                    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers
                });

                var loginCode = await DatabaseContext.ExecuteSelectQuery("Select * from Settings");
                await discordSocketClient.LoginAsync(TokenType.Bot, loginCode[0][0].ToString());
                await discordSocketClient.StartAsync();

                //Events
                AppDomain.CurrentDomain.UnhandledException += Unhandled_Exception;
                TaskScheduler.UnobservedTaskException += Unhandled_TaskException;
                discordSocketClient.Ready += DiscordSocketClient_Ready; ;
                discordSocketClient.Log += DiscordSocketClient_Log;

                //Adding country update handler
                _countryUpdateHandler = new AutomaticCountryRankUpdateHandler(discordSocketClient);
                _countryUpdateHandler.SubscribeToScoreLiveFeed();

                //activate update handlers 
                var beatSaberCardCollection = new BeatSaberCardCollection(discordSocketClient).CheckFinishedMatches();

                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task DiscordSocketClient_Ready()
        {
            if (_hasBeenInitializedBefore) return;
            _hasBeenInitializedBefore = true;

            //Adding events 
            discordSocketClient.MessageReceived += DiscordSocketClient_MessageReceived; ;
            discordSocketClient.ReactionAdded += DiscordSocketClient_ReactionAdded;
            discordSocketClient.ReactionRemoved += DiscordSocketClient_ReactionRemoved;
            discordSocketClient.UserJoined += DiscordSocketClient_UserJoined;
            discordSocketClient.ButtonExecuted += DiscordSocketClient_ButtonExecuted;

            //Adding CommandHandler
            _slashCommandHandler = new SlashCommandHandler(discordSocketClient);
            _slashCommandHandler.CreateSlashCommands();

            //Adding Feedback handler
            new FeedbackHandler(discordSocketClient, _countryUpdateHandler.CountryList);

            //Adding the messageHandler
            _messageReceivedHandler = new MessageReceivedHandler();

            //Setup up the depencendy injection
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(discordSocketClient);
            serviceCollection.AddSingleton<ILogger, Logger>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            //Getting needed objects for the program class 
            _logger = serviceProvider.GetService<ILogger>();

            //Inserting playing info on the bot
            _startTime = DateTime.Now;
            var playingGame = await DatabaseContext.ExecuteSelectQuery("Select * from Settings");
            await discordSocketClient.SetGameAsync("/Help");

            //Show top 10 servers 
            for(var i = 0; i < 10; i++)
            {
                var guild = discordSocketClient.Guilds.ToList().OrderByDescending(x => x.MemberCount).ToList()[i];
                Console.WriteLine($"{guild.Name} - {guild.MemberCount}");
            }

            //Automatic updates                                            
            StartAllUpdateTimers();

        }

        private void StartAllUpdateTimers()
        {

            var updater = new UpdateTimer(discordSocketClient);
            var cupOfTheDayHandler = new CupOfTheDayHandler(discordSocketClient);

            new BeatSaberCardCollection(discordSocketClient).NotifyUsersOnNewCardPacks();
            updater.UpdateAtTimeOfDay(() => cupOfTheDayHandler.ResetDailyMap(discordSocketClient), "Reset Daily Map Map of the day", 24, 0, 0);
            updater.UpdateAtTimeOfDay(() => BeatSaberCardCollection.GiveAllUsersDailyTrades(), "Give all users Daily trade", 24, 0, 0);
            updater.UpdateAtTimeOfDay(() => DataCollectionService.UpdateData(), "Data Collection Update", 24, 0, 0);
            updater.Start(() => UpdateSilverhazeDiscordRank(), "SilverhazeDiscordRankUpdate", 0, 30, 0);
            updater.Start(() => new RankTrackerHandler(discordSocketClient).CheckForAllRankChanges(), "RankTrackerUpdate", 0, 15, 0); ;
            updater.Start(() => _countryUpdateHandler.UpdateRanks(), "UpdateRolesInCountryDiscords", 0, 5, 0);
            updater.Start(() => updateServersAndUsersCount(), "Discord server and user count", 1, 0, 0);
            updater.Start(() => updateBotStatistics(), "Update bot stats", 1, 0, 0);
            updater.Start(async () =>
            {
                using (var httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync("https://cdn.wes.cloud/beatstar/bssb/v2-all.json");
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var websitePath = GlobalConfiguration.WebsiteRoot + @"DataCollection\AllMapData.json";
                    File.WriteAllText(websitePath, responseBody);                    
                }
            }, "All Map Data Collection Update", 24, 0, 0);

            async Task UpdateSilverhazeDiscordRank()
            {
                var guild = discordSocketClient.GetGuild(627156958880858113);
                var stats = await new ScoresaberAPI("76561198033166451").GetPlayerFull();
                await guild.GetCategoryChannel(780597859527557130).ModifyAsync(x => x.Name = $"Rank: #{stats.playerInfo.rank} | PP: {stats.playerInfo.Pp}");
            }

            Task updateServersAndUsersCount()
            {
                var guild = discordSocketClient.GetGuild(731936395223892028);
                guild.GetTextChannel(770821423668920321).ModifyAsync(x => x.Name = $"server-count: {discordSocketClient.Guilds.Count}");
                var userCount = 0;
                foreach (var g in discordSocketClient.Guilds) userCount += g.MemberCount;
                guild.GetTextChannel(770821486914437120).ModifyAsync(x => x.Name = $"user-count: {userCount}");

                guild.GetTextChannel(821918821076959232).ModifyAsync(x => x.Name = $"Calls-each-hour: {_slashCommandHandler.TotalCommandsUsed}");
                _slashCommandHandler.TotalCommandsUsed = 0;
                return Task.CompletedTask;
            }

            Task updateBotStatistics()
            {
                var userCount = 0;
                foreach (var g in discordSocketClient.Guilds) userCount += g.MemberCount;
                var stats = new Dictionary<string, object>();

                stats.Add("usercount", userCount);
                stats.Add("servercount", discordSocketClient.Guilds.Count);

                var json = JsonConvert.SerializeObject(stats);
                File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/statistics.json", json);

                return Task.CompletedTask;
            }
        }

        private async Task DiscordSocketClient_UserJoined(SocketGuildUser guildUser)
        {
            var guild = discordSocketClient.Guilds.FirstOrDefault(x => x.Id == (ulong)505485680344956928);
            if(guildUser.Guild.Id == guild.Id)
            {
                var addRole = guild.Roles.FirstOrDefault(x => x.Name == "Nieuwkomer");
                await guildUser.AddRoleAsync(addRole);
            }  
        }

        public async Task UserJoinedMessage(IUser user)
        {
            var server = new GuildService(discordSocketClient, 505485680344956928);
            var guildUser = await server.ConvertUserToGuildUser(user);
            await server.UserJoinedMessage(guildUser);
        }

        private Task DiscordSocketClient_ButtonExecuted(SocketMessageComponent arg)
        {
            try
            {
                new ButtonHandler().HandleButton(discordSocketClient, arg);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.Log(Logger.LogCode.error, ex.ToString(), null, "ButtonClickedException");
                return Task.CompletedTask;
            }
        }

        private async Task DiscordSocketClient_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            try
            {
                var socketChannel = (ISocketMessageChannel)await channel.DownloadAsync();
                new ReactionAddedHandler().HandleReaction(discordSocketClient, reaction, socketChannel, _reactionWatcher, this);
            }
            catch (Exception ex)
            {
                _logger.Log(Logger.LogCode.error, ex.ToString(), null, "ReactionAddedException");
            }
        }

        private async Task DiscordSocketClient_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            try
            {
                var socketChannel = (ISocketMessageChannel)await channel.DownloadAsync();
                new ReactionRemovedHandler().HandleReaction(discordSocketClient, reaction, socketChannel, _reactionWatcher, this);
            }
            catch (Exception ex)
            {
                _logger.Log(Logger.LogCode.error, ex.ToString(), null, "ReactionRemovedException");
            }
        }

        private async Task DiscordSocketClient_MessageReceived(SocketMessage message)
        {
            try
            {
                if (!rateLimit.IsUserRateLimited(message.Author.Id))
                {
                    Task.Run(async () =>
                    {
                        var isBotCall = await _messageReceivedHandler.HandleMessage(discordSocketClient, message, this);
                        if (isBotCall)
                        {
                            commandsEachHour++;
                            var rateLimitCount = rateLimit.AddCall(message.Author.Id);
                            if (rateLimitCount > rateLimit.callsBeforeLimit)
                            {
                                message.Channel.SendMessageAsync($"<@!{message.Author.Id}> You are being rate limited from now on. The rate limit is {rateLimit.callsBeforeLimit} calls each minute. No worries, you can use commands soon again.");

                            }
                        }
                    });
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