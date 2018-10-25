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

        private async Task MessageReceived(SocketMessage message)
        {
            try
            {
                if (message.Content.Substring(0, 3).Contains("!bs"))
                {
                    if (message.Content.Contains(" top10 "))
                    {
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetTop10Players());
                    }

                    if (message.Content.Contains(" topsong "))
                    {
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetTopSongList(message.Content));
                    }

                    if (message.Content.Contains(" search "))
                    {
                        await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetPlayer(message.Content.Substring(11)));
                    }

                    if (message.Content.Contains(" songs "))
                    {
                        try
                        {
                            await message.Channel.SendMessageAsync("", false, await BeatSaberInfoExtension.GetSongs(message.Content.Substring(10)));
                        }
                        catch
                        {
                            await message.Channel.SendMessageAsync("Error, Search term is too wide");
                        }

                    }

                    if (message.Content.Contains(" help "))
                    {
                        var helpMessage = "";
                        var builder = new EmbedBuilder();
                        builder.WithTitle("Help");
                        builder.WithDescription("All Commands to be used");
                        foreach (var helpCommand in CommandHelper.Help())
                        {
                            helpMessage += helpCommand + "\n";
                        }
                        builder.AddInlineField("Commands", helpMessage);
                        builder.WithColor(Color.Red);

                        await message.Channel.SendMessageAsync("", false, builder);
                    }
                }
            }
            catch
            {
                await message.Channel.SendMessageAsync("Command is not working at the moment! please try again later");
            }
                await log(message.ToString());
        }      
    }
}
