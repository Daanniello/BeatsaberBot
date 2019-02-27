using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordBeatSaberBot
{
    public class CommandHelper
    {
        private List<HelpObject> helpObjects = new List<HelpObject>();
        private static List<string> helpCommands = new List<string>();
        private DiscordSocketClient Discord;

        public CommandHelper(DiscordSocketClient discord)
        {
            Discord = discord;

            CreateHelpObject("Search", "Looks up a beat saber player", 
                "!bs search <Playername> \n" +
                "!bs search *If scoresaber is linked with discord");

            CreateHelpObject("Songs", "Looks up a beat saber song",
                "!bs songs <songname> \n");

            CreateHelpObject("TopSong", "Looks up the top pp song from a player",
                "!bs topsong <playername> \n" +
                "!bs topsong *If scoresaber is linked with discord");

            CreateHelpObject("RecentSong", "Looks up the recent song a player has played",
                "!bs recentsong <playername> \n" +
                "!bs recentsong *If scoresaber is linked with discord");

            CreateHelpObject("Country", "Looks up a ranking list from a country",
                "!bs country <countrycode> <top x> * 50 is the max \n" +
                "!bs country <playername>");

            CreateHelpObject("RequestVerification", "Links your discord with beatsaber",
                "!bs RequestVerification <ScoresaberID> \n");

            CreateHelpObject("Playerbase", "Gets the total playercount of beat saber or a country",
                "!bs playerbase \n" +
                "!bs playerbase <country>");

            CreateHelpObject("Mod", "Download link to the scoresaber installer",
                "!bs Mod \n");

            CreateHelpObject("Compare", "Compares two beat saber players",
                "!bs compare <player1> vs <player2> \n");

            CreateHelpObject("Invite", "Gets the bot invite link",
                "!bs invite \n");

            CreateHelpObject("Top10", "Gets the top 10 global players",
                "!bs top10 \n");

            CreateHelpObject("Poll", "Creates a poll with 2 options",
                "Example: !bs poll Is this a good example? Yes | No \n" +
                "You can use markdown from discord \n" +
                "!bs poll <Question + options>");

        }

        public void CreateHelpObject(string name, string description, string moreInfo)
        {
            helpObjects.Add(new HelpObject(name, description, moreInfo));

        }

        public HelpObject GetHelpObject(string name)
        {
            return helpObjects.FirstOrDefault(x => x.name.ToLower() == name.ToLower());
        }

        public Embed GetHelpObjectEmbed(string name)
        {
            var helpObject = GetHelpObject(name);

            var embedFields = new List<EmbedFieldBuilder>();
            embedFields.Add(new EmbedFieldBuilder {
                Name = "Usage:",
                Value = helpObject.moreInfo
            });

            var embedBuilder = new EmbedBuilder
            {
                Title = helpObject.name,
                Description = helpObject.description,
                Fields = embedFields,
                ThumbnailUrl = "https://cdn.discordapp.com/app-icons/504633036902498314/8640cf47aeac6cf7fd071e111467cac5.png?size=512",
                Color = Color.Gold,
                Footer = new EmbedFooterBuilder() { Text = "created by: @SilverHaze#0001", IconUrl = "https://cdn.discordapp.com/avatars/138439306774577152/a_ece649bd8de91e0cd338bd47e5eabc87.png" }
            };
            return embedBuilder.Build();
        }

        public Embed GetHelpList()
        {
            var embedFields = new List<EmbedFieldBuilder>();
            foreach (var help in helpObjects)
            {

                embedFields.Add(new EmbedFieldBuilder
                {
                    Name = help.name,
                    Value = help.description
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
                "***For more info, use !bs help <Command Name>*** \n*Used by " + Discord.Guilds.Count + " servers with a total of "+ userCount + " users*\n",
                Fields = embedFields,
                ThumbnailUrl = "https://cdn.discordapp.com/app-icons/504633036902498314/8640cf47aeac6cf7fd071e111467cac5.png?size=512",
                Color = Color.Gold,
                Footer = new EmbedFooterBuilder() { Text = "created by: @SilverHaze#0001", IconUrl = "https://cdn.discordapp.com/avatars/138439306774577152/a_ece649bd8de91e0cd338bd47e5eabc87.png" }

            };
            return embedBuilder.Build();
        }

        public class HelpObject
        {
            public string name;
            public string description;
            public string moreInfo;

            public HelpObject(string name, string description, string moreInfo)
            {
                this.name = name;
                this.description = description;
                this.moreInfo = moreInfo;
            }
        }
    }

    
}
