using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;

namespace DiscordBeatSaberBot
{
    class Logger
    {
        private DiscordSocketClient _discord;

        public enum LogCode{
            error,
            warning,
            debug
        }

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
            }
            var embedbuilder = new EmbedBuilder
            {
                Title = code.ToString(),
                Description = message,
                Color = color
            };
            await _discord.GetGuild(505485680344956928).GetTextChannel(592877335355850752).SendMessageAsync("", false, embedbuilder.Build());
        }
    }
}
