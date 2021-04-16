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

            var HelpListFinal = new Dictionary<string, CommandHelpProperty>();

            foreach (var enumItem in Enum.GetNames(typeof(HelpAttribute.Catergories)))
            {
                foreach (var helpItem in ReflectionExtension.GetAllCustomHelpAttributes())
                {

                    if (helpItem.Value.CommandCatergory.ToString() == enumItem)
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
                    Value = help.Value.CommandInfo
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
                                "***For more info, use !bs help (CommandName)*** \n*Used by " + Discord.Guilds.Count +
                                " servers with a total of " + userCount + " users*\n",
                        Fields = embedArray.ToList(),
                        ThumbnailUrl =
                            "https://cdn.discordapp.com/icons/731936395223892028/0c8bb286f1864f5ff4a917e9423ff5a6.png?size=512",
                        Color = Color.Gold,
                        Footer = new EmbedFooterBuilder
                        {
                            Text = "created by: @SilverHaze#0001",
                            IconUrl =
                                "https://cdn.discordapp.com/avatars/138439306774577152/1084ee232303df04772bf68f1d41ef83.png"
                        }
                    });
                }

                return embedBuilders;
            }

            if (pagenr == -1) pagenr = 0;
            return embedBuilders[pagenr].Build();
        }

        static public Embed GetSpecificHelp(string commandName)
        {
            var embed = new EmbedBuilder();

            foreach (var helpItem in ReflectionExtension.GetAllCustomHelpAttributes())
            {

                if (helpItem.Key.ToLower() == commandName.ToLower())
                {
                    embed.Title = $"Extra Help for {helpItem.Value.CommandName}";
                    embed.Description = $"Usage: {helpItem.Value.CommandUsage}";
                    embed.Color = Color.Gold;
                    embed.AddField(new EmbedFieldBuilder() { Name = helpItem.Value.CommandName, Value = $"info: {helpItem.Value.CommandInfo} \n Category: {helpItem.Value.CommandCatergory}" });
                }


            }

            return embed.Build();
        }


        static public Embed GetHelpListRaw()
        {
            var embed = new EmbedBuilder();

            foreach (var helpItem in ReflectionExtension.GetAllCustomHelpAttributes())
            {
                embed.Title = $"Help List Raw";
                embed.Color = Color.Gold;
                embed.AddField(new EmbedFieldBuilder() { Name = helpItem.Value.CommandName, Value = $"info: {helpItem.Value.CommandInfo} \n Category: {helpItem.Value.CommandCatergory}" });
            }

            return embed.Build();
        }
    }
}
