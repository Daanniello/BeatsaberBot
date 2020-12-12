using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordBeatSaberBot.Config
{
    class ReactionRolesConfig
    {
        public static Dictionary<string, string> NbsgReactions = new Dictionary<string, string>
        {
                    { "<:windows:553375150138195968>", "WMR" },
                    { "❕", "NSFW" },
                    { "<:AYAYA:509158069809315850>", "Weeb" },
                    { "<:minecraft:600768239261319217>", "Minecraft" },
                    { "<:osu:578679882553491493>", "Osu!" },
                    { "<:vrchat:537413837100548115>", "VRChat" },
                    { "<:pavlov:593542245022695453>", "Pavlov" },
                    { "🗺", "Mapper" },
                    //{ "💻", "Modder" },
                    { "<:terebilo:508313942297280518>", "Normale-Grip" },
                    { "<:miitchelW:557923575944970241>", "Botchal-Grip" },
                    { "🆕", "Overige-Grip" },
                    { "🇹", "Tracker Sabers" },
                    { "🇵", "Palm-Grip" },
                    { "🇨", "Claw-Grip" },
                    { "🇫", "F-Grip" },
                    { "🇷", "R-Grip" },
                    { "🇰", "K-Grip" },
                    { "🇻", "V-Grip" },
                    { "🇧", "B-Grip" },
                    { "🇽", "X-Grip" },
                    { "<:oculus:537368385206616075>", "Oculus" },
                    { "<:vive:537368500277084172>", "Vive" },
                    { "<:indexvr:589754441545154570>", "Index" },
                    { "<:pimax:614170153185312789>", "Pimax" },
                    { "<:megaotherway:526402963372245012>", "Event" },
                    { "<:skyToxicc:479251361028898828>", "Giftig" },
                    { "<:BeatSaberTime:633400410400489487>", "Beat saber multiplayer" },
                    { "🎮", "Andere games multiplayer" },
                    { "<:owopeak:401481118165237760>", "IRL event" },
                    { "<:peepoGroningen:600782037325971458>", "Groningen" },
                    { "<:peepoFriesland:600782036923449344>", "Friesland" },
                    { "<:peepoDrenthe:600782037556920372>", "Drenthe" },
                    { "<:peepoOverijssel:600782037649063947>", "Overijssel" },
                    { "<:peepoFlevoland:600782037812772870>", "Flevoland" },
                    { "<:peepoGelderland:600782037590474780>", "Gelderland" },
                    { "<:peepoUtrecht:600782037472903169>", "Utrecht" },
                    { "<:peepoNoordHolland:600782038035070986>", "Noord-Holland" },
                    { "<:peepoZuidHolland:600782037682749448>", "Zuid-Holland" },
                    { "<:peepoZeeland:600782037049409547>", "Zeeland" },
                    { "<:peepoBrabant:600782036642430986>", "Noord-Brabant" },
                    { "<:peepoLimburg:600782036919123968>", "Limburg" },
                    { "<:PETTHEWORLDCUP:725606068386005012>", "729279152712056902" } //Foreign Channel
        };

        public static Dictionary<string, string> SilverhazeReactions = new Dictionary<string, string>
        {
            { "<:AWEE:588758943686328330>", "627294872365170699" }, //Anime
            { "<:silverGasm:628988811329929276>", "635918669976829962" }, //NSFW
            { "<:silverHug:681594969336709380>", "718466543004155915" }, //Community
            { "<:PingSilver:637282051862560782>", "782584263563673620" }, //Live Notifications            
            { "<:pimax:782546887001112596>", "782547202962751499" }, //Pimax
            { "<:wmr:782546888087830537>", "782547419086716938" }, //WMR
            { "<:vive:782546887503904778>", "782547455941279767" }, //Vive
            { "<:playstation:782546886850641932>", "782547482500136971" }, //PlaystationVR
            { "<:oculus:782548572021063740>", "782547519057690635" }, //Oculus
            { "<:index:782546887219740673>", "782547566348599306" }, //IndexVR
            { "⚪", "782551901085499402" }, //White
            { "🔴", "782552016123068446" }, //Red
            { "🔵", "782552142887649280" }, //Blue
            { "\U0001f7e4", "782552193139343401" }, //Brown
            { "\U0001f7e3", "782552272743432195" }, //Purple
            { "\U0001f7e2", "782552333220708414" }, //Green
            { "\U0001f7e1", "782552477274341376" }, //Yellow
            { "\U0001f7e0", "782552560720150549" }, //Orange

        };

        public static Dictionary<string, string> GetReactionRoles()
        {
            var dic = new Dictionary<string, string>();
            NbsgReactions.ToList().ForEach(x => dic.Add(x.Key, x.Value));
            SilverhazeReactions.ToList().ForEach(x => dic.Add(x.Key, x.Value));
            return dic;
        }
    }
}
