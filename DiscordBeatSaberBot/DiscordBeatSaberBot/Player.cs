using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    class Player
    {
        public string name { get; set; }
        public int rank { get; set; }
        public int countryRank { get; set; }

        public string countryName { get; set; }

        public string countryIcon { get; set; }

        public int playCount { get; set; }
        public string totalScore { get; set; }

        public string pp { get; set; }

        public string steamLink { get; set; }

        public string imgLink { get; set; }

        public Player Next { get; set; }

        public Player Before { get; set; }

        public Player(string name)
        {
            this.name = name;
        }

        public async Task<List<string>> GetPlayerId()
        {
            var realPlayerIdList = new List<string>();
            var playerIdList = new List<List<string>>();
            var playerNameList = new List<string>();
            var url = "https://scoresaber.com/global?search=" + name.Replace(" ", "+");
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']");

                try
                {
                    playerIdList = table.Descendants("tr").Skip(1).Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList()).ToList();
                    playerNameList = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']").Descendants("span").Where(span => span.GetAttributeValue("class", "") == "songTop pp").Select(span => WebUtility.HtmlDecode(span.InnerText)).ToList();
                    var counter = 0;
                    foreach (var playerId in playerIdList)
                    {
                        if (playerNameList[counter].ToUpper() == name.ToUpper())
                        { realPlayerIdList.Add(playerId[0]); }

                        counter++;
                    }
                }
                catch { return null; }
            }

            return realPlayerIdList;
        }



    }
}
