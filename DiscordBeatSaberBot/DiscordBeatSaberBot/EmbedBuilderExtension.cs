using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBeatSaberBot
{
    class EmbedBuilderExtension
    {
        public static EmbedBuilder EmbedBuilder()
        {
            return null;
        }

        public static EmbedBuilder NullEmbed(string Title, string description, string contentTitle, string content)
        {
            var builder = new EmbedBuilder();
            builder.WithTitle(Title);
            builder.WithDescription(description);
            if (contentTitle != null || content != null)
            {
                builder.AddField(contentTitle, content);
            }
            builder.Timestamp = DateTimeOffset.Now;

            builder.WithColor(Color.DarkRed);
            return builder;
        }
    }
}
