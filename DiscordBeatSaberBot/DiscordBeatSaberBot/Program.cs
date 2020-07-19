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

namespace DiscordBeatSaberBot
{
    public class Program
    {
        private ILogger _logger;
        private Dictionary<string, string> _reactionWatcher = ReactionRolesConfig.GetReactionRoles();
        private DateTime _startTime;
        private DiscordSocketClient discordSocketClient;

        public static void Main(string[] args)
        {                  
            AppDomain.CurrentDomain.UnhandledException += Unhandled_Exception;
            try { new Program().MainAsync().GetAwaiter().GetResult(); } catch (Exception ex) { Console.WriteLine(ex); }
        }

        public static async void Unhandled_Exception(object sender, dynamic e)
        {
            Console.WriteLine("Unhandled_Exception\n\n" + e.ExceptionObject.ToString());
        }

        public static void Unhandled_TaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Console.WriteLine("Unhandled_TaskException\n\n" + e);
            e.SetObserved();
        }

        public async Task MainAsync()
        {
            /*var players = DatabaseContext.ExecuteSelectQuery("Select * from Player where CountryCode='NL'");

            foreach (var player in players)
            {
                DatabaseContext.ExecuteInsertQuery($"Insert into PlayerInCountry (DiscordId, GuildId) values ({player[1]}, 505485680344956928)");
            }*/


            try
            {
                discordSocketClient = new DiscordSocketClient();
                Console.WriteLine("Connecting to Discord...");
                Console.WriteLine($"Discord Token used: {DiscordBotCode.discordBotCode}");
                await discordSocketClient.LoginAsync(TokenType.Bot, DiscordBotCode.discordBotCode);
                await discordSocketClient.StartAsync();

                //Events
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
            try
            {               
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
                var settingData = JsonExtension.GetJsonData("../../../BeatSaberSettings.txt");
                await discordSocketClient.SetGameAsync(settingData.GetValueOrDefault("gamePlaying").ToString());
                var updater = new UpdateTimer(discordSocketClient);
                updater.Start(() => updater.DutchDiscordUserCount(_startTime), 1);
                updater.Start(() => updater.EventNotification(), 1);

                _logger.ConsoleLog("initialization completed.");
            }
            catch (Exception ex)
            {
                _logger.Log(Logger.LogCode.error, "Init Exception!! \n" + ex);
            }           
        }

        private async Task OnUserJoined(SocketGuildUser guildUser)
        {
            var guild = discordSocketClient.Guilds.FirstOrDefault(x => x.Id == (ulong) 505485680344956928);
            var addRole = guild.Roles.FirstOrDefault(x => x.Name == "Nieuwkomer");
            await guildUser.AddRoleAsync(addRole);
        }

        public async Task UserJoinedMessage(SocketUser user)
        {
            var server = new GuildService(discordSocketClient, 505485680344956928);
            var guildUser = server.ConvertUserToGuildUser(user);
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
                _logger.Log(Logger.LogCode.error, ex.ToString());
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
                _logger.Log(Logger.LogCode.error, ex.ToString());
            }
        }

        private async Task MessageReceived(SocketMessage message)
        {      
            try
            {
                new MessageReceivedHandler().HandleMessage(discordSocketClient, message, this);
            }
            catch (Exception ex)
            {
                //If a command fails its job. return a message to the user and log the error.
                Console.WriteLine(ex);
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Error", ex.Message, null, null).Build());
                await _logger.Log(Logger.LogCode.error, ex.ToString());  
            }
        }

        private async Task RankFeedTimer(CancellationToken token)
        {
            var watch = Stopwatch.StartNew();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var belgiumRankFeed = new CountryRankFeed(discordSocketClient, "BE");
                    var gbRankFeed = new CountryRankFeed(discordSocketClient, "GB");
                    var au_nzRankFeed = new CountryRankFeed(discordSocketClient, "AU", "NZ");
                    var dkRankFeed = new CountryRankFeed(discordSocketClient, "DK");

                    Console.WriteLine("Updating news feed in 60 sec");
                    await Task.Delay(60000 - (int) (watch.ElapsedMilliseconds % 1000), token);

                    Console.WriteLine("Startin NL feed...");
                    try
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();
                        await DutchRankFeed.DutchRankingFeed(discordSocketClient);
                        stopwatch.Stop();
                        Console.WriteLine("NL Feed updatetime: " + stopwatch.Elapsed);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("News Feed Crashed" + ex + "NL");
                        _logger.Log(Logger.LogCode.error, "NL Feed Crashed \n\n" + ex);
                    }

                    Console.WriteLine("Ending NL feed...");

                    await Task.Delay(20000 - (int)(watch.ElapsedMilliseconds % 1000), token);
                    Console.WriteLine("Startin DK feed...");
                    try
                    {
                        await dkRankFeed.SendFeedInCountryDiscord(511188151968989197, 680415200108871784);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("News Feed Crashed" + ex + "DK");
                    }

                    await Task.Delay(20000 - (int) (watch.ElapsedMilliseconds % 1000), token);
                    Console.WriteLine("Startin AU_NZ feed...");
                    try
                    {
                        await au_nzRankFeed.SendFeedInCountryDiscord(471250128615899136, 550387948294766611);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("News Feed Crashed" + ex + "AU_NZ");
                    }

                    Console.WriteLine("Ending AU_NZ feed...");

                    await Task.Delay(20000 - (int) (watch.ElapsedMilliseconds % 1000), token);
                    Console.WriteLine("Startin GB feed...");
                    try
                    {
                        await gbRankFeed.SendFeedInCountryDiscord(483482746824687616, 535187354122584070);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("News Feed Crashed" + ex + "GB");
                    }

                    Console.WriteLine("Ending GB feed...");

                    await Task.Delay(20000 - (int) (watch.ElapsedMilliseconds % 1000), token);
                    Console.WriteLine("Startin BE feed...");
                    try
                    {
                        await belgiumRankFeed.SendFeedInCountryDiscord(561207570669371402, 634091663526199307);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("News Feed Crashed" + ex + "BE");
                    }

                    Console.WriteLine("Ending BE feed...");


                    Console.WriteLine("News Feed Updated");
                }
                catch (Exception ex)
                {
                    _logger.Log(Logger.LogCode.error, ex.ToString());
                }

                Console.WriteLine("Task token: " + token.IsCancellationRequested);
            }
        }
    }
}