using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Discord.WebSocket;
using DiscordBeatSaberBot.Models.ScoreberAPI;
using HtmlAgilityPack;
using Newtonsoft.Json;

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

        public int ReplaysWatched{ get; set; }

        //public async Task<List<string>> GetPlayerId()
        //{
        //    var realPlayerIdList = new List<string>();
        //    var playerIdList = new List<List<string>>();
        //    var playerNameList = new List<string>();
        //    var encodedName = HttpUtility.UrlEncode(name);

        //    string url = "https://scoresaber.com/global?search=" + encodedName;
        //    using (var client = new HttpClient())
        //    {
        //        string html = await client.GetStringAsync(url);
        //        var doc = new HtmlDocument();
        //        doc.LoadHtml(html);

        //        var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']");

        //        if (table == null) return null;

        //        try
        //        {
        //            playerIdList.Add(table.Descendants("tr").Skip(1).Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList()).ToList().First());
        //            playerNameList = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']").Descendants("span").Where(span => span.GetAttributeValue("class", "") == "songTop pp").Select(span => WebUtility.HtmlDecode(span.InnerText)).ToList();
        //            int counter = 0;

       

        //            foreach (var playerId in playerIdList)
        //            {
        //                if (playerNameList[counter].ToUpper() == name.ToUpper() && realPlayerIdList.Count <= 3)
        //                    realPlayerIdList.Add(playerId[0]);

        //                counter++;
        //            }
        //        }
        //        catch
        //        {
        //            return null;
        //        }
        //    }

        //    return realPlayerIdList;
        //}

        public async Task<string> GetPlayerId()
        {
            string scoresaberId = null;

            string url = "https://new.scoresaber.com/api/players/by-name/" + name;
            using (var client = new HttpClient())
            {
                var infoPlayerRaw = await client.GetAsync(url);
                if (infoPlayerRaw.StatusCode != HttpStatusCode.OK) return null;
                var players1ScoresaberID = JsonConvert.DeserializeObject<List<ScoreSaberSearchByNameModel>>(infoPlayerRaw.Content.ReadAsStringAsync().Result);
                var player1search = players1ScoresaberID.Where(x => x.Name.ToLower() == name.ToLower());
                if (player1search.Count() == 0) return null;
                scoresaberId = players1ScoresaberID.Where(x => x.Name.ToLower() == name.ToLower()).First().Playerid;
            }

            return scoresaberId;
        }
    }
}