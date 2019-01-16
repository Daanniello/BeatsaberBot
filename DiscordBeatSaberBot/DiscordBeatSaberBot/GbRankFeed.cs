using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace DiscordBeatSaberBot
{
    public static class GbRankFeed
    {
        public static async Task<(List<string>,List<string>,List<string>)> GetGbRankList()
        {
            var tab = 2;
            var playerImg = new List<string>();
            var playerRank = new List<string>();
            var playerName = new List<string>();

            for (var x = 1; x <= tab; x++)
            {
                var url = "https://scoresaber.com/global/" + x + "&country=gb";
                using (var client = new HttpClient())
                {
                    var html = await client.GetStringAsync(url);
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);

                    var table = doc.DocumentNode.SelectNodes("//figure[@class='image is-24x24']");
                    playerImg.AddRange(table.Descendants("img").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("src", ""))).ToList());

                    var ranks = doc.DocumentNode.SelectNodes("//td[@class='rank']");
                    playerRank.AddRange(ranks.Select(a => WebUtility.HtmlDecode(a.InnerText).Replace("#", "").Replace(@"\r\n", "").Trim()).ToList());

                    var names = doc.DocumentNode.SelectNodes("//td[@class='player']");
                    playerName.AddRange(names.Select(a => WebUtility.HtmlDecode(a.InnerText).Replace(@"\r\n", "").Trim()).ToList());
                }
            }

            return (playerImg, playerRank, playerName);
        }

        public static async Task<Dictionary<int, List<string>>> GetOldRankList()
        {
            var filePath = "../../../GbRankList.txt";

            var data = new Dictionary<int, List<string>>();
            try
            {
                using (var r = new StreamReader(filePath))
                {
                    var json = r.ReadToEnd();
                    data = JsonConvert.DeserializeObject<Dictionary<int, List<string>>>(json);
                }
            }
            catch
            {
                data.Add(1, new List<string>{"naam", "img"});
            }
            
            return data;
        }

        public static async Task<Dictionary<int, List<string>>> UpdateGbRankList()
        {
            var filePath = "../../../GbRankList.txt";
            var rankList = await GetGbRankList();
            var newData = new Dictionary<int, List<string>>();

            for (var x = 0; x < rankList.Item1.Count; x++)
            {
                 newData.Add(int.Parse(rankList.Item2[x]), new List<string>{rankList.Item3[x], rankList.Item1[x]});
            }

            using (var file = File.CreateText(filePath))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, newData);
            }

            return newData;
        }

        public static async Task<List<EmbedBuilder>> MessagesToSend()
        {
            var embedBuilders = new List<EmbedBuilder>();

            var oldRankList = await GetOldRankList();
            await UpdateGbRankList();
            var newRankList = await GetGbRankList();
            

            var oldCache = new List<string>();

            var counter = 0;
            foreach (var player in oldRankList)
            {
                //player.Key.ToString() != newRankList.Item2[counter] 
                //OldList                NewList                       OldList            NewList
                if (player.Value[0] != newRankList.Item3[counter])
                {
                    if (!oldCache.Contains(newRankList.Item3[counter]))
                    {
                        var imgUrl = newRankList.Item1[counter].Replace("\"", "");
                        if (imgUrl == "/imports/images/oculus.png")
                        {
                            imgUrl = "https://scoresaber.com/imports/images/oculus.png";
                        }

                        // No Message
                        embedBuilders.Add(new EmbedBuilder
                        {
                            Title = "Congrats, " + newRankList.Item3[counter],
                            Description = newRankList.Item3[counter] + " is now rank **#" + newRankList.Item2[counter] + "** from the GB beat saber spelers",

                            ThumbnailUrl = newRankList.Item1[counter].Replace("\"", ""),
                            //Color = GetColorFromRank(int.Parse(newRankList.Item2[counter])),
                            

                        });
                        Console.WriteLine("Feed GB - Message:" + newRankList.Item3[counter] + " is now rank **#" + newRankList.Item2[counter] + "** from the GB beat saber spelers");
                    }
                    else
                    {
                        // New Message 
                        
                    }                  
                }

                oldCache.Add(player.Value[0]);

                counter++;
            }

            return embedBuilders;
        }

        public static async Task GbRankingFeed(DiscordSocketClient discord)
        {
            var guilds = discord.Guilds.Where(x => x.Id == 483482746824687616);
            //var channels = guilds.First().Channels.Where(x => x.Id == 504392851229114369);
            //channels.First().
            var embeds = await MessagesToSend();
            foreach (var embed in embeds)
            {
                await discord.GetGuild(483482746824687616).GetTextChannel(521717100381863967).SendMessageAsync("", false, embed);
            }

            //GB Server
            //483482746824687616
            //535187354122584070

            // test bot server
            //439514151040057344 guild
            //504392851229114369 channel                                            


        }

        private static Color GetColorFromRank(int rank)
        {
            if (rank < 1)
            {
                 return Color.Red;
            }else if (rank <= 3)
            {
                return Color.Blue;
            }
            else if (rank <= 10)
            {
                return Color.Green;
            }
            else if (rank <= 25)
            {
                return Color.Orange;
            }
            else if (rank <= 50)
            {
                return Color.Purple;
            }
            else if (rank <= 100)
            {
                return Color.DarkPurple;
            }
            else if (rank <= 250)
            {
                return Color.DarkRed;
            }
            else
            {
                return Color.Default;
            }
        }
    }
}
