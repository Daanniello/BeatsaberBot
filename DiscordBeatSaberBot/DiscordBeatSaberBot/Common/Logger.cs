using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using static DiscordBeatSaberBot.Logger;

namespace DiscordBeatSaberBot
{
    interface ILogger
    {
        Task Log(LogCode logCode, string logMessage, SocketMessage discordSocketMessage = null);
        void ConsoleLog(string message);
    }

    public class Logger : ILogger
    {
        public enum LogCode
        {
            fatal_error,
            error,
            warning,
            debug,
            message
        }

        private readonly DiscordSocketClient _discord;   

        public Logger(DiscordSocketClient discordSocketClient)
        {
            _discord = discordSocketClient;
        }

        public async Task Log(LogCode code, string message, SocketMessage messageInfo = null)
        {
            if (message.Length > 2000) return;
            var color = Color.Green;
            switch (code)
            {
                case LogCode.error:
                    color = Color.Red;
                    break;

                case LogCode.warning:
                    color = Color.Orange;
                    break;

                case LogCode.debug:
                    color = Color.Blue;
                    break;
                case LogCode.message:
                    color = Color.Green;
                    break;
                case LogCode.fatal_error:
                    color = Color.DarkRed;
                    message = "<@138439306774577152> \n" + message;
                    break;
            }

            var embedbuilder = new EmbedBuilder
            {
                Title = code.ToString(),
                Description = message,
                Color = color,
                Footer = new EmbedFooterBuilder()
                {
                    Text = DateTime.Now.ToString()
                }
            };

            //Console.WriteLine(embedbuilder.Title + "\n\n" + embedbuilder.Description);
            try
            {
                await _discord.GetGuild(731936395223892028).GetTextChannel(770821971679248394).SendMessageAsync("", false, embedbuilder.Build());
                //await _discord.GetGuild(677437721081413633).GetTextChannel(682874265594363916).SendMessageAsync("", false, embedbuilder.Build());
            }
            catch
            {
                Console.WriteLine("No connection to discord");
            }
        }

        public void ConsoleLog(string message)
        {
            Console.WriteLine(message);
        }
    }
}