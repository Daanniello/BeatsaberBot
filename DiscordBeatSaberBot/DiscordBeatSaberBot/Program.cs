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
        private bool hasBeenInitialized = false; 

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
            if (hasBeenInitialized) return;
            try
            {
                hasBeenInitialized = true;
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


                //Automatic updates
                var updater = new UpdateTimer(discordSocketClient);
                var liveFeed = new DiscordScoreSaberLiveFeed(discordSocketClient);
                updater.Start(() => liveFeed.Start(), "ScoresaberLiveFeed", 0, 0, 15);
                updater.Start(() => updateServersAndUsersCount(), "Discord server and user count", 5, 0, 0);
                Task updateServersAndUsersCount()
                {
                    var guild = discordSocketClient.GetGuild(731936395223892028);
                    guild.GetTextChannel(770821423668920321).ModifyAsync(x => x.Name = $"server-count: {discordSocketClient.Guilds.Count}");
                    var userCount = 0;
                    foreach (var g in discordSocketClient.Guilds) userCount += g.Users.Count;
                    guild.GetTextChannel(770821486914437120).ModifyAsync(x => x.Name = $"user-count: {userCount}");
                    return Task.CompletedTask;
                }
                //updater.Start(() => DutchRankFeed.GetScoresaberLiveFeed(discordSocketClient), "ScoresaberLiveFeed", 0, 0, 20);


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