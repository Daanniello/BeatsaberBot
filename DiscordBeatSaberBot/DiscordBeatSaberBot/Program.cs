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
            


            try
            {
                discordSocketClient = new DiscordSocketClient();
                Console.WriteLine("Connecting to Discord...");

                var loginCode = await DatabaseContext.ExecuteSelectQuery("Select * from Settings");
                await discordSocketClient.LoginAsync(TokenType.Bot, loginCode[0][0].ToString());
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
                var playingGame = await DatabaseContext.ExecuteSelectQuery("Select * from Settings");
                await discordSocketClient.SetGameAsync(playingGame[0][1].ToString());
                var updater = new UpdateTimer(discordSocketClient);
                //updater.Start(() => updater.DutchDiscordUserCount(_startTime), 1);
                //updater.Start(() => updater.EventNotification(), 1);
                updater.Start(() => updater.AutomaticUnMute(), 0, 1, 0);
                updater.Start(() => DutchRankFeed.GetScoresaberLiveFeed(discordSocketClient), 0, 0, 20);
          
                //updater.Start(() => new Giphy().PostTrendingGif(discordSocketClient, 627156958880858113, 749742808851808266), 5, 0, 0);
                //updater.Start(() => new ScoresaberRankMapsFeed(discordSocketClient).CheckIfRankedRequestTopChanged(), 0, 30, 0);

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
    }
}