using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    static class BeatSaberInfoExtension
    {
        public static async Task<List<List<string>>> GetPlayers()
        {
            string url = "https://scoresaber.com/global";
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']");
                return table.Descendants("tr")
                    .Skip(1)
                    .Select(tr => tr.Descendants("td")
                            .Select(td => WebUtility.HtmlDecode(td.InnerText))
                        .ToList())
                    .ToList();
            }
        }

        public static async Task<EmbedBuilder> GetTop10Players()
        {
            var topInfo = "";
            var counter = 0;
            var top10 = await GetPlayers();
            foreach (var player in top10)
            {
                var infoToTell = "";
                foreach (var result in player)
                {
                    var item = result.Replace(@"\r\n", " ").Trim();
                    if (!string.IsNullOrEmpty(item))
                    {
                        infoToTell += item + " ";
                    }
                }
                topInfo += infoToTell + "\n";
                counter++;
                if (counter >= 10)
                {
                    break;
                }
            }

            var builder = new EmbedBuilder();
            builder.WithTitle("Top 10 Beatsaber Players");
            builder.WithDescription("Top 10 best beatsaber players");
            builder.AddInlineField("Players", topInfo);

            builder.WithColor(Color.Red);
            return builder;
        }

        public static async Task<EmbedBuilder> GetPlayer(string playerName)
        {

            var playerInfo = (List<List<string>>)null;
            var playerId = (List<List<string>>)null;
            var playerInfoToTell = "";
            var url = "https://scoresaber.com/global?search=" + playerName.Replace(" ", "+");

            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']");
                playerInfo = table.Descendants("tr")
                    .Skip(1)
                    .Select(tr => tr.Descendants("td")
                        .Select(td => WebUtility.HtmlDecode(td.InnerText))
                        .ToList())
                    .ToList();

                playerInfo = table.Descendants("tr")
                    .Skip(1)
                    .Select(tr => tr.Descendants("td")
                        .Select(td => WebUtility.HtmlDecode(td.InnerText))
                        .ToList())
                    .ToList();
            }

            foreach (var player in playerInfo)
            {
                foreach (var result in playerInfo)
                {
                    var infoToTell = "";
                    foreach (var value in result)
                    {
                        var item = value.Replace(@"\r\n", " ").Trim();
                        if (!string.IsNullOrEmpty(item))
                        {
                            infoToTell += item + "/";
                        }
                    }
                    playerInfoToTell += infoToTell + "\n";
                }
            }
            var playerInfoToTellArray = playerInfoToTell.Split("/");

            var builder = new EmbedBuilder();
            builder.WithTitle("Player Information");
            builder.WithDescription("All Information about " + playerName);
            builder.AddInlineField("Player",
                "Name: " + playerInfoToTellArray[1] + "\n" +
                "Rank: " + playerInfoToTellArray[0] + "\n" +              
                "Peformance points: " + playerInfoToTellArray[2] + "\n");

            builder.WithColor(Color.Red);
            return builder;
        }

        public static async Task<EmbedBuilder> GetSongs(string search)
        {
            var songs = (List<List<string>>)null;
            var pictureUrl = new List<List<string>>();
            var url = "https://beatsaver.com/search/all/" + search.Replace(" ", "%20");

            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//div[@class='row']");
                songs = table.Descendants("tr")
                    .Skip(1)
                    .Select(tr => tr.Descendants("td")
                        .Select(td => WebUtility.HtmlDecode(td.InnerText + td.InnerHtml))
                        .ToList())
                    .ToList();

                pictureUrl = table.Descendants("tr")
                    .Select(tr => tr.Descendants("img")
                        .Select(img => WebUtility.HtmlDecode(img.GetAttributeValue("src", "")))
                        .ToList())
                    .ToList();
            }

            var songNameList = new List<string>();
            var downloadLinkList = new List<string>();
            var authorList = new List<string>();
            var difficultiesList = new List<string>();
            foreach (var songInfo in songs)
            {
                foreach (var info in songInfo)
                {
                    if (info.Contains("Song"))
                    {
                        songNameList.Add(info);
                    }

                    if (info.Contains("Author"))
                    {
                        authorList.Add(info);
                    }

                    if (info.Contains("Difficulties"))
                    {
                        difficultiesList.Add(info);
                    }

                    if (info.Contains("href"))
                    {
                        var addon = info.Split('"').Where(x => x.Contains("beatsaver"));
                        downloadLinkList.Add(addon.First());
                    }
                }
            }

            var pictureFilter = new List<string>();
            foreach (var pictureList in pictureUrl)
            {
                foreach (var picture in pictureList)
                {
                    if (!string.IsNullOrEmpty(picture))
                    {
                        pictureFilter.Add(picture);
                    }
                }
            }

            var builder = new EmbedBuilder();
            builder.WithTitle("Song Info");
            builder.WithDescription("All songs that contains " + search);
            for (int i = 0; i < pictureFilter.Count; i++)
            {
                builder.AddInlineField(songNameList[i], "\n" + difficultiesList[i] + "\n" + authorList[i] + "\n" + "\nSong image link:\n" + pictureFilter[i] + "\nDownload Link\n" + downloadLinkList[i] + "\n \n ");
            }

            builder.WithColor(Color.Red);
            return builder;
        }

        public static async Task<EmbedBuilder> GetTopSongList(string search)
        {
            var TopSongInfo = new List<string>();
            var TopSongUrl = new List<List<string>>();
            var topSongUrlString = "";
            var topList = new List<List<string>>();
            var url = "https://scoresaber.com/top";
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='table table-bordered']");
                TopSongInfo = table.Descendants("tr")
                    .Skip(1)
                    .Select(tr => tr.Descendants("td")
                        .Select(td => WebUtility.HtmlDecode(td.InnerText))
                        .ToList()).First()
                    .ToList();

                TopSongUrl = table.Descendants("tr").Skip(1)
                    .Select(tr => tr.Descendants("a")
                        .Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", "")))
                        .ToList())
                    .ToList();
                topSongUrlString = TopSongUrl.First().First();
            }

            using (var client = new HttpClient())
            {
                url = "https://scoresaber.com" + topSongUrlString;
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='table table-bordered ']");
                topList = table.Descendants("tr")
                    .Select(tr => tr.Descendants("a")
                        .Select(a => WebUtility.HtmlDecode(a.InnerText))
                        .ToList())
                    .ToList();
            }

            var builder = new EmbedBuilder();
            builder.WithTitle("Most Played Song Top List");
            builder.WithDescription("All songs that contains " + search);
            var output = "";
            for (var x = 1; x <= 10; x++)
            {
                output += "#" + x + topList[x].First() + "\n";
            }

            builder.AddInlineField("Top Players", output);

            builder.WithColor(Color.Red);
            return builder;
        }
    }
}
