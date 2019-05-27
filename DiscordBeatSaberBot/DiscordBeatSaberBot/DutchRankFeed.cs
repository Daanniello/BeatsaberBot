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
    public static class DutchRankFeed
    {
        private static DiscordSocketClient _discord;

        public static async Task<(List<string>,List<string>,List<string>, List<string>)> GetDutchRankList()
        {
            var tab = 5;
            var playerImg = new List<string>();
            var playerRank = new List<string>();
            var playerName = new List<string>();
            var playerId = new List<string>();

            for (var x = 1; x <= tab; x++)
            {
                var url = "https://scoresaber.com/global/" + x + "&country=nl";
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
                    playerId.AddRange(names.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href",""))).ToList());
                }
            }

            return (playerImg, playerRank, playerName, playerId);
        }

        public static async Task<Dictionary<int, List<string>>> GetOldRankList()
        {
            var filePath = "../../../DutchRankList.txt";

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

        public static async Task<Dictionary<int, List<string>>> UpdateDutchRankList()
        {
            var filePath = "../../../DutchRankList.txt";
            var rankList = await GetDutchRankList();
            var newData = new Dictionary<int, List<string>>();

            for (var x = 0; x < rankList.Item1.Count; x++)
            {
                 newData.Add(int.Parse(rankList.Item2[x]), new List<string>{rankList.Item3[x], rankList.Item4[x], rankList.Item1[x]});
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
            await UpdateDutchRankList();
            var newRankList = await GetDutchRankList();
            

            var oldCache = new List<string>();

            var counter = 0;
            foreach (var player in oldRankList)
            {
                //player.Key.ToString() != newRankList.Item2[counter] 
                //OldList                NewList                       OldList            NewList
                if (player.Value[1] != newRankList.Item4[counter])
                {
                    RankDownCheck(int.Parse(newRankList.Item2[counter]),
                        oldRankList.FirstOrDefault(x => x.Value[1] == newRankList.Item4[counter]).Key,
                        newRankList.Item4[counter].Replace("/u/", ""));

                    if (!oldCache.Contains(newRankList.Item4[counter]))
                    {
                        var imgUrl = newRankList.Item1[counter].Replace("\"", "");
                        if (imgUrl == "/imports/images/oculus.png")
                        {
                            imgUrl = "https://i.ibb.co/s1Rf1Lt/oculus.png";
                        }

                        // No Message
                        embedBuilders.Add(new EmbedBuilder
                        {
                            Title = "Congrats, " + newRankList.Item3[counter],
                            Description = newRankList.Item3[counter] + " is nu rank **#" + newRankList.Item2[counter] + "** van de Nederlandse beat saber spelers \n" + GetRankUpNotify(int.Parse(newRankList.Item2[counter]), oldRankList.FirstOrDefault(x => x.Value[1] == newRankList.Item4[counter]).Key, ulong.Parse(newRankList.Item4[counter].Replace("/u/", ""))),

                            ThumbnailUrl = imgUrl,
                                        
                            Color = GetColorFromRank(int.Parse(newRankList.Item2[counter])),
                            

                        });
                        Console.WriteLine("Feed NL - Message:" + newRankList.Item3[counter] + " is now rank **#" + newRankList.Item2[counter] + "** from the US beat saber spelers");
                    }
                    else
                    {
                        // New Message 
                        
                    }                  
                }

                oldCache.Add(player.Value[1]);

                counter++;
            }

            return embedBuilders;
        }

        public static async Task DutchRankingFeed(DiscordSocketClient discord)
        {
            _discord = discord;
            var guilds = discord.Guilds.Where(x => x.Id == 439514151040057344);
            //var channels = guilds.First().Channels.Where(x => x.Id == 504392851229114369);
            //channels.First().
            var embeds = await MessagesToSend();
            foreach (var embed in embeds)
            {
                await discord.GetGuild(505485680344956928).GetTextChannel(520613984668221440).SendMessageAsync("", false, embed);
            }
            


            //NL Server 
            //505485680344956928
            //505732796245868564

            // test bot server
            //439514151040057344 guild
            //504392851229114369 channel
            //var channelToMessageTo = channels.Select(x => x.)                                            


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
                return Color.DarkMagenta;
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

        private static string GetRankUpNotify(int rank, int? oldRank, ulong scoresaberId)
        {
            string message = "";
            if (rank == 0 && oldRank > 0)
            {
                GiveRole(scoresaberId.ToString(), "Koos Rankloos");
            }
            if (rank == 1 && oldRank > 1)
            {
                GiveRole(scoresaberId.ToString(), "Nummer 1");
                message = "Een nieuwe #1 ;O praise the new King";
            }
            else if (rank <= 3 && oldRank > 3)
            {
                GiveRole(scoresaberId.ToString(), "Top 3");
                message = "Een nieuwe top #3 makker GGGGGGGGG";
            }
            else if (rank <= 10 && oldRank > 10)
            {
                GiveRole(scoresaberId.ToString(), "Top 10");
                message = "Een nieuwe top #10 gamer, Kolonisatie bitches!!";
            }
            else if (rank <= 25 && oldRank > 25)
            {
                GiveRole(scoresaberId.ToString(), "Top 25");
                message = "Een nieuwe top #25, grats!! watch out tho, iemand komt zn rank weer terug halen";
            }
            else if (rank <= 50 && oldRank > 50)
            {
                GiveRole(scoresaberId.ToString(), "Top 50");
                message = "Een nieuwe top #50!!!, YAY stuur een invite voor de discord >;d nU!";
            }
            else if (rank <= 100 && oldRank > 100)
            {
                GiveRole(scoresaberId.ToString(), "Top 100");
                message = "Een nieuwe top #100, het begin van een nieuwe pro";
            }
            else if (rank <= 250 && oldRank == null)
            {
                message = "Een nieuwe top #250, Welkom nieuwkomer ;O";
                GiveRole(scoresaberId.ToString(), "Top 250");
            }

            
            //TODO if rank is bigger as 250


            return message;
        }

        public async static Task RankDownCheck(int rank, int oldRank, string scoresaberId)
        {
 
            if (rank > 1 && oldRank == 1 && rank <= 3)
            {
                GiveRole(scoresaberId.ToString(), "Top 3");
            }
            else if (rank > 3 && oldRank <= 3 && rank <= 10)
            {
                GiveRole(scoresaberId.ToString(), "Top 10");
            }
            else if (rank > 10 && oldRank <= 10 && rank <= 25)
            {
                GiveRole(scoresaberId.ToString(), "Top 25");
            }
            else if (rank > 25 && oldRank <= 25 && rank <= 50)
            {
                GiveRole(scoresaberId.ToString(), "Top 50");
            }
            else if (rank > 50 && oldRank <= 50 && rank <= 100)
            {
                GiveRole(scoresaberId.ToString(), "Top 100");
            }
            else if (rank > 100 && oldRank <= 100 && rank <= 250)
            {
                GiveRole(scoresaberId.ToString(), "Top 250");
            }
        }

        public async static Task GiveRole(string scoresaberId, string roleName, DiscordSocketClient discord)
        {
            _discord = discord;
            await GiveRole(scoresaberId, roleName);
        }

        public async static void GiveRoleWithRank(int rank, string scoresaberId)
        {
            if (rank == 0) return;
            if (rank <= 1) await GiveRole(scoresaberId, "Nummer 1");
            else if (rank <= 3) await GiveRole(scoresaberId, "Top 3");
            else if (rank <= 10) await GiveRole(scoresaberId, "Top 10");
            else if (rank <= 25) await GiveRole(scoresaberId, "Top 25");
            else if (rank <= 50) await GiveRole(scoresaberId, "Top 50");
            else if (rank <= 100) await GiveRole(scoresaberId, "Top 100");
            else if (rank <= 250) await GiveRole(scoresaberId, "Top 250");
            else if (rank <= 500) await GiveRole(scoresaberId, "Top 500");
            else if (rank > 500) await GiveRole(scoresaberId, "Top 501+");

        }

        public async static Task GiveRole(string scoresaberId ,string roleName)
        {
            var rolenames = new string[]
            {
                "nummer 1",
                "Top 3",
                "Top 10",
                "Top 25",
                "Top 50",
                "Top 100",
                "Top 250",
            };
            RoleAssignment r = new RoleAssignment(_discord);
            var userDiscordId = r.GetDiscordIdWithScoresaberId(scoresaberId);
            if (userDiscordId != 0)
            {

                var guild_id = (ulong)505485680344956928;
                var guild = _discord.Guilds.FirstOrDefault(x => x.Id == guild_id);
                var user = guild.GetUser(userDiscordId);

                if (user.Id != 221373638979485696)
                {
                    foreach (var userRole in user.Roles)
                    {
                        if (rolenames.Contains(userRole.Name))
                        {
                            await user.RemoveRoleAsync(userRole);
                        }
                    }
                }
                
                var role = guild.Roles.FirstOrDefault(x => x.Name == roleName);
                await user.AddRoleAsync(role);
                Console.WriteLine("User: " + user.Username + "| Added: " + role.Name);
            }
        }      
    }
}
