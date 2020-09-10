﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Color = Discord.Color;
using Discord.WebSocket;
using DiscordBeatSaberBot.Api.ScoreberAPI;
using DiscordBeatSaberBot.Api.ScoreberAPI.Models;
using DiscordBeatSaberBot.Models;
using DiscordBeatSaberBot.Models.ScoreberAPI;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp.Serialization.Json;

namespace DiscordBeatSaberBot.Extensions
{
    internal static class BeatSaberInfoExtension
    {
        public static async Task<List<List<string>>> GetPlayers()
        {
            var url = "https://scoresaber.com/global";
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']");
                return table.Descendants("tr").Skip(1).Select(tr =>
                    tr.Descendants("td").Select(td => WebUtility.HtmlDecode(td.InnerText)).ToList()).ToList();
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
                        infoToTell += item + " ";
                }

                topInfo += infoToTell + "\n";
                counter++;
                if (counter >= 10)
                    break;
            }

            var builder = new EmbedBuilder();
            builder.WithTitle("Top 10 Beatsaber Players");
            builder.WithDescription("Top 10 best beatsaber players");
            builder.AddField("Players", topInfo);

            builder.WithColor(Color.Red);
            return builder;
        }

        public static async Task<Player> GetPlayerInfoWithScoresaberId(string scoresaberId)
        {
            var url = "https://scoresaber.com/u/" + scoresaberId;

            var player = new Player("");
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var playerName = doc.DocumentNode.SelectSingleNode("//h5[@class='title is-5']").InnerText
                    .Replace("\n", "").Replace("\r", "").Trim();
                var playerList = await GetPlayerInfo(playerName);
                player = playerList.First();
            }

            return player;
        }

        public static async Task<EmbedBuilder> SearchLinkedPlayer(string ScoresaberId)
        {
            var url = "https://scoresaber.com/u/" + ScoresaberId;

            var player = new Player("");
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var playerName = doc.DocumentNode.SelectSingleNode("//h5[@class='title is-5']").InnerText
                    .Replace("\n", "").Replace("\r", "").Trim();
                var playerList = await GetPlayerInfo(playerName);
                player = playerList.First();
            }

            var countryNameSmall = player.countryName;

            var ppNext = "-";
            var ppBefore = "-";
            var playerNextName = "Not Found o.o";

            var nextAndBefore = await RankedNeighbours(player.name, player.rank, 1);
            var playerNext = new Player(nextAndBefore.Item1)
            {
                pp = await GetPlayerPP(nextAndBefore.Item1)
            };
            var playerBefore = new Player(nextAndBefore.Item2)
            {
                pp = await GetPlayerPP(nextAndBefore.Item2)
            };

            try
            {
                player.Next = playerNext;
                player.Before = playerBefore;
            }
            catch
            {
                Console.WriteLine(nextAndBefore.Item1 + " or " + nextAndBefore.Item2 + " is not found");
            }

            var ppNextDouble = Math.Round(double.Parse(player.Next.pp) - double.Parse(player.pp), 0);
            var ppBeforeDouble = Math.Round(double.Parse(player.pp) - double.Parse(player.Before.pp), 0);
            if (player.Before.pp == "0")
                ppBefore = "No Search results";
            else
                ppBefore = ppBeforeDouble.ToString();
            if (player.Next.pp == "0")
                ppNext = "No Search results";
            else
                ppNext = ppNextDouble.ToString();


            playerNextName = player.Next.name;

            var builder = new EmbedBuilder();

            if (player.steamLink != "#")

            {
                builder.ThumbnailUrl = player.imgLink;
                builder.Title = "**" + player.name.ToUpper() + " :flag_" + countryNameSmall.ToLower() + ":" + "**";
                builder.Url = "https://scoresaber.com/u/" + ScoresaberId;
                builder.AddField("`ID: " + ScoresaberId.Replace("/u/", "") + "`",
                    "```Global Ranking: #" + player.rank + "\n\n" + "Country Ranking: #" + player.countryRank + "\n\n" +
                    "Play Count: " + player.playCount + "\n\n" + "Total Score: " + player.totalScore + "\n\n" +
                    "Performance Points: " + player.pp + "\n\n" + "Replays Watched: " + player.ReplaysWatched +
                    "``` \n\n" + "```Player above: " + playerNextName + "\n\n" + "PP till UpRank: " + ppNext + "\n\n" +
                    "PP till DeRank: " + ppBefore + "``` \n [Click here for steam link](" + player.steamLink + ")\n\n");
            }
            else
            {
                builder.ThumbnailUrl = "https://scoresaber.com/imports/images/oculus.png";
                builder.Title = "**" + player.name.ToUpper() + " :flag_" + countryNameSmall.ToLower() + ":" + "**";
                builder.Url = "https://scoresaber.com/u/" + ScoresaberId;
                builder.AddField("`ID: " + ScoresaberId.Replace("/u/", "") + "`",
                    "```Global Ranking: #" + player.rank + "\n\n" + "Country Ranking: #" + player.countryRank + "\n\n" +
                    "Play Count: " + player.playCount + "\n\n" + "Total Score: " + player.totalScore + "\n\n" +
                    "Performance Points: " + player.pp + "\n\n" + "Replays Watched: " + player.ReplaysWatched +
                    "``` \n\n" + "```Player above: " + playerNextName + "\n\n" + "PP till UpRank: " + ppNext + "\n\n" +
                    "PP till DeRank: " + ppBefore + "```");
            }


            var rankColor = Rank.GetRankColor(player.rank);
            builder.WithColor(await rankColor);
            return builder;
        }

        static public async Task<List<EmbedBuilder>> GetPlayerSearchInfoEmbed(string scoresaberId, SocketMessage message)
        {
            var embedBuilderList = new List<EmbedBuilder>();

            var searchedPlayerInfo = await new ScoresaberAPI(scoresaberId, message).GetPlayerFull();
            var embedBuilder = new EmbedBuilder
            {
                Title = $"**{searchedPlayerInfo.playerInfo.Name} :flag_{searchedPlayerInfo.playerInfo.Country.ToLower()}:**",
                ThumbnailUrl =
                $"https://new.scoresaber.com{searchedPlayerInfo.playerInfo.Avatar}",
                Url = $"https://new.scoresaber.com/u/{searchedPlayerInfo.playerInfo.PlayerId}",
            };
            embedBuilder.AddField(
                $"`ID: {searchedPlayerInfo.playerInfo.PlayerId}`",
                $"```cs\n" +
                $"Global Rank:              #{searchedPlayerInfo.playerInfo.rank} \n\n" +
                $"Country Rank:             #{searchedPlayerInfo.playerInfo.CountryRank} \n\n" +
                $"Average Ranked Acc:       '{searchedPlayerInfo.scoreStats.AvarageRankedAccuracy} %' \n\n" +
                $"PP:                       '{searchedPlayerInfo.playerInfo.Pp} PP' \n\n" +
                $"```" +
                "\n\n" +
                $"```cs\n" +
                $"Total Plays:              {searchedPlayerInfo.scoreStats.TotalPlayCount} \n\n" +
                $"Total Score:              {searchedPlayerInfo.scoreStats.TotalScore} \n\n" +
                $"Total Ranked Plays:       {searchedPlayerInfo.scoreStats.RankedPlayerCount} \n\n" +
                $"Total Ranked Score:       {searchedPlayerInfo.scoreStats.TotalRankedScore} \n\n" +
                $"" +
                $"" +
                $"```" +
                "\n\n" +
                $"```cs\n" +
                $"Role:                     '{searchedPlayerInfo.playerInfo.Role}' \n\n" +
                $"Inactive:                 '{searchedPlayerInfo.playerInfo.Inactive}' \n\n" +
                $"Banned:                   '{searchedPlayerInfo.playerInfo.Banned}' \n\n" +
                //$"Rank History:             {searchedPlayerInfo.playerInfo.History} \n\n" +
                $"" +
                $"```"
            );

            //embedBuilder.ImageUrl = $"https://new.scoresaber.com/api/static/badges/{searchedPlayerInfo.playerInfo.Badges.First().Image}";

            embedBuilderList.Add(embedBuilder);
            foreach (var badge in searchedPlayerInfo.playerInfo.Badges)
            {
                embedBuilderList.Add(new EmbedBuilder() { ImageUrl = $"https://new.scoresaber.com/api/static/badges/{badge.Image}", Title = badge.Description });
            }

            return embedBuilderList;
        }

        public static async Task<List<EmbedBuilder>> GetSongs(string search)
        {
            var titles = new List<string>();
            var songs = new List<string>();


            var pics = new List<string>();


            var url = "https://beatsaver.com/search/all/0?key=" + search.Replace(" ", "+");

            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                titles = doc.DocumentNode.SelectNodes("//a[@class='has-text-weight-semibold is-size-3']")
                    .Select(x => x.InnerText).ToList();
                songs = doc.DocumentNode.SelectNodes("//table[@class='table is-fullwidth']")
                    .Select(x => WebUtility.HtmlDecode(x.InnerText)).ToList();
                songs = songs.Select(x => x.Replace("\n", "")).ToList();
                var regex = new Regex("[ ]{3,}");
                songs = songs.Select(x => regex.Replace(x, "~").Trim()).ToList();

                pics = doc.DocumentNode.SelectNodes("//img").Skip(1).Select(x => x.GetAttributeValue("src", ""))
                    .ToList();
            }

            var builderList = new List<EmbedBuilder>();

            for (var x = 0; x < songs.Count; x++)
            {
                var attributes = songs[x].Split("~");
                builderList.Add(new EmbedBuilder
                {
                    Title = attributes[2],
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder {Name = attributes[4].Split(":")[0], Value = attributes[4].Split(":")[1]},
                        new EmbedFieldBuilder {Name = attributes[3].Split(":")[0], Value = attributes[3].Split(":")[1]},
                        new EmbedFieldBuilder
                        {
                            Name = attributes[1].Split(":")[0],
                            Value = attributes[1].Split(":")[1] + ":" + attributes[1].Split(":")[2] + ":" +
                                    attributes[1].Split(":")[3]
                        },
                        new EmbedFieldBuilder {Name = attributes[5].Split(":")[0], Value = attributes[5].Split(":")[1]},
                        new EmbedFieldBuilder {Name = "Stats", Value = attributes[6].Replace("||", "|")},
                        new EmbedFieldBuilder {Name = attributes[7].Split(":")[0], Value = attributes[7].Split(":")[1]}
                    },
                    Color = Color.Blue,
                    ThumbnailUrl = pics[x]
                });
            }


            return builderList;
        }

        public static async Task<List<EmbedBuilder>> GetRanks()
        {
            var builderList = new List<EmbedBuilder>();

            var builder = new EmbedBuilder();
            builder.WithTitle("Top " + Rank.rankMaster);
            builder.WithColor(await Rank.GetRankColor(Rank.rankMaster));
            builderList.Add(builder);

            builder = new EmbedBuilder();
            builder.WithTitle("<" + Rank.rankChallenger);
            builder.WithColor(await Rank.GetRankColor(Rank.rankChallenger));
            builderList.Add(builder);

            builder = new EmbedBuilder();
            builder.WithTitle("<" + Rank.rankDiamond);
            builder.WithColor(await Rank.GetRankColor(Rank.rankDiamond));
            builderList.Add(builder);

            builder = new EmbedBuilder();
            builder.WithTitle("<" + Rank.rankPlatinum);
            builder.WithColor(await Rank.GetRankColor(Rank.rankPlatinum));
            builderList.Add(builder);

            builder = new EmbedBuilder();
            builder.WithTitle("<" + Rank.rankGold);
            builder.WithColor(await Rank.GetRankColor(Rank.rankGold));
            builderList.Add(builder);

            builder = new EmbedBuilder();
            builder.WithTitle("<" + Rank.rankSilver);
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
            builder.WithDescription(GlobalConfiguration.inviteLink);
            return builder;
        }

        public static async Task<string> GetImageUrlFromId(string playerId)
        {
            var playerImgUrl = "";
            using (var client = new HttpClient())
            {
                var url = "https://scoresaber.com" + playerId;
                var html = await client.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                playerImgUrl = doc.DocumentNode.SelectSingleNode("//img[@class='image is-96x96']")
                    .GetAttributeValue("src", "");
            }

            return playerImgUrl;
        }

        public static async Task<string> GetPlayerId(string search)
        {
            var player = new Player(search);
            return await player.GetPlayerId();
        }

        public static async Task<string> GetPlayerIdsWithUsername(string search)
        {
            using (var client = new HttpClient())
            {
                var playerInfoJsonData = await client.GetStringAsync($"https://new.scoresaber.com/api/players/by-name/{search}");
                var playerInfo = JsonConvert.DeserializeObject<ScoresaberSearchPlayerModel>(playerInfoJsonData);
                return playerInfo.Playerid;
            }
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
            var playerSongRank = "";
            var playerName = "";

            var url = "https://scoresaber.com/u/" + playerId.Replace("/u/", "");
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking songs']");
                playerTopSongImg = "https://scoresaber.com" + table.Descendants("tbody")
                                       .Select(tr =>
                                           tr.Descendants("img").Select(a =>
                                               WebUtility.HtmlDecode(a.GetAttributeValue("src", ""))).ToList()).ToList()
                                       .First().First();
                playerTopSongLink = table.Descendants("tbody").Select(tr =>
                        tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", "")))
                            .ToList())
                    .ToList().First().First();
                playerTopSongName = table.Descendants("tbody")
                    .Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList()).ToList()
                    .First().First();
                playerTopSongPP =
                    StringCleanup(doc.DocumentNode.SelectSingleNode("//span[@class='scoreTop ppValue']").InnerText);
                playerTopSongAcc =
                    StringCleanup(doc.DocumentNode.SelectSingleNode("//span[@class='scoreBottom']").InnerText);
                songName = doc.DocumentNode.SelectSingleNode("//span[@class='songTop pp']").InnerText;
                songDifficulty = doc.DocumentNode.SelectSingleNode("//span[@class='songTop pp']").Descendants("span")
                    .First().InnerText;
                songAuthor = doc.DocumentNode.SelectSingleNode("//span[@class='songTop mapper']").InnerText;
                playerSongRank = StringCleanup(doc.DocumentNode.SelectNodes("//th[@class='rank']")[1].InnerText);
                playerName = StringCleanup(doc.DocumentNode.SelectSingleNode("//h5[@class='title is-5']").InnerText);
            }

            string StringCleanup(string RawContent)
            {
                var cleanContent = RawContent.Replace("\n", "").Trim();
                return cleanContent;
            }

            var builder = new EmbedBuilder();
            builder.WithTitle("**Top song from: " + playerName + "**");
            builder.WithDescription("**Song name:** " + songName + "\n" + "**Difficulty:** " + songDifficulty + "\n" +
                                    "**Author:** " + songAuthor + "\n\n" + "**Rank:** " + playerSongRank + "\n**" +
                                    playerTopSongAcc.Split(' ')[0] + "** " + playerTopSongAcc.Split(' ')[1] +
                                    "\n**PP**: " + playerTopSongPP + "\n\n" + "https://scoresaber.com" +
                                    playerTopSongLink + "\n");
            builder.WithImageUrl(playerTopSongImg);
            builder.WithUrl(url);
            try
            {
                builder.WithThumbnailUrl(await GetImageUrlFromId(playerId));
            }
            catch
            {
            }

            return builder;
        }

        public static async Task<EmbedBuilder> GetNewRecentSongWithScoresaberId(string playerId)
        {
            var url = $"https://new.scoresaber.com/api/player/{playerId}/scores/recent/1";
            var embedBuilder = new EmbedBuilder();

            using (var client = new HttpClient())
            {
                var httpResponseMessage = await client.GetAsync(url);

                if (httpResponseMessage.StatusCode != HttpStatusCode.OK) return EmbedBuilderExtension.NullEmbed("Scoresaber Error", $"Status code: {httpResponseMessage.StatusCode}");

                var recentSongsJsonData = await httpResponseMessage.Content.ReadAsStringAsync();

                var recentSongsInfo = JsonConvert.DeserializeObject<ScoresaberSongsModel>(recentSongsJsonData);
                var recentSong = recentSongsInfo.Scores[0];

                //Download info from beat saver
                var beatsaverUrl = $"https://beatsaver.com/api/maps/by-hash/{recentSong.Id}";

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(beatsaverUrl),
                    Method = HttpMethod.Get,
                };

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
                client.DefaultRequestHeaders.Add("User-Agent", "C# App");
                client.DefaultRequestHeaders.Connection.Add("keep-alive");

                var httpResponseMessage2 = await client.SendAsync(request);

                if (httpResponseMessage2.StatusCode != HttpStatusCode.OK) return EmbedBuilderExtension.NullEmbed("BeatSaver Error", $"Status code: {httpResponseMessage2.StatusCode}");

                var recentSongsJsonDataBeatSaver = await httpResponseMessage2.Content.ReadAsStringAsync();
                var recentSongsInfoBeatSaver = JsonConvert.DeserializeObject<BeatSaverMapInfoModel>(recentSongsJsonDataBeatSaver);


                var playerInfoJsonData = await client.GetStringAsync($"https://new.scoresaber.com/api/player/{playerId}/full");
                var playerInfo1 = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(playerInfoJsonData);
                var playerInfo = playerInfo1.playerInfo;


                embedBuilder = new EmbedBuilder
                {
                    Title = $"**Recent Song From: {playerInfo.Name} :flag_{playerInfo.Country.ToLower()}:**",
                    ImageUrl =
                $"https://scoresaber.com/imports/images/songs/{recentSong.Id}.png",
                    Url = $"https://scoresaber.com/u/{playerInfo.PlayerId}",
                };

                object acc = "";
                object mods = "";

                if (recentSong.Mods != "") mods = $"Mods:               '{recentSong.Mods}' \n";
                if (recentSong.MaxScoreEx != 0)
                {
                    double percentage = Convert.ToDouble(recentSong.UScore) / Convert.ToDouble(recentSong.MaxScoreEx) * 100;
                    acc = $"Accuracy:           { Math.Round(percentage, 2)}% \n";
                }

                var metadataDynamic = recentSongsInfoBeatSaver.Metadata.Characteristics[0].Difficulties;
                dynamic difficulty = metadataDynamic.GetType().GetProperty(recentSong.GetDifficulty()).GetValue(metadataDynamic, null);
                var mapExtraDetails = "";
                if (difficulty != null)
                {
                    mapExtraDetails = $"```cs\n" +
                    $"Bpm:                '{recentSongsInfoBeatSaver.Metadata.Bpm}' \n" +
                    $"Duration:           '{recentSongsInfoBeatSaver.Metadata.Duration}' \n" +
                    $"Notes:              '{difficulty.Notes}' \n" +
                    $"Njs:                '{difficulty.Njs}' \n" +
                    $"NjsOffset:          '{difficulty.NjsOffset}' \n" +
                    $"Bombs:              '{difficulty.Bombs}' \n" +
                    $"Obstacles:          '{difficulty.Obstacles}' \n" +
                    $"```";
                }

                var actualWeight = Math.Round(recentSong.Pp * recentSong.Weight, 2);

                embedBuilder.AddField($"Song Name: {recentSong.Name}",
                    $"```cs\n" +
                    //$"Song Name:          {recentSong.Name} \n" +
                    //$"Song Sub name:            {recentSong.SongSubName} \n\n" +
                    $"Difficulty:         {recentSong.GetDifficulty()} \n" +
                    $"Song Author name:   {recentSong.SongAuthorName} \n" +
                    $"Map Author name:    {recentSong.LevelAuthorName} \n" +
                    $"```" +

                    $"```cs\n" +
                    $"Rank:               #{recentSong.Rank} \n" +
                    $"Score:              {recentSong.ScoreScore} \n" +
                    acc +
                    $"PP:                 {recentSong.Pp} \n" +
                    $"PP Weight:          {actualWeight} \n" +
                    $"```" +

                    $"```cs\n" +
                    $"Time Set:           '{recentSong.Timeset.DateTime.ToShortDateString()} {recentSong.Timeset.DateTime.ToShortTimeString()}' \n" +
                    $"Max Score Ex:       '{recentSong.MaxScoreEx}' \n" +
                    mods +
                    $"```" +

                    mapExtraDetails +

                    "\n" +
                    $"Url:                https://scoresaber.com/leaderboard/{recentSong.LeaderboardId} \n\n" +
                    $"Direct download:       https://beatsaver.com/{recentSongsInfoBeatSaver.DirectDownload} \n\n"
                );


            }
            return embedBuilder;
        }

        public static async Task<EmbedBuilder> GetNewTopSongWithScoresaberId(string playerId)
        {
            var url = $"https://new.scoresaber.com/api/player/{playerId}/scores/top/1";
            var embedBuilder = new EmbedBuilder();

            using (var client = new HttpClient())
            {
                var httpResponseMessage = await client.GetAsync(url);

                if (httpResponseMessage.StatusCode != HttpStatusCode.OK) return EmbedBuilderExtension.NullEmbed("Scoresaber Error", $"Status code: {httpResponseMessage.StatusCode}");

                var TopSongsJsonData = await httpResponseMessage.Content.ReadAsStringAsync();

                var TopSongsJsonInfo = JsonConvert.DeserializeObject<ScoresaberSongsModel>(TopSongsJsonData);
                var TopSong = TopSongsJsonInfo.Scores[0];

                var playerInfoJsonData = await client.GetStringAsync($"https://new.scoresaber.com/api/player/{playerId}/full");
                var playerInfo1 = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(playerInfoJsonData);
                var playerInfo = playerInfo1.playerInfo;


                embedBuilder = new EmbedBuilder
                {
                    Title = $"**Recent Song From: {playerInfo.Name} :flag_{playerInfo.Country.ToLower()}:**",
                    ImageUrl =
                $"https://new.scoresaber.com/api/static/covers/{TopSong.Id}.png",
                    Url = $"https://scoresaber.com/u/{playerInfo.PlayerId}",
                };

                object acc = "";
                object mods = "";

                if (TopSong.Mods != "") mods = $"Mods:               '{TopSong.Mods}' \n";
                if (TopSong.MaxScoreEx != 0)
                {
                    double percentage = Convert.ToDouble(TopSong.UScore) / Convert.ToDouble(TopSong.MaxScoreEx) * 100;
                    acc = $"Accuracy:           { Math.Round(percentage, 2)}% \n";
                }

                var actualWeight = Math.Round(TopSong.Pp * TopSong.Weight, 2);

                embedBuilder.AddField($"Song Name: {TopSong.Name}",
                    $"```cs\n" +
                    //$"Song Name:          {recentSong.Name} \n" +
                    //$"Song Sub name:            {recentSong.SongSubName} \n\n" +
                    $"Difficulty:         {TopSong.GetDifficulty()} \n" +
                    $"Song Author name:   {TopSong.SongAuthorName} \n" +
                    $"Map Author name:    {TopSong.LevelAuthorName} \n" +
                    $"```" +

                    $"```cs\n" +
                    $"Rank:               #{TopSong.Rank} \n" +
                    $"Score:              {TopSong.ScoreScore} \n" +
                    acc +
                    $"PP:                 {TopSong.Pp} \n" +
                    $"PP Weight:          {actualWeight} \n" +
                    $"```" +

                    $"```cs\n" +
                    $"Time Set:           '{TopSong.Timeset.DateTime.ToShortDateString()} {TopSong.Timeset.DateTime.ToShortTimeString()}' \n" +
                    $"Max Score Ex:       '{TopSong.MaxScoreEx}' \n" +
                    mods +
                    $"```" +
                    "\n" +
                    $"Url:                https://scoresaber.com/leaderboard/{TopSong.LeaderboardId} \n\n"
                );


            }
            return embedBuilder;
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
            var playerName = "";
            var playerSongRank = "";

            var url = "https://scoresaber.com/u/" + playerId.Replace("/u/", "") + "&sort=2";
            using (var client = new HttpClient())
            {
                var sw = new Stopwatch();
                sw.Start();
                var html = await client.GetStringAsync(url);
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking songs']");
                playerTopSongImg = "https://scoresaber.com" + table.Descendants("tbody")
                                       .Select(tr =>
                                           tr.Descendants("img").Select(a =>
                                               WebUtility.HtmlDecode(a.GetAttributeValue("src", ""))).ToList()).ToList()
                                       .First().First();
                playerTopSongLink = table.Descendants("tbody").Select(tr =>
                        tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", "")))
                            .ToList())
                    .ToList().First().First();
                playerTopSongName = StringCleanup(table.Descendants("tbody")
                    .Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList()).ToList()
                    .First().First());
                playerTopSongPP =
                    StringCleanup(doc.DocumentNode.SelectSingleNode("//span[@class='scoreTop ppValue']").InnerText)
                        .Split(' ').First();
                playerTopSongAcc = doc.DocumentNode.SelectSingleNode("//span[@class='scoreBottom']").InnerText;
                songName = StringCleanup(doc.DocumentNode.SelectSingleNode("//span[@class='songTop pp']").InnerText);
                songDifficulty = StringCleanup(doc.DocumentNode.SelectSingleNode("//span[@class='songTop pp']")
                    .Descendants("span").First().InnerText);
                songAuthor =
                    StringCleanup(doc.DocumentNode.SelectSingleNode("//span[@class='songTop mapper']").InnerText);
                playerName = StringCleanup(doc.DocumentNode.SelectSingleNode("//h5[@class='title is-5']").InnerText);
                playerSongRank = StringCleanup(doc.DocumentNode.SelectNodes("//th[@class='rank']")[1].InnerText);
            }

            string StringCleanup(string RawContent)
            {
                var cleanContent = RawContent.Replace("\n", "").Trim();
                return cleanContent;
            }

            var songDetails = playerTopSongName;


            var builder = new EmbedBuilder();
            //builder.AddField( "", "**Songname:** " + songName + "\n" + "**Difficulty:** " + songDifficulty + "\n" + "**Author:** " + songAuthor + "\n\n" + "**Rank: **" + playerSongRank + "\n**Score: **" + playerTopSongAcc + "\n" + "**PP:** " + playerTopSongPP + "\n\n" + "https://scoresaber.com" + playerTopSongLink + "\n");
            builder.WithDescription("**Songname:** " + songName + "\n" + "**Difficulty:** " + songDifficulty + "\n" +
                                    "**Author:** " + songAuthor + "\n\n" + "**Rank: **" + playerSongRank + "\n**" +
                                    playerTopSongAcc.Split(' ')[0] + "** " + playerTopSongAcc.Split(' ')[1] + "\n" +
                                    "**PP:** " + playerTopSongPP + "\n\n" + "https://scoresaber.com" +
                                    playerTopSongLink + "\n");
            builder.WithTitle("**Recent song from: " + playerName + "**");
            builder.WithUrl("https://scoresaber.com/u/" + playerId.Replace("/u/", ""));
            builder.WithImageUrl(playerTopSongImg);
            try
            {
                builder.WithThumbnailUrl(await GetImageUrlFromId(playerId));
            }
            catch
            {
            }

            return builder;
        }

        public static async Task<(string, string)> RankedNeighbours(string playerName, int playerRank,
            int recursionLoop = 0)
        {
            //var playerInfo = await GetPlayerInfo(playerName, recursionLoop);
            double Rank = playerRank;
            var t = Rank / 50;
            var tab = Math.Ceiling(t);
            var rankOnTab = Rank % 50;
            var count = Rank;
            var Names = new List<string>();
            var namesTop = new List<string>();
            var namesBottom = new List<string>();

            var url = "https://scoresaber.com/global/" + tab;
            await GetNames(url);

            async Task GetNames(string infoUrl,
                int otherPage = 0)
            {
                using (var client = new HttpClient())
                {
                    var html = await client.GetStringAsync(infoUrl);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']");
                    if (otherPage == 1)
                        namesBottom.AddRange(table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText))
                            .ToList());
                    else if (otherPage == 2)
                        namesTop.AddRange(table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText))
                            .ToList());
                    else
                        Names.AddRange(table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList());
                }
            }

            if (rankOnTab < 4 && rankOnTab != 0 && tab != 1)
            {
                url = "https://scoresaber.com/global/" + (tab - 1);
                await GetNames(url, 1);
            }

            if (rankOnTab > 47 || rankOnTab == 0)
            {
                url = "https://scoresaber.com/global/" + (tab + 1);
                await GetNames(url, 2);
            }

            var topx = new List<string>();
            for (var x = 0; x < 50; x++)
                if (x < rankOnTab + 3 && x > rankOnTab - 3)
                    try
                    {
                        var add = Names[x].Replace("\r\n", " ").Replace("&nbsp&nbsp", "");
                        add = add.Trim();
                        topx.Add(add);
                    }
                    catch
                    {
                        topx.Add("NoResults");
                    }

            var outputList = new List<string>();
            outputList.AddRange(namesBottom);
            outputList.AddRange(Names);
            outputList.AddRange(namesTop);

            var builder = new EmbedBuilder();
            var output = new List<string>();
            var counter = 1;

            if (outputList.Count >= 100)
                rankOnTab += 50;

            foreach (var rank in outputList)
            {
                if (counter > rankOnTab - 2 && counter < rankOnTab + 2)
                {
                    if (rank.ToLower().Contains(playerName.ToLower()))
                        output.Add(rank.Replace("\r\n", " ").Replace("&nbsp&nbsp", "").Trim());
                    else
                        output.Add(rank.Replace("\r\n", " ").Replace("&nbsp&nbsp", "").Trim());
                }

                counter += 1;
            }

            if (output.Count != 3)
                do
                {
                    output.Add("PlayerNotFound");
                } while (output.Count < 3);

            return (output[0], output[2]);
        }

        public static async Task<string> GetPlayerCountryRank(string name)
        {
            var playerInfo = (List<List<string>>)null;
            var playerInfo2 = (List<List<string>>)null;
            var playerId = await GetPlayerId(name);
            var url = "https://scoresaber.com" + playerId.First();
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//div[@class='columns']");

                playerInfo = table.Descendants("div").Skip(1).Select(tr =>
                        tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", "")))
                            .ToList())
                    .ToList();

                playerInfo2 = table.Descendants("div").Skip(1).Select(tr =>
                    tr.Descendants("li").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList()).ToList();
            }

            return playerInfo.First()[2].Replace("/global?country=", "") +
                   playerInfo2.First()[0].Replace("\r\n", "").Trim();
        }

        public static async Task<List<Player>> GetPlayerInfo(string playerName, int recursionLoop = 0,
            bool skipNeighbour = false)
        {
            var players = new List<Player>();
            await ScrapPlayerInfo(playerName, recursionLoop, skipNeighbour, players);
            return players;
        }

        private static async Task ScrapPlayerInfo(string playerName, int recursionLoop, bool skipNeighbour,
            List<Player> players)
        {
            var ids = await GetPlayerId(playerName);

            var recursionCounter = recursionLoop;

            var playerInfo = (List<List<string>>)null;
            var playerInfo2 = (List<List<string>>)null;
            var playerImg = (List<List<string>>)null;
            var url = "https://scoresaber.com/global?search=" + playerName.Replace(" ", "+");


            var counter = 0;
            foreach (var id in ids)
            {
                if (counter >= 3)
                {
                    counter++;
                    continue;
                }

                var player = new Player(playerName);

                url = "https://scoresaber.com" + ids[counter];
                using (var client = new HttpClient())
                {
                    var html = await client.GetStringAsync(url);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var table = doc.DocumentNode.SelectSingleNode("//div[@class='columns']");

                    playerInfo = table.Descendants("div").Skip(1).Select(tr =>
                        tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", "")))
                            .ToList()).ToList();
                    playerInfo2 = table.Descendants("div").Skip(1).Select(tr =>
                        tr.Descendants("li").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList()).ToList();
                    playerImg = table.Descendants("div").Select(tr =>
                        tr.Descendants("img").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("src", "")))
                            .ToList()).ToList();
                }

                var rank = playerInfo2.First()[0].Replace("\r\n", "").Trim();
                var ranks = rank.Split('-');

                player.rank =
                    int.Parse(ranks[0].Split("/")[0].Replace("Player Ranking: #", "").Replace(",", "").Trim());
                player.steamLink = playerInfo.First()[0];
                player.countryName = playerInfo.First()[2].Replace("/global?country=", "").ToUpper();
                player.countryRank =
                    int.Parse(ranks[1].Replace("(", "").Replace(")", "").Replace("#", "").Replace(",", ""));
                var pp = playerInfo2.First()[1].Replace("\r\n", "").Replace("Performance Points: ", "").Replace(",", "")
                    .Replace("pp", "").Trim();
                player.pp = pp.Split('.')[0];
                player.playCount = int.Parse(playerInfo2.First()[2].Replace("\r\n", "").Replace(",", "")
                    .Replace("Play Count: ", "").Trim());
                player.totalScore = playerInfo2.First()[3].Replace("\r\n", "").Replace("Total Score: ", "").Trim();
                player.countryIcon = ":flag_" + player.countryName + ":";
                player.imgLink = playerImg.First().First();
                player.name = player.name;
                player.ReplaysWatched = int.Parse(playerInfo2.First()[4].Replace("\r\n", "")
                    .Replace("Replays Watched by Others: ", "").Trim());

                //player.scoresaberLink = url;

                var nextAndBefore = await RankedNeighbours(playerName, player.rank, 1);
                var playerNext = new Player(nextAndBefore.Item1)
                {
                    pp = await GetPlayerPP(nextAndBefore.Item1)
                };
                var playerBefore = new Player(nextAndBefore.Item2)
                {
                    pp = await GetPlayerPP(nextAndBefore.Item2)
                };

                try
                {
                    player.Next = playerNext;
                    player.Before = playerBefore;
                }
                catch
                {
                    Console.WriteLine(nextAndBefore.Item1 + " or " + nextAndBefore.Item2 + " is not found");
                }

                players.Add(player);
                counter++;
            }
        }

        public static async Task<string> GetPlayerPP(string Name)
        {
            var url = "https://scoresaber.com/global?search=" + Name;
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var PP = doc.DocumentNode.SelectSingleNode("//span[@class='scoreTop ppValue']");
                if (PP == null)
                    return "0";
                var pp = PP.InnerText.Replace("\r\n", "").Replace("Performance Points: ", "").Replace(",", "")
                    .Replace("pp", "").Trim();
                return pp.Split('.')[0];
            }
        }

        public static async Task<EmbedBuilder> GetComparedEmbedBuilder(string message, SocketMessage socketMessage, DiscordSocketClient discordSocketClient)
        {
            //Check if the message is set up correctly
            if (message.Length == 0 || message == null) return EmbedBuilderExtension.NullEmbed("Format is not set up correctly", "Use the following format: !bs compare player1 player2");

            //Prepare player data             
            var players = message.Split(' ');

            if (players.Count() > 2) return EmbedBuilderExtension.NullEmbed("oh oh...", $"One of your inputs contains a space. Connect the name as the following: !bs compare silverhaze Duh<>Hello");

            var player1 = players[0].Replace("<>", " ");
            var player2 = players[1].Replace("<>", " ");

            var player1containsmention = false;
            var player2containsmention = false;

            if (player1.Contains("@"))
            {
                var r = new RoleAssignment(discordSocketClient);
                var discordId = player1.Replace("<@!", "").Replace(">", "");
                if (await r.CheckIfDiscordIdIsLinked(discordId))
                {
                    player1 = await r.GetScoresaberIdWithDiscordId(discordId);
                    player1containsmention = true;
                }
                else
                {
                    return EmbedBuilderExtension.NullEmbed("Not Linked error", $"{player1} is not linked with his/her scoresaber");
                }
            }

            if (player2.Contains("@"))
            {
                var r = new RoleAssignment(discordSocketClient);
                var discordId = player2.Replace("<@!", "").Replace(">", "");
                if (await r.CheckIfDiscordIdIsLinked(discordId))
                {
                    player2 = await r.GetScoresaberIdWithDiscordId(discordId);
                    player2containsmention = true;
                }
                else
                {
                    return EmbedBuilderExtension.NullEmbed("Not Linked error", $"{player2} is not linked with his/her scoresaber");
                }
            }

            var urlPlayer1 = $"https://new.scoresaber.com/api/players/by-name/{player1}";
            var urlPlayer2 = $"https://new.scoresaber.com/api/players/by-name/{player2}";

            var player1Info = new ScoresaberPlayerFullModel();
            var player2Info = new ScoresaberPlayerFullModel();

            using (HttpClient hc = new HttpClient())
            {
                var player1ScoresaberID = player1;
                var player2ScoresaberID = player2;

                if (!player1containsmention)
                {
                    var infoPlayer1Raw = await hc.GetAsync(urlPlayer1);
                    if (infoPlayer1Raw.StatusCode != HttpStatusCode.OK) return EmbedBuilderExtension.NullEmbed("Scoresaber Error", $"**Player 1 status:** {infoPlayer1Raw.StatusCode}");
                    var players1ScoresaberID = JsonConvert.DeserializeObject<List<ScoreSaberSearchByNameModel>>(infoPlayer1Raw.Content.ReadAsStringAsync().Result);
                    var player1search = players1ScoresaberID.Where(x => x.Name.ToLower() == player1.ToLower());
                    if (player1search.Count() == 0) return EmbedBuilderExtension.NullEmbed("Error", $"{player1} could not be found");
                    player1ScoresaberID = players1ScoresaberID.Where(x => x.Name.ToLower() == player1.ToLower()).First().Playerid;
                }

                if (!player2containsmention)
                {
                    var infoPlayer2Raw = await hc.GetAsync(urlPlayer2);
                    if (infoPlayer2Raw.StatusCode != HttpStatusCode.OK) return EmbedBuilderExtension.NullEmbed("Scoresaber Error", $"**Player 2 status:** {infoPlayer2Raw.StatusCode}");
                    var players2ScoresaberID = JsonConvert.DeserializeObject<List<ScoreSaberSearchByNameModel>>(infoPlayer2Raw.Content.ReadAsStringAsync().Result);
                    var player2search = players2ScoresaberID.Where(x => x.Name.ToLower() == player2.ToLower());
                    if (player2search.Count() == 0) return EmbedBuilderExtension.NullEmbed("Error", $"{player2} could not be found");
                    player2ScoresaberID = players2ScoresaberID.Where(x => x.Name.ToLower() == player2.ToLower()).First().Playerid;
                }

                //GET players info 
                var urlPlayerInfo1 = $"https://new.scoresaber.com/api/player/{player1ScoresaberID}/full";
                var player1InfoRaw = await hc.GetStringAsync(urlPlayerInfo1);
                player1Info = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(player1InfoRaw);

                var urlPlayerInfo2 = $"https://new.scoresaber.com/api/player/{player2ScoresaberID}/full";
                var player2InfoRaw = await hc.GetStringAsync(urlPlayerInfo2);
                player2Info = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(player2InfoRaw);
            }

            var embedBuilder = new EmbedBuilder
            {
                Title = $"**Compare Info from: \n{player1Info.playerInfo.Name} :flag_{player1Info.playerInfo.Country.ToLower()}:   &   {player2Info.playerInfo.Name} :flag_{player2Info.playerInfo.Country.ToLower()}:**",
            };

            embedBuilder.AddField($"\n\nDifference:", $"" +
                $"```md\n" +
                $"Yellow: < {player1Info.playerInfo.Name}> \nBlue:   <{ player2Info.playerInfo.Name}>\n\n" +
                $"Rank:                 {getRedOrGreenAndValue(player1Info.playerInfo.rank, player2Info.playerInfo.rank, true)} \n" +
                $"PP:                   {getRedOrGreenAndValue(Math.Round(player1Info.playerInfo.Pp), Math.Round(player2Info.playerInfo.Pp))} \n" +
                $"Accuracy:             {getRedOrGreenAndValue((float)player1Info.scoreStats.AvarageRankedAccuracy, (float)player2Info.scoreStats.AvarageRankedAccuracy)} \n" +
                $"```" +

                $"```md\n" +
                $"TotalPlayCount:       {getRedOrGreenAndValue(player1Info.scoreStats.TotalPlayCount, player2Info.scoreStats.TotalPlayCount)} \n" +
                $"TotalRankedPlayCount: {getRedOrGreenAndValue(player1Info.scoreStats.RankedPlayerCount, player2Info.scoreStats.RankedPlayerCount)} \n" +
                $"TotalScore:           {getRedOrGreenAndValue((float)player1Info.scoreStats.TotalScore, (float)player2Info.scoreStats.TotalScore)} \n" +
                $"TotalRankedScore:     {getRedOrGreenAndValue((float)player1Info.scoreStats.TotalRankedScore, (float)player2Info.scoreStats.TotalRankedScore)} \n" +
                $"```"
                +
                $"```md\n" +
                $"CountryRank:          {getRedOrGreenAndValue(player1Info.playerInfo.CountryRank, player2Info.playerInfo.CountryRank, true)} \n" +
                $"BadgeCount:           {getRedOrGreenAndValue(player1Info.playerInfo.Badges.Count(), player2Info.playerInfo.Badges.Count())} \n" +
                $"```"
            );

            string getRedOrGreenAndValue(dynamic value, dynamic value2, bool shouldBeLower = false)
            {
                var color = "<";
                bool isYellow = false;

                if (shouldBeLower)
                {
                    if (Math.Min(value, value2) == value)
                    {
                        color = "< ";
                        isYellow = true;
                    }
                }
                else
                {
                    if (Math.Max(value, value2) == value)
                    {
                        color = "< ";
                        isYellow = true;
                    }
                }

                var endResult = Math.Abs(value - value2);

                try
                {
                    endResult = Math.Round(endResult, 2);
                }
                catch
                {

                }

                string v = value.ToString("#,##0,,M", CultureInfo.InvariantCulture);
                if (value.ToString().Length > 7)
                {
                    value = v;
                }

                string v2 = value2.ToString("#,##0,,M", CultureInfo.InvariantCulture);
                if (value2.ToString().Length > 7)
                {
                    value2 = v2;
                }

                string e2 = endResult.ToString("#,##0,,M", CultureInfo.InvariantCulture);
                if (endResult.ToString().Length > 7)
                {
                    endResult = e2;
                }

                return $"< {getValueWithSpacingAfter(value.ToString())}>    {color}{getValueWithSpacingAfter(endResult.ToString(), isYellow)}>     <{getValueWithSpacingAfter(value2.ToString())}>";
            }

            string getValueWithSpacingAfter(string value, bool isYellow = false)
            {
                var spaces = "";

                for (var x = 6; x > value.Length; x--)
                {
                    spaces += " ";
                }

                if (!isYellow) spaces += " ";

                return value + spaces;
            }



            return embedBuilder;
        }

        public static async Task GetAndCreateUserCardImage(string scoresaberId, string topic)
        {
            var playerRaw = new ScoresaberAPI(scoresaberId);
            var playerData = await playerRaw.GetPlayerFull();

            var player = playerData.playerInfo;
            var playerStats = playerData.scoreStats;

            var rankingCardCreator = new ImageCreator("../../../Resources/img/UserCard-Template.png");

            //Add player main info
            rankingCardCreator.AddText(player.Name.ToUpper(), System.Drawing.Color.White, 20, 120, 10);
            rankingCardCreator.AddText(player.Country.ToUpper(), System.Drawing.Color.White, 12, 120, 45);
            var rankWidth = rankingCardCreator.AddText($"#{player.rank}", System.Drawing.Color.White, 12, 120, 68).Width;
            rankingCardCreator.AddText($"#{player.CountryRank}", System.Drawing.Color.FromArgb(176, 176, 176), 8, 120 + rankWidth, 72);
            rankingCardCreator.AddText($"{player.Pp}PP", System.Drawing.Color.White, 15, 120, 88);
            rankingCardCreator.AddText(topic, System.Drawing.Color.White, 15, 340, 88);

            rankingCardCreator.AddImage($"https://scoresaber.com/imports/images/flags/{player.Country}.png", 150, 50, 20, 15);
            rankingCardCreator.AddImage($"https://new.scoresaber.com{player.Avatar}", 15, 13, 100, 100);            

            //Finish Card
            rankingCardCreator.Create("../../../Resources/img/UserCard.png");
        }

        public static async Task GetAndCreateRecentsongsCardImage(string scoresaberId)
        {
            var playerRaw = new ScoresaberAPI(scoresaberId);
            var playerRecentScores = await playerRaw.GetScoresRecent();
            

            var rankingCardCreator = new ImageCreator("../../../Resources/img/RecentsongsCard-Template.png");

            //Add SongInfo
            var marigin = 0;
            for (var x = 0; x < 5; x++)
            {
                rankingCardCreator.AddText($"{playerRecentScores.Scores[x].Name}", System.Drawing.Color.White, 50, 320, marigin + 40);

                var rankcolor = System.Drawing.Color.Gray;
                if (playerRecentScores.Scores[x].Rank == 1) rankcolor = System.Drawing.Color.Goldenrod;
                if (playerRecentScores.Scores[x].Rank == 2) rankcolor = System.Drawing.Color.Silver;
                if (playerRecentScores.Scores[x].Rank == 3) rankcolor = System.Drawing.Color.SaddleBrown;

                rankingCardCreator.AddText($"#{playerRecentScores.Scores[x].Rank}", rankcolor, 50, 320, marigin + 135);

                if (playerRecentScores.Scores[x].Pp > 0)
                {
                    double percentage = Convert.ToDouble(playerRecentScores.Scores[x].UScore) / Convert.ToDouble(playerRecentScores.Scores[x].MaxScoreEx) * 100;
                    var acc = Math.Round(percentage, 2);
                    rankingCardCreator.AddText($"{acc}%", rankcolor, 50, 520, marigin + 135);
                }
                

                var ppfontsize = 60;
                if (playerRecentScores.Scores[x].Pp > 300) ppfontsize = 65;
                if (playerRecentScores.Scores[x].Pp > 400) ppfontsize = 70;
                if (playerRecentScores.Scores[x].Pp > 500) ppfontsize = 80;

                if (playerRecentScores.Scores[x].Pp != 0) rankingCardCreator.AddText($"+ {Math.Round(playerRecentScores.Scores[x].Pp,2)}PP", System.Drawing.Color.Green, ppfontsize, 1320, marigin + 120);
                rankingCardCreator.AddText($"{playerRecentScores.Scores[x].GetDifficulty()}", System.Drawing.Color.White, 30, 80, marigin + 255);
                rankingCardCreator.AddText($"Time set: {playerRecentScores.Scores[x].Timeset.DateTime.ToShortDateString()} {playerRecentScores.Scores[x].Timeset.DateTime.ToShortTimeString()}     {Math.Round((DateTime.Now - playerRecentScores.Scores[x].Timeset).TotalDays, 1)} days ago", System.Drawing.Color.Gray, 30, 1000, marigin + 260);

                rankingCardCreator.AddImageRounded($"https://new.scoresaber.com/api/static/covers/{playerRecentScores.Scores[x].Id}.png", 15, marigin, 250, 250);
                marigin += 330 - (x * 3);
            }
            
            //Finish Card
            rankingCardCreator.Create("../../../Resources/img/RecentsongsCard.png");
        }

        public static async Task GetAndCreateTopsongsCardImage(string scoresaberId)
        {
            var playerRaw = new ScoresaberAPI(scoresaberId);
            var playerTopScores = await playerRaw.GetTopScores();


            var rankingCardCreator = new ImageCreator("../../../Resources/img/RecentsongsCard-Template.png");

            //Add SongInfo
            var marigin = 0;
            for (var x = 0; x < 5; x++)
            {
                rankingCardCreator.AddText($"{playerTopScores.Scores[x].Name}", System.Drawing.Color.White, 50, 320, marigin + 40);

                var rankcolor = System.Drawing.Color.Gray;
                if (playerTopScores.Scores[x].Rank == 1) rankcolor = System.Drawing.Color.Goldenrod;
                if (playerTopScores.Scores[x].Rank == 2) rankcolor = System.Drawing.Color.Silver;
                if (playerTopScores.Scores[x].Rank == 3) rankcolor = System.Drawing.Color.SaddleBrown;

                rankingCardCreator.AddText($"#{playerTopScores.Scores[x].Rank}", rankcolor, 50, 320, marigin + 135);

                if (playerTopScores.Scores[x].Pp > 0)
                {
                    double percentage = Convert.ToDouble(playerTopScores.Scores[x].UScore) / Convert.ToDouble(playerTopScores.Scores[x].MaxScoreEx) * 100;
                    var acc = Math.Round(percentage, 2);
                    rankingCardCreator.AddText($"{acc}%", rankcolor, 50, 520, marigin + 135);
                }


                var ppfontsize = 60;
                if (playerTopScores.Scores[x].Pp > 300) ppfontsize = 65;
                if (playerTopScores.Scores[x].Pp > 400) ppfontsize = 70;
                if (playerTopScores.Scores[x].Pp > 500) ppfontsize = 80;

                if (playerTopScores.Scores[x].Pp != 0) rankingCardCreator.AddText($"+ {Math.Round(playerTopScores.Scores[x].Pp, 2)}PP", System.Drawing.Color.Green, ppfontsize, 1320, marigin + 120);
                rankingCardCreator.AddText($"{playerTopScores.Scores[x].GetDifficulty()}", System.Drawing.Color.White, 30, 80, marigin + 255);
                rankingCardCreator.AddText($"Time set: {playerTopScores.Scores[x].Timeset.DateTime.ToShortDateString()} {playerTopScores.Scores[x].Timeset.DateTime.ToShortTimeString()}     {Math.Round((DateTime.Now - playerTopScores.Scores[x].Timeset).TotalDays, 1)} days ago", System.Drawing.Color.Gray, 30, 1000, marigin + 260);

                rankingCardCreator.AddImageRounded($"https://new.scoresaber.com/api/static/covers/{playerTopScores.Scores[x].Id}.png", 15, marigin, 250, 250);
                marigin += 330 - (x * 3);
            }

            //Finish Card
            rankingCardCreator.Create("../../../Resources/img/RecentsongsCard.png");
        }

        public static async Task GetAndCreateProfileImage(string scoresaberId)
        {
            var playerRaw = new ScoresaberAPI(scoresaberId);
            var playerData = await playerRaw.GetPlayerFull();
            var playerScoresData = await playerRaw.GetTopScores();
            var player = playerData.playerInfo;
            var playerStats = playerData.scoreStats;
            var playerTopStats = playerScoresData.Scores[0];
            

            var rankingCardCreator = new ImageCreator("../../../Resources/img/RankingCard-Template.png");
            //Add player main info

            rankingCardCreator.AddText(player.Name.ToUpper(), System.Drawing.Color.White, 100, 1100, 800);
            rankingCardCreator.AddText(player.Country.ToUpper(), System.Drawing.Color.White, 100, 1250, 950);
            var rankWidth = rankingCardCreator.AddText($"#{player.rank}", System.Drawing.Color.White, 100, 1100, 1200).Width;
            rankingCardCreator.AddText($"#{player.CountryRank}", System.Drawing.Color.FromArgb(176, 176, 176), 60, 1080 + rankWidth, 1250);
            rankingCardCreator.AddText($"{player.Pp}PP", System.Drawing.Color.White, 100, 1100, 1350);

            rankingCardCreator.AddImage($"https://scoresaber.com/imports/images/flags/{player.Country}.png", 1120, 1000, 120, 100);
            rankingCardCreator.AddNoteSlashEffect($"https://new.scoresaber.com{player.Avatar}", 200, 800, 800, 800);

            //Add Date
            rankingCardCreator.AddText(DateTime.UtcNow.ToString("dd MMM. yyyy"), System.Drawing.Color.White, 100, 3600, 650);
            rankingCardCreator.AddText(DateTime.UtcNow.ToString("HH:mm"), System.Drawing.Color.FromArgb(176, 176, 176), 70, 3950, 800);

            var customNumberSeperator = new NumberFormatInfo { NumberGroupSeparator = " " };

            //Add scorestats 
            rankingCardCreator.AddText("ACC.", System.Drawing.Color.FromArgb(251, 211, 64), 90, 3350, 1140);
            rankingCardCreator.AddText($"{playerStats.AvarageRankedAccuracy}%", System.Drawing.Color.FromArgb(251, 211, 64), 100, 3750, 1125);

            rankingCardCreator.AddText("SCORE", System.Drawing.Color.FromArgb(251, 211, 64), 90, 3350, 1300);
            rankingCardCreator.AddText(playerStats.TotalScore.ToString("n", customNumberSeperator).Split('.')[0], System.Drawing.Color.FromArgb(251, 211, 64), 110, 3750, 1280);

            rankingCardCreator.AddText("RANKED", System.Drawing.Color.FromArgb(251, 211, 64), 40, 3370, 1470);
            rankingCardCreator.AddText(playerStats.TotalRankedScore.ToString("n", customNumberSeperator).Split('.')[0], System.Drawing.Color.FromArgb(251, 211, 64), 60, 3760, 1450);

            rankingCardCreator.AddText("PLAYS", System.Drawing.Color.FromArgb(251, 211, 64), 90, 3350, 1560);
            rankingCardCreator.AddText(playerStats.TotalPlayCount.ToString("n", customNumberSeperator).Split('.')[0], System.Drawing.Color.FromArgb(251, 211, 64), 110, 3750, 1540);

            rankingCardCreator.AddText("RANKED", System.Drawing.Color.FromArgb(251, 211, 64), 40, 3370, 1720);
            rankingCardCreator.AddText(playerStats.RankedPlayerCount.ToString("n", customNumberSeperator).Split('.')[0], System.Drawing.Color.FromArgb(251, 211, 64), 60, 3760, 1700);

            //Add best score
            var nameFontSize = 130;
            if (playerTopStats.Name.Length >= 25) { 
                nameFontSize = 70;
                var spaces = playerTopStats.Name.Split(" ");

                var firstname = "";
                var lastname = "";
                var splitcount = 0;
                var length = 0;
                foreach(var space in spaces)
                {
                    if (length > 25)
                    {
                        lastname += space + " ";
                        continue;
                    }
                    else
                    {
                        splitcount++;
                        firstname += space + " ";
                        length += space.Length;
                    }
                    
                    
                }
                rankingCardCreator.AddText(firstname, System.Drawing.Color.White, nameFontSize, 100, 2300);
                rankingCardCreator.AddText(lastname, System.Drawing.Color.White, nameFontSize, 100, 2415);

            }else if (playerTopStats.Name.Length >= 52)
            {
                rankingCardCreator.AddText("Fuck this map name", System.Drawing.Color.White, nameFontSize, 100, 2300);
            }
            else
            {
                rankingCardCreator.AddText(playerTopStats.Name, System.Drawing.Color.White, nameFontSize, 100, 2300);
            }
            rankingCardCreator.AddText(playerTopStats.GetDifficulty().Replace("Plus", "+"), System.Drawing.Color.FromArgb(176, 176, 176), 110, 100, 2500);
            rankingCardCreator.AddText($"{playerTopStats.Pp.ToString("0.00")}PP", System.Drawing.Color.White, 160, 2950, 2270);
            rankingCardCreator.AddText($"{Math.Round(Convert.ToDouble(playerTopStats.UScore) / Convert.ToDouble(playerTopStats.MaxScoreEx) * 100, 3).ToString("0.00")}%", System.Drawing.Color.FromArgb(176, 176, 176), 110, 3320, 2500);
            rankingCardCreator.AddImageRounded($"https://new.scoresaber.com/api/static/covers/{playerTopStats.Id}.png", 3950, 2020, 800, 800);

            rankingCardCreator.Create("../../../Resources/img/RankingCard.png");         
        }

        public static async Task<EmbedBuilder> GetImprovableMapsByAccFromToplist(string scoresaberId, double wishedAcc)
        {
            var playerTopPageList = new List<ScoresaberSongsModel>();

            using (var client = new HttpClient())
            {
                for (var x = 1; x <= 8; x++)
                {
                    var url = $"https://new.scoresaber.com/api/player/{scoresaberId}/scores/top/{x}";
                    var httpCall = await client.GetAsync(url);
                    if (httpCall.StatusCode != HttpStatusCode.OK) return EmbedBuilderExtension.NullEmbed("Scoresaber Error", $"**Cant find maps on page:** {x}");
                    playerTopPageList.Add(JsonConvert.DeserializeObject<ScoresaberSongsModel>(httpCall.Content.ReadAsStringAsync().Result));
                }
            }

            var playerTopList = new List<Score>();
            foreach (var topModel in playerTopPageList)
            {
                foreach (var map in topModel.Scores)
                {
                    playerTopList.Add(map);
                }
            }

            var ppleftList = new Dictionary<string, double>();
            var ppAndCurrentWeightList = new Dictionary<double, double>();
            var mapsAccList = new Dictionary<string, double>();

            foreach (var map in playerTopList)
            {


                double percentage = Convert.ToDouble(map.UScore) / Convert.ToDouble(map.MaxScoreEx) * 100;
                var acc = Math.Round(percentage, 2);

                if (acc >= wishedAcc) continue;
                var name = map.Name;
                double wishedPp = 0;

                wishedPp = PpCalculator.GetPpFromWishedAccByCurrentPpAndAcc(acc, map.UScore, map.MaxScoreEx, wishedAcc);

                //Adding weight
                ppAndCurrentWeightList.Add(map.Pp, map.Weight);

                var wishedRank = 1;
                for (var x = 0; x < playerTopList.Count(); x++)
                {
                    if (wishedPp + map.Pp < playerTopList[x].Pp)
                    {
                        wishedRank = x;
                    }
                }

                var wishedPpWeight = Math.Pow(0.965, wishedRank - 1);//0.965 ^ (n - 1)
                var wishedPpWeighted = wishedPp * wishedPpWeight;

                try
                {
                    ppleftList.Add($"**{map.Name} ({map.GetDifficulty()})** \n" +
                        $"Pp for current acc {acc}%: {map.Pp}pp \n" +
                        $"Pp for {wishedAcc}%: {Math.Round(wishedPp + map.Pp, 2)}pp \n" +
                        $"Pp gain: ", Math.Round(wishedPpWeighted, 2));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                mapsAccList.Add($"**{map.Name} ({map.GetDifficulty()})** \n" +
                        $"Pp for current acc {acc}%: {map.Pp}pp \n" +
                        $"Pp for {wishedAcc}%: {Math.Round(wishedPp + map.Pp, 2)}pp \n" +
                        $"Pp gain: ", acc);
            }

            double avgAcc = 0;
            for (var x = 0; x < mapsAccList.Count(); x++)
            {
                avgAcc += mapsAccList.Values.ToArray()[x];
            }
            avgAcc = avgAcc / mapsAccList.Count();

            List<double> sortedPpLeftList = ppleftList.Values.OrderByDescending(d => d).ToList();
            var MapMessage = "";
            var messageCount = 0;
            for (var x = 0; x < sortedPpLeftList.Count(); x++)
            {
                if (x >= sortedPpLeftList.Count()) break;
                var f = ppleftList.First(d => d.Value == sortedPpLeftList[x]).Key;
                var g = mapsAccList.GetValueOrDefault(f);
                if (g < (avgAcc)) { continue; };
                //if (x >= 10) break;
                if (messageCount >= 10) break;
                messageCount += 1;
                MapMessage += $"{ppleftList.First(d => d.Value == sortedPpLeftList[x]).Key}: **{sortedPpLeftList[x]}pp** \n\n";
            }

            return new EmbedBuilder()
            {
                Title = "To Improve",
                Description = $"Average acc: **{Math.Round(avgAcc, 2)}** \nAvg acc is from maps lower than the wished acc, from the player's top {playerTopList.Count()} maps. \nMaps with a current acc lower than the avg acc are considered 'not passable'. \n\n" + MapMessage,
                Footer = new EmbedFooterBuilder() { Text = "The values are currently not correct, this is still just a test function" }
            };


        }
    }
}