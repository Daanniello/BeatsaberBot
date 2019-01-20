using System;
using System.Collections.Generic;
using Discord;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Discord.WebSocket;

namespace DiscordBeatSaberBot
{
    static class Rank
    {
        public static Color Master = Color.Red;
        public static Color Challenger = Color.Magenta;
        public static Color Diamond = Color.Blue;
        public static Color Platinum = Color.Teal;
        public static Color Gold = Color.Gold;
        public static Color Silver = Color.LighterGrey;
        public static Color bronze = Color.DarkOrange;

        public static int rankMaster = 10; 
        public static int rankChallenger = 500;
        public static int rankDiamond = 5000;
        public static int rankPlatinum = 10000;
        public static int rankGold = 40000;
        public static int rankSilver = 80000;

        static public async Task<Color> GetRankColor(int rank)
        {
            if (rank <= 10)
            {
                return Master;
            }
            else if (rank <= 500)
            {
                return Challenger;
            }else if (rank <= 5000)
            {
                return Diamond;
            }
            if (rank <= 10000)
            {
                return Platinum;
            }
            else if (rank <= 40000)
            {
                return Gold;
            }
            if (rank <= 80000)
            {
                return Silver;
            }

            return bronze;
        }

        public static async Task<Embed> GetPlayerbaseCount(SocketMessage messageInfo)
        {
            //Playerbase
            string message = messageInfo.Content.Substring(4);
            int tab = 10000;
            string country = "";
            //string url = "https://scoresaber.com/global/" + tab + country;
            //https://scoresaber.com/global/2&country=us

            int playerBaseCount = 0;
            var t = message.Split(' ');
            try{
                t[1] = t[1];
            }catch
            {
                t =new string[] { "",""};
            }
            if (message.Split(' ').Length > 1)
            {
                country = "&country=" + t[1];
            }

            bool loop = true;
            int tempCount = 0;
            int minTab = 4000;
            int plusTab = 4000;
            int minusCounter = 0;
            do
            {
                if (tab < 0)
                {
                    tab = 1;
                }
                tempCount = await getList();

                if (tempCount == 0)
                {
                    int val = 2;
                    tab = tab - minTab;

                    if (minusCounter >= 1)
                    {
                        val = 1;
                    }
                    minTab = minTab / val;
                    minusCounter++;
                }
                if (tempCount == 50)
                {
                    tab = tab + minTab;
                    plusTab = plusTab / 2;
                    minusCounter = 0;
                }
                if (tempCount > 0 && tempCount < 50)
                {
                    playerBaseCount = tab * 50 + tempCount;
                    loop = false;
                }
                playerBaseCount += tempCount;


            } while (loop);

            async Task<int> getList()
            {
                using (var client = new HttpClient())
                {
                    var html = await client.GetStringAsync("https://scoresaber.com/global/" + tab + country);
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);

                    var table = doc.DocumentNode.SelectNodes("//td[@class='rank']");
                    int listCount = 0;
                    if (table != null)
                    {
                        listCount = table.Count;
                    }
                    return listCount;
                }
            }

            var embedBuilder = new EmbedBuilder
            {
                Title = "Playerbase Count " + t[1],
                Description = "The Current playerbase count is ***" + playerBaseCount + "*** \n Tabs: ***" + tab + "***",
                Color = Color.Blue
            };

            return embedBuilder.Build();
        }
    }
}
