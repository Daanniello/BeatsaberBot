using Discord;
using Discord.WebSocket;
using System;

namespace DiscordBeatSaberBot
{
    internal class Logger
    {
        public enum LogCode
        {
            fatal_error,
            error,
            warning,
            debug
        }

        private readonly DiscordSocketClient _discord;

        public Logger(DiscordSocketClient discord)
        {
            _discord = discord;
        }

        public async void Log(LogCode code, string message)
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
                case LogCode.fatal_error:
                    color = Color.DarkRed;
                    message = "<@138439306774577152> \n" + message;
                    break;
            }

            var embedbuilder = new EmbedBuilder
            {
                Title = code.ToString(),
                Description = message,
                Color = color
            };

            Console.WriteLine(embedbuilder.Title + "\n\n" + embedbuilder.Description);
            await _discord.GetGuild(505485680344956928).GetTextChannel(592877335355850752).SendMessageAsync("", false, embedbuilder.Build());
        }
    }
}