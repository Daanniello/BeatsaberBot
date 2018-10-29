using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

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
                    await message.Channel.TriggerTypingAsync(new RequestOptions { Timeout = 20 });
                    if (message.Content.Contains(" top10"))
                    {
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetTop10Players());
                    }

                    if (message.Content.Contains(" topsong"))
                    {
                        if (message.Content.Length == 11)
                        {
                            await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetTopSongList(message.Content));
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetTopSongList(message.Content.Substring(12)));
                        }
                    }

                    if (message.Content.Contains(" search"))
                    {
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetPlayer(message.Content.Substring(11)));
                    }

                    if (message.Content.Contains(" invite"))
                    {
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetInviteLink());
                    }

                    if (message.Content.Contains(" addrole"))
                    {
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.AddRole(message));
                    }

                    if (message.Content.Contains(" country"))
                    {
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetTopxCountry(message.Content.Substring(12)));
                    }

                    if (message.Content.Contains(" recentsong"))
                    {
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetRecentSongWithId(await BeatSaberInfoExtension.GetPlayerId(message.Content.Substring(15))));
                    }

                    if (message.Content.Contains(" ranks"))
                    {
                        var builderList = await BeatSaberInfoExtension.GetRanks();
                        foreach(var builder in builderList)
                        {
                            await message.Channel.SendMessageAsync("", false, builder);
                        }
                    }

                    if (message.Content.Contains(" songs"))
                    {
                        var builderList = await BeatSaberInfoExtension.GetSongs(message.Content.Substring(10));
                        if(builderList.Count > 6)
                        {
                            await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Search term to wide", "I can not post more as 6 songs. " +
                                "\n Try searching with a more specific word please. \n" +
                                ":rage:", null,null));
                        }
                        else
                        {
                            foreach (var builder in builderList)
                            {

                                await message.Channel.SendMessageAsync("", false, builder);
                            }
                        }
                    }

                    if (message.Content.Contains(" help"))
                    {
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
