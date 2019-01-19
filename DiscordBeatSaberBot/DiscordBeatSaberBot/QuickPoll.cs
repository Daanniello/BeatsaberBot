using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    class QuickPoll
    {
        private SocketMessage _messageInfo;
        public QuickPoll(SocketMessage messageInfo)
        {
            _messageInfo = messageInfo;
        }

        public async Task CreatePoll()
        {
            string question = _messageInfo.Content.Substring(9);

            EmbedBuilder builder = new EmbedBuilder
            {
                Color = Color.DarkRed,
                Title = "QuickPoll",
                Description = question,
                ThumbnailUrl = @"https://i.ibb.co/Y08zHnb/Pika.png"
            };
            Embed embed = builder.Build();

            var completedMessage = await _messageInfo.Channel.SendMessageAsync("", false, embed);
            await completedMessage.AddReactionAsync(new Emoji("⬅"));
            await completedMessage.AddReactionAsync(new Emoji("➡"));
        }
    }
}
