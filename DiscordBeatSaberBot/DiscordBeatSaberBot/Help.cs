using Discord;
using Discord.WebSocket;
using System.Collections.Generic;

namespace DiscordBeatSaberBot
{
    class Help
    {
        static public Embed GetHelpList(DiscordSocketClient Discord)
        {
            var embedFields = new List<EmbedFieldBuilder>();
            foreach (var help in ReflectionExtension.GetAllCustomHelpAttributes())
            {
                embedFields.Add(new EmbedFieldBuilder
                {
                    Name = help.Key,
                    Value = help.Value
                });
            }

            var userCount = 0;
            foreach (var x in Discord.Guilds)
            {
                userCount += x.MemberCount;
            }

            var embedBuilder = new EmbedBuilder
            {
                Title = "**Command List** :question: \n\n" +
                        "***For more info, use !bs help <Command Name>*** \n*Used by " + Discord.Guilds.Count + " servers with a total of " + userCount + " users*\n",
                Fields = embedFields,
                ThumbnailUrl = "https://cdn.discordapp.com/app-icons/504633036902498314/8640cf47aeac6cf7fd071e111467cac5.png?size=512",
                Color = Color.Gold,
                Footer = new EmbedFooterBuilder { Text = "created by: @SilverHaze#0001", IconUrl = "https://cdn.discordapp.com/avatars/138439306774577152/a_ece649bd8de91e0cd338bd47e5eabc87.png" }
            };
            return embedBuilder.Build();
        }
    }
}
