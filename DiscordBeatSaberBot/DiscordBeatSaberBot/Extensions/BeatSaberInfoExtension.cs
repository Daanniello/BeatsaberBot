using System;
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
using ScoreSaberLib;

namespace DiscordBeatSaberBot.Extensions
{
    internal static class BeatSaberInfoExtension
    {






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

        public static async Task GetAndPostMapInfoWithKey(SocketSlashCommand command, string key)
        {
            var embedBuilder = new EmbedBuilder();

            using (var client = new HttpClient())
            {

                //Download beatsaver recentsong data
                var mapInfoBeatSaver = await BeatSaverApi.GetMapByKey(key);
                if (mapInfoBeatSaver == null)
                {
                    await command.Channel.SendMessageAsync("oh oh, beat saver died or the input is not correct. try again.");
                    return;
                }

                var cardCreator = new ImageCreator("../../../Resources/img/EmbedBackground-Template.png");
                cardCreator.AddImage($"{mapInfoBeatSaver.Versions.First().CoverUrl}", 0, 0, 1080, 720, 0.1f);

                cardCreator.AddText($"Length:", System.Drawing.Color.White, 24, 50, 50);
                cardCreator.AddText($"{mapInfoBeatSaver.Metadata.Duration}", System.Drawing.Color.White, 24, 250, 50);

                cardCreator.AddText($"BPM:", System.Drawing.Color.White, 24, 50, 100);
                cardCreator.AddText($"{mapInfoBeatSaver.Metadata.Bpm}", System.Drawing.Color.White, 24, 250, 100);

                cardCreator.AddText($"NJS:", System.Drawing.Color.White, 24, 50, 150);
                cardCreator.AddText($"{mapInfoBeatSaver.Versions.First().Diffs.First().Njs}", System.Drawing.Color.White, 24, 250, 150);

                cardCreator.AddText($"Notes:", System.Drawing.Color.White, 24, 50, 200);
                cardCreator.AddText($"{mapInfoBeatSaver.Versions.First().Diffs.First().Notes}", System.Drawing.Color.White, 24, 250, 200);

                //Right side
                cardCreator.AddTextFloatRight($"Downloads:", System.Drawing.Color.White, 24, 230, 50);
                cardCreator.AddTextFloatRight($"{mapInfoBeatSaver.Stats.Downloads}", System.Drawing.Color.White, 24, 50, 50);

                cardCreator.AddTextFloatRight($"Upvotes:", System.Drawing.Color.White, 24, 230, 100);
                cardCreator.AddTextFloatRight($"{mapInfoBeatSaver.Stats.Upvotes}", System.Drawing.Color.White, 24, 50, 100);

                cardCreator.AddTextFloatRight($"Downvotes:", System.Drawing.Color.White, 24, 230, 150);
                cardCreator.AddTextFloatRight($"{mapInfoBeatSaver.Stats.Downvotes}", System.Drawing.Color.White, 24, 50, 150);

                cardCreator.AddTextFloatRight($"Ratio:", System.Drawing.Color.White, 24, 230, 200);
                cardCreator.AddTextFloatRight($"{Math.Round(100 * mapInfoBeatSaver.Stats.Score, 2)}%", System.Drawing.Color.White, 24, 50, 200);

                cardCreator.AddTextFloatRight($"Plays:", System.Drawing.Color.White, 24, 230, 250);
                cardCreator.AddTextFloatRight($"{mapInfoBeatSaver.Stats.Plays}", System.Drawing.Color.White, 24, 50, 250);

                cardCreator.AddTextFloatRight($"Upload Date:", System.Drawing.Color.White, 24, 230, 300);
                cardCreator.AddTextFloatRight($"{mapInfoBeatSaver.Uploaded.UtcDateTime.ToShortDateString()}", System.Drawing.Color.White, 24, 50, 300);

                //Add available difficulties
                cardCreator.AddTextWithBackGround("Easy", System.Drawing.Color.White, 24, mapInfoBeatSaver.Versions.First().Diffs.Any(x => x.Difficulty == "Easy") ? System.Drawing.Color.FromArgb(60, 179, 113) : System.Drawing.Color.LightGray, 50, mapInfoBeatSaver.Versions.First().Diffs.First().Difficulty == "Easy" ? 630 - 15 : 630);
                cardCreator.AddTextWithBackGround("Normal", System.Drawing.Color.White, 24, mapInfoBeatSaver.Versions.First().Diffs.Any(x => x.Difficulty == "Normal") ? System.Drawing.Color.FromArgb(89, 176, 244) : System.Drawing.Color.LightGray, 180, mapInfoBeatSaver.Versions.First().Diffs.First().Difficulty == "Normal" ? 630 - 15 : 630);
                cardCreator.AddTextWithBackGround("Hard", System.Drawing.Color.White, 24, mapInfoBeatSaver.Versions.First().Diffs.Any(x => x.Difficulty == "Hard") ? System.Drawing.Color.FromArgb(254, 99, 71) : System.Drawing.Color.LightGray, 365, mapInfoBeatSaver.Versions.First().Diffs.First().Difficulty == "Hard" ? 630 - 15 : 630);
                cardCreator.AddTextWithBackGround("Expert", System.Drawing.Color.White, 24, mapInfoBeatSaver.Versions.First().Diffs.Any(x => x.Difficulty == "Expert") ? System.Drawing.Color.FromArgb(192, 42, 66) : System.Drawing.Color.LightGray, 500, mapInfoBeatSaver.Versions.First().Diffs.First().Difficulty == "Expert" ? 630 - 15 : 630);
                cardCreator.AddTextWithBackGround("Expert+", System.Drawing.Color.White, 24, mapInfoBeatSaver.Versions.First().Diffs.Any(x => x.Difficulty == "ExpertPlus") ? System.Drawing.Color.FromArgb(143, 72, 219) : System.Drawing.Color.LightGray, 670, mapInfoBeatSaver.Versions.First().Diffs.First().Difficulty == "ExpertPlus" ? 630 - 15 : 630);

                await cardCreator.Create($"../../../Resources/img/EmbedBackground-{key}.png");

                embedBuilder = new EmbedBuilder
                {
                    Title = $"**{mapInfoBeatSaver.Name} by {mapInfoBeatSaver.Metadata.LevelAuthorName}**",
                    ImageUrl = $"attachment://EmbedBackground-{key}.png",
                    ThumbnailUrl = $"https://beatsaver.com/cdn/{mapInfoBeatSaver.Id}/{mapInfoBeatSaver.Versions.First().Hash}.jpg",
                    Color = Color.Blue,
                    Footer = new EmbedFooterBuilder() { Text = $"Hash: {mapInfoBeatSaver.Versions.First().Hash}\nID: {mapInfoBeatSaver.Id}\nKey: {mapInfoBeatSaver.Id}" }
                };

                embedBuilder.AddField("Links",
                    $"\n[Map link (BeatSaver)](https://beatsaver.com/beatmap/{mapInfoBeatSaver.Id})" +
                    $"\n[Mapper: {mapInfoBeatSaver.Uploader.Name}](https://beatsaver.com/uploader/{mapInfoBeatSaver.Uploader.Id})" +
                    $"\n[Image](https://beatsaver.com/{mapInfoBeatSaver.Versions.First().CoverUrl})");



                embedBuilder.AddField("Description", "\n" +
                  //$"Available Difficulties: " +
                  //$"{(mapInfoBeatSaver.Metadata.Difficulties.Easy.ToString() == "False" ? "" : "Easy - ")}" +
                  //$"{(mapInfoBeatSaver.Metadata.Difficulties.Normal.ToString() == "False" ? "" : "Normal - ")}" +
                  //$"{(mapInfoBeatSaver.Metadata.Difficulties.Hard.ToString() == "False" ? "" : "Hard - ")}" +
                  //$"{(mapInfoBeatSaver.Metadata.Difficulties.Expert.ToString() == "False" ? "" : "Expert - ")}" +
                  //$"{(mapInfoBeatSaver.Metadata.Difficulties.ExpertPlus.ToString() == "False" ? "" : "Expert+")}" +
                  mapInfoBeatSaver.Description +
                  "\n" +
                  $"[Download Map](https://beatsaver.com{mapInfoBeatSaver?.Versions.First().DownloadUrl}) - " +
                  $"[Preview Map](https://skystudioapps.com/bs-viewer/?id={mapInfoBeatSaver?.Id}) - " +
                  $"[Song on Spotify]({await new Spotify().SearchItem(mapInfoBeatSaver.Metadata.SongName, mapInfoBeatSaver.Metadata.SongAuthorName)})");



                await command.Channel.SendFileAsync($"../../../Resources/img/EmbedBackground-{key}.png", embed: embedBuilder.Build());
                File.Delete($"../../../Resources/img/EmbedBackground-{key}.png");
            }
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

        public static async Task<EmbedBuilder> GetComparedEmbedBuilderNew(SocketSlashCommand command, DiscordSocketClient discordSocketClient)
        {

            //Prepare player data             
            var players = command.Data.Options;

            if (players.Count() > 2) return EmbedBuilderExtension.NullEmbed("Format error", $"Format incorrect");
            if (players.Count() < 2) return EmbedBuilderExtension.NullEmbed("Format error", $"Use the command like this `!bs compare @player1 @player2` \ndont forget a space");

            var player1 = players.ElementAt(0).Value.ToString();
            var player2 = players.ElementAt(1).Value.ToString();

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
            await command.Channel.SendFileAsync($"../../../Resources/img/UserCompareCard_{player1}_{player2ScoresaberID}.png");
            File.Delete($"../../../Resources/img/UserCompareCard_{player1}_{player2}.png");

            var cardId = await BeatSaberInfoExtension.GetAndCreateCompareImage(player1Info, player2Info);
            await command.Channel.SendMessageAsync($"{GlobalConfiguration.BotImageStorageLink}CompareCard_{player1}_{player2ScoresaberID}_{cardId}.png");

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

        public static async Task<Guid> GetAndCreateCompareImage(ScoresaberPlayerFullModel scoresaberId1, ScoresaberPlayerFullModel scoresaberId2)
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

            var cardID = Guid.NewGuid();
            await rankingCardCreator.Create($"{GlobalConfiguration.BotImageStoragePath}CompareCard_{scoresaberId1.playerInfo.PlayerId}_{scoresaberId2.playerInfo.PlayerId}_{cardID}.png");
            return cardID;
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

            rankingCardCreator.AddImage($"https://flagpedia.net/data/flags/w580/{playerOne.Country.ToLower()}.png", 130, 50, 20, 15);
            rankingCardCreator.AddImageRounded(playerOne.Avatar.Contains("oculus") ? $"https://cdn.scoresaber.com/avatars/oculus.png" : $"https://new.scoresaber.com{playerOne.Avatar}", 15, 43, 70, 70);

            //Add player Two main info
            rankingCardCreator.AddTextFloatRight(playerTwo.Name.ToUpper(), System.Drawing.Color.White, 15, 13, 10);
            rankingCardCreator.AddTextFloatRight(playerTwo.Country.ToUpper(), System.Drawing.Color.White, 12, 100, 45);

            rankingCardCreator.AddImage($"https://flagpedia.net/data/flags/w580/{playerTwo.Country.ToLower()}.png", 350, 50, 20, 15);
            rankingCardCreator.AddImageRounded(playerTwo.Avatar.Contains("oculus") ? $"https://cdn.scoresaber.com/avatars/oculus.png" : $"https://new.scoresaber.com{playerTwo.Avatar}", 415, 43, 70, 70);


            rankingCardCreator.AddImage($"../../../Resources/img/Street_Fighter_VS_logo.png", 225, 28, 70, 70, isLocalFile: true);

            //Finish Card
            await rankingCardCreator.Create($"../../../Resources/img/UserCompareCard_{scoresaberId1}_{scoresaberId2}.png");
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
            rankingCardCreator.AddText(player.Name.ToUpper(), System.Drawing.Color.White, 20, 120, 10, "Poppins");
            rankingCardCreator.AddText(player.Country.ToUpper(), System.Drawing.Color.White, 12, 120, 45, "Poppins");
            var rankWidth = rankingCardCreator.AddText($"#{player.rank}", System.Drawing.Color.White, 12, 120, 68, "Poppins").Width;
            rankingCardCreator.AddText($"#{player.CountryRank}", System.Drawing.Color.FromArgb(176, 176, 176), 8, 120 + rankWidth, 72, "Poppins");
            rankingCardCreator.AddText($"{player.Pp}PP", System.Drawing.Color.White, 15, 120, 88, "Poppins");
            rankingCardCreator.AddTextFloatRight(topic, System.Drawing.Color.White, 15, 10, 88, "Poppins");

            rankingCardCreator.AddImage($"https://flagpedia.net/data/flags/w580/{player.Country.ToLower()}.png", 150, 50, 20, 15);
            rankingCardCreator.AddImage($"https://new.scoresaber.com{player.Avatar}", 15, 13, 100, 100);

            //Finish Card
            rankingCardCreator.Create($"../../../Resources/img/UserCard_{scoresaberId}.png");
        }

        public static async Task<Guid> GetAndCreateRecentsongsCardImage(string scoresaberId, int page = 1, bool isTopsong = false)
        {
            var api = new ScoreSaberClient().Api;

            var playerRaw = new ScoresaberAPI(scoresaberId);
            var playerRecentScores = isTopsong ? await api.Players.GetPlayerScores(Convert.ToInt64(scoresaberId), limit: 10, Players.sort.top, page: page) :  await api.Players.GetPlayerScores(Convert.ToInt64(scoresaberId), limit: 10, Players.sort.recent, page: page);

            if (playerRecentScores == null)
            {
                return Guid.Empty;
            }

            var rankingCardCreator = new ImageCreator("../../../Resources/img/RecentsongsCard-Template-new2.png");

            //Add SongInfo
            var marigin = 0;
            for (var x = 0; x < 8; x++)
            {
                //rankingCardCreator.AddImage($"https://new.scoresaber.com/api/static/covers/{playerRecentScores.Scores[x].Id}.png", 190, 10 + marigin, 1370, 180, 0.1f, blurItensity: 5);

                var fontsize = 50;
                if (playerRecentScores[x].Leaderboard.SongName.Count() > 20) fontsize = 50;
                if (playerRecentScores[x].Leaderboard.SongName.Count() > 30) fontsize = 40;
                if (playerRecentScores[x].Leaderboard.SongName.Count() > 40) fontsize = 30;
                if (playerRecentScores[x].Leaderboard.SongName.Count() > 50) fontsize = 20;

                var rankcolor = System.Drawing.Color.White;
                if (playerRecentScores[x].Score.Rank == 1) rankcolor = System.Drawing.Color.FromArgb(255, 173, 0);
                if (playerRecentScores[x].Score.Rank == 2) rankcolor = System.Drawing.Color.FromArgb(200, 209, 247);
                if (playerRecentScores[x].Score.Rank == 3) rankcolor = System.Drawing.Color.FromArgb(150, 116, 68);

                if (rankcolor != System.Drawing.Color.White) rankingCardCreator.DrawRectangle(20, marigin, 2080, 3, rankcolor);
                if (rankcolor != System.Drawing.Color.White) rankingCardCreator.DrawRectangle(20, marigin + 185, 2080, 3, rankcolor);

                rankingCardCreator.AddText($"{playerRecentScores[x].Leaderboard.SongName.ToUpper()}", rankcolor, fontsize, 220, marigin + 0, "Poppins");

                var extraSpaceForNonPP = 375;
                if (playerRecentScores[x].Score.Pp == 0) extraSpaceForNonPP = 375;

                //Rank
                rankingCardCreator.AddTextCenter($"#{playerRecentScores[x].Score.Rank}", System.Drawing.Color.Black, 73, 1435 + extraSpaceForNonPP, marigin - 5, "Poppins");
                var rankTextSize = rankingCardCreator.AddTextCenter($"#{playerRecentScores[x].Score.Rank}", rankcolor, 70, 1440 + extraSpaceForNonPP, marigin + 0, "Poppins");

                //Acc
                double percentage = Convert.ToDouble(playerRecentScores[x].Score.BaseScore) / Convert.ToDouble(playerRecentScores[x].Leaderboard.MaxScore) * 100;
                var acc = Math.Round(percentage, 2);
                rankingCardCreator.AddTextCenter($"{String.Format("{0:n}", acc)}%", rankcolor, 50, 1450 + extraSpaceForNonPP, marigin + 100, "Poppins");

                //Miss
                var misses = playerRecentScores[x].Score.MissedNotes + playerRecentScores[x].Score.BadCuts;
                rankingCardCreator.AddTextFloatRight($"{(misses > 0 ? misses.ToString() + " miss" : "Full Combo")}", rankcolor, 50, 350, marigin + 100, "Poppins");


                var ppfontsize = 60;

                if (playerRecentScores[x].Score.Pp != 0) rankingCardCreator.AddTextFloatRight($"{String.Format("{0:n}", playerRecentScores[x].Score.Pp)}PP", System.Drawing.Color.FromArgb(124, 252, 0), ppfontsize, 350, marigin + 0, "Poppins");

                rankingCardCreator.AddText($"{Math.Round((DateTime.Now - (DateTimeOffset) playerRecentScores[x].Score.TimeSet).TotalDays, 1)} days ago", System.Drawing.Color.Gray, 30, 225, marigin + 130, "Poppins");

                rankingCardCreator.AddImageRounded($"{playerRecentScores[x].Leaderboard.CoverImage}", 15, 0 + marigin, 190, 190);

                rankingCardCreator.AddText($"{playerRecentScores[x].Leaderboard.Difficulty.DifficultyRaw.Replace("_", " ").Trim().Split(" ").First().ToUpper()}", rankcolor, 30, 225, marigin + 70, "Poppins");

                marigin += 210 - (x + 1 * 1);
            }

            //Finish Card
            var cardID = Guid.NewGuid();
            if (!isTopsong) await rankingCardCreator.Create($"{GlobalConfiguration.BotImageStoragePath}RecentsongsCard_{scoresaberId}_{cardID}.png");
            else await rankingCardCreator.Create($"{GlobalConfiguration.BotImageStoragePath}TopsongsCard_{scoresaberId}_{cardID}.png");
            return cardID;
        }

        //public static async Task GetAndCreateTopsongsCardImage(string scoresaberId, int page = 1)
        //{
        //    var playerRaw = new ScoresaberAPI(scoresaberId);
        //    var playerTopScores = await playerRaw.GetTopScores(page);


        //    var rankingCardCreator = new ImageCreator("../../../Resources/img/RecentsongsCard-Template.png");

        //    //Add SongInfo
        //    var marigin = 0;
        //    for (var x = 0; x < 5; x++)
        //    {
        //        rankingCardCreator.AddText($"{playerTopScores.Scores[x].Name}", System.Drawing.Color.White, 50, 320, marigin + 40);

        //        var rankcolor = System.Drawing.Color.Gray;
        //        if (playerTopScores.Scores[x].Rank == 1) rankcolor = System.Drawing.Color.Goldenrod;
        //        if (playerTopScores.Scores[x].Rank == 2) rankcolor = System.Drawing.Color.Silver;
        //        if (playerTopScores.Scores[x].Rank == 3) rankcolor = System.Drawing.Color.SaddleBrown;

        //        var rankTextSize = rankingCardCreator.AddText($"#{playerTopScores.Scores[x].Rank}", rankcolor, 50, 320, marigin + 135);

        //        if (playerTopScores.Scores[x].Pp > 0)
        //        {
        //            double percentage = Convert.ToDouble(playerTopScores.Scores[x].UScore) / Convert.ToDouble(playerTopScores.Scores[x].MaxScoreEx) * 100;
        //            var acc = Math.Round(percentage, 2);
        //            rankingCardCreator.AddText($"{acc}%", rankcolor, 50, 350 + rankTextSize.Width, marigin + 135);
        //        }


        //        var ppfontsize = 60;
        //        if (playerTopScores.Scores[x].Pp > 300) ppfontsize = 65;
        //        if (playerTopScores.Scores[x].Pp > 400) ppfontsize = 70;
        //        if (playerTopScores.Scores[x].Pp > 500) ppfontsize = 80;

        //        if (playerTopScores.Scores[x].Pp != 0) rankingCardCreator.AddText($"+ {Math.Round(playerTopScores.Scores[x].Pp, 2)}PP", System.Drawing.Color.Green, ppfontsize, 1320, marigin + 120);
        //        rankingCardCreator.AddText($"{playerTopScores.Scores[x].GetDifficulty()}", System.Drawing.Color.White, 30, 80, marigin + 255);
        //        rankingCardCreator.AddText($"Time set: {playerTopScores.Scores[x].Timeset.DateTime.ToShortDateString()} {playerTopScores.Scores[x].Timeset.DateTime.ToShortTimeString()}", System.Drawing.Color.Gray, 30, 1000, marigin + 260);
        //        rankingCardCreator.AddTextFloatRight($"{Math.Round((DateTime.Now - playerTopScores.Scores[x].Timeset).TotalDays, 1)} days ago", System.Drawing.Color.Gray, 30, 50, marigin + 260);

        //        rankingCardCreator.AddImageRounded($"https://new.scoresaber.com/api/static/covers/{playerTopScores.Scores[x].Id}.png", 15, marigin, 250, 250);
        //        marigin += 330 - (x * 3);
        //    }

        //    //Finish Card
        //    await rankingCardCreator.Create($"{GlobalConfiguration.BotImageStoragePath}TopsongsCard_{scoresaberId}.png");
        //}

        public static async Task<Guid> GetAndCreateProfileImage(string scoresaberId)
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

            rankingCardCreator.AddImage($"https://flagpedia.net/data/flags/w580/{player.Country.ToLower()}.png", 1120, 1000, 120, 100);
            rankingCardCreator.AddNoteSlashEffect(player.Avatar.Contains("oculus") ? $"https://cdn.scoresaber.com/avatars/oculus.png" : $"https://new.scoresaber.com{player.Avatar}", 200, 800, 800, 800);

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

            var cardID = Guid.NewGuid();
            await rankingCardCreator.Create($"{GlobalConfiguration.BotImageStoragePath}RankingCard_{scoresaberId}_{cardID}.png");
            return cardID;
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