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
using DiscordBeatSaberBot.Api.Spotify;
using System.IO;
using DiscordBeatSaberBot.Api.BeatSaverApi;
using DiscordBeatSaberBot.Api.BeatSaviourApi;
using DiscordBeatSaberBot.Commands.Functions;

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
            builder.Description = $"[Invite the bot to your server]({GlobalConfiguration.inviteLink})\n\n[Join the bots discord server](https://discord.gg/S3D3Yyu)";
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

        public static async Task GetAndPostMapInfoWithKey(SocketMessage message, string key)
        {
            var embedBuilder = new EmbedBuilder();

            using (var client = new HttpClient())
            {

                //Download beatsaver recentsong data
                var mapInfoBeatSaver = await BeatSaverApi.GetMapByKey(key);
                if (mapInfoBeatSaver == null)
                {
                    await message.Channel.SendMessageAsync("oh oh, beat saver died or the input is not correct. try again.");
                    return;
                }

                var difficultyToGetDataFrom = mapInfoBeatSaver.Metadata.Difficulties.Easy ? "Easy" : mapInfoBeatSaver.Metadata.Difficulties.Normal ? "Normal" : mapInfoBeatSaver.Metadata.Difficulties.Hard ? "Hard" : mapInfoBeatSaver.Metadata.Difficulties.Expert ? "Expert" : mapInfoBeatSaver.Metadata.Difficulties.ExpertPlus ? "ExpertPlus" : "None";
                var metadataDynamic = mapInfoBeatSaver.Metadata.Characteristics.First(x => x.Name == "Standard").Difficulties;
                dynamic difficulty = metadataDynamic.GetType().GetProperty(difficultyToGetDataFrom).GetValue(metadataDynamic, null);

                var cardCreator = new ImageCreator("../../../Resources/img/EmbedBackground-Template.png");
                cardCreator.AddImage($"https://scoresaber.com/imports/images/songs/{mapInfoBeatSaver.Hash.ToUpper()}.png", 0, 0, 1080, 720, 0.1f);

                cardCreator.AddText($"Lenght:", System.Drawing.Color.White, 24, 50, 50);
                cardCreator.AddText($"{(difficultyToGetDataFrom == "ExpertPlus" ? metadataDynamic.ExpertPlus.Length : difficulty.lenght)}", System.Drawing.Color.White, 24, 250, 50);

                cardCreator.AddText($"BPM:", System.Drawing.Color.White, 24, 50, 100);
                cardCreator.AddText($"{mapInfoBeatSaver.Metadata.Bpm}", System.Drawing.Color.White, 24, 250, 100);

                cardCreator.AddText($"NJS:", System.Drawing.Color.White, 24, 50, 150);
                cardCreator.AddText($"{ (difficultyToGetDataFrom == "ExpertPlus" ? metadataDynamic.ExpertPlus.Njs : difficulty.njs)}", System.Drawing.Color.White, 24, 250, 150);

                cardCreator.AddText($"Notes:", System.Drawing.Color.White, 24, 50, 200);
                cardCreator.AddText($"{ (difficultyToGetDataFrom == "ExpertPlus" ? metadataDynamic.ExpertPlus.Notes : difficulty.notes)}", System.Drawing.Color.White, 24, 250, 200);

                //Right side
                cardCreator.AddTextFloatRight($"Downloads:", System.Drawing.Color.White, 24, 230, 50);
                cardCreator.AddTextFloatRight($"{mapInfoBeatSaver.Stats.Downloads}", System.Drawing.Color.White, 24, 50, 50);

                cardCreator.AddTextFloatRight($"Upvotes:", System.Drawing.Color.White, 24, 230, 100);
                cardCreator.AddTextFloatRight($"{mapInfoBeatSaver.Stats.UpVotes}", System.Drawing.Color.White, 24, 50, 100);

                cardCreator.AddTextFloatRight($"Downvotes:", System.Drawing.Color.White, 24, 230, 150);
                cardCreator.AddTextFloatRight($"{mapInfoBeatSaver.Stats.DownVotes}", System.Drawing.Color.White, 24, 50, 150);

                cardCreator.AddTextFloatRight($"Ratio:", System.Drawing.Color.White, 24, 230, 200);
                cardCreator.AddTextFloatRight($"{Math.Round(100 * mapInfoBeatSaver.Stats.Rating, 2)}%", System.Drawing.Color.White, 24, 50, 200);

                cardCreator.AddTextFloatRight($"Plays:", System.Drawing.Color.White, 24, 230, 250);
                cardCreator.AddTextFloatRight($"{mapInfoBeatSaver.Stats.Plays}", System.Drawing.Color.White, 24, 50, 250);

                cardCreator.AddTextFloatRight($"Upload Date:", System.Drawing.Color.White, 24, 230, 300);
                cardCreator.AddTextFloatRight($"{mapInfoBeatSaver.Uploaded.UtcDateTime.ToShortDateString()}", System.Drawing.Color.White, 24, 50, 300);

                //Add available difficulties
                cardCreator.AddTextWithBackGround("Easy", System.Drawing.Color.White, 24, mapInfoBeatSaver.Metadata.Difficulties.Easy ? System.Drawing.Color.FromArgb(60, 179, 113) : System.Drawing.Color.LightGray, 50, difficultyToGetDataFrom == "Easy" ? 630 - 15 : 630);
                cardCreator.AddTextWithBackGround("Normal", System.Drawing.Color.White, 24, mapInfoBeatSaver.Metadata.Difficulties.Normal ? System.Drawing.Color.FromArgb(89, 176, 244) : System.Drawing.Color.LightGray, 180, difficultyToGetDataFrom == "Normal" ? 630 - 15 : 630);
                cardCreator.AddTextWithBackGround("Hard", System.Drawing.Color.White, 24, mapInfoBeatSaver.Metadata.Difficulties.Hard ? System.Drawing.Color.FromArgb(254, 99, 71) : System.Drawing.Color.LightGray, 365, difficultyToGetDataFrom == "Hard" ? 630 - 15 : 630);
                cardCreator.AddTextWithBackGround("Expert", System.Drawing.Color.White, 24, mapInfoBeatSaver.Metadata.Difficulties.Expert ? System.Drawing.Color.FromArgb(192, 42, 66) : System.Drawing.Color.LightGray, 500, difficultyToGetDataFrom == "Expert" ? 630 - 15 : 630);
                cardCreator.AddTextWithBackGround("Expert+", System.Drawing.Color.White, 24, mapInfoBeatSaver.Metadata.Difficulties.ExpertPlus ? System.Drawing.Color.FromArgb(143, 72, 219) : System.Drawing.Color.LightGray, 670, difficultyToGetDataFrom == "ExpertPlus" ? 630 - 15 : 630);

                await cardCreator.Create($"../../../Resources/img/EmbedBackground-{key}.png");

                embedBuilder = new EmbedBuilder
                {
                    Title = $"**{mapInfoBeatSaver.Name} by {mapInfoBeatSaver.Metadata.LevelAuthorName}**",
                    ImageUrl = $"attachment://EmbedBackground-{key}.png",
                    ThumbnailUrl = $"https://beatsaver.com/cdn/{mapInfoBeatSaver.Key}/{mapInfoBeatSaver.Hash}.jpg",
                    Footer = new EmbedFooterBuilder() { Text = $"Hash: {mapInfoBeatSaver.Hash}\nID: {mapInfoBeatSaver.Id}\nKey: {mapInfoBeatSaver.Key}" }
                };

                embedBuilder.AddField("Links",
                    $"\n[Map link (BeatSaver)](https://beatsaver.com/beatmap/{mapInfoBeatSaver.Key})" +
                    $"\n[Mapper: {mapInfoBeatSaver.Uploader.Username}](https://beatsaver.com/uploader/{mapInfoBeatSaver.Uploader.Id})" +
                    $"\n[Image](https://beatsaver.com/{mapInfoBeatSaver.CoverUrl})");

             

                embedBuilder.AddField( "Description", "\n" +
                  //$"Available Difficulties: " +
                  //$"{(mapInfoBeatSaver.Metadata.Difficulties.Easy.ToString() == "False" ? "" : "Easy - ")}" +
                  //$"{(mapInfoBeatSaver.Metadata.Difficulties.Normal.ToString() == "False" ? "" : "Normal - ")}" +
                  //$"{(mapInfoBeatSaver.Metadata.Difficulties.Hard.ToString() == "False" ? "" : "Hard - ")}" +
                  //$"{(mapInfoBeatSaver.Metadata.Difficulties.Expert.ToString() == "False" ? "" : "Expert - ")}" +
                  //$"{(mapInfoBeatSaver.Metadata.Difficulties.ExpertPlus.ToString() == "False" ? "" : "Expert+")}" +
                  mapInfoBeatSaver.Description + 
                  "\n" +
                  $"[Download Map](https://beatsaver.com{mapInfoBeatSaver?.DirectDownload}) - " +
                  $"[Preview Map](https://skystudioapps.com/bs-viewer/?id={mapInfoBeatSaver?.Key}) - " +
                  $"[Song on Spotify]({await new Spotify().SearchItem(mapInfoBeatSaver.Metadata.SongName, mapInfoBeatSaver.Metadata.SongAuthorName)})");



                await message.Channel.SendFileAsync($"../../../Resources/img/EmbedBackground-{key}.png", embed: embedBuilder.Build());
                File.Delete($"../../../Resources/img/EmbedBackground-{key}.png");
            }
        }

        public static async Task GetAndPostRecentSongWithScoresaberIdNew(string playerId, SocketMessage message, int recentsongNr = 1)
        {
            var embedBuilder = new EmbedBuilder();

            using (var client = new HttpClient())
            {
                var scoresaberApi = new ScoresaberAPI(playerId);
                var beatSaviourApi = new BeatSaviourApi(playerId);

                //Download scoresaber recentsong data
                var recentSong = await scoresaberApi.GetScoresRecent(recentsongNr);

                //Download beatsaver recentsong data
                var recentSongsInfoBeatSaver = await new BeatSaverApi(recentSong.Id).GetRecentSongData();

                //Download scoresaber full player data
                var playerFullData = await scoresaberApi.GetPlayerFull();
                var playerInfo = playerFullData.playerInfo;

                //Download BeatSaviour livedata 
                var playerMostRecentLiveData = await beatSaviourApi.GetMostRecentLiveData(recentSong.Id, recentSong.GetDifficulty());
                var hasBeatSaviour = playerMostRecentLiveData == null ? false : true;

                var cardCreator = new ImageCreator("../../../Resources/img/EmbedBackground-Template.png");
                cardCreator.AddImage($"https://scoresaber.com/imports/images/songs/{recentSong.Id}.png", 0, 0, 1080, 720, 0.1f);

                var maxScore = 0;
                if (recentSongsInfoBeatSaver != null)
                {
                    var metadataDynamic = recentSongsInfoBeatSaver.Metadata.Characteristics.First(x => x.Name == "Standard").Difficulties;
                    dynamic difficulty = metadataDynamic.GetType().GetProperty(recentSong.GetDifficulty()).GetValue(metadataDynamic, null);

                    cardCreator.AddText($"Bpm:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, 0);
                    cardCreator.AddText($"{recentSongsInfoBeatSaver.Metadata.Bpm}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, 0);

                    cardCreator.AddText($"Duration:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 30 : 40);
                    cardCreator.AddText($"{recentSongsInfoBeatSaver.Metadata.Duration}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 30 : 40);

                    cardCreator.AddText($"Notes:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 60 : 80);
                    cardCreator.AddText($"{(recentSong.GetDifficulty() == "ExpertPlus" ? metadataDynamic.ExpertPlus.Notes : difficulty.notes)}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 60 : 80);

                    cardCreator.AddText($"NJS:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 90 : 120);
                    cardCreator.AddText($"{(recentSong.GetDifficulty() == "ExpertPlus" ? metadataDynamic.ExpertPlus.Njs : difficulty.njs)}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 90 : 120);

                    cardCreator.AddText($"NJS offset:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 120 : 160);
                    cardCreator.AddText($"{(recentSong.GetDifficulty() == "ExpertPlus" ? metadataDynamic.ExpertPlus.NjsOffset : Math.Round(Convert.ToDouble(difficulty.njsOffset), 2))}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 120 : 160);

                    cardCreator.AddText($"Bombs:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 150 : 200);
                    cardCreator.AddText($"{(recentSong.GetDifficulty() == "ExpertPlus" ? metadataDynamic.ExpertPlus.Bombs : difficulty.bombs)}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 150 : 200);

                    cardCreator.AddText($"Obstacles:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 180 : 240);
                    cardCreator.AddText($"{(recentSong.GetDifficulty() == "ExpertPlus" ? metadataDynamic.ExpertPlus.Obstacles : difficulty.obstacles)}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 180 : 240);

                    cardCreator.AddText($"Max Score:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 210 : 280);
                    cardCreator.AddText($"{recentSong.MaxScoreEx}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 210 : 280);

                    cardCreator.AddText($"Mods:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 240 : 320);
                    cardCreator.AddText($"{recentSong.Mods}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 240 : 320);

                    var noteCount = (recentSong.GetDifficulty() == "ExpertPlus" ? metadataDynamic.ExpertPlus.Notes : difficulty.notes);
                    maxScore = (Convert.ToInt32(noteCount) - 13) * 920 + 4715;
                    //maxScore = Convert.ToInt32(noteCount) * 920;
                    if (maxScore < 0) maxScore = 0;

                }
                if (hasBeatSaviour)
                {
                    cardCreator.AddTextFloatRight($"#{recentSong.Rank}", System.Drawing.Color.White, 50, 500, 0);
                    cardCreator.AddTextFloatRight($"{recentSong.Pp}PP", System.Drawing.Color.White, 30, 600, 70);
                    cardCreator.AddTextFloatRight($"({Math.Round(recentSong.Pp * recentSong.Weight, 2)}PP)", System.Drawing.Color.White, 15, 500, 85);
                    cardCreator.AddTextFloatRight($"{Math.Round(Convert.ToDouble(recentSong.UScore) / Convert.ToDouble(recentSong.MaxScoreEx == 0 ? maxScore : recentSong.MaxScoreEx) * 100, 2)}%", System.Drawing.Color.White, 30, 500, 110);
                }
                else
                {
                    cardCreator.AddTextFloatRight($"#{recentSong.Rank}", System.Drawing.Color.White, 50, 0, 0);
                    cardCreator.AddTextFloatRight($"{recentSong.Pp}PP", System.Drawing.Color.White, 30, 100, 70);
                    cardCreator.AddTextFloatRight($"({Math.Round(recentSong.Pp * recentSong.Weight, 2)}PP)", System.Drawing.Color.White, 15, 0, 85);
                    cardCreator.AddTextFloatRight($"{Math.Round(Convert.ToDouble(recentSong.UScore) / Convert.ToDouble(recentSong.MaxScoreEx) * 100, 2)}%", System.Drawing.Color.White, 30, 0, 110);
                }

                //Create AccGrid If needed
                if (hasBeatSaviour)
                {
                    var PbIncrease = playerMostRecentLiveData.Trackers.ScoreTracker.PersonalBest == 0 ? "First Play" : $"{(playerMostRecentLiveData.Trackers.ScoreTracker.RawScore * 100) / playerMostRecentLiveData.Trackers.ScoreTracker.PersonalBest}% Increase";
                    cardCreator.AddTextFloatRight($"{PbIncrease}", System.Drawing.Color.White, 20, 500, 155);

                    var misses = "";
                    if (playerMostRecentLiveData.Trackers.HitTracker.Miss.ToString() == "0") misses = "FC";
                    else misses = playerMostRecentLiveData.Trackers.HitTracker.Miss.ToString();
                    cardCreator.AddText($"Misses:", System.Drawing.Color.Gray, 25, 0, 280);
                    cardCreator.AddText($"{misses}", System.Drawing.Color.White, 25, 240, 280);

                    cardCreator.AddText($"Pauses:", System.Drawing.Color.Gray, 25, 0, 320);
                    cardCreator.AddText($"{playerMostRecentLiveData.Trackers.WinTracker.NbOfPause}", System.Drawing.Color.White, 25, 240, 320);

                    cardCreator.AddText($"Max Combo:", System.Drawing.Color.Gray, 25, 0, 360);
                    cardCreator.AddText($"{playerMostRecentLiveData.Trackers.HitTracker.MaxCombo}", System.Drawing.Color.White, 25, 240, 360);

                    cardCreator.AddImage($"https://i.imgur.com/qYAKHbO.png", 235, 420, 80, 80); //Left Hand
                    cardCreator.AddImage($"https://i.imgur.com/djd63gV.png", 360, 420, 80, 80); //Right Hand
                    cardCreator.AddImage($"https://i.imgur.com/fBQueGL.png", 480, 410, 100, 100); //Both Hands

                    cardCreator.AddText($"Acc:", System.Drawing.Color.Gray, 25, 0, 510);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AccLeft, 2)}", System.Drawing.Color.White, 25, 275, 510);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AccRight, 2)}", System.Drawing.Color.White, 25, 410, 510);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AverageAcc, 2)}", System.Drawing.Color.White, 25, 530, 510);
                    //cardCreator.AddTextCenter($"%", System.Drawing.Color.White, 15, 630, 510);

                    cardCreator.AddText($"Speed:", System.Drawing.Color.Gray, 25, 0, 550);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.LeftSpeed * 3.6, 2)}", System.Drawing.Color.White, 25, 275, 550);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.RightSpeed * 3.6, 2)}", System.Drawing.Color.White, 25, 410, 550);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AverageSpeed * 3.6, 2)}", System.Drawing.Color.White, 25, 530, 550);
                    cardCreator.AddTextCenter($"KM/H", System.Drawing.Color.Gray, 15, 630, 550);

                    cardCreator.AddText($"Distance:", System.Drawing.Color.Gray, 25, 0, 590);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.DistanceTracker.LeftHand / 1000, 2)}", System.Drawing.Color.White, 25, 275, 590);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.DistanceTracker.RightHand / 1000, 2)}", System.Drawing.Color.White, 25, 410, 590);
                    cardCreator.AddTextCenter($"{Math.Round((playerMostRecentLiveData.Trackers.DistanceTracker.LeftHand + playerMostRecentLiveData.Trackers.DistanceTracker.RightHand) / 1000, 2)}", System.Drawing.Color.White, 25, 530, 590);
                    cardCreator.AddTextCenter($"KM", System.Drawing.Color.Gray, 15, 630, 590);

                    cardCreator.AddText($"PreSwing:", System.Drawing.Color.Gray, 25, 0, 630);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.LeftPreswing * 100, 2)}", System.Drawing.Color.White, 25, 275, 630);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.RightPreswing * 100, 2)}", System.Drawing.Color.White, 25, 410, 630);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AveragePreswing * 100, 2)}", System.Drawing.Color.White, 25, 530, 630);
                    cardCreator.AddTextCenter($"%", System.Drawing.Color.Gray, 15, 630, 630);

                    cardCreator.AddText($"PostSwing:   ", System.Drawing.Color.Gray, 25, 0, 670);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.LeftPostswing * 100, 2)}", System.Drawing.Color.White, 25, 275, 670);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.RightPostswing * 100, 2)}", System.Drawing.Color.White, 25, 410, 670);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AveragePostswing * 100, 2)}", System.Drawing.Color.White, 25, 530, 670);
                    cardCreator.AddTextCenter($"%", System.Drawing.Color.Gray, 15, 630, 670);
                    cardCreator.AddTextFloatRight($"Data from the BeatSavior mod", System.Drawing.Color.Gray, 15, 20, 690);


                    var accGrid = playerMostRecentLiveData.Trackers.AccuracyTracker.GridAcc;
                    var hitGrid = playerMostRecentLiveData.Trackers.AccuracyTracker.GridCut;

                    var smallestAcc = accGrid.Min(x => x.Double);
                    var biggestAcc = accGrid.Max(x => x.Double);


                    var gridXReplacement = -20;
                    var gridYReplacement = 40;
                    cardCreator.AddText($"Acc Grid", System.Drawing.Color.White, 25, 800 + gridXReplacement, 320 + gridYReplacement);

                    //Create colors on the grid acc to see what could be improved.
                    cardCreator.DrawRectangle(705 + gridXReplacement, 375 + gridYReplacement, 90, 90, createGridColor(accGrid[8].Double));
                    cardCreator.DrawRectangle(795 + gridXReplacement, 375 + gridYReplacement, 90, 90, createGridColor(accGrid[9].Double));
                    cardCreator.DrawRectangle(885 + gridXReplacement, 375 + gridYReplacement, 90, 90, createGridColor(accGrid[10].Double));
                    cardCreator.DrawRectangle(975 + gridXReplacement, 375 + gridYReplacement, 90, 90, createGridColor(accGrid[11].Double));

                    cardCreator.DrawRectangle(705 + gridXReplacement, 465 + gridYReplacement, 90, 90, createGridColor(accGrid[4].Double));
                    cardCreator.DrawRectangle(795 + gridXReplacement, 465 + gridYReplacement, 90, 90, createGridColor(accGrid[5].Double));
                    cardCreator.DrawRectangle(885 + gridXReplacement, 465 + gridYReplacement, 90, 90, createGridColor(accGrid[6].Double));
                    cardCreator.DrawRectangle(975 + gridXReplacement, 465 + gridYReplacement, 90, 90, createGridColor(accGrid[7].Double));

                    cardCreator.DrawRectangle(705 + gridXReplacement, 555 + gridYReplacement, 90, 90, createGridColor(accGrid[0].Double));
                    cardCreator.DrawRectangle(795 + gridXReplacement, 555 + gridYReplacement, 90, 90, createGridColor(accGrid[1].Double));
                    cardCreator.DrawRectangle(885 + gridXReplacement, 555 + gridYReplacement, 90, 90, createGridColor(accGrid[2].Double));
                    cardCreator.DrawRectangle(975 + gridXReplacement, 555 + gridYReplacement, 90, 90, createGridColor(accGrid[3].Double));

                    System.Drawing.Color createGridColor(double? value)
                    {
                        try
                        {
                            if (value == null) return System.Drawing.Color.Gray;

                            var perPoint = 0.01 * 256 / (biggestAcc - smallestAcc);
                            var x = (value - smallestAcc) * 100;
                            var f = perPoint * x + 128;

                            double r = 0;
                            double g = 0;
                            if (f <= 128)
                            {
                                r = 128;
                                g = 0;
                            }

                            if (f > 128 && f <= 256)
                            {
                                r = 128;
                                g = (double)f - 128;
                            }
                            if (f > 256)
                            {
                                r = 389 - (double)f;
                                g = 128;
                            }

                            return System.Drawing.Color.FromArgb(120, Convert.ToInt32(r), Convert.ToInt32(g), 0);
                        }
                        catch (Exception ex)
                        {
                            return System.Drawing.Color.Gray;
                        }
                    }

                    //Add Data to the grid
                    cardCreator.AddTextCenter(accGrid[8].String == null ? Math.Round(float.Parse(accGrid[8].Double.ToString()), 1).ToString() : accGrid[8].String, System.Drawing.Color.White, 18, 750 + gridXReplacement, 405 + gridYReplacement);
                    cardCreator.AddTextCenter(accGrid[9].String == null ? Math.Round(float.Parse(accGrid[9].Double.ToString()), 1).ToString() : accGrid[9].String, System.Drawing.Color.White, 18, 840 + gridXReplacement, 405 + gridYReplacement);
                    cardCreator.AddTextCenter(accGrid[10].String == null ? Math.Round(float.Parse(accGrid[10].Double.ToString()), 1).ToString() : accGrid[10].String, System.Drawing.Color.White, 18, 930 + gridXReplacement, 405 + gridYReplacement);
                    cardCreator.AddTextCenter(accGrid[11].String == null ? Math.Round(float.Parse(accGrid[11].Double.ToString()), 1).ToString() : accGrid[11].String, System.Drawing.Color.White, 18, 1020 + gridXReplacement, 405 + gridYReplacement);

                    //cardCreator.DrawLineBetweenPoints(System.Drawing.Color.Gray, 3, 795, 380, 795, 650);

                    cardCreator.AddTextCenter(accGrid[4].String == null ? Math.Round(float.Parse(accGrid[4].Double.ToString()), 2).ToString() : accGrid[4].String, System.Drawing.Color.White, 18, 750 + gridXReplacement, 495 + gridYReplacement);
                    cardCreator.AddTextCenter(accGrid[5].String == null ? Math.Round(float.Parse(accGrid[5].Double.ToString()), 2).ToString() : accGrid[5].String, System.Drawing.Color.White, 18, 840 + gridXReplacement, 495 + gridYReplacement);
                    cardCreator.AddTextCenter(accGrid[6].String == null ? Math.Round(float.Parse(accGrid[6].Double.ToString()), 2).ToString() : accGrid[6].String, System.Drawing.Color.White, 18, 930 + gridXReplacement, 495 + gridYReplacement);
                    cardCreator.AddTextCenter(accGrid[7].String == null ? Math.Round(float.Parse(accGrid[7].Double.ToString()), 2).ToString() : accGrid[7].String, System.Drawing.Color.White, 18, 1020 + gridXReplacement, 495 + gridYReplacement);

                    //cardCreator.DrawLineBetweenPoints(System.Drawing.Color.Gray, 3, 885, 380, 885, 650);

                    cardCreator.AddTextCenter(accGrid[0].String == null ? Math.Round(float.Parse(accGrid[0].Double.ToString()), 2).ToString() : accGrid[0].String, System.Drawing.Color.White, 18, 750 + gridXReplacement, 585 + gridYReplacement);
                    cardCreator.AddTextCenter(accGrid[1].String == null ? Math.Round(float.Parse(accGrid[1].Double.ToString()), 2).ToString() : accGrid[1].String, System.Drawing.Color.White, 18, 840 + gridXReplacement, 585 + gridYReplacement);
                    cardCreator.AddTextCenter(accGrid[2].String == null ? Math.Round(float.Parse(accGrid[2].Double.ToString()), 2).ToString() : accGrid[2].String, System.Drawing.Color.White, 18, 930 + gridXReplacement, 585 + gridYReplacement);
                    cardCreator.AddTextCenter(accGrid[3].String == null ? Math.Round(float.Parse(accGrid[3].Double.ToString()), 2).ToString() : accGrid[3].String, System.Drawing.Color.White, 18, 1020 + gridXReplacement, 585 + gridYReplacement);

                    //cardCreator.DrawLineBetweenPoints(System.Drawing.Color.Gray, 3, 975, 380, 975, 650);

                    //cardCreator.DrawLineBetweenPoints(System.Drawing.Color.Gray, 3, 705, 470, 1065, 470);
                    //cardCreator.DrawLineBetweenPoints(System.Drawing.Color.Gray, 3, 705, 580, 1065, 580);

                    //if (hitGrid != null)
                    //{
                    //    cardCreator.AddTextFloatRight(hitGrid[8].ToString(), System.Drawing.Color.White, 12, 305, 5);
                    //    cardCreator.AddTextFloatRight(hitGrid[9].ToString(), System.Drawing.Color.White, 12, 205, 5);
                    //    cardCreator.AddTextFloatRight(hitGrid[10].ToString(), System.Drawing.Color.White, 12, 105, 5);
                    //    cardCreator.AddTextFloatRight(hitGrid[11].ToString(), System.Drawing.Color.White, 12, 5, 5);
                    //    cardCreator.AddTextFloatRight(hitGrid[4].ToString(), System.Drawing.Color.White, 12, 305, 105);
                    //    cardCreator.AddTextFloatRight(hitGrid[5].ToString(), System.Drawing.Color.White, 12, 205, 105);
                    //    cardCreator.AddTextFloatRight(hitGrid[6].ToString(), System.Drawing.Color.White, 12, 105, 105);
                    //    cardCreator.AddTextFloatRight(hitGrid[7].ToString(), System.Drawing.Color.White, 12, 5, 105);
                    //    cardCreator.AddTextFloatRight(hitGrid[0].ToString(), System.Drawing.Color.White, 12, 305, 205);
                    //    cardCreator.AddTextFloatRight(hitGrid[1].ToString(), System.Drawing.Color.White, 12, 205, 205);
                    //    cardCreator.AddTextFloatRight(hitGrid[2].ToString(), System.Drawing.Color.White, 12, 105, 205);
                    //    cardCreator.AddTextFloatRight(hitGrid[3].ToString(), System.Drawing.Color.White, 12, 5, 205);
                    //}

                    //add acc graph
                    cardCreator.AddText($"Acc Graph", System.Drawing.Color.White, 25, 760, 30);

                    Dictionary<float, float> graphPoints = playerMostRecentLiveData.Trackers.ScoreGraphTracker.Graph.ToDictionary(x => (float)Convert.ToDouble(x.Key), x => (float)Convert.ToDouble(x.Value));
                    var lowestValue = graphPoints.Values.Min();
                    var highestValue = graphPoints.Values.Max();
                    var zoomFactor = 25 - (float)(((highestValue - lowestValue) * 100) * 2.8);
                    if (zoomFactor < 0) zoomFactor = 2;
                    cardCreator.AddAccGraph(740, 90, 300, 250, graphPoints, Convert.ToInt32(playerMostRecentLiveData.SongDuration), 1, System.Drawing.Color.White, zoomFactor);

                }

                await cardCreator.Create($"../../../Resources/img/EmbedBackground-{playerInfo.PlayerId}.png");

                embedBuilder = new EmbedBuilder
                {
                    Title = $"**{recentSong.SongAuthorName} - {recentSong.Name} by {recentSong.LevelAuthorName}**",
                    ImageUrl = $"attachment://EmbedBackground-{playerInfo.PlayerId}.png",
                    Url = $"https://scoresaber.com/leaderboard/{recentSong.LeaderboardId}",
                    ThumbnailUrl = $"https://scoresaber.com/imports/images/songs/{recentSong.Id}.png",
                    Footer = new EmbedFooterBuilder() { Text = $"Time Set: {recentSong.Timeset.DateTime.ToShortDateString() + " | " + recentSong.Timeset.DateTime.ToShortTimeString()} UTC" }
                };

                embedBuilder.Author = new EmbedAuthorBuilder() { IconUrl = $"https://new.scoresaber.com{playerInfo.Avatar}", Name = $"{ playerInfo.Name}", Url = $"https://scoresaber.com/u/{playerInfo.PlayerId}" };
                try
                {
                    var clickables =
                  "\n" +
                  $"[Download Map](https://beatsaver.com{recentSongsInfoBeatSaver?.DirectDownload}) - " +
                  $"[Preview Map](https://skystudioapps.com/bs-viewer/?id={recentSongsInfoBeatSaver?.Key}) - " +
                  $"[Song on Spotify]({await new Spotify().SearchItem(recentSong.Name, recentSong.SongAuthorName)})";
                    embedBuilder.AddField(recentSong.GetDifficulty(), clickables);
                }
                catch (Exception ex)
                {

                }

                await message.Channel.SendFileAsync($"../../../Resources/img/EmbedBackground-{playerInfo.PlayerId}.png", embed: embedBuilder.Build());
                File.Delete($"../../../Resources/img/EmbedBackground-{playerInfo.PlayerId}.png");
            }

        }

        public static async Task CreateUserSearchEmbedWithScoresaberIDAndSend(string scoresaberID, SocketMessage message, DiscordSocketClient discord)
        {
            var isInDatabase = await new RoleAssignment(discord).GetDiscordIdWithScoresaberId(scoresaberID) != 0;
            var hasSettingsPage = await SettingsQuestionList.HasSettingsPage(scoresaberID);
            var playerModel = await new ScoresaberAPI(scoresaberID).GetPlayerFull();
            if(playerModel == null)
            {
                await message.Channel.SendMessageAsync("oh oh... Scoresaber/Discord ID is incorrect or the Scoresaber api crashed. Try again.");
                return;
            }

            var embedBuilder = new EmbedBuilder();
            embedBuilder.Title = playerModel.playerInfo.Name;
            embedBuilder.Url = $"https://scoresaber.com/u/{playerModel.playerInfo.PlayerId}";
            embedBuilder.ThumbnailUrl = $"https://new.scoresaber.com{playerModel.playerInfo.Avatar}";
            embedBuilder.AddField("👤 Get their profile", "-------------------------------------");
            embedBuilder.AddField($"🔧 {(hasSettingsPage ? "Get their settings" : "~~Get their settings~~")}", "-------------------------------------");
            embedBuilder.AddField("🔁 Compare yourself to this user", "-------------------------------------");
            embedBuilder.AddField("🆕 Get their recentsongs", "-------------------------------------");
            embedBuilder.AddField("📰 Get their recentsong", "-------------------------------------");
            embedBuilder.AddField("👑 Get their topsongs", "-------------------------------------");

            var msg = await message.Channel.SendMessageAsync("", false, embedBuilder.Build());

            await msg.AddReactionAsync(new Emoji("👤"));
            if(hasSettingsPage) await msg.AddReactionAsync(new Emoji("🔧"));
            await msg.AddReactionAsync(new Emoji("🔁"));
            await msg.AddReactionAsync(new Emoji("🆕"));
            await msg.AddReactionAsync(new Emoji("📰"));
            await msg.AddReactionAsync(new Emoji("👑"));

            //Get reactions           
            var socketReacitonsList = new List<SocketReaction>();
            discord.ReactionAdded += DiscordSocketClient_ReactionAdded;            
            async Task DiscordSocketClient_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
            {
                if (arg1.Id == msg.Id && arg3.UserId != 504633036902498314)
                {
                    if (socketReacitonsList.Contains(arg3)) return;
                    socketReacitonsList.Add(arg3);

                    if (arg3.Emote.Name == "👤")
                    {
                        await GetAndCreateProfileImage(scoresaberID);
                        await message.Channel.SendFileAsync($"../../../Resources/img/RankingCard_{scoresaberID}.png", $"<@!{arg3.UserId}> here you go. What an amazing profile!");
                        File.Delete($"../../../Resources/img/RankingCard_{scoresaberID}.png");
                    }
                    if (arg3.Emote.Name == "🔧")
                    {
                        await message.Channel.SendMessageAsync($"<@!{arg3.UserId}> here you go. Did you know you can use `!bs statistics` to see community stats?", false, await SettingsQuestionList.GetSettingsPageWithScoresaberID(scoresaberID));
                    }
                    if (arg3.Emote.Name == "🔁")
                    {
                        var discordID = arg3.UserId.ToString();
                        if (!await new RoleAssignment(discord).CheckIfDiscordIdIsLinked(discordID))
                        {
                            await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed($"You are not linked {arg3.User}", "Link your scoresaber by using the command `!bs link [scoresaberID]`").Build());
                            return;
                        }
                        var scoresaberID2 = await RoleAssignment.GetScoresaberIdWithDiscordId(discordID);
                        var scoresaberID2FullModel = await new ScoresaberAPI(scoresaberID2).GetPlayerFull();
                        await GetAndCreateUserCompareImage(scoresaberID2, scoresaberID);
                        await GetAndCreateCompareImage(scoresaberID2FullModel, playerModel);
                        await message.Channel.SendFileAsync($"../../../Resources/img/UserCompareCard_{scoresaberID2}_{scoresaberID}.png", $"<@!{arg3.UserId}> here you go. I wonder who is better");
                        await message.Channel.SendFileAsync($"../../../Resources/img/CompareCard_{scoresaberID2}_{scoresaberID}.png");
                        
                        File.Delete($"../../../Resources/img/CompareCard_{scoresaberID2}_{scoresaberID}.png");
                        File.Delete($"../../../Resources/img/UserCompareCard_{scoresaberID2}_{scoresaberID}.png");
                    }
                    if (arg3.Emote.Name == "🆕")
                    {
                        await GetAndCreateRecentsongsCardImage(scoresaberID);
                        await message.Channel.SendFileAsync($"../../../Resources/img/RecentsongsCard_{scoresaberID}.png", $"<@!{arg3.UserId}> here you go.");
                        File.Delete($"../../../Resources/img/RecentsongsCard_{scoresaberID}.png");                  
                    }
                    if (arg3.Emote.Name == "📰")
                    {
                        await message.Channel.SendMessageAsync($"<@!{arg3.UserId}> here you go. This data is actually really helpfull!");
                        await GetAndPostRecentSongWithScoresaberIdNew(scoresaberID, message);
                    }
                        if (arg3.Emote.Name == "👑")
                    {
                        await GetAndCreateTopsongsCardImage(scoresaberID);
                        await message.Channel.SendFileAsync($"../../../Resources/img/TopsongsCard_{scoresaberID}.png", $"<@!{arg3.UserId}> here you go. ");
                        File.Delete($"../../../Resources/img/TopsongsCard_{scoresaberID}.png");  
                    }       
                }                
            }
        }

        public static async Task GetNewTopSongWithScoresaberIdNew(string playerId, SocketMessage message, int topsongNr = 1)
        {
            var embedBuilder = new EmbedBuilder();

            using (var client = new HttpClient())
            {
                var scoresaberApi = new ScoresaberAPI(playerId);
                var beatSaviourApi = new BeatSaviourApi(playerId);

                //Download scoresaber recentsong data
                var topSong = await scoresaberApi.GetTopScores(topsongNr);

                //Download beatsaver topSong data
                var topSongsInfoBeatSaver = await new BeatSaverApi(topSong.Id).GetRecentSongData();

                //Download scoresaber full player data
                var playerFullData = await scoresaberApi.GetPlayerFull();
                var playerInfo = playerFullData.playerInfo;

                //Download BeatSaviour livedata 
                var playerMostRecentLiveData = await beatSaviourApi.GetMostRecentLiveData(topSong.Id, topSong.GetDifficulty());
                var hasBeatSaviour = playerMostRecentLiveData == null ? false : true;

                var cardCreator = new ImageCreator("../../../Resources/img/EmbedBackground-Template.png");
                cardCreator.AddImage($"https://scoresaber.com/imports/images/songs/{topSong.Id}.png", 0, 0, 1080, 720, 0.1f);

                cardCreator.AddTextFloatRight($"#{topSong.Rank}", System.Drawing.Color.White, 50, 0, 0);
                cardCreator.AddTextFloatRight($"{topSong.Pp}PP", System.Drawing.Color.White, 30, 100, 70);
                cardCreator.AddTextFloatRight($"({Math.Round(topSong.Pp * topSong.Weight, 2)}PP)", System.Drawing.Color.White, 15, 0, 85);
                cardCreator.AddTextFloatRight($"{Math.Round(Convert.ToDouble(topSong.UScore) / Convert.ToDouble(topSong.MaxScoreEx) * 100, 2)}%", System.Drawing.Color.White, 30, 0, 110);

                if (topSongsInfoBeatSaver != null)
                {
                    var metadataDynamic = topSongsInfoBeatSaver.Metadata.Characteristics[0].Difficulties;
                    dynamic difficulty = metadataDynamic.GetType().GetProperty(topSong.GetDifficulty()).GetValue(metadataDynamic, null);

                    cardCreator.AddText($"Bpm:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, 0);
                    cardCreator.AddText($"{topSongsInfoBeatSaver.Metadata.Bpm}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, 0);

                    cardCreator.AddText($"Duration:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 30 : 40);
                    cardCreator.AddText($"{topSongsInfoBeatSaver.Metadata.Duration}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 30 : 40);

                    cardCreator.AddText($"Notes:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 60 : 80);
                    cardCreator.AddText($"{(topSong.GetDifficulty() == "ExpertPlus" ? metadataDynamic.ExpertPlus.Notes : difficulty.notes)}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 60 : 80);

                    cardCreator.AddText($"NJS:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 90 : 120);
                    cardCreator.AddText($"{(topSong.GetDifficulty() == "ExpertPlus" ? metadataDynamic.ExpertPlus.Njs : difficulty.njs)}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 90 : 120);

                    cardCreator.AddText($"NJS offset:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 120 : 160);
                    cardCreator.AddText($"{(topSong.GetDifficulty() == "ExpertPlus" ? metadataDynamic.ExpertPlus.NjsOffset : difficulty.njsOffset)}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 120 : 160);

                    cardCreator.AddText($"Bombs:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 150 : 200);
                    cardCreator.AddText($"{(topSong.GetDifficulty() == "ExpertPlus" ? metadataDynamic.ExpertPlus.Bombs : difficulty.bombs)}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 150 : 200);

                    cardCreator.AddText($"Obstacles:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 180 : 240);
                    cardCreator.AddText($"{(topSong.GetDifficulty() == "ExpertPlus" ? metadataDynamic.ExpertPlus.Obstacles : difficulty.obstacles)}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 180 : 240);

                    cardCreator.AddText($"Max Score:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 210 : 280);
                    cardCreator.AddText($"{topSong.MaxScoreEx}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 210 : 280);

                    cardCreator.AddText($"Mods:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 240 : 320);
                    cardCreator.AddText($"{topSong.Mods}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 240 : 320);

                }

                //Create AccGrid If needed
                if (hasBeatSaviour)
                {
                    var PbIncrease = playerMostRecentLiveData.Trackers.ScoreTracker.PersonalBest == 0 ? "First Play" : $"{(playerMostRecentLiveData.Trackers.ScoreTracker.RawScore * 100) / playerMostRecentLiveData.Trackers.ScoreTracker.PersonalBest}% Increase";
                    cardCreator.AddTextFloatRight($"{PbIncrease}", System.Drawing.Color.White, 20, 0, 155);

                    var misses = "";
                    if (playerMostRecentLiveData.Trackers.HitTracker.Miss.ToString() == "0") misses = "FC";
                    else misses = playerMostRecentLiveData.Trackers.HitTracker.Miss.ToString();
                    cardCreator.AddText($"Misses:", System.Drawing.Color.Gray, 25, 300, 80);
                    cardCreator.AddText($"{misses}", System.Drawing.Color.White, 25, 540, 80);

                    cardCreator.AddText($"Pauses:", System.Drawing.Color.Gray, 25, 300, 120);
                    cardCreator.AddText($"{playerMostRecentLiveData.Trackers.WinTracker.NbOfPause}", System.Drawing.Color.White, 25, 540, 120);

                    cardCreator.AddText($"Max Combo:", System.Drawing.Color.Gray, 25, 300, 160);
                    cardCreator.AddText($"{playerMostRecentLiveData.Trackers.HitTracker.MaxCombo}", System.Drawing.Color.White, 25, 540, 160);

                    cardCreator.AddImage($"https://i.imgur.com/qYAKHbO.png", 235, 420, 80, 80); //Left Hand
                    cardCreator.AddImage($"https://i.imgur.com/djd63gV.png", 360, 420, 80, 80); //Right Hand
                    cardCreator.AddImage($"https://i.imgur.com/fBQueGL.png", 480, 410, 100, 100); //Both Hands

                    cardCreator.AddText($"Acc:", System.Drawing.Color.Gray, 25, 0, 510);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AccLeft, 2)}", System.Drawing.Color.White, 25, 275, 510);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AccRight, 2)}", System.Drawing.Color.White, 25, 410, 510);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AverageAcc, 2)}", System.Drawing.Color.White, 25, 530, 510);
                    //cardCreator.AddTextCenter($"%", System.Drawing.Color.White, 15, 630, 510);

                    cardCreator.AddText($"Speed:", System.Drawing.Color.Gray, 25, 0, 550);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.LeftSpeed * 3.6, 2)}", System.Drawing.Color.White, 25, 275, 550);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.RightSpeed * 3.6, 2)}", System.Drawing.Color.White, 25, 410, 550);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AverageSpeed * 3.6, 2)}", System.Drawing.Color.White, 25, 530, 550);
                    cardCreator.AddTextCenter($"KM/H", System.Drawing.Color.Gray, 15, 630, 550);

                    cardCreator.AddText($"Distance:", System.Drawing.Color.Gray, 25, 0, 590);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.DistanceTracker.LeftHand / 1000, 2)}", System.Drawing.Color.White, 25, 275, 590);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.DistanceTracker.RightHand / 1000, 2)}", System.Drawing.Color.White, 25, 410, 590);
                    cardCreator.AddTextCenter($"{Math.Round((playerMostRecentLiveData.Trackers.DistanceTracker.LeftHand + playerMostRecentLiveData.Trackers.DistanceTracker.RightHand) / 1000, 2)}", System.Drawing.Color.White, 25, 530, 590);
                    cardCreator.AddTextCenter($"KM", System.Drawing.Color.Gray, 15, 630, 590);

                    cardCreator.AddText($"PreSwing:", System.Drawing.Color.Gray, 25, 0, 630);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.LeftPreswing * 100, 2)}", System.Drawing.Color.White, 25, 275, 630);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.RightPreswing * 100, 2)}", System.Drawing.Color.White, 25, 410, 630);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AveragePreswing * 100, 2)}", System.Drawing.Color.White, 25, 530, 630);
                    cardCreator.AddTextCenter($"%", System.Drawing.Color.Gray, 15, 630, 630);

                    cardCreator.AddText($"PostSwing:   ", System.Drawing.Color.Gray, 25, 0, 670);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.LeftPostswing * 100, 2)}", System.Drawing.Color.White, 25, 275, 670);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.RightPostswing * 100, 2)}", System.Drawing.Color.White, 25, 410, 670);
                    cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AveragePostswing * 100, 2)}", System.Drawing.Color.White, 25, 530, 670);
                    cardCreator.AddTextCenter($"%", System.Drawing.Color.Gray, 15, 630, 670);
                    cardCreator.AddTextFloatRight($"Data from the BeatSavior mod", System.Drawing.Color.Gray, 15, 0, 690);


                    var accGrid = playerMostRecentLiveData.Trackers.AccuracyTracker.GridAcc;
                    var hitGrid = playerMostRecentLiveData.Trackers.AccuracyTracker.GridCut;

                    cardCreator.AddText($"Acc Grid", System.Drawing.Color.White, 25, 800, 320);

                    var smallestAcc = accGrid.Min(x => x.Double);
                    var biggestAcc = accGrid.Max(x => x.Double);

                    //Create colors on the grid acc to see what could be improved.
                    cardCreator.DrawRectangle(705, 375, 90, 90, createGridColor(accGrid[8].Double));
                    cardCreator.DrawRectangle(795, 375, 90, 90, createGridColor(accGrid[9].Double));
                    cardCreator.DrawRectangle(885, 375, 90, 90, createGridColor(accGrid[10].Double));
                    cardCreator.DrawRectangle(975, 375, 90, 90, createGridColor(accGrid[11].Double));

                    cardCreator.DrawRectangle(705, 465, 90, 90, createGridColor(accGrid[4].Double));
                    cardCreator.DrawRectangle(795, 465, 90, 90, createGridColor(accGrid[5].Double));
                    cardCreator.DrawRectangle(885, 465, 90, 90, createGridColor(accGrid[6].Double));
                    cardCreator.DrawRectangle(975, 465, 90, 90, createGridColor(accGrid[7].Double));

                    cardCreator.DrawRectangle(705, 555, 90, 90, createGridColor(accGrid[0].Double));
                    cardCreator.DrawRectangle(795, 555, 90, 90, createGridColor(accGrid[1].Double));
                    cardCreator.DrawRectangle(885, 555, 90, 90, createGridColor(accGrid[2].Double));
                    cardCreator.DrawRectangle(975, 555, 90, 90, createGridColor(accGrid[3].Double));

                    System.Drawing.Color createGridColor(double? value)
                    {
                        try
                        {
                            if (value == null) return System.Drawing.Color.Gray;

                            var perPoint = 0.01 * 256 / (biggestAcc - smallestAcc);
                            var x = (value - smallestAcc) * 100;
                            var f = perPoint * x + 128;

                            double r = 0;
                            double g = 0;
                            if (f <= 128)
                            {
                                r = 128;
                                g = 0;
                            }

                            if (f > 128 && f <= 256)
                            {
                                r = 128;
                                g = (double)f - 128;
                            }
                            if (f > 256)
                            {
                                r = 389 - (double)f;
                                g = 128;
                            }

                            return System.Drawing.Color.FromArgb(120, Convert.ToInt32(r), Convert.ToInt32(g), 0);
                        }
                        catch (Exception ex)
                        {
                            return System.Drawing.Color.Gray;
                        }
                    }

                    //Add Data to the grid
                    cardCreator.AddTextCenter(accGrid[8].String == null ? Math.Round(float.Parse(accGrid[8].Double.ToString()), 1).ToString() : accGrid[8].String, System.Drawing.Color.White, 18, 750, 405);
                    cardCreator.AddTextCenter(accGrid[9].String == null ? Math.Round(float.Parse(accGrid[9].Double.ToString()), 1).ToString() : accGrid[9].String, System.Drawing.Color.White, 18, 840, 405);
                    cardCreator.AddTextCenter(accGrid[10].String == null ? Math.Round(float.Parse(accGrid[10].Double.ToString()), 1).ToString() : accGrid[10].String, System.Drawing.Color.White, 18, 930, 405);
                    cardCreator.AddTextCenter(accGrid[11].String == null ? Math.Round(float.Parse(accGrid[11].Double.ToString()), 1).ToString() : accGrid[11].String, System.Drawing.Color.White, 18, 1020, 405);

                    //cardCreator.DrawLineBetweenPoints(System.Drawing.Color.Gray, 3, 795, 380, 795, 650);

                    cardCreator.AddTextCenter(accGrid[4].String == null ? Math.Round(float.Parse(accGrid[4].Double.ToString()), 2).ToString() : accGrid[4].String, System.Drawing.Color.White, 18, 750, 495);
                    cardCreator.AddTextCenter(accGrid[5].String == null ? Math.Round(float.Parse(accGrid[5].Double.ToString()), 2).ToString() : accGrid[5].String, System.Drawing.Color.White, 18, 840, 495);
                    cardCreator.AddTextCenter(accGrid[6].String == null ? Math.Round(float.Parse(accGrid[6].Double.ToString()), 2).ToString() : accGrid[6].String, System.Drawing.Color.White, 18, 930, 495);
                    cardCreator.AddTextCenter(accGrid[7].String == null ? Math.Round(float.Parse(accGrid[7].Double.ToString()), 2).ToString() : accGrid[7].String, System.Drawing.Color.White, 18, 1020, 495);

                    //cardCreator.DrawLineBetweenPoints(System.Drawing.Color.Gray, 3, 885, 380, 885, 650);

                    cardCreator.AddTextCenter(accGrid[0].String == null ? Math.Round(float.Parse(accGrid[0].Double.ToString()), 2).ToString() : accGrid[0].String, System.Drawing.Color.White, 18, 750, 585);
                    cardCreator.AddTextCenter(accGrid[1].String == null ? Math.Round(float.Parse(accGrid[1].Double.ToString()), 2).ToString() : accGrid[1].String, System.Drawing.Color.White, 18, 840, 585);
                    cardCreator.AddTextCenter(accGrid[2].String == null ? Math.Round(float.Parse(accGrid[2].Double.ToString()), 2).ToString() : accGrid[2].String, System.Drawing.Color.White, 18, 930, 585);
                    cardCreator.AddTextCenter(accGrid[3].String == null ? Math.Round(float.Parse(accGrid[3].Double.ToString()), 2).ToString() : accGrid[3].String, System.Drawing.Color.White, 18, 1020, 585);

                    //cardCreator.DrawLineBetweenPoints(System.Drawing.Color.Gray, 3, 975, 380, 975, 650);

                    //cardCreator.DrawLineBetweenPoints(System.Drawing.Color.Gray, 3, 705, 470, 1065, 470);
                    //cardCreator.DrawLineBetweenPoints(System.Drawing.Color.Gray, 3, 705, 580, 1065, 580);

                    //if (hitGrid != null)
                    //{
                    //    cardCreator.AddTextFloatRight(hitGrid[8].ToString(), System.Drawing.Color.White, 12, 305, 5);
                    //    cardCreator.AddTextFloatRight(hitGrid[9].ToString(), System.Drawing.Color.White, 12, 205, 5);
                    //    cardCreator.AddTextFloatRight(hitGrid[10].ToString(), System.Drawing.Color.White, 12, 105, 5);
                    //    cardCreator.AddTextFloatRight(hitGrid[11].ToString(), System.Drawing.Color.White, 12, 5, 5);
                    //    cardCreator.AddTextFloatRight(hitGrid[4].ToString(), System.Drawing.Color.White, 12, 305, 105);
                    //    cardCreator.AddTextFloatRight(hitGrid[5].ToString(), System.Drawing.Color.White, 12, 205, 105);
                    //    cardCreator.AddTextFloatRight(hitGrid[6].ToString(), System.Drawing.Color.White, 12, 105, 105);
                    //    cardCreator.AddTextFloatRight(hitGrid[7].ToString(), System.Drawing.Color.White, 12, 5, 105);
                    //    cardCreator.AddTextFloatRight(hitGrid[0].ToString(), System.Drawing.Color.White, 12, 305, 205);
                    //    cardCreator.AddTextFloatRight(hitGrid[1].ToString(), System.Drawing.Color.White, 12, 205, 205);
                    //    cardCreator.AddTextFloatRight(hitGrid[2].ToString(), System.Drawing.Color.White, 12, 105, 205);
                    //    cardCreator.AddTextFloatRight(hitGrid[3].ToString(), System.Drawing.Color.White, 12, 5, 205);
                    //}


                    //ADD SCORE GRAPH 

                }

                await cardCreator.Create($"../../../Resources/img/EmbedBackground-{playerInfo.PlayerId}.png");

                embedBuilder = new EmbedBuilder
                {
                    Title = $"**{topSong.SongAuthorName} - {topSong.Name}**",
                    ImageUrl = $"attachment://EmbedBackground-{playerInfo.PlayerId}.png",
                    Url = $"https://scoresaber.com/leaderboard/{topSong.LeaderboardId}",
                    ThumbnailUrl = $"https://scoresaber.com/imports/images/songs/{topSong.Id}.png",
                    Footer = new EmbedFooterBuilder() { Text = $"Time Set: {topSong.Timeset.DateTime.ToShortDateString() + " | " + topSong.Timeset.DateTime.ToShortTimeString()} UTC" }
                };

                embedBuilder.Author = new EmbedAuthorBuilder() { IconUrl = $"https://new.scoresaber.com{playerInfo.Avatar}", Name = $"{ playerInfo.Name}", Url = $"https://scoresaber.com/u/{playerInfo.PlayerId}" };
                try
                {
                    var clickables =
                  "\n" +
                  $"[Download Map](https://beatsaver.com{topSongsInfoBeatSaver?.DirectDownload}) - " +
                  $"[Preview Map](https://skystudioapps.com/bs-viewer/?id={topSongsInfoBeatSaver?.Key}) - " +
                  $"[Song on Spotify]({await new Spotify().SearchItem(topSong.Name, topSong.SongAuthorName)})";
                    embedBuilder.AddField(topSong.GetDifficulty(), clickables);
                }
                catch (Exception ex)
                {

                }

                await message.Channel.SendFileAsync($"../../../Resources/img/EmbedBackground-{playerInfo.PlayerId}.png", embed: embedBuilder.Build());
                File.Delete($"../../../Resources/img/EmbedBackground-{playerInfo.PlayerId}.png");
            }
        }

        public static async Task<EmbedBuilder> GetNewTopSongWithScoresaberId(string playerId, int topsongNr = 1)
        {
            var url = $"https://new.scoresaber.com/api/player/{playerId}/scores/top/1";
            var embedBuilder = new EmbedBuilder();

            using (var client = new HttpClient())
            {
                var scoresaberApi = new ScoresaberAPI(playerId);
                var beatSaviourApi = new BeatSaviourApi(playerId);

                //Download scoresaber recentsong data
                var TopSong = await scoresaberApi.GetTopScores(topsongNr);

                var playerInfoJsonData = await client.GetStringAsync($"https://new.scoresaber.com/api/player/{playerId}/full");
                var playerInfo1 = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(playerInfoJsonData);
                var playerInfo = playerInfo1.playerInfo;


                embedBuilder = new EmbedBuilder
                {
                    Title = $"**Top Song From: {playerInfo.Name} :flag_{playerInfo.Country.ToLower()}:**",
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

        public static async Task<EmbedBuilder> GetComparedEmbedBuilderNew(string message, SocketMessage socketMessage, DiscordSocketClient discordSocketClient)
        {
            //Check if the message is set up correctly
            if (message.Length == 0 || message == null) return EmbedBuilderExtension.NullEmbed("Format is not set up correctly", "Use the following format: `!bs compare player1 player2`");

            //Prepare player data             
            var players = message.Split(' ');

            if (players.Count() > 2) return EmbedBuilderExtension.NullEmbed("oh oh...", $"Format incorrect");
            if (players.Count() < 2) return EmbedBuilderExtension.NullEmbed("oh oh...", $"Use the command like this `!bs compare @player1 @player2` \ndont forget a space");

            var player1 = players[0];
            var player2 = players[1];

            var player1containsmention = false;
            var player2containsmention = false;

            if (player1.Contains("@"))
            {
                var r = new RoleAssignment(discordSocketClient);
                var discordId = player1.Replace("<@", "").Replace(">", "").Replace("!", "");
                if (await r.CheckIfDiscordIdIsLinked(discordId))
                {
                    player1 = await RoleAssignment.GetScoresaberIdWithDiscordId(discordId);
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
                var discordId = player2.Replace("<@", "").Replace(">", "").Replace("!", "");
                if (await r.CheckIfDiscordIdIsLinked(discordId))
                {
                    player2 = await RoleAssignment.GetScoresaberIdWithDiscordId(discordId);
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

            var player1ScoresaberID = player1;
            var player2ScoresaberID = player2;

            using (HttpClient hc = new HttpClient())
            {


                if (!player1containsmention)
                {
                    if (!player1.All(char.IsDigit))
                    {
                        var infoPlayer1Raw = await hc.GetAsync(urlPlayer1);
                        if (infoPlayer1Raw.StatusCode != HttpStatusCode.OK) return EmbedBuilderExtension.NullEmbed("Scoresaber Error", $"**Player 1 status:** {infoPlayer1Raw.StatusCode}");
                        var json = await infoPlayer1Raw.Content.ReadAsStringAsync();
                        var playerlist = JsonConvert.DeserializeObject<ScoreSaberSearchByNameModel>(json).Players;
                        var player1search = playerlist.Where(x => x.PlayerName.ToLower() == player1.Replace("_", " ").ToLower());
                        if (player1search.Count() == 0) return EmbedBuilderExtension.NullEmbed("Error", $"{player2} could not be found");
                        player1ScoresaberID = player1search.First().PlayerId;
                        var urlPlayerInfo1 = $"https://new.scoresaber.com/api/player/{player1ScoresaberID}/full";
                        var player1InfoRaw = await hc.GetStringAsync(urlPlayerInfo1);
                        player1Info = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(player1InfoRaw);
                    }
                    else
                    {
                        //GET players info 
                        var urlPlayerInfo1 = $"https://new.scoresaber.com/api/player/{player1ScoresaberID}/full";
                        var player1InfoRaw = await hc.GetStringAsync(urlPlayerInfo1);
                        player1Info = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(player1InfoRaw);
                    }
                }
                else
                {
                    //GET players info 
                    var urlPlayerInfo1 = $"https://new.scoresaber.com/api/player/{player1ScoresaberID}/full";
                    var player1InfoRaw = await hc.GetStringAsync(urlPlayerInfo1);
                    player1Info = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(player1InfoRaw);
                }

                if (!player2containsmention)
                {
                    if (!player2.All(char.IsDigit))
                    {
                        var infoPlayer2Raw = await hc.GetAsync(urlPlayer2.Replace("_", " "));
                        if (infoPlayer2Raw.StatusCode != HttpStatusCode.OK) return EmbedBuilderExtension.NullEmbed("Scoresaber Error", $"**Player 2 status:** {infoPlayer2Raw.StatusCode}");
                        var json = await infoPlayer2Raw.Content.ReadAsStringAsync();
                        var playerlist = JsonConvert.DeserializeObject<ScoreSaberSearchByNameModel>(json).Players;
                        var player2search = playerlist.Where(x => x.PlayerName.ToLower() == player2.Replace("_", " ").ToLower());
                        if (player2search.Count() == 0) return EmbedBuilderExtension.NullEmbed("Error", $"{player2} could not be found");
                        player2ScoresaberID = player2search.First().PlayerId;
                        var urlPlayerInfo2 = $"https://new.scoresaber.com/api/player/{player2ScoresaberID}/full";
                        var player2InfoRaw = await hc.GetStringAsync(urlPlayerInfo2);
                        player2Info = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(player2InfoRaw);
                    }
                    else
                    {
                        var urlPlayerInfo2 = $"https://new.scoresaber.com/api/player/{player2ScoresaberID}/full";
                        var player2InfoRaw = await hc.GetStringAsync(urlPlayerInfo2);
                        player2Info = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(player2InfoRaw);
                    }
                }
                else
                {
                    var urlPlayerInfo2 = $"https://new.scoresaber.com/api/player/{player2ScoresaberID}/full";
                    var player2InfoRaw = await hc.GetStringAsync(urlPlayerInfo2);
                    player2Info = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(player2InfoRaw);
                }
            }

            await BeatSaberInfoExtension.GetAndCreateUserCompareImage(player1ScoresaberID, player2ScoresaberID);
            await socketMessage.Channel.SendFileAsync($"../../../Resources/img/UserCompareCard_{player1}_{player2ScoresaberID}.png");
            File.Delete($"../../../Resources/img/UserCompareCard_{player1}_{player2}.png");

            await BeatSaberInfoExtension.GetAndCreateCompareImage(player1Info, player2Info);
            await socketMessage.Channel.SendFileAsync($"../../../Resources/img/CompareCard_{player1}_{player2ScoresaberID}.png");
            File.Delete($"../../../Resources/img/CompareCard_{player1}_{player2}.png");

            return null;
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
                    player1 = await RoleAssignment.GetScoresaberIdWithDiscordId(discordId);
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
                    player2 = await RoleAssignment.GetScoresaberIdWithDiscordId(discordId);
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
                    var playerList = JsonConvert.DeserializeObject<ScoreSaberSearchByNameModel>(infoPlayer1Raw.Content.ReadAsStringAsync().Result).Players;
                    var player1search = playerList.Where(x => x.PlayerName.ToLower() == player1.ToLower());
                    if (player1search.Count() == 0) return null;
                    player1ScoresaberID = player1search.First().PlayerId;
                }

                if (!player2containsmention)
                {
                    var infoPlayer2Raw = await hc.GetAsync(urlPlayer2);
                    if (infoPlayer2Raw.StatusCode != HttpStatusCode.OK) return EmbedBuilderExtension.NullEmbed("Scoresaber Error", $"**Player 2 status:** {infoPlayer2Raw.StatusCode}");
                    var playerList = JsonConvert.DeserializeObject<ScoreSaberSearchByNameModel>(infoPlayer2Raw.Content.ReadAsStringAsync().Result).Players;
                    var player2search = playerList.Where(x => x.PlayerName.ToLower() == player2.ToLower());
                    if (player2search.Count() == 0) return null;
                    player2ScoresaberID = player2search.First().PlayerId;
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

        public static async Task GetAndCreateCompareImage(ScoresaberPlayerFullModel scoresaberId1, ScoresaberPlayerFullModel scoresaberId2)
        {
            var rankingCardCreator = new ImageCreator("../../../Resources/img/CompareCard-Template.png");
            var topDataPlayerOne = await new ScoresaberAPI(scoresaberId1.playerInfo.PlayerId.ToString()).GetTopScores();
            var topPpPlayPlayerOne = topDataPlayerOne.Scores.First().Pp;
            var topDataPlayerTwo = await new ScoresaberAPI(scoresaberId2.playerInfo.PlayerId.ToString()).GetTopScores();
            var topPpPlayPlayerTwo = topDataPlayerTwo.Scores.First().Pp;

            var offset = 15;

            //Player One Info 
            rankingCardCreator.AddText($"#{FormatNumber(scoresaberId1.playerInfo.rank)}", SelectColorAndAddDifference(scoresaberId1.playerInfo.rank, scoresaberId2.playerInfo.rank, true), 4, 15, 10 + offset);
            rankingCardCreator.AddText($"#{FormatNumber(scoresaberId1.playerInfo.CountryRank)}", SelectColorAndAddDifference(scoresaberId1.playerInfo.CountryRank, scoresaberId2.playerInfo.CountryRank, true), 4, 15, 35 + offset);
            rankingCardCreator.AddText($"{FormatNumber(scoresaberId1.playerInfo.Pp)} PP", SelectColorAndAddDifference(scoresaberId1.playerInfo.Pp, scoresaberId2.playerInfo.Pp, false), 4, 15, 60 + offset);
            rankingCardCreator.AddText($"{scoresaberId1.scoreStats.AvarageRankedAccuracy}%", SelectColorAndAddDifference(scoresaberId1.scoreStats.AvarageRankedAccuracy, scoresaberId2.scoreStats.AvarageRankedAccuracy, false), 4, 15, 85 + offset);
            rankingCardCreator.AddText($"{FormatNumber(scoresaberId1.scoreStats.TotalPlayCount)}", SelectColorAndAddDifference(scoresaberId1.scoreStats.TotalPlayCount, scoresaberId2.scoreStats.TotalPlayCount, false), 4, 15, 110 + offset);
            rankingCardCreator.AddText($"{FormatNumber(scoresaberId1.scoreStats.RankedPlayerCount)}", SelectColorAndAddDifference(scoresaberId1.scoreStats.RankedPlayerCount, scoresaberId2.scoreStats.RankedPlayerCount, false), 4, 15, 135 + offset);
            rankingCardCreator.AddText($"{FormatNumber(scoresaberId1.scoreStats.TotalScore)}", SelectColorAndAddDifference(scoresaberId1.scoreStats.TotalScore, scoresaberId2.scoreStats.TotalScore, false), 4, 15, 160 + offset);
            rankingCardCreator.AddText($"{FormatNumber(scoresaberId1.scoreStats.TotalRankedScore)}", SelectColorAndAddDifference(scoresaberId1.scoreStats.TotalRankedScore, scoresaberId2.scoreStats.TotalRankedScore, false), 4, 15, 185 + offset);
            rankingCardCreator.AddText($"{scoresaberId1.playerInfo.Badges.Count()}", SelectColorAndAddDifference(scoresaberId1.playerInfo.Badges.Count(), scoresaberId2.playerInfo.Badges.Count(), false), 4, 15, 210 + offset);
            rankingCardCreator.AddText($"{topPpPlayPlayerOne}PP", SelectColorAndAddDifference(topPpPlayPlayerOne, topPpPlayPlayerTwo, false), 4, 15, 235 + offset);

            //Player Two Info 
            rankingCardCreator.AddTextFloatRight($"#{FormatNumber(scoresaberId2.playerInfo.rank)}", SelectColorAndAddDifference(scoresaberId2.playerInfo.rank, scoresaberId1.playerInfo.rank, true), 4, 15, 10 + offset);
            rankingCardCreator.AddTextFloatRight($"#{FormatNumber(scoresaberId2.playerInfo.CountryRank)}", SelectColorAndAddDifference(scoresaberId2.playerInfo.CountryRank, scoresaberId1.playerInfo.CountryRank, true), 4, 15, 35 + offset);
            rankingCardCreator.AddTextFloatRight($"{FormatNumber(scoresaberId2.playerInfo.Pp)} PP", SelectColorAndAddDifference(scoresaberId2.playerInfo.Pp, scoresaberId1.playerInfo.Pp, false), 4, 15, 60 + offset);
            rankingCardCreator.AddTextFloatRight($"{scoresaberId2.scoreStats.AvarageRankedAccuracy}%", SelectColorAndAddDifference(scoresaberId2.scoreStats.AvarageRankedAccuracy, scoresaberId1.scoreStats.AvarageRankedAccuracy, false), 4, 15, 85 + offset);
            rankingCardCreator.AddTextFloatRight($"{FormatNumber(scoresaberId2.scoreStats.TotalPlayCount)}", SelectColorAndAddDifference(scoresaberId2.scoreStats.TotalPlayCount, scoresaberId1.scoreStats.TotalPlayCount, false), 4, 15, 110 + offset);
            rankingCardCreator.AddTextFloatRight($"{FormatNumber(scoresaberId2.scoreStats.RankedPlayerCount)}", SelectColorAndAddDifference(scoresaberId2.scoreStats.RankedPlayerCount, scoresaberId1.scoreStats.RankedPlayerCount, false), 4, 15, 135 + offset);
            rankingCardCreator.AddTextFloatRight($"{FormatNumber(scoresaberId2.scoreStats.TotalScore)}", SelectColorAndAddDifference(scoresaberId2.scoreStats.TotalScore, scoresaberId1.scoreStats.TotalScore, false), 4, 15, 160 + offset);
            rankingCardCreator.AddTextFloatRight($"{FormatNumber(scoresaberId2.scoreStats.TotalRankedScore)}", SelectColorAndAddDifference(scoresaberId2.scoreStats.TotalRankedScore, scoresaberId1.scoreStats.TotalRankedScore, false), 4, 15, 185 + offset);
            rankingCardCreator.AddTextFloatRight($"{scoresaberId2.playerInfo.Badges.Count()}", SelectColorAndAddDifference(scoresaberId2.playerInfo.Badges.Count(), scoresaberId1.playerInfo.Badges.Count(), false), 4, 15, 210 + offset);
            rankingCardCreator.AddTextFloatRight($"{topPpPlayPlayerTwo}PP", SelectColorAndAddDifference(topPpPlayPlayerTwo, topPpPlayPlayerOne, false), 4, 15, 235 + offset);


            System.Drawing.Color SelectColorAndAddDifference(dynamic one, dynamic two, bool lowerWins)
            {
                one = Convert.ToDouble(one);
                two = Convert.ToDouble(two);

                if (lowerWins)
                {
                    if (one < two) return System.Drawing.Color.FromArgb(136, 217, 105);
                    if (one > two) return System.Drawing.Color.FromArgb(214, 33, 46);
                }
                else
                {
                    if (one > two) return System.Drawing.Color.FromArgb(136, 217, 105);
                    if (one < two) return System.Drawing.Color.FromArgb(214, 33, 46);
                }

                return System.Drawing.Color.LightGray;
            }

            string FormatNumber(dynamic number)
            {
                number = Convert.ToDouble(number);
                var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                nfi.NumberGroupSeparator = " ";
                string formatted = number.ToString("#,0", nfi);
                return formatted;
            }

            rankingCardCreator.AddText("Global Rank", System.Drawing.Color.White, 4, 200, 10 + offset);
            rankingCardCreator.AddText("Country Rank", System.Drawing.Color.White, 4, 195, 35 + offset);
            rankingCardCreator.AddText("PP", System.Drawing.Color.White, 4, 245, 60 + offset);
            rankingCardCreator.AddText("Accuracy", System.Drawing.Color.White, 4, 207, 85 + offset);
            rankingCardCreator.AddText("Play Count", System.Drawing.Color.White, 4, 197, 110 + offset);
            rankingCardCreator.AddText("Ranked Play Count", System.Drawing.Color.White, 4, 165, 135 + offset);
            rankingCardCreator.AddText("Score", System.Drawing.Color.White, 4, 225, 160 + offset);
            rankingCardCreator.AddText("Ranked Score", System.Drawing.Color.White, 4, 195, 185 + offset);
            rankingCardCreator.AddText("Badge Count", System.Drawing.Color.White, 4, 200, 210 + offset);
            rankingCardCreator.AddText("Top PP Play", System.Drawing.Color.White, 4, 203, 235 + offset);
            rankingCardCreator.Create($"../../../Resources/img/CompareCard_{scoresaberId1.playerInfo.PlayerId}_{scoresaberId2.playerInfo.PlayerId}.png");
        }
        public static async Task GetAndCreateUserCompareImage(string scoresaberId1, string scoresaberId2)
        {
            var playerOneRaw = new ScoresaberAPI(scoresaberId1);
            var playerTwoRaw = new ScoresaberAPI(scoresaberId2);
            var playerOneData = await playerOneRaw.GetPlayerFull();
            var playerTwoData = await playerTwoRaw.GetPlayerFull();

            var playerOne = playerOneData.playerInfo;
            var playerTwo = playerTwoData.playerInfo;
            var playerOneStats = playerOneData.scoreStats;
            var playerTwoStats = playerTwoData.scoreStats;

            var rankingCardCreator = new ImageCreator("../../../Resources/img/UserCard-Template.png");

            //Add player One main info
            rankingCardCreator.AddText(playerOne.Name.ToUpper(), System.Drawing.Color.White, 15, 13, 10);
            rankingCardCreator.AddText(playerOne.Country.ToUpper(), System.Drawing.Color.White, 12, 100, 45);

            rankingCardCreator.AddImage($"https://scoresaber.com/imports/images/flags/{playerOne.Country.ToLower()}.png", 130, 50, 20, 15);
            rankingCardCreator.AddImageRounded($"https://new.scoresaber.com{playerOne.Avatar}", 15, 43, 70, 70);

            //Add player Two main info
            rankingCardCreator.AddTextFloatRight(playerTwo.Name.ToUpper(), System.Drawing.Color.White, 15, 13, 10);
            rankingCardCreator.AddTextFloatRight(playerTwo.Country.ToUpper(), System.Drawing.Color.White, 12, 100, 45);

            rankingCardCreator.AddImage($"https://scoresaber.com/imports/images/flags/{playerTwo.Country.ToLower()}.png", 350, 50, 20, 15);
            rankingCardCreator.AddImageRounded($"https://new.scoresaber.com{playerTwo.Avatar}", 415, 43, 70, 70);


            rankingCardCreator.AddImage($"https://upload.wikimedia.org/wikipedia/commons/7/70/Street_Fighter_VS_logo.png", 225, 28, 70, 70);

            //Finish Card
            rankingCardCreator.Create($"../../../Resources/img/UserCompareCard_{scoresaberId1}_{scoresaberId2}.png");
        }
        public static async Task GetAndCreateUserCardImage(string scoresaberId, string topic)
        {
            var playerRaw = new ScoresaberAPI(scoresaberId);
            var playerData = await playerRaw.GetPlayerFull();

            if (playerData == null) return;

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

            rankingCardCreator.AddImage($"https://scoresaber.com/imports/images/flags/{player.Country.ToLower()}.png", 150, 50, 20, 15);
            rankingCardCreator.AddImage($"https://new.scoresaber.com{player.Avatar}", 15, 13, 100, 100);

            //Finish Card
            rankingCardCreator.Create($"../../../Resources/img/UserCard_{scoresaberId}.png");
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

                var rankTextSize = rankingCardCreator.AddText($"#{playerRecentScores.Scores[x].Rank}", rankcolor, 50, 320, marigin + 135);

                if (playerRecentScores.Scores[x].Pp > 0)
                {
                    double percentage = Convert.ToDouble(playerRecentScores.Scores[x].UScore) / Convert.ToDouble(playerRecentScores.Scores[x].MaxScoreEx) * 100;
                    var acc = Math.Round(percentage, 2);
                    rankingCardCreator.AddText($"{acc}%", rankcolor, 50, 350 + rankTextSize.Width, marigin + 135);
                }


                var ppfontsize = 60;
                if (playerRecentScores.Scores[x].Pp > 300) ppfontsize = 65;
                if (playerRecentScores.Scores[x].Pp > 400) ppfontsize = 70;
                if (playerRecentScores.Scores[x].Pp > 500) ppfontsize = 80;

                if (playerRecentScores.Scores[x].Pp != 0) rankingCardCreator.AddText($"+ {Math.Round(playerRecentScores.Scores[x].Pp, 2)}PP", System.Drawing.Color.Green, ppfontsize, 1320, marigin + 120);
                rankingCardCreator.AddText($"{playerRecentScores.Scores[x].GetDifficulty()}", System.Drawing.Color.White, 30, 80, marigin + 255);
                rankingCardCreator.AddText($"Time set: {playerRecentScores.Scores[x].Timeset.DateTime.ToShortDateString()} {playerRecentScores.Scores[x].Timeset.DateTime.ToShortTimeString()}     {Math.Round((DateTime.Now - playerRecentScores.Scores[x].Timeset).TotalDays, 1)} days ago", System.Drawing.Color.Gray, 30, 1000, marigin + 260);

                rankingCardCreator.AddImageRounded($"https://new.scoresaber.com/api/static/covers/{playerRecentScores.Scores[x].Id}.png", 15, marigin, 250, 250);
                marigin += 330 - (x * 3);
            }

            //Finish Card
            rankingCardCreator.Create($"../../../Resources/img/RecentsongsCard_{scoresaberId}.png");
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

                var rankTextSize = rankingCardCreator.AddText($"#{playerTopScores.Scores[x].Rank}", rankcolor, 50, 320, marigin + 135);

                if (playerTopScores.Scores[x].Pp > 0)
                {
                    double percentage = Convert.ToDouble(playerTopScores.Scores[x].UScore) / Convert.ToDouble(playerTopScores.Scores[x].MaxScoreEx) * 100;
                    var acc = Math.Round(percentage, 2);
                    rankingCardCreator.AddText($"{acc}%", rankcolor, 50, 350 + rankTextSize.Width, marigin + 135);
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
            rankingCardCreator.Create($"../../../Resources/img/TopsongsCard_{scoresaberId}.png");
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

            rankingCardCreator.AddImage($"https://scoresaber.com/imports/images/flags/{player.Country.ToLower()}.png", 1120, 1000, 120, 100);
            rankingCardCreator.AddNoteSlashEffect($"https://new.scoresaber.com{player.Avatar}", 200, 800, 800, 800);

            var rowCount = Math.Round((double)player.Badges.Count() / 7, 0);
            for (var i = 0; i < player.Badges.Count(); i++)
            {
                var row = Math.Round((decimal)(i / 7), 0);
                float x = (float)(2730 - (450 * row));
                var y = 650 + i * 175;
                if (row > 0) y = 650 + (i - 7) * 175;

                rankingCardCreator.AddImage($"https://new.scoresaber.com/api/static/badges/{player.Badges[i].Image}", x, y, 80 * 5, 30 * 5);
            }

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
            if (playerTopStats.Name.Length >= 25)
            {
                nameFontSize = 70;
                var spaces = playerTopStats.Name.Split(" ");

                var firstname = "";
                var lastname = "";
                var splitcount = 0;
                var length = 0;
                foreach (var space in spaces)
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

            }
            else if (playerTopStats.Name.Length >= 52)
            {
                rankingCardCreator.AddText("Fuck this map name", System.Drawing.Color.White, nameFontSize, 100, 2300);
            }
            else
            {
                rankingCardCreator.AddText(playerTopStats.Name, System.Drawing.Color.White, nameFontSize, 100, 2300);
            }
            rankingCardCreator.AddText(playerTopStats.GetDifficulty().Replace("Plus", "+"), System.Drawing.Color.FromArgb(176, 176, 176), 110, 100, 2500);
            rankingCardCreator.AddTextFloatRight($"{playerTopStats.Pp.ToString("0.00")}PP", System.Drawing.Color.White, 160, 950, 2270);
            rankingCardCreator.AddTextFloatRight($"{Math.Round(Convert.ToDouble(playerTopStats.UScore) / Convert.ToDouble(playerTopStats.MaxScoreEx) * 100, 3).ToString("0.00")}%", System.Drawing.Color.FromArgb(176, 176, 176), 110, 950, 2500);
            rankingCardCreator.AddImageRounded($"https://new.scoresaber.com/api/static/covers/{playerTopStats.Id}.png", 3950, 2020, 800, 800);

            rankingCardCreator.Create($"../../../Resources/img/RankingCard_{scoresaberId}.png");
        }

        public static async Task<EmbedBuilder> GetImprovableMapsByAccFromToplist(string scoresaberId, double wishedAcc)
        {
            if (wishedAcc == 0) return EmbedBuilderExtension.NullEmbed("No wished acc added", "use the command like this `!bs improve 95` or `!bs improve 94,5`");

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