using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace DiscordBeatSaberBot
{
    public class CountryRankFeed
    {
        private static DiscordSocketClient _discord;
        private static Logger _logger;
        private readonly string countryCode;
        private readonly string countryCodeCombo;

        public CountryRankFeed(DiscordSocketClient discord, params string[] CountryCode)
        {
            _discord = discord;
            _logger = new Logger(discord);
            countryCode = CountryCode[0];
            countryCodeCombo = CountryCode[0];
            for (int i = 1; i < CountryCode.Length; i++)
            {
                countryCodeCombo += "," + CountryCode[i];
                countryCode += "_" + CountryCode[i];
            }
        }

        public async Task SendFeedInCountryDiscord(ulong guildId, ulong channelId)
        {
            var embeds = new List<EmbedBuilder>();
            try
            {
                embeds = await MessagesToSend();
            }
            catch
            {
                _logger.Log(Logger.LogCode.warning, "Scoresaber is being annoying " + countryCodeCombo + "");
                return;
            }

            var guild = _discord.GetGuild(guildId);
            var channel = guild.GetTextChannel(channelId);
            foreach (var embed in embeds)
            {
                try
                {
                    await channel.SendMessageAsync("", false, embed.Build());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(embed + " EX: " + ex);
                }
            }
        }

        public async Task<(List<string>, List<string>, List<string>, List<string>)> GetCountryRankList(int tabs)
        {
            int tab = tabs;
            var playerImg = new List<string>();
            var playerRank = new List<string>();
            var playerName = new List<string>();
            var playerId = new List<string>();

            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 0, 10);
                for (int x = 1; x <= tab; x++)
                {
                    string url = "https://scoresaber.com/global/" + x + "&country=" + countryCodeCombo;

                    string html = "";
                    try
                    {
                        var reponse = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url));
                        if (!reponse.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException();
                        }
                            
                        html = await client.GetStringAsync(url);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(Logger.LogCode.fatal_error, "Scoresaber ignored me >;d\n\n" + ex);

                        throw ex;
                    }

                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var table = doc.DocumentNode.SelectNodes("//figure[@class='image is-24x24']");
                    playerImg.AddRange(table.Descendants("img").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("src", ""))).ToList());

                    var ranks = doc.DocumentNode.SelectNodes("//td[@class='rank']");
                    playerRank.AddRange(ranks.Select(a => WebUtility.HtmlDecode(a.InnerText).Replace("#", "").Replace(@"\r\n", "").Trim()).ToList());

                    var names = doc.DocumentNode.SelectNodes("//td[@class='player']");
                    playerName.AddRange(names.Select(a => WebUtility.HtmlDecode(a.InnerText).Replace(@"\r\n", "").Trim()).ToList());
                    playerId.AddRange(names.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList());

                    Console.WriteLine(url);
                    await Task.Delay(2000);
                }
            }

            return (playerImg, playerRank, playerName, playerId);
        }

        public async Task<Dictionary<int, List<string>>> GetOldRankList()
        {
            string filePath = "../../../CountryRankingLists/" + countryCode + "RankList.txt";

            var data = new Dictionary<int, List<string>>();
            try
            {
                using (var r = new StreamReader(filePath))
                {
                    string json = r.ReadToEnd();
                    data = JsonConvert.DeserializeObject<Dictionary<int, List<string>>>(json);
                }
            }
            catch
            {
                data.Add(1, new List<string> { "naam", "img" });
            }

            return data;
        }

        public async Task<Dictionary<int, List<string>>> UpdateCountryRankList()
        {
            string filePath = "../../../CountryRankingLists/" + countryCode + "RankList.txt";
            var rankList = await GetCountryRankList(5);
            var newData = new Dictionary<int, List<string>>();

            for (int x = 0; x < rankList.Item1.Count; x++)
            {
                newData.Add(int.Parse(rankList.Item2[x]), new List<string> { rankList.Item3[x], rankList.Item4[x], rankList.Item1[x] });
            }

            using (var file = File.CreateText(filePath))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, newData);
            }

            return newData;
        }

        public async Task<List<EmbedBuilder>> MessagesToSend()
        {
            var embedBuilders = new List<EmbedBuilder>();

            var oldRankList = await GetOldRankList();
            await UpdateCountryRankList();
            var newRankList = await GetCountryRankList(5);


            var oldCache = new List<string>();

            int counter = 0;
            foreach (var player in oldRankList)
            {
                //player.Key.ToString() != newRankList.Item2[counter] 
                //OldList                NewList                       OldList            NewList
                if (player.Value[1] != newRankList.Item4[counter])
                    if (!oldCache.Contains(newRankList.Item4[counter]))
                    {
                        string imgUrl = newRankList.Item1[counter].Replace("\"", "");
                        if (imgUrl == "/imports/images/oculus.png")
                            imgUrl = "https://scoresaber.com/imports/images/oculus.png";
                        else
                            imgUrl = "https://scoresaber.com" + imgUrl;

                        // No Message
                        try
                        {
                            embedBuilders.Add(new EmbedBuilder
                            {
                                Title = "Congrats, " + newRankList.Item3[counter],
                                Description = newRankList.Item3[counter] + " is now rank **#" + newRankList.Item2[counter] + "** from the " + countryCodeCombo + " beat saber players",
                                Url = "https://scoresaber.com" + newRankList.Item4[counter],
                                ThumbnailUrl = imgUrl,

                                Color = GetColorFromRank(int.Parse(newRankList.Item2[counter]))
                            });
                        }
                        catch
                        {
                            embedBuilders.Add(new EmbedBuilder
                            {
                                Title = "Congrats, " + newRankList.Item3[counter],
                                Description = newRankList.Item3[counter] + " is now rank **#" + newRankList.Item2[counter] + "** from the " + countryCodeCombo + " beat saber players",
                                Color = GetColorFromRank(int.Parse(newRankList.Item2[counter]))
                            });
                            var Logger = new Logger(_discord);
                            Logger.Log(Logger.LogCode.debug, "-" + countryCodeCombo + " feed- \nUrl is not correct. \nWrong url is from: " + newRankList.Item3[counter] + "\nAnd the url is: " + imgUrl);
                        }

                        Console.WriteLine("Feed " + countryCodeCombo + " - Message:" + newRankList.Item3[counter] + " is now rank **#" + newRankList.Item2[counter] + "** from the " + countryCodeCombo + " beat saber players");
                    }

                oldCache.Add(player.Value[1]);

                counter++;
            }

            return embedBuilders;
        }

        private static Color GetColorFromRank(int rank)
        {
            if (rank < 1)
                return Color.Purple;
            if (rank <= 10)
                return Color.Red;
            if (rank <= 25)
                return Color.Green;
            if (rank <= 50)
                return Color.Blue;
            if (rank <= 100)
                return Color.Gold;
            if (rank <= 250)
                return Color.Magenta;
            return Color.Default;
        }
    }
}