using System;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBeatSaberBot
{
    class Help
    {
        static public Embed GetHelpList(DiscordSocketClient Discord, int pagenr = -1)
        {
            var embedFields = new List<EmbedFieldBuilder>();

            var HelpListFinal = new Dictionary<string, string>();

            foreach (var enumItem in Enum.GetNames(typeof(HelpAttribute.Catergories)))
            {
                HelpListFinal.Add(enumItem, "----------------------");
                foreach (var helpItem in ReflectionExtension.GetAllCustomHelpAttributes())
                {
                    
                    if (helpItem.Key.Contains(enumItem))
                    {
                        HelpListFinal.Add(helpItem.Key, helpItem.Value);
                    }
                    
                }
            }


            foreach (var help in HelpListFinal)
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

            var embedBuilders = new List<EmbedBuilder>();
            createPages();
            List<EmbedBuilder> createPages()
            {
                var pageCountLeft = embedFields.Count % 5;
                var pageCountIn = Math.Ceiling(Convert.ToDecimal(embedFields.Count / 5));
                var pageCount = pageCountIn;

                if (pageCountLeft == 0) pageCount -= 1;

                for (var i = 0; i <= pageCount; i++)
                {
                    var leftOver = 5;
                    if (pageCount == i) leftOver = pageCountLeft;
                    var embedArray = embedFields.ToArray().SubArray(i * 5, leftOver);


                    embedBuilders.Add(new EmbedBuilder
                    {
                        Title = "**Command List** :question:    Page: [" + (i + 1) + "/" + (pageCount + 1) + "] \n\n" +
                                "***For more info, use !bs help <Command Name>*** \n*Used by " + Discord.Guilds.Count +
                                " servers with a total of " + userCount + " users*\n",
                        Fields = embedArray.ToList(),
                        ThumbnailUrl =
                            "https://cdn.discordapp.com/app-icons/504633036902498314/8640cf47aeac6cf7fd071e111467cac5.png?size=512",
                        Color = Color.Gold,
                        Footer = new EmbedFooterBuilder
                        {
                            Text = "created by: @SilverHaze#0001",
                            IconUrl =
                                "https://cdn.discordapp.com/avatars/138439306774577152/a_ece649bd8de91e0cd338bd47e5eabc87.png"
                        }
                    });
                }

                return embedBuilders;
            }

            if (pagenr == -1) pagenr = 0;
            return embedBuilders[pagenr].Build();
        }
    }
}
