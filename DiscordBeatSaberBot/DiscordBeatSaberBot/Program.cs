using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace DiscordBeatSaberBot
{
    public class Program
    {
        private DiscordSocketClient discordSocketClient;
        private List<SavedMessages> messageCache;

        public static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            TimerRunning(new CancellationToken());
            discordSocketClient = new DiscordSocketClient();
            await log("Logging into Discord");
            await discordSocketClient.LoginAsync(TokenType.Bot, DiscordBotCode.discordBotCode);
            await discordSocketClient.StartAsync();
            await log("Discord Bot is now live");

            messageCache = new List<SavedMessages>();

            //Events
            discordSocketClient.MessageReceived += MessageReceived;
            discordSocketClient.ReactionAdded += ReactionAdded;


            await Task.Delay(-1);
        }

        private Task log(string message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }

        private async Task<Task> ReactionAdded(Cacheable<IUserMessage, ulong> cache,
            ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (reaction.Emote.ToString() == "⬅")
                await messageCache.First().message.ModifyAsync(msg =>
                {
                    msg.Content = "";
                    msg.Embed = messageCache.First().Embedbuilders.First().Build();
                });

            if (reaction.Emote.ToString() == "➡") { }


            return Task.CompletedTask;
        }

        private async Task<Task> MessageReceived(SocketMessage message)
        {
            if (message.Author.Username == "BeatSaber Bot") return Task.CompletedTask;

            await MessageDelete.DeleteMessageCheck(message);


            try
            {
                if (message.Content.Substring(0, 3).Contains("!bs"))
                {
                    if (message.Content.Contains(" top10"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = Configuration.TypingTimeOut
                        });
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetTop10Players());
                    }
                    else if (message.Content.Contains(" topsong"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = Configuration.TypingTimeOut
                        });
                        if (message.Content.Length == 11) await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetTopSongList(message.Content));
                        else await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetTopSongList(message.Content.Substring(12)));
                    }
                    else if (message.Content.Contains(" search"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = Configuration.TypingTimeOut
                        });
                        var savedMessage = new SavedMessages(message as SocketUserMessage);

                        foreach (var embed in await BeatSaberInfoExtension.GetPlayer(message.Content.Substring(11)))
                        {
                            var completedMessage = await message.Channel.SendMessageAsync("", false, embed);
                            savedMessage.AddEmbed(embed);
                            //await completedMessage.AddReactionAsync(new Emoji("⬅"));
                            //await completedMessage.AddReactionAsync(new Emoji("➡"));
                        }

                        messageCache.Add(savedMessage);
                    }
                    else if (message.Content.Contains(" invite"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = Configuration.TypingTimeOut
                        });
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetInviteLink());
                    }
                    else if (message.Content.Contains(" compare"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = Configuration.TypingTimeOut
                        });
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.PlayerComparer(message.Content.Substring(12)));
                    }
                    else if (message.Content.Contains(" addrole"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = Configuration.TypingTimeOut
                        });
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.AddRole(message));
                    }
                    else if (message.Content.Contains(" country"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = Configuration.TypingTimeOut
                        });
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetTopxCountry(message.Content.Substring(12)));
                    }
                    else if (message.Content.Contains(" mod"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = Configuration.TypingTimeOut
                        });
                        await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Mods Installer", "https://github.com/Umbranoxio/BeatSaberModInstaller/releases/download/2.1.6/BeatSaberModManager.exe", null, null));
                    }
                    else if (message.Content.Contains(" addRankFeed"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = Configuration.TypingTimeOut
                        });
                        var filePath = "RankFeedPlayers.txt";
                        var _data = new Dictionary<ulong, string[]>();

                        using (var r = new StreamReader(filePath))
                        {
                            var json = r.ReadToEnd();
                            _data = JsonConvert.DeserializeObject<Dictionary<ulong, string[]>>(json);
                        }

                        var player = message.Content.Substring(16);
                        var playerObject = new List<Player>();
                        try { playerObject = await BeatSaberInfoExtension.GetPlayerInfo(player); }
                        catch (Exception ex)
                        {
                            await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Error", ex.ToString(), null, null));
                            return Task.CompletedTask;
                        }

                        if (playerObject.Count == 0)
                        {
                            await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("No Player found with the name " + player, "Try again with a different name", null, null));
                            return Task.CompletedTask;
                        }

                        var firstItem = false;
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
                            await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Already contains this Discord user", "Sorry, you can only follow one beat saber player on the moment attached to your discord", null, null));
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
                            await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Error", "Could not add player into the RankList. Player: " + player + " DiscordId: " + message.Author.Id + " Rank: " + playerObject.First().rank, null, null));
                            return Task.CompletedTask;
                        }

                        using (var file = File.CreateText(filePath))
                        {
                            var serializer = new JsonSerializer();
                            serializer.Serialize(file, _data);
                        }

                        await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Successfully added to the Ranking Feed", "You will now be updated in DM when you lose or gain too many ranks, based on your beat saber rank.", null, null));
                    }
                    else if (message.Content.Contains(" addFeed"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = Configuration.TypingTimeOut
                        });

                        var _data = new List<ulong[]>();

                        using (var r = new StreamReader("FeedChannels.txt"))
                        {
                            var json = r.ReadToEnd();
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

                        var isDouble = false;
                        foreach (var ids in _data)
                            if (ids.Contains(channel.Id) && ids.Contains(channel.Guild.Id))
                                isDouble = true;

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

                            await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Beat Saber Feed", "Successfully added the beat saber feed to this channel, I will post new beat saber updates directly into this channel.", null, null));
                        }
                        else { await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Beat Saber Feed", "Unsuccessfully added the beat saber feed to this channel, This channel is already in the list", null, null)); }
                    }
                    else if (message.Content.Contains(" pplist"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = Configuration.TypingTimeOut
                        });
                        await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Performance Points Song List", "Google spreadsheet with all ranked songs listed \n https://docs.google.com/spreadsheets/d/1ufWgF2tWS0gD3pIr0_d37EkIcmCrUy1x6hyzPEZDPNc/edit#gid=1775412672", null, null));
                    }
                    else if (message.Content.Contains(" recentsong"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = Configuration.TypingTimeOut
                        });
                        var id = await BeatSaberInfoExtension.GetPlayerId(message.Content.Substring(15));
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetRecentSongWithId(id[0]));
                    }
                    else if (message.Content.Contains(" ranks"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = Configuration.TypingTimeOut
                        });
                        var builderList = await BeatSaberInfoExtension.GetRanks();
                        foreach (var builder in builderList) await message.Channel.SendMessageAsync("", false, builder);
                    }
                    else if (message.Content.Contains(" songs"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = Configuration.TypingTimeOut
                        });
                        var builderList = await BeatSaberInfoExtension.GetSongs(message.Content.Substring(10));
                        if (builderList.Count > 6) await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Search term to wide", "I can not post more as 6 songs. " + "\n Try searching with a more specific word please. \n" + ":rage:", null, null));
                        else
                            foreach (var builder in builderList)
                                await message.Channel.SendMessageAsync("", false, builder);
                    }
                    else if (message.Content.Contains(" help"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions
                        {
                            Timeout = 20
                        });
                        var helpMessage = "";
                        var builder = new EmbedBuilder();
                        builder.WithTitle("Help");
                        builder.WithDescription("All Commands to be used");
                        foreach (var helpCommand in CommandHelper.Help()) helpMessage += helpCommand + "\n\n";

                        builder.AddInlineField("Commands", helpMessage);
                        builder.WithColor(Color.Red);

                        await message.Channel.SendMessageAsync("", false, builder);
                        return Task.CompletedTask;
                    }
                    else { EmbedBuilderExtension.NullEmbed("Oops", "There is no command like that, try something else", null, null); }
                }
            }
            catch
            {
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Error", "Something went wrong, try again later", null, null));
                await log(message.ToString());
            }

            await log(message.ToString());
            return Task.CompletedTask;
        }

        private async void TimerRunning(CancellationToken token)
        {
            //var startTime = DateTime.Now;
            var watch = Stopwatch.StartNew();
            while (!token.IsCancellationRequested)
                try
                {
                    Console.WriteLine("News Feed Updated");
                    await Task.Delay(150000 - (int) (watch.ElapsedMilliseconds % 1000), token);
                    //await Feed.UpdateCheck(discordSocketClient);
                    await Feed.RanksInfoFeed(discordSocketClient);
                    await DutchRankFeed.DutchRankingFeed(discordSocketClient);
                }
                catch (TaskCanceledException) { }
        }
    }
}