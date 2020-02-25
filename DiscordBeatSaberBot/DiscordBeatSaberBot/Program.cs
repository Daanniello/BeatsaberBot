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

namespace DiscordBeatSaberBot
{
    public class Program
    {
        private Logger _logger;
        private Dictionary<string, string> _reactionWatcher;
        private DateTime _startTime;
        private DiscordSocketClient discordSocketClient;
        private BeatSaberHourCounter DutchHourCounter;

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += Unhandled_Exception;
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public static void Unhandled_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Console.WriteLine("Unhandled_Exception\n\n" + e);
        }

        public static void Unhandled_HttpException(object sender, HttpRequestException e)
        {
            Console.WriteLine("Unhandled_Exception\n\n" + e);
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
            discordSocketClient = new DiscordSocketClient();
            await log("Connecting to Discord...");
            await discordSocketClient.LoginAsync(TokenType.Bot, DiscordBotCode.discordBotCode);
            await discordSocketClient.StartAsync();

            //Events
            TaskScheduler.UnobservedTaskException += Unhandled_TaskException;
            discordSocketClient.MessageReceived += MessageReceived;
            discordSocketClient.ReactionAdded += ReactionAdded;
            discordSocketClient.ReactionRemoved += ReactionRemoved;
            discordSocketClient.UserJoined += OnUserJoined;
            discordSocketClient.GuildMemberUpdated += OnUserUpdated;
            discordSocketClient.Disconnected += onDisconnected;
            discordSocketClient.Ready += Init;

            await log("Connecting to Discord...");

            await Task.Delay(-1);
        }

        private async Task onDisconnected(Exception e)
        {
            _logger.Log(Logger.LogCode.error, "BOT DISCONNECTED! \n " + e);
        }

        private async Task Init()
        {
            try
            {
                await log("Discord Bot is now Connected.");

                _startTime = DateTime.Now;
                _logger = new Logger(discordSocketClient);
                var settingData = JsonExtension.GetJsonData("../../../BeatSaberSettings.txt");
                await discordSocketClient.SetGameAsync(settingData.GetValueOrDefault("gamePlaying").ToString());
                DutchHourCounter = new BeatSaberHourCounter(discordSocketClient);

                var updater = new UpdateTimer(discordSocketClient);
                updater.Start(() => updater.DutchDiscordUserCount(_startTime), 1);
                updater.Start(() => updater.EventNotification(), 1);
                updater.Start(() => updater.DutchWeeklyEndHoursCheck(), 1);

                _reactionWatcher = new Dictionary<string, string>
                {
                    { "<:windows:553375150138195968>", "WMR" },
                    { "❕", "NSFW" },
                    { "<:AYAYA:509158069809315850>", "Weeb" },
                    { "<:minecraft:600768239261319217>", "Minecraft" },
                    { "<:osu:578679882553491493>", "Osu!" },
                    { "<:vrchat:537413837100548115>", "VRChat" },
                    { "<:pavlov:593542245022695453>", "Pavlov" },
                    { "🗺", "Mapper" },
                    { "💻", "Modder" },
                    { "<:terebilo:508313942297280518>", "Normale-Grip" },
                    { "<:miitchelW:557923575944970241>", "Botchal-Grip" },
                    { "🆕", "Overige-Grip" },
                    { "🇹", "Tracker Sabers" },
                    { "🇵", "Palm-Grip" },
                    { "🇨", "Claw-Grip" },
                    { "🇫", "F-Grip" },
                    { "🇷", "R-Grip" },
                    { "🇰", "K-Grip" },
                    { "🇻", "V-Grip" },
                    { "🇧", "B-Grip" },
                    { "🇽", "X-Grip" },
                    { "<:oculus:537368385206616075>", "Oculus" },
                    { "<:vive:537368500277084172>", "Vive" },
                    { "<:indexvr:589754441545154570>", "Index" },
                    { "<:pimax:614170153185312789>", "Pimax" },
                    { "<:megaotherway:526402963372245012>", "Event" },
                    { "<:skyToxicc:479251361028898828>", "Toxic" },
                    { "<:BeatSaberTime:633400410400489487>", "Beat saber multiplayer" },
                    { "🎮", "Andere games multiplayer" },
                    { "<:owopeak:401481118165237760>", "IRL event" },

                    //Provincies
                    { "<:peepoGroningen:600782037325971458>", "Groningen" },
                    { "<:peepoFriesland:600782036923449344>", "Friesland" },
                    { "<:peepoDrenthe:600782037556920372>", "Drenthe" },
                    { "<:peepoOverijssel:600782037649063947>", "Overijssel" },
                    { "<:peepoFlevoland:600782037812772870>", "Flevoland" },
                    { "<:peepoGelderland:600782037590474780>", "Gelderland" },
                    { "<:peepoUtrecht:600782037472903169>", "Utrecht" },
                    { "<:peepoNoordHolland:600782038035070986>", "Noord-Holland" },
                    { "<:peepoZuidHolland:600782037682749448>", "Zuid-Holland" },
                    { "<:peepoZeeland:600782037049409547>", "Zeeland" },
                    { "<:peepoBrabant:600782036642430986>", "Noord-Brabant" },
                    { "<:peepoLimburg:600782036919123968>", "Limburg" },

                    //Silverhaze's server
                    { "<:AWEE:588758943686328330>", "Anime" },
                    { "<:silverGasm:628988811329929276>", "NSFW" }
                };


                try
                {
                    var RankingFeedThread = new Thread(() => RankFeedTimer(new CancellationToken()));
                    RankingFeedThread.Start();
                    //TimerRunning(new CancellationToken());
                }
                catch
                {
                    _logger.Log(Logger.LogCode.error, "Thread exeption");
                }
            }
            catch (Exception ex)
            {
                _logger.Log(Logger.LogCode.error, "Init Exception!! \n" + ex);
            }
        }

        private Task log(string message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }

        private async Task OnUserUpdated(SocketGuildUser userOld, SocketGuildUser userNew)
        {
            try
            {
                if (discordSocketClient.GetGuild(505485680344956928).Users.Contains(userNew))
                    if (DutchHourCounter != null)
                        DutchHourCounter.TurnOnCounterForPlayer(userOld, userNew);
            }
            catch (Exception ex)
            {
                _logger.Log(Logger.LogCode.error, ex.ToString());
            }
        }

        private async Task OnUserJoined(SocketGuildUser guildUser)
        {
            //UserJoinedMessage(guildUser);
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

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> cache,
            ISocketMessageChannel channel,
            SocketReaction reaction)
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

        private async Task ReactionRemoved(Cacheable<IUserMessage, ulong> cache,
            ISocketMessageChannel channel,
            SocketReaction reaction)
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
                new MessageReceivedHandler().HandleMessage(discordSocketClient, message, _logger, DutchHourCounter, this);
            }
            catch (Exception ex)
            {
                //If a command fails its job. return a message to the user and log the error.
                Console.WriteLine(ex);
                await message.Channel.SendMessageAsync("", false,
                    EmbedBuilderExtension.NullEmbed("Error", ex.Message, null, null).Build());
                _logger.Log(Logger.LogCode.error, ex.ToString());
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