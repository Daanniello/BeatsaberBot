using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DiscordBeatSaberBot
{
    internal class QuickPoll
    {
        private readonly SocketMessage _messageInfo;

        public QuickPoll(SocketMessage messageInfo)
        {
            _messageInfo = messageInfo;
        }

        public async Task CreatePoll()
        {
            string question = _messageInfo.Content.Substring(9);

            var builder = new EmbedBuilder
            {
                Color = Color.DarkRed,
                Title = "QuickPoll",
                Description = question,
                Footer = new EmbedFooterBuilder { Text = "Reminder: Use *!bs poll question* to create a new poll" },
                ThumbnailUrl = @"https://i.ibb.co/Y08zHnb/Pika.png"
            };
            var embed = builder.Build();

            var completedMessage = await _messageInfo.Channel.SendMessageAsync("", false, embed);
            await completedMessage.AddReactionAsync(Emote.Parse("<:green_check:671412276594475018>"));
            await completedMessage.AddReactionAsync(Emote.Parse("<:red_check:671413258468720650>"));
        }
    }
}