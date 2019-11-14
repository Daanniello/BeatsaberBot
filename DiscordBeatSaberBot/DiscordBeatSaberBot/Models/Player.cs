using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace DiscordBeatSaberBot
{
    internal class Player
    {
        public Player(string name)
        {
            this.name = name;
        }

        public string name { get; set; }
        public int rank { get; set; }
        public int countryRank { get; set; }

        public string countryName { get; set; }

        public string countryIcon { get; set; }

        public int playCount { get; set; }
        public string totalScore { get; set; }

        public string pp { get; set; }

        public string steamLink { get; set; }

        //public string scoresaberLink { get; set; }

        public string imgLink { get; set; }

        public Player Next { get; set; }

        public Player Before { get; set; }

        public async Task<List<string>> GetPlayerId()
        {
            var realPlayerIdList = new List<string>();
            var playerIdList = new List<List<string>>();
            var playerNameList = new List<string>();
            string url = "https://scoresaber.com/global?search=" + name.Replace(" ", "+");
            using (var client = new HttpClient())
            {
                string html = await client.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']");

                try
                {
                    playerIdList = table.Descendants("tr").Skip(1).Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList()).ToList();
                    playerNameList = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']").Descendants("span").Where(span => span.GetAttributeValue("class", "") == "songTop pp").Select(span => WebUtility.HtmlDecode(span.InnerText)).ToList();
                    int counter = 0;
                    foreach (var playerId in playerIdList)
                    {
                        if (playerNameList[counter].ToUpper() == name.ToUpper() && realPlayerIdList.Count <= 3)
                            realPlayerIdList.Add(playerId[0]);

                        counter++;
                    }
                }
                catch
                {
                    return null;
                }
            }

            return realPlayerIdList;
        }
    }
}