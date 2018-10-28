﻿using Discord;
using Discord.WebSocket;
using System;
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
            var playerInfo2 = (List<List<string>>)null;
            var playerImg = (List<List<string>>)null;
            var playerId = "";
            var playerTopSongImg = "";
            var playerTopSongLink = "";
            var playerTopSongName = "";
            var url = "https://scoresaber.com/global?search=" + playerName.Replace(" ", "+");

            playerId = await GetPlayerId(playerName);

            if (string.IsNullOrEmpty(playerId))
            {
                return EmbedBuilderExtension.NullEmbed("No Results", "I am sorry, I could not find any person named: " + playerName, null, null);
            }

            url = "https://scoresaber.com" + playerId;
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//div[@class='columns']");

                playerInfo = table.Descendants("div")
                    .Skip(1)
                    .Select(tr => tr.Descendants("a")
                        .Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", "")))
                        .ToList())
                    .ToList();
                playerInfo2 = table.Descendants("div")
                    .Skip(1)
                    .Select(tr => tr.Descendants("li")
                        .Select(a => WebUtility.HtmlDecode(a.InnerText))
                        .ToList())
                    .ToList();
                playerImg = table.Descendants("div")
                    .Select(tr => tr.Descendants("img")
                        .Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("src", "")))
                        .ToList())
                    .ToList();
            }


            var steam = playerInfo.First()[0];
            var country = playerInfo.First()[2].Replace("/global?country=", "");
            var rank = playerInfo2.First()[0].Replace("\r\n", "").Trim();
            var pp = playerInfo2.First()[1].Replace("\r\n", "").Trim();
            var playCount = playerInfo2.First()[2].Replace("\r\n", "").Trim();
            var totalScore = playerInfo2.First()[3].Replace("\r\n", "").Trim();

            var ranks = rank.Split('-');

            var builder = new EmbedBuilder();
            
            if (steam != "#")
            {
                builder.ThumbnailUrl = playerImg.First().First();
                builder.AddInlineField(playerName,
                ranks[0] + "\n\n" +
                "Country Ranking: " + ranks[1].Replace("(", "").Replace(")", "") + "\n\n" +
                "Country: " + country.ToUpper() + " :flag_" + country + ":" + "\n\n" +
                playCount + "\n\n" +
                totalScore + "\n\n" +
                pp + "\n\n" +
                "Steam: " + steam + "\n" + "\n");
            }
            else
            {
                builder.AddInlineField(playerName,
                ranks[0] + "\n\n" +
                "Country Ranking: " + ranks[1].Replace("(", "").Replace(")", "") + "\n\n" +
                "Country: " + country.ToUpper() + " :flag_" + country + ":" + "\n\n" +
                playCount + "\n\n" +
                totalScore + "\n\n" +
                pp + "\n\n" + "Oculus user");
            }
            
            builder.Timestamp = DateTimeOffset.Now;
            var rankValue = ranks[0].Split('#')[1].Replace(",", "");
            var rankInt = int.Parse(rankValue);
            var rankColor = Rank.GetRankColor(rankInt);
            builder.WithColor(await rankColor);
            return builder;
        }

        public static async Task<List<EmbedBuilder>> GetSongs(string search)
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
                        .Select(td => WebUtility.HtmlDecode(td.InnerHtml))
                        .ToList())
                    .ToList();

                pictureUrl = table.Descendants("tr")
                    .Select(tr => tr.Descendants("img")
                        .Select(img => WebUtility.HtmlDecode(img.GetAttributeValue("src", "")))
                        .ToList())
                    .ToList();

            }

            if (songs.Count <= 0)
            {
                var returnList = new List<EmbedBuilder>();
                returnList.Add(EmbedBuilderExtension.NullEmbed("No songs found", "Sorry, I did not find any songs with the term: " + search, null, null));
                return returnList;
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

            var builderList = new List<EmbedBuilder>();
            for (int i = 0; i < pictureFilter.Count; i++)
            {
                var builder = new EmbedBuilder();
                builder.WithTitle("Song Info");
                builder.WithThumbnailUrl(pictureFilter[i]);
                builder.WithDescription("All songs that contains " + search);
                builder.AddInlineField(songNameList[i], "\n" + difficultiesList[i] + "\n" + authorList[i] + "\n" + "\nDownload Link\n" + downloadLinkList[i] + "\n \n ");
                builder.WithColor(Color.Red);
                builderList.Add(builder);
            }

            
            return builderList;
        }

        public static async Task<EmbedBuilder> GetTopSongList(string search)
        {
            if (search != "!bs topsong")
            {
                var build = await GetBestSongWithId(await GetPlayerId(search));
                build.WithTitle("Top song from " + search);
                return build;
            }
            var TopSongInfo = new List<string>();
            var TopSongUrl = new List<List<string>>();
            var topSongUrlString = "";
            var topSongImg = new List<List<string>>();
            var topSongImgString = "";
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

                topSongImg = table.Descendants("tr")
                    .Skip(1)
                    .Select(tr => tr.Descendants("img")
                        .Select(img => WebUtility.HtmlDecode(img.GetAttributeValue("src","")))
                        .ToList())
                    .ToList();

                TopSongUrl = table.Descendants("tr").Skip(1)
                    .Select(tr => tr.Descendants("a")
                        .Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", "")))
                        .ToList())
                    .ToList();
                topSongUrlString = TopSongUrl.First().First();
                topSongImgString = "https://scoresaber.com" + topSongImg.First().First();
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
            
            var songName = TopSongInfo[0].Replace("\r\n", "").Trim();
            var songDifficulty = TopSongInfo[2];
            var songStar = TopSongInfo[4];
            var songPlays = TopSongInfo[3];


            var builder = new EmbedBuilder();
            builder.WithTitle(songName.ToString());
            builder.WithDescription(
                "Difficulty: " + songDifficulty + "\n" +
                "Star rate: " + songStar + "\n" +
                "Plays last 24H: " + songPlays
                );
            builder.WithThumbnailUrl(topSongImgString);
            var output = "";
            for (var x = 1; x <= 10; x++)
            {
                output += "#" + x + topList[x].First() + "\n";
            }

            builder.AddInlineField("Top Players", output);

            builder.WithColor(Color.Red);
            return builder;
        }

        public static async Task<List<EmbedBuilder>> GetRanks()
        {
            var builderList = new List<EmbedBuilder>();

            var builder = new EmbedBuilder();
            builder.WithTitle("Top " + Rank.rankMaster.ToString());
            builder.WithColor(await Rank.GetRankColor(Rank.rankMaster));
            builderList.Add(builder);

            builder = new EmbedBuilder();
            builder.WithTitle("<" + Rank.rankChallenger.ToString());
            builder.WithColor(await Rank.GetRankColor(Rank.rankChallenger));
            builderList.Add(builder);

            builder = new EmbedBuilder();
            builder.WithTitle("<" + Rank.rankDiamond.ToString());
            builder.WithColor(await Rank.GetRankColor(Rank.rankDiamond));
            builderList.Add(builder);

            builder = new EmbedBuilder();
            builder.WithTitle("<" + Rank.rankPlatinum.ToString());
            builder.WithColor(await Rank.GetRankColor(Rank.rankPlatinum));
            builderList.Add(builder);

            builder = new EmbedBuilder();
            builder.WithTitle("<" + Rank.rankGold.ToString());
            builder.WithColor(await Rank.GetRankColor(Rank.rankGold));
            builderList.Add(builder);

            builder = new EmbedBuilder();
            builder.WithTitle("<" + Rank.rankSilver.ToString());
            builder.WithColor(await Rank.GetRankColor(Rank.rankSilver));
            builderList.Add(builder);

            builder = new EmbedBuilder();
            builder.WithTitle("+" + Rank.rankSilver);
            builder.WithColor(await Rank.GetRankColor(9999999));
            builderList.Add(builder);

            return builderList;
        }

        public static async Task<EmbedBuilder> GetInviteLink()
        {
            var builder = new EmbedBuilder();
            builder.WithTitle("Invitation Link");
            builder.WithDescription(Configuration.inviteLink);
            return builder;
        }

        public static async Task<string> GetImageUrlFromId(string playerId)
        {
            var playerImgUrl = "";
            using (var client = new HttpClient())
            {
                var url = "https://scoresaber.com" + playerId;
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                playerImgUrl = doc.DocumentNode.SelectSingleNode("//img[@class='image is-96x96']").GetAttributeValue("src","");

            }

            return playerImgUrl;
        }

        public static async Task<string> GetPlayerId(string search)
        {
            var playerId = "";
            var url = "https://scoresaber.com/global?search=" + search.Replace(" ", "+");
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']");

                try
                {
                    playerId = table.Descendants("tr").Skip(1).Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList()).ToList().First().First();
                }
                catch
                {
                    return "";
                }
            }

            return playerId;
        }

        public static async Task<EmbedBuilder> GetBestSongWithId(string playerId)
        {
            var playerTopSongImg = "";
            var playerTopSongLink = "";
            var playerTopSongName = "";
            var playerTopSongPP = "";
            var playerTopSongAcc = "";
            var songName = "";
            var songDifficulty = "";
            var songAuthor = "";

            var url = "https://scoresaber.com" + playerId;
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking songs']");
                playerTopSongImg = "https://scoresaber.com" + table.Descendants("tbody").Select(tr => tr.Descendants("img").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("src", ""))).ToList()).ToList().First().First();
                playerTopSongLink = table.Descendants("tbody").Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList()).ToList().First().First();
                playerTopSongName = table.Descendants("tbody").Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList()).ToList().First().First();
                playerTopSongPP = doc.DocumentNode.SelectSingleNode("//span[@class='scoreTop ppValue']").InnerText;
                playerTopSongAcc = doc.DocumentNode.SelectSingleNode("//span[@class='scoreBottom']").InnerText;
                songName = doc.DocumentNode.SelectSingleNode("//span[@class='songTop pp']").InnerText;
                songDifficulty = doc.DocumentNode.SelectSingleNode("//span[@class='songTop pp']").Descendants("span").First().InnerText;
                songAuthor = doc.DocumentNode.SelectSingleNode("//span[@class='songTop mapper']").InnerText;

            }
            var builder = new EmbedBuilder();
            builder.AddInlineField("Song", "name: " + songName + "\n" +
                                           "difficulty: " + songDifficulty + "\n" +
                                           "Author: " + songAuthor + "\n" +
                                               "pp from this song: " +playerTopSongPP + "\n" +
                                               playerTopSongAcc + "\n" + "https://scoresaber.com" + playerTopSongLink + "\n");
            builder.WithImageUrl(playerTopSongImg);
            builder.WithThumbnailUrl(await GetImageUrlFromId(playerId));
            return builder;
        }

        public static async Task<EmbedBuilder> GetRecentSongWithId(string playerId)
        {
            var playerTopSongImg = "";
            var playerTopSongLink = "";
            var playerTopSongName = "";
            var playerTopSongPP = "";
            var playerTopSongAcc = "";
            var songName = "";
            var songDifficulty = "";
            var songAuthor = "";

            var url = "https://scoresaber.com" + playerId + "&sort=2";
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking songs']");
                playerTopSongImg = "https://scoresaber.com" + table.Descendants("tbody").Select(tr => tr.Descendants("img").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("src", ""))).ToList()).ToList().First().First();
                playerTopSongLink = table.Descendants("tbody").Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList()).ToList().First().First();
                playerTopSongName = table.Descendants("tbody").Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList()).ToList().First().First();
                playerTopSongPP = doc.DocumentNode.SelectSingleNode("//span[@class='scoreTop ppValue']").InnerText;
                playerTopSongAcc = doc.DocumentNode.SelectSingleNode("//span[@class='scoreBottom']").InnerText;
                songName = doc.DocumentNode.SelectSingleNode("//span[@class='songTop pp']").InnerText;
                songDifficulty = doc.DocumentNode.SelectSingleNode("//span[@class='songTop pp']").Descendants("span").First().InnerText;
                songAuthor = doc.DocumentNode.SelectSingleNode("//span[@class='songTop mapper']").InnerText;

            }

            var songDetails = playerTopSongName;
            

            var builder = new EmbedBuilder();
            builder.AddInlineField("Song", "name: " + songName + "\n" +
                                           "difficulty: " + songDifficulty + "\n" +
                                           "Author: " + songAuthor + "\n" +
                                               "pp from this song: " + playerTopSongPP + "\n" +
                                               playerTopSongAcc + "\n" + "https://scoresaber.com" + playerTopSongLink + "\n");
            builder.WithImageUrl(playerTopSongImg);
            builder.WithThumbnailUrl(await GetImageUrlFromId(playerId));
            return builder;
        }

        public static async Task<EmbedBuilder> GetTopxCountry(string search)
        {
            var input = search.Split(" ");
            if (!input[1].Any(char.IsDigit))
            {
                var name = search;
                var countryrank = await GetPlayerCountryRank(name);
                return await GetTopCountryWithName(countryrank);
            }
            if (int.Parse(input[1]) > 50)
            {
                return EmbedBuilderExtension.NullEmbed("Sorry", "Search amount is too big", null, null);
            }
            decimal t = int.Parse(input[1]) / 50;
            var tab = Math.Ceiling(t) + 1;
            var count = int.Parse(input[1]);
            var Names = new List<string>();
            var ids = new List<string>();
            
            for (var x = 1; x <= tab; x++)
            {
                var url = "https://scoresaber.com/global/" + x + "&country=" + input[0];
                using (var client = new HttpClient())
                {
                    var html = await client.GetStringAsync(url);
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);

                    var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']");
                    Names.AddRange(table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList());

                    table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']");
                    ids = table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList();
                }
            }
            
            var topx = new List<string>();
            for (var x = 0; x < int.Parse(input[1]); x++)
            {
                var add = Names[x].Replace("\r\n", " ").Replace("&nbsp&nbsp", "");
                add = add.Trim();
                topx.Add(add);
            }

            var builder = new EmbedBuilder();
            var output = "";
            var counter = 1;
            foreach (var rank in topx) {

                    output += "#" + counter+ " " + rank + "\n";
                
                counter += 1;
            }

            builder.AddInlineField("Top " + input[1] + " " + input[0].ToUpper() + " :flag_" + input[0] + ":", output);
            return builder;
        }

        public static async Task<EmbedBuilder> GetTopCountryWithName(string search)
        {
            //TODO replace search
            var input = search.Split(" ");
            decimal t = int.Parse(input[1]) / 50;
            var tab = Math.Ceiling(t) + 1;
            var count = int.Parse(input[1]);
            var Names = new List<string>();
            var ids = new List<string>();

                var url = "https://scoresaber.com/global/" + tab + "&country=" + input[0];
                using (var client = new HttpClient())
                {
                    var html = await client.GetStringAsync(url);
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);

                    var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']");
                    Names.AddRange(table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList());

                    table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']");
                    ids = table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList();
                }

            var topx = new List<string>();
            for (var x = 0; x < 21; x++)
            {
                var add = Names[x].Replace("\r\n", " ").Replace("&nbsp&nbsp", "");
                add = add.Trim();
                topx.Add(add);
            }

            var builder = new EmbedBuilder();
            var output = "";
            var counter = 1;
            foreach (var rank in topx)
            {

                output += "#" + counter + " " + rank + "\n";

                counter += 1;
            }

            builder.AddInlineField("Top " + input[1] + " " + input[0].ToUpper() + " :flag_" + input[0] + ":", output);
            return builder;
        }

        public static async Task<string> GetPlayerCountryRank(string name)
        {
            var playerInfo = (List<List<string>>)null;
            var playerInfo2 = (List<List<string>>)null;
            var url = "https://scoresaber.com" + await GetPlayerId(name);
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//div[@class='columns']");

                playerInfo = table.Descendants("div")
                    .Skip(1)
                    .Select(tr => tr.Descendants("a")
                        .Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", "")))
                        .ToList())
                    .ToList();

                playerInfo2 = table.Descendants("div")
                    .Skip(1)
                    .Select(tr => tr.Descendants("li")
                        .Select(a => WebUtility.HtmlDecode(a.InnerText))
                        .ToList()).ToList();
                 }
            return playerInfo.First()[2].Replace("/global?country=", "") + playerInfo2.First()[0].Replace("\r\n", "").Trim();
        }
    }
}
