using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Color = Discord.Color;

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
            Console.WriteLine("Unhandled_Exception\n\n" + e.ExceptionObject.message);
            //Environment.Exit(Environment.ExitCode);
            //new Program().MainAsync().GetAwaiter().GetResult();
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

            await log("Connecting to discord...");

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

                _reactionWatcher = new Dictionary<string, string>();
                _reactionWatcher.Add("<:windows:553375150138195968>", "WMR");
                _reactionWatcher.Add("❕", "NSFW");
                _reactionWatcher.Add("<:AYAYA:509158069809315850>", "Anime");
                _reactionWatcher.Add("<:AWEE:588758943686328330>", "Anime"); //Silverhaze's server
                _reactionWatcher.Add("<:minecraft:600768239261319217>", "Minecraft");
                _reactionWatcher.Add("<:osu:578679882553491493>", "Osu!");
                _reactionWatcher.Add("<:vrchat:537413837100548115>", "VRChat");
                _reactionWatcher.Add("<:pavlov:593542245022695453>", "Pavlov");
                _reactionWatcher.Add("🗺", "Mapper");
                _reactionWatcher.Add("💻", "Modder");
                _reactionWatcher.Add("<:terebilo:508313942297280518>", "Normale-Grip");
                _reactionWatcher.Add("<:miitchelW:557923575944970241>", "Botchal-Grip");
                _reactionWatcher.Add("🆕", "Overige-Grip");
                _reactionWatcher.Add("🇹", "Tracker Sabers");
                _reactionWatcher.Add("🇵", "Palm-Grip");
                _reactionWatcher.Add("🇨", "Claw-Grip");
                _reactionWatcher.Add("🇫", "F-Grip");
                _reactionWatcher.Add("🇷", "R-Grip");
                _reactionWatcher.Add("🇰", "K-Grip");
                _reactionWatcher.Add("🇻", "V-Grip");
                _reactionWatcher.Add("🇧", "B-Grip");
                _reactionWatcher.Add("🇽", "X-Grip");
                _reactionWatcher.Add("<:oculus:537368385206616075>", "Oculus");
                _reactionWatcher.Add("<:vive:537368500277084172>", "Vive");
                _reactionWatcher.Add("<:indexvr:589754441545154570>", "Index");
                _reactionWatcher.Add("<:pimax:614170153185312789>", "Pimax");
                _reactionWatcher.Add("<:megaotherway:526402963372245012>", "Event");
                _reactionWatcher.Add("<:silverGasm:628988811329929276>", "NSFW"); //Silverhaze s server


                try
                {
                    var RankingFeedThread = new Thread(() => TimerRunning(new CancellationToken()));
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
            _logger.Log(Logger.LogCode.debug, "New user joined: " + guildUser.Username);
            var server = new GuildService(discordSocketClient);
            await server.UserJoinedMessage(guildUser);

            var guild = discordSocketClient.Guilds.FirstOrDefault(x => x.Id == (ulong)505485680344956928);
            var addRole = guild.Roles.FirstOrDefault(x => x.Name == "Nieuwkomer");
            await guildUser.AddRoleAsync(addRole);
        }

        private async Task<Task> ReactionAdded(Cacheable<IUserMessage, ulong> cache,
            ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            try
            {
                if (reaction.MessageId.ToString() == "586248421715738629")
                {
                    var eventDetailChannel = (ISocketMessageChannel)discordSocketClient.GetChannel(572721078359556097);
                    var embededMessage = (IUserMessage)await eventDetailChannel.GetMessageAsync(586248421715738629);

                    var embedInfo = embededMessage.Embeds.First();
                    var guild = discordSocketClient.Guilds.FirstOrDefault(x => x.Id == (ulong)505485680344956928);
                    var user = guild.GetUser(reaction.User.Value.Id);

                    var embedBuilder = new EmbedBuilder
                    {
                        Title = embedInfo.Title,
                        Description = embedInfo.Description + "\n" + user.Username,
                        Footer = new EmbedFooterBuilder { Text = embedInfo.Footer.ToString() },
                        Color = embedInfo.Color
                    };

                    await embededMessage.ModifyAsync(msg => msg.Embed = embedBuilder.Build());
                }

                if (channel.Id == 549343990957211658)
                {
                    var guild = discordSocketClient.Guilds.FirstOrDefault(x => x.Id == (ulong)505485680344956928);
                    var user = guild.GetUser(reaction.User.Value.Id);
                    var userRoles = user.Roles;
                    foreach (var role in userRoles)
                    {
                        if (role.Name == "Nieuwkomer")
                        {
                            var addRole = guild.Roles.FirstOrDefault(x => x.Name == "Koos Rankloos");
                            var deleteRole = guild.Roles.FirstOrDefault(x => x.Name == "Nieuwkomer");
                            await user.AddRoleAsync(addRole);
                            await user.RemoveRoleAsync(deleteRole);
                        }
                    }
                }

                if (channel.Id == 549350982081970176)
                {
                    bool authenticationCheck()
                    {
                        var guild = discordSocketClient.Guilds.FirstOrDefault(x => x.Id == (ulong)505485680344956928);
                        var userRoles = guild.GetUser(reaction.User.Value.Id).Roles;
                        foreach (var role in userRoles)
                        {
                            if (role.Name == "Staff")
                                return true;
                        }

                        return false;
                    }

                    if (authenticationCheck())
                    {
                        //{✅}
                        //{⛔}
                        // vinkje = status veranderen en actie uitvoeren om er in te zetten
                        // denied is status veranderen en user mention gebruiken
                        var messageFromReaction = await reaction.Channel.GetMessageAsync(reaction.MessageId);
                        var casted = (IUserMessage)messageFromReaction;
                        var usedEmbed = casted.Embeds.First();
                        if (reaction.Emote.Name == "⛔")
                        {
                            var deniedEmbed = new EmbedBuilder();
                            //need staff check
                            try
                            {
                                deniedEmbed = new EmbedBuilder
                                {
                                    Title = usedEmbed.Title,
                                    Description = usedEmbed.Description.Replace("Waiting for approval", "Denied"),
                                    ThumbnailUrl = usedEmbed.Thumbnail.Value.Url,
                                    Color = Color.Red
                                };
                            }
                            catch
                            {
                                deniedEmbed = new EmbedBuilder
                                {
                                    Title = usedEmbed.Title,
                                    Description = usedEmbed.Description.Replace("Waiting for approval", "Denied"),
                                    Color = Color.Red
                                };
                            }

                            await casted.ModifyAsync(msg => msg.Embed = deniedEmbed.Build());
                            await casted.RemoveAllReactionsAsync();
                        }

                        if (reaction.Emote.Name == "✅")
                        {
                            var digitsOnly = new Regex(@"[^\d]");
                            var oneSpace = new Regex("[ ]{2,}");
                            var IDS = oneSpace.Replace(digitsOnly.Replace(usedEmbed.Description, " "), "-").Split("-");
                            string discordId = IDS[2];
                            string scoresaberId = IDS[1];
                            var approvedEmbed = new EmbedBuilder();
                            try
                            {
                                approvedEmbed = new EmbedBuilder
                                {
                                    Title = usedEmbed.Title,
                                    Description = usedEmbed.Description.Replace("Waiting for approval", "Approved"),
                                    ThumbnailUrl = usedEmbed.Thumbnail.Value.Url,
                                    Color = Color.Green
                                };
                            }
                            catch
                            {
                                approvedEmbed = new EmbedBuilder
                                {
                                    Title = usedEmbed.Title,
                                    Description = usedEmbed.Description.Replace("Waiting for approval", "Approved"),
                                    Color = Color.Green
                                };
                            }


                            bool check = await new RoleAssignment(discordSocketClient).LinkAccount(discordId, scoresaberId);
                            if (check)
                            {
                                await casted.ModifyAsync(msg => msg.Embed = approvedEmbed.Build());
                                await casted.RemoveAllReactionsAsync();
                            }

                            var player = await BeatSaberInfoExtension.GetPlayerInfoWithScoresaberId(scoresaberId);
                            DutchRankFeed.GiveRoleWithRank(player.countryRank, scoresaberId);
                            var m = new GuildService(discordSocketClient, 505485680344956928);
                            await m.AddRole("Verified", m.Guild.GetUser(new RoleAssignment(discordSocketClient).GetDiscordIdWithScoresaberId(scoresaberId)));
                            await m.DeleteRole("Link my discord please", m.Guild.GetUser(new RoleAssignment(discordSocketClient).GetDiscordIdWithScoresaberId(scoresaberId)));
                            await m.DeleteRole("Koos Rankloos", m.Guild.GetUser(new RoleAssignment(discordSocketClient).GetDiscordIdWithScoresaberId(scoresaberId)));
                        }
                    }
                }

                //Add Roles from reactions added to specific channels 
                if (channel.Id == 510227606822584330 || channel.Id == 627292184143724544)
                {
                    var guild = discordSocketClient.GetGuild(505485680344956928);
                    if (channel.Id == 510227606822584330)
                        guild = discordSocketClient.GetGuild(505485680344956928);
                    else if (channel.Id == 627292184143724544)
                        guild = discordSocketClient.GetGuild(627156958880858113);
                    var user = guild.GetUser(reaction.UserId);

                    foreach (var reactionDic in _reactionWatcher)
                    {
                        if (reactionDic.Key == reaction.Emote.ToString())
                        {
                            var role = guild.Roles.FirstOrDefault(x => x.Name == reactionDic.Value);
                            await (user as IGuildUser).AddRoleAsync(role);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(Logger.LogCode.error, ex.ToString());
            }

            return Task.CompletedTask;
        }

        private async Task<Task> ReactionRemoved(Cacheable<IUserMessage, ulong> cache,
            ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            //reaction.MessageId.ToString() == File.ReadAllText("EventMessage.txt") || 
            if (reaction.MessageId.ToString() == "586248421715738629")
            {
                var eventDetailChannel = (ISocketMessageChannel)discordSocketClient.GetChannel(572721078359556097);
                var embededMessage = (IUserMessage)await eventDetailChannel.GetMessageAsync(586248421715738629);

                var embedInfo = embededMessage.Embeds.First();
                var guild = discordSocketClient.Guilds.FirstOrDefault(x => x.Id == (ulong)505485680344956928);
                var user = guild.GetUser(reaction.User.Value.Id);

                var embedBuilder = new EmbedBuilder
                {
                    Title = embedInfo.Title,
                    Description = embedInfo.Description.Replace("\n" + user.Username, "").Trim(),
                    Footer = new EmbedFooterBuilder { Text = embedInfo.Footer.ToString() },
                    Color = embedInfo.Color
                };

                await embededMessage.ModifyAsync(msg => msg.Embed = embedBuilder.Build());
            }

            //Remove Roles from reactions added to specific channels 
            try
            {
                if (channel.Id == 510227606822584330 || channel.Id == 627292184143724544)
                {
                    var guild = discordSocketClient.GetGuild(505485680344956928);
                    if (channel.Id == 510227606822584330)
                        guild = discordSocketClient.GetGuild(505485680344956928);
                    else if (channel.Id == 627292184143724544)
                        guild = discordSocketClient.GetGuild(627156958880858113);

                    var user = guild.GetUser(reaction.UserId);
                    foreach (var reactionDic in _reactionWatcher)
                    {
                        if (reactionDic.Key == reaction.Emote.ToString())
                        {
                            var role = guild.Roles.FirstOrDefault(x => x.Name == reactionDic.Value);
                            await (user as IGuildUser).RemoveRoleAsync(role);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(Logger.LogCode.error, reaction.Emote.Name + "\n" + ex);
            }

            return Task.CompletedTask;
        }

        private async Task<Task> MessageReceived(SocketMessage message)
        {
            try
            {
                //Server s = new Server(discordSocketClient, "");
                //await s.AddVRroleMessage(null, true);
                if (message.Author.Username == "BeatSaber Bot") return Task.CompletedTask;
                if (message.Author.Id != 546850627029041153 && message.Content.Contains("ur an idiot"))
                {
                    var user = await message.Channel.GetUserAsync(138439306774577152);
                    var chn = await user.GetOrCreateDMChannelAsync();
                    await chn.SendMessageAsync(message.Author.Username + " was it!");
                }

                if (message.Author.Id == 546850627029041153 && message.Content.Contains("ur an idiot"))
                    await message.Channel.SendMessageAsync("hehe that is true");

                MessageDelete.DeleteMessageCheck(message, discordSocketClient);
                if (message.Content.Length <= 3)
                    return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.Log(Logger.LogCode.error, ex.ToString());
            }

            try
            {
                if (message.Content.Substring(0, 3).Contains("!bs"))
                {
                    Console.WriteLine(message.Content);
                    if (message.Content.Contains(" help"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = 20
                        });
                        var help = new CommandHelper(discordSocketClient);

                        if (message.Content.Length == 8)
                        {
                            await message.Channel.SendMessageAsync("", false, help.GetHelpList());
                        }
                        else
                        {
                            string parameter = message.Content.Substring(9);
                            await message.Channel.SendMessageAsync("", false, help.GetHelpObjectEmbed(parameter));
                        }


                        return Task.CompletedTask;
                    }

                    if (message.Content.Contains(" top10"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        var embedTask = await BeatSaberInfoExtension.GetTop10Players();
                        await message.Channel.SendMessageAsync("", false, embedTask.Build());
                    }
                    else if (message.Content.Contains(" rolecolor"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        var moderationHelper = new GuildService(discordSocketClient, 505485680344956928);
                        if (moderationHelper.UserHasRole(message.Author, "Staff") || moderationHelper.UserHasRole(message.Author, "Verslaafd"))
                        {
                            var roles = discordSocketClient.GetGuild(505485680344956928).Roles;
                            var role = roles.FirstOrDefault(r => r.Name == "Verslaafd");
                            try
                            {
                                string hexcode = message.Content.Split(" ")[2].Trim();
                                var colorConverter = new ColorConverter();
                                dynamic color = colorConverter.ConvertFromString(hexcode);
                                await role.ModifyAsync(r => r.Color = new Color(color.R, color.G, color.B));
                                await message.Channel.SendMessageAsync("Color has been changed");
                            }
                            catch
                            {
                                await message.Channel.SendMessageAsync("Error: Color is not correct");
                            }
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync("You do not have permission");
                        }
                    }
                    else if (message.Content.Contains(" topsong"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        var r = new RoleAssignment(discordSocketClient);
                        if (r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()) && message.Content.Trim().Count() == 11)
                        {
                            string scoresaberId = r.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
                            var embedTask = await BeatSaberInfoExtension.GetBestSongWithId(scoresaberId);
                            await message.Channel.SendMessageAsync("", false, embedTask.Build());
                        }
                        else
                        {
                            var embedTask = await BeatSaberInfoExtension.GetTopSongList(message.Content.Substring(12));
                            await message.Channel.SendMessageAsync("", false, embedTask.Build());
                        }
                    }
                    else if (message.Content.Contains(" search"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        var r = new RoleAssignment(discordSocketClient);
                        if (message.Content.Contains("@"))
                        {
                            string discordId = message.Content.Substring(11).Replace("<", "").Replace(">", "").Replace("@", "").Replace("!", "");
                            if (r.CheckIfDiscordIdIsLinked(discordId))
                            {
                                string scoresaberId = r.GetScoresaberIdWithDiscordId(discordId);
                                var embedTask = await BeatSaberInfoExtension.SearchLinkedPlayer(scoresaberId);
                                await message.Channel.SendMessageAsync("", false, embedTask.Build());
                            }
                            else
                            {
                                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("User's discord not linked", "Your discord is not linked yet. Type !bs verification [Scoresaberlink] to link it.", null, null).Build());

                            }
                        }
                        else if (r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()) && message.Content.Count() == 10)
                        {
                            string scoresaberId = r.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
                            var embedTask = await BeatSaberInfoExtension.SearchLinkedPlayer(scoresaberId);
                            await message.Channel.SendMessageAsync("", false, embedTask.Build());
                        }
                        else
                        {
                            if (message.Content.Substring(10).Count() == 0)
                            {
                                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("User's discord not linked", "Your discord is not linked yet. Type !bs verification [Scoresaberlink] to link it.",null,null).Build());
                                return Task.CompletedTask;
                            }
                            foreach (var embed in await BeatSaberInfoExtension.GetPlayer(message.Content.Substring(11)))
                            {
                                var completedMessage = await message.Channel.SendMessageAsync("", false, embed.Build());

                                //await completedMessage.AddReactionAsync(new Emoji("⬅"));
                                //await completedMessage.AddReactionAsync(new Emoji("➡"));
                            }
                        }
                    }
                    else if (message.Content.Contains(" updateroles"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        await UpdateDiscordBeatsaberRanksNL.UpdateNLAsync(discordSocketClient);
                        await message.Channel.SendMessageAsync("Done");
                    }
                    else if (message.Content.Contains(" linkednames"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        if (new GuildService(discordSocketClient, 505485680344956928).IsStaffInGuild(message.Author.Id, 505486321595187220))
                        {
                            string embed = new RoleAssignment(discordSocketClient).GetLinkedDiscordNamesEmbed();
                            await message.Channel.SendMessageAsync(embed);
                        }
                    }
                    else if (message.Content.Contains(" notlinkednames"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        if (new GuildService(discordSocketClient, 505485680344956928).IsStaffInGuild(message.Author.Id, 505486321595187220))
                        {
                            string embed = new RoleAssignment(discordSocketClient).GetNotLinkedDiscordNamesInGuildEmbed(505485680344956928);
                            await message.Channel.SendMessageAsync(embed);
                        }
                    }
                    else if (message.Content.Contains(" changecolor"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        if (true)
                            await message.Channel.SendMessageAsync("", false, new EmbedBuilder
                            {
                                Title = "Je bent niet gemachtigt om je kleur aan te passen.",
                                Description = "Win het weekelijkse 'meeste uren event' om deze functie te krijgen"
                            }.Build());
                    }
                    else if (message.Content.Contains(" createevent"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        var thread = new Thread(() => new EventManager(message, discordSocketClient));
                        thread.Start();
                    }
                    else if (message.Content.Contains(" playing"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        string msg = message.Content.Substring(12);
                        //C:\Users\Daan\Documents\GitHub\BeatsaberBot\DiscordBeatSaberBot\DiscordBeatSaberBot\BeatSaberSettings.txt
                        JsonExtension.InsertJsonData(@"../../../BeatSaberSettings.txt", "gamePlaying", msg);
                        await discordSocketClient.SetGameAsync(msg);
                        await message.Channel.SendMessageAsync("Game now set to " + msg);
                    }

                    else if (message.Content.Contains(" invite"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        var embedTask = await BeatSaberInfoExtension.GetInviteLink();
                        await message.Channel.SendMessageAsync("", false, embedTask.Build());
                    }
                    else if (message.Content.Contains(" poll"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        var poll = new QuickPoll(message);
                        await poll.CreatePoll();
                    }
                    else if (message.Content.Contains(" playerbase"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        await message.Channel.SendMessageAsync("", false, await Rank.GetPlayerbaseCount(message));
                    }
                    else if (message.Content.Contains(" compare"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        var embedTask = await BeatSaberInfoExtension.PlayerComparer(message.Content.Substring(12));
                        await message.Channel.SendMessageAsync("", false, embedTask.Build());
                    }
                    else if (message.Content.Contains(" resetdutchhours"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        DutchHourCounter.InsertAndResetAllDutchMembers(discordSocketClient);
                        await message.Channel.SendMessageAsync("Reset completed");
                    }
                    else if (message.Content.Contains(" requestverification"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });

                        var r = new RoleAssignment(discordSocketClient);

                        if (r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()))
                        {
                            await message.Channel.SendMessageAsync("Your Discord ID is already linked with your scoresaber, No worries " + message.Author.Username);
                            return Task.CompletedTask;
                        }
                        else
                        {
                            string ScoresaberId = message.Content.Substring(24);
                            ScoresaberId = Regex.Replace(ScoresaberId, "[^0-9]", "");

                            if (!ValidationExtension.IsDigitsOnly(ScoresaberId))
                            {
                                await message.Channel.SendMessageAsync("Scoresaber ID is wrong");
                                return Task.CompletedTask;
                            }

                            if (await ValidationExtension.IsDutch(ScoresaberId))
                            {
                                r.MakeRequest(message);
                            }
                            else
                            {
                                JsonExtension.InsertJsonData("../../../GlobalAccounts.txt", message.Author.Id.ToString(), ScoresaberId);
                                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Added user to the list", "Added " + message.Author.Id.ToString() + " with scoresaberID " + ScoresaberId + " to the global list", null, null).Build());
                            }
                        }
                                          
                        var moderationHelper = new GuildService(discordSocketClient, 505485680344956928);

                        var user = message.Author;
                        if (moderationHelper.UserHasRole(user, "Nieuwkomer"))
                        {
                            await moderationHelper.AddRole("Link my discord please", user);
                            await moderationHelper.DeleteRole("Nieuwkomer", user);
                        }


                    }

                    else if (message.Content.Contains(" country"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        var embedTask = await BeatSaberInfoExtension.GetTopxCountry(message.Content.Substring(12));
                        await message.Channel.SendMessageAsync("", false, embedTask.Build());
                    }
                    else if (message.Content.Contains(" mod"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Mods Installer", "https://github.com/Umbranoxio/BeatSaberModInstaller/releases/download/2.1.6/BeatSaberModManager.exe", null, null).Build());
                    }
                    else if (message.Content.Contains(" topdutchhours"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        await message.Channel.SendMessageAsync("", false, DutchHourCounter.GetTop25BeatSaberHours());
                    }
                    else if (message.Content.Contains(" addRankFeed"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        string filePath = "RankFeedPlayers.txt";
                        var _data = new Dictionary<ulong, string[]>();

                        using (var r = new StreamReader(filePath))
                        {
                            string json = r.ReadToEnd();
                            _data = JsonConvert.DeserializeObject<Dictionary<ulong, string[]>>(json);
                        }

                        string player = message.Content.Substring(16);
                        var playerObject = new List<Player>();
                        try
                        {
                            playerObject = await BeatSaberInfoExtension.GetPlayerInfo(player);
                        }
                        catch (Exception ex)
                        {
                            await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Error", ex.ToString(), null, null).Build());
                            return Task.CompletedTask;
                        }

                        if (playerObject.Count == 0)
                        {
                            await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("No Player found with the name " + player, "Try again with a different name", null, null).Build());
                            return Task.CompletedTask;
                        }

                        bool firstItem = false;
                        if (_data == null)
                        {
                            _data = new Dictionary<ulong, string[]>();
                            _data.Add(message.Author.Id, new[]
                            {
                                player, playerObject.First().rank.ToString()
                            });
                            using (var file = File.CreateText(filePath))
                            {
                                var serializer = new JsonSerializer();
                                serializer.Serialize(file, _data);
                            }

                            firstItem = true;
                        }

                        if (_data.ContainsKey(message.Author.Id) && !firstItem)
                        {
                            await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Already contains this Discord user", "Sorry, you can only follow one beat saber player on the moment attached to your discord", null, null).Build());
                            return Task.CompletedTask;
                        }

                        try
                        {
                            _data.Add(message.Author.Id, new[]
                            {
                                player, playerObject.First().rank.ToString()
                            });
                        }
                        catch
                        {
                            await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Error", "Could not add player into the RankList. Player: " + player + " DiscordId: " + message.Author.Id + " Rank: " + playerObject.First().rank, null, null).Build());
                            return Task.CompletedTask;
                        }

                        using (var file = File.CreateText(filePath))
                        {
                            var serializer = new JsonSerializer();
                            serializer.Serialize(file, _data);
                        }

                        await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Successfully added to the Ranking Feed", "You will now be updated in DM when you lose or gain too many ranks, based on your beat saber rank.", null, null).Build());
                    }
                    else if (message.Content.Contains(" addFeed"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });

                        var _data = new List<ulong[]>();

                        using (var r = new StreamReader("FeedChannels.txt"))
                        {
                            string json = r.ReadToEnd();
                            _data = JsonConvert.DeserializeObject<List<ulong[]>>(json);
                        }

                        var channel = message.Channel as SocketGuildChannel;

                        if (_data == null)
                        {
                            _data = new List<ulong[]>();
                            _data.Add(new[]
                            {
                                channel.Id, channel.Guild.Id
                            });
                            using (var file = File.CreateText("FeedChannels.txt"))
                            {
                                var serializer = new JsonSerializer();
                                serializer.Serialize(file, _data);
                            }
                        }

                        bool isDouble = false;
                        foreach (var ids in _data)
                        {
                            if (ids.Contains(channel.Id) && ids.Contains(channel.Guild.Id))
                                isDouble = true;
                        }

                        if (!isDouble)
                        {
                            _data.Add(new[]
                            {
                                channel.Id, channel.Guild.Id
                            });

                            using (var file = File.CreateText("FeedChannels.txt"))
                            {
                                var serializer = new JsonSerializer();
                                serializer.Serialize(file, _data);
                            }

                            await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Beat Saber Feed", "Successfully added the beat saber feed to this channel, I will post new beat saber updates directly into this channel.", null, null).Build());
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Beat Saber Feed", "Unsuccessfully added the beat saber feed to this channel, This channel is already in the list", null, null).Build());
                        }
                    }
                    else if (message.Content.Contains(" pplist"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Performance Points Song List", "Google spreadsheet with all ranked songs listed \n https://docs.google.com/spreadsheets/d/1ufWgF2tWS0gD3pIr0_d37EkIcmCrUy1x6hyzPEZDPNc/edit#gid=1775412672", null, null).Build());
                    }
                    else if (message.Content.Contains(" recentsong"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        var r = new RoleAssignment(discordSocketClient);
                        if (r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()) && message.Content.Count() == 14)
                        {
                            string scoresaberId = r.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
                            var embedTask = await BeatSaberInfoExtension.GetRecentSongWithId(scoresaberId);
                            await message.Channel.SendMessageAsync("", false, embedTask.Build());
                        }
                        else
                        {
                            if (message.Content.Length <= 18)
                            {
                                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Search failed", "Search value is not long enough. it should be larger than 3 characters.", null, null).Build());
                                return Task.CompletedTask;
                            }

                            string username = message.Content.Substring(15);
                            var id = await BeatSaberInfoExtension.GetPlayerId(username);
                            var embedTask = await BeatSaberInfoExtension.GetRecentSongWithId(id[0]);
                            await message.Channel.SendMessageAsync("", false, embedTask.Build());
                        }
                    }
                    else if (message.Content.Contains(" ranks"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        var builderList = await BeatSaberInfoExtension.GetRanks();
                        foreach (var builder in builderList)
                        {
                            await message.Channel.SendMessageAsync("", false, builder.Build());
                        }
                    }
                    else if (message.Content.Contains(" songs"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = GlobalConfiguration.TypingTimeOut
                        });
                        var builderList = await BeatSaberInfoExtension.GetSongs(message.Content.Substring(10));
                        if (builderList.Count > 4) await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Search term to wide", "I can not post more as 6 songs. " + "\n Try searching with a more specific word please. \n" + ":rage:", null, null).Build());
                        else
                            foreach (var builder in builderList)
                            {
                                await message.Channel.SendMessageAsync("", false, builder.Build());
                            }
                    }

                    else
                    {
                        EmbedBuilderExtension.NullEmbed("Oops", "There is no command like that, try something else", null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _logger.Log(Logger.LogCode.error, ex.ToString());
            }


            return Task.CompletedTask;
        }

        private async void TimerRunning(CancellationToken token)
        {
            //var startTime = DateTime.Now;
            var watch = Stopwatch.StartNew();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var belgiumRankFeed = new CountryRankFeed(discordSocketClient, "BE");
                    var gbRankFeed = new CountryRankFeed(discordSocketClient, "GB");
                    var au_nzRankFeed = new CountryRankFeed(discordSocketClient, "AU", "NZ");


                    Console.WriteLine("Updating news feed in 60 sec");
                    await Task.Delay(60000 - (int)(watch.ElapsedMilliseconds % 1000), token);

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

                    await Task.Delay(20000 - (int)(watch.ElapsedMilliseconds % 1000), token);
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

                    await Task.Delay(20000 - (int)(watch.ElapsedMilliseconds % 1000), token);
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