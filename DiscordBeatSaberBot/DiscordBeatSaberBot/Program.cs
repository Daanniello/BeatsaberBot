using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    public class Program
    {
        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var discordSocketClient = new DiscordSocketClient();
            await log("Logging into Discord");
            await discordSocketClient.LoginAsync(TokenType.Bot, DiscordBotCode.discordBotCode);
            await discordSocketClient.StartAsync();
            await log("Discord Bot is now live");

            //Events
            discordSocketClient.MessageReceived += MessageReceived;

            await Task.Delay(-1);
        }

        private Task log(string message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }

        private async Task<Task> MessageReceived(SocketMessage message)
        {
            if (message.Author.Username == "BeatSaber Bot")
            {
                return Task.CompletedTask;
            }

            try
            {
                if (message.Content.Substring(0, 3).Contains("!bs"))
                {
                    
                    if (message.Content.Contains(" top10"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions { Timeout = Configuration.TypingTimeOut });
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetTop10Players());
                    }
                    else if (message.Content.Contains(" topsong"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions { Timeout = Configuration.TypingTimeOut });
                        if (message.Content.Length == 11)
                        {
                            await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetTopSongList(message.Content));
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetTopSongList(message.Content.Substring(12)));
                        }
                    }
                    else if (message.Content.Contains(" search"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions { Timeout = Configuration.TypingTimeOut });
                        foreach (var embed in await BeatSaberInfoExtension.GetPlayer(message.Content.Substring(11)))
                        {
                            await message.Channel.SendMessageAsync("", false, embed);
                        }
                    }
                    else if (message.Content.Contains(" invite"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions { Timeout = Configuration.TypingTimeOut });
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetInviteLink());
                    }
                    else if (message.Content.Contains(" compare"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions { Timeout = Configuration.TypingTimeOut });
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.PlayerComparer(message.Content.Substring(12)));
                    }
                    else if (message.Content.Contains(" addrole"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions { Timeout = Configuration.TypingTimeOut });
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.AddRole(message));
                    }
                    else if (message.Content.Contains(" country"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions { Timeout = Configuration.TypingTimeOut });
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetTopxCountry(message.Content.Substring(12)));
                    }
                    else if (message.Content.Contains(" pplist"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions { Timeout = Configuration.TypingTimeOut });
                        await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Performance Points Song List", "Google spreadsheet with all ranked songs listed \n https://docs.google.com/spreadsheets/d/1ufWgF2tWS0gD3pIr0_d37EkIcmCrUy1x6hyzPEZDPNc/edit#gid=1775412672",null,null));
                    }
                    else if (message.Content.Contains(" recentsong"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions { Timeout = Configuration.TypingTimeOut });
                        var id = await BeatSaberInfoExtension.GetPlayerId(message.Content.Substring(15));
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetRecentSongWithId(id[0]));
                    }
                    else if (message.Content.Contains(" ranks"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions { Timeout = Configuration.TypingTimeOut });
                        var builderList = await BeatSaberInfoExtension.GetRanks();
                        foreach (var builder in builderList)
                        {
                            await message.Channel.SendMessageAsync("", false, builder);
                        }
                    }
                    else if (message.Content.Contains(" songs"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions { Timeout = Configuration.TypingTimeOut });
                        var builderList = await BeatSaberInfoExtension.GetSongs(message.Content.Substring(10));
                        if (builderList.Count > 6)
                        {
                            await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Search term to wide", "I can not post more as 6 songs. " + "\n Try searching with a more specific word please. \n" + ":rage:", null, null));
                        }
                        else
                        {
                            foreach (var builder in builderList)
                            {
                                await message.Channel.SendMessageAsync("", false, builder);
                            }
                        }
                    }
                    else if (message.Content.Contains(" help"))
                    {
                        await message.Channel.TriggerTypingAsync(new RequestOptions { Timeout = 20 });
                        var helpMessage = "";
                        var builder = new EmbedBuilder();
                        builder.WithTitle("Help");
                        builder.WithDescription("All Commands to be used");
                        foreach (var helpCommand in CommandHelper.Help())
                        {
                            helpMessage += helpCommand + "\n\n";
                        }

                        builder.AddInlineField("Commands", helpMessage);
                        builder.WithColor(Color.Red);

                        await message.Channel.SendMessageAsync("", false, builder);
                        return Task.CompletedTask;
                    }
                    else
                    {
                        EmbedBuilderExtension.NullEmbed("Oops", "There is no command like that, try something else", null, null);
                    }
                    
                }
            }
            catch
            {
                await log(message.ToString());
            }

            await log(message.ToString());
            return Task.CompletedTask;
        }
    }
}