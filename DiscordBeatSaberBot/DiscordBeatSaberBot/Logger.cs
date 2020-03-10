using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    public class Logger
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

        public Logger(DiscordSocketClient discord)
        {
            _discord = discord;
        }

        public async Task Log(LogCode code, string message)
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

            Console.WriteLine(embedbuilder.Title + "\n\n" + embedbuilder.Description);
            try
            {
                await _discord.GetGuild(505485680344956928).GetTextChannel(592877335355850752).SendMessageAsync("", false, embedbuilder.Build());
                //await _discord.GetGuild(677437721081413633).GetTextChannel(682874265594363916).SendMessageAsync("", false, embedbuilder.Build());
            }
            catch
            {
                Console.WriteLine("No Internet connection");
            }
        }
    }
}