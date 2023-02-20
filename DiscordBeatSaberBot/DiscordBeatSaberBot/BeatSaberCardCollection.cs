using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using ScoreSaberLib;
using System.Drawing;
using DiscordBeatSaberBot.Api.BeatSaverApi;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Discord;
using System.Threading.Tasks;
using Color = System.Drawing.Color;
using System.Globalization;
using Discord.Rest;
using DiscordBeatSaberBot.Api.BeatSaverApi.Models.New;
using System.Net;
using DiscordBeatSaberBot.Api.BeatSaverApi.Models.v2;

namespace DiscordBeatSaberBot
{
    public class BeatSaberCardCollection
    {
        private static List<string> BanList = new List<string>()
        {
            "3433762889998282"
        };

        private DiscordSocketClient _discord;
        private bool hasNextBeenPressed = false;
        private bool tradeHasBeenAccepted = false;
        private SocketUser userToTradeWith;
        private SocketUser userToStakeWith;
        private ulong dmMessageID;
        private string stakeDifficulty;
        private SocketSlashCommand _command;

        public BeatSaberCardCollection(DiscordSocketClient discord, bool shouldInitButtonExecuted = false)
        {
            _discord = discord;
            if (shouldInitButtonExecuted) _discord.ButtonExecuted += DiscordButtonConvert;
        }

        public static async Task<bool> DrawAndSendRandomFifaCard(DiscordSocketClient discord, SocketSlashCommand command)
        {
            try
            {
                //Send opening message to channel
                var playerTimeout = GetPlayerTimeout(command.User.Id.ToString());
                if (playerTimeout.PacksLeft == 0) await command.Channel.SendMessageAsync($"Opening a pack... this is your last pack!");
                else await command.Channel.SendMessageAsync($"Opening a pack... Remaining packs: {playerTimeout.PacksLeft}");

                //Get player info
                var players = new List<ScoreSaberLib.Models.PlayerInfoModel.Player>();
                var amount = 20;
                for (var i = 1; i <= amount; i++)
                {
                    var playersData = await new ScoreSaberClient().Api.Players.GetPlayers(page: i);
                    if (playersData != null) players.AddRange(playersData.Players);
                }
                if (players.Count != 1000)
                {
                    await ErrorCase(command, "An unexpected error with Scoresaber occurred.");
                    return false;
                }
                players = players.OrderBy(x => x.Rank).ToList();
                var top10 = 100;
                var top50 = 500;
                var top100 = 2000;
                var top250 = 20000;
                var top500 = 50000;
                var top1000 = 100000;
                var nr = new Random().Next(0, 100000);
                var rangeBegin = 0;
                var rangeEnd = 0;

                if (nr < top10)
                {
                    rangeBegin = 0;
                    rangeEnd = 10;
                }
                else if (nr < top50)
                {
                    rangeBegin = 10;
                    rangeEnd = 50;
                }
                else if (nr < top100)
                {
                    rangeBegin = 50;
                    rangeEnd = 100;
                }
                else if (nr < top250)
                {
                    rangeBegin = 100;
                    rangeEnd = 250;
                }
                else if (nr < top500)
                {
                    rangeBegin = 250;
                    rangeEnd = 500;
                }
                else if (nr <= top1000)
                {
                    rangeBegin = 500;
                    rangeEnd = 1000;
                }

                var random = new Random();
                var player = players[random.Next(rangeBegin, rangeEnd)];

                //TEST ZONE
                //player = players.FirstOrDefault(x => x.Id == "76561199306816565");
                //nr = 150;

                //Prevent banned people from showing up. 
                if (BanList.Contains(player.Id))
                {
                    do
                    {
                        player = players[random.Next(rangeBegin, rangeEnd)];
                    } while (BanList.Contains(player.Id));
                }

                var playerFull = await new ScoreSaberClient().Api.Players.GetPlayer(Convert.ToInt64(player.Id));
                if (playerFull == null)
                {
                    await ErrorCase(command, "An unexpected error with Scoresaber occurred.");
                    return false;
                }

                var hashList = new List<string>();
                var scores = new List<ScoreSaberLib.Models.PlayerScoresModel.PlayerScore>();
                var scores100 = await new ScoreSaberClient().Api.Players.GetPlayerScores(Convert.ToInt64(player.Id), 100, sort: Players.sort.top, page: 1);
                var scores200 = await new ScoreSaberClient().Api.Players.GetPlayerScores(Convert.ToInt64(player.Id), 100, sort: Players.sort.top, page: 2);
                var scores100recent = await new ScoreSaberClient().Api.Players.GetPlayerScores(Convert.ToInt64(player.Id), 100, sort: Players.sort.recent, page: 1);
                scores.AddRange(scores100);
                scores.AddRange(scores200);
                scores.AddRange(scores100recent);
                foreach (var score in scores100) hashList.Add(score.Leaderboard.SongHash);
                foreach (var score in scores200) hashList.Add(score.Leaderboard.SongHash);
                foreach (var score in scores100recent) hashList.Add(score.Leaderboard.SongHash);

                //Get all played maps with their tags
                var beatSaverMaps = new List<JToken>();

                if (hashList.Count >= 50)
                {
                    JObject data = await new BeatSaverApi("").GetMapsByHash(hashList.GetRange(0, 49));
                    var maps = data.Children().Where(x => x.ToString().Contains("\"tags\""));
                    beatSaverMaps.AddRange(maps);
                }
                if (hashList.Count >= 100)
                {
                    JObject data2 = await new BeatSaverApi("").GetMapsByHash(hashList.GetRange(51, 49));
                    var maps2 = data2.Children().Where(x => x.ToString().Contains("\"tags\""));
                    beatSaverMaps.AddRange(maps2);
                }
                if (hashList.Count >= 150)
                {
                    JObject data3 = await new BeatSaverApi("").GetMapsByHash(hashList.GetRange(101, 49));
                    var maps3 = data3.Children().Where(x => x.ToString().Contains("\"tags\""));
                    beatSaverMaps.AddRange(maps3);
                }
                if (hashList.Count >= 200)
                {
                    JObject data4 = await new BeatSaverApi("").GetMapsByHash(hashList.GetRange(151, 49));
                    var maps4 = data4.Children().Where(x => x.ToString().Contains("\"tags\""));
                    beatSaverMaps.AddRange(maps4);
                }
                if (hashList.Count >= 250)
                {
                    JObject data5 = await new BeatSaverApi("").GetMapsByHash(hashList.GetRange(201, 49));
                    var maps5 = data5.Children().Where(x => x.ToString().Contains("\"tags\""));
                    beatSaverMaps.AddRange(maps5);
                }
                if (hashList.Count >= 300)
                {
                    JObject data6 = await new BeatSaverApi("").GetMapsByHash(hashList.GetRange(251, 49));
                    var maps6 = data6.Children().Where(x => x.ToString().Contains("\"tags\""));
                    beatSaverMaps.AddRange(maps6);
                }

                var hashTags = new Dictionary<string, string>();
                foreach (var map in beatSaverMaps)
                {
                    var tags = map.First().Children().Where(x => x.ToString().Contains("\"tags\""));
                    dynamic m = map;
                    if (hashTags.Keys.FirstOrDefault(x => x.Contains(m.Name)) != null) continue;
                    hashTags.Add(m.Name, tags.First().ToString());
                }

                //calculate points
                var speed = 0;
                var tech = 0;
                var stamina = 0;
                var accuracy = 0;
                var total = 0;

                var speedHashes = hashTags.Where(x => x.Value.ToLower().Contains("speed") || x.Value.ToLower().Contains("challenge") && !x.Value.ToLower().Contains("speedcore"));
                var speedCount = 0;
                foreach (var speedhash in speedHashes)
                {
                    var score = scores.FirstOrDefault(x => x.Leaderboard.SongHash.ToLower() == speedhash.Key);
                    var totalPlays = score.Leaderboard.Plays;
                    var playerRank = score.Score.Rank;
                    var points = 100 - ((int)playerRank * 100 / ((int)totalPlays));
                    var pointloss = (double)player.Rank / 2000;
                    var newpoints = (double)points - pointloss * (double)points;
                    points = (int)newpoints;
                    speedCount++;
                    speed += points;
                }
                if (speedCount > 0) speed = speed / speedCount;

                var accHashes = hashTags.Where(x => x.Value.ToLower().Contains("accuracy") || x.Value.ToLower().Contains("balanced"));
                var accCount = 0;
                foreach (var acchash in accHashes)
                {
                    var score = scores.FirstOrDefault(x => x.Leaderboard.SongHash.ToLower() == acchash.Key);
                    var totalPlays = score.Leaderboard.Plays;
                    var playerRank = score.Score.Rank;
                    var points = 100 - ((int)playerRank * 100 / ((int)totalPlays));
                    var pointloss = (double)player.Rank / 2000;
                    var newpoints = (double)points - pointloss * (double)points;
                    points = (int)newpoints;
                    accCount++;
                    accuracy += points;
                }
                if (accCount > 0) accuracy = accuracy / accCount;

                var staminaHashes = hashTags.Where(x => x.Value.ToLower().Contains("fitness") || x.Value.ToLower().Contains("dance"));
                var staminaCount = 0;
                foreach (var staminahash in staminaHashes)
                {
                    var score = scores.FirstOrDefault(x => x.Leaderboard.SongHash.ToLower() == staminahash.Key);
                    var totalPlays = score.Leaderboard.Plays;
                    var playerRank = score.Score.Rank;
                    var points = 100 - ((int)playerRank * 100 / ((int)totalPlays));
                    var pointloss = (double)player.Rank / 2000;
                    var newpoints = (double)points - pointloss * (double)points;
                    points = (int)newpoints;
                    staminaCount++;
                    stamina += points;
                }
                if (staminaCount > 0) stamina = stamina / staminaCount;

                var techHashes = hashTags.Where(x => x.Value.ToLower().Contains("tech"));
                var techCount = 0;
                foreach (var techhash in techHashes)
                {
                    var score = scores.FirstOrDefault(x => x.Leaderboard.SongHash.ToLower() == techhash.Key);
                    var totalPlays = score.Leaderboard.Plays;
                    var playerRank = score.Score.Rank;
                    var points = 100 - ((int)playerRank * 100 / ((int)totalPlays));
                    var pointloss = (double)player.Rank / 2000;
                    var newpoints = (double)points - pointloss * (double)points;
                    points = (int)newpoints;
                    techCount++;
                    tech += points;
                }
                if (techCount > 0) tech = tech / techCount;

                var amountOfStats = 4;
                if (speed == 0) amountOfStats--;
                if (accuracy == 0) amountOfStats--;
                if (tech == 0) amountOfStats--;
                if (stamina == 0) amountOfStats--;
                total = amountOfStats == 0 ? 0 : (speed + accuracy + tech + stamina) / amountOfStats;

                //Create card 
                var cardCreator = new ImageCreator("../../../Resources/img/FIFA_Card_Template2.png");
                cardCreator.AddImageRounded(player.ProfilePicture.ToString(), 0, -0, 735 * 4, 1211, 0.90f, 8);
                cardCreator.AddImageRounded(player.ProfilePicture.ToString(), 278, 250, 400, 400);
                cardCreator.AddImage("../../../Resources/img/FIFA_Card_Template2.png", 0, 0, 735, 1211, isLocalFile: true);

                if (playerFull.Badges != null)
                {
                    var badges = (JArray)playerFull.Badges;
                    if (badges.Count > 0)
                    {
                        var url = badges.Children().Last().Children().Last().Children().First().ToString();
                        cardCreator.AddImage(url, 140, 520, 80, 30);
                    }
                }

                cardCreator.AddTextCenter(total.ToString(), Color.Black, 72, 185 + 5, 175 + 5);
                cardCreator.AddTextCenter(total.ToString(), Color.FromArgb(103, 90, 55), 72, 185, 175);

                cardCreator.AddText(player.Country, Color.FromArgb(103, 90, 55), 48, 130, 285);

                cardCreator.AddImageRounded($"https://flagpedia.net/data/flags/w580/{player.Country.ToLower()}.png", 140, 415, 80, 60);

                cardCreator.AddTextCenter(speed == 0 ? "?" : speed.ToString(), Color.Black, 36, 220 + 5, 840 + 5);
                cardCreator.AddTextCenter(stamina == 0 ? "?" : stamina.ToString(), Color.Black, 36, 220 + 5, 960 + 5);
                cardCreator.AddTextCenter(tech == 0 ? "?" : tech.ToString(), Color.Black, 36, 530 + 5, 840 + 5);
                cardCreator.AddTextCenter(accuracy == 0 ? "?" : accuracy.ToString(), Color.Black, 36, 530 + 5, 960 + 5);

                cardCreator.AddTextCenter(speed == 0 ? "?" : speed.ToString(), Color.FromArgb(103, 90, 55), 36, 220, 840);
                cardCreator.AddTextCenter(stamina == 0 ? "?" : stamina.ToString(), Color.FromArgb(103, 90, 55), 36, 220, 960);
                cardCreator.AddTextCenter(tech == 0 ? "?" : tech.ToString(), Color.FromArgb(103, 90, 55), 36, 530, 840);
                cardCreator.AddTextCenter(accuracy == 0 ? "?" : accuracy.ToString(), Color.FromArgb(103, 90, 55), 36, 530, 960);


                cardCreator.AddTextCenter(player.Name, Color.Black, player.Name.Count() > 12 ? 32 : 58, 380 + 5, player.Name.Count() > 12 ? 670 + 5 : 650 + 5);
                cardCreator.AddTextCenter(player.Name, Color.FromArgb(103, 90, 55), player.Name.Count() > 12 ? 32 : 58, 380, player.Name.Count() > 12 ? 670 : 650);

                cardCreator.AddMask("../../../Resources/img/FIFA_Card_Mask.png", isLocalFile: true);

                if (nr < top10) cardCreator.AddImage("../../../Resources/img/FIFA_Card_effect_10.png", 0, 0, 735, 1211, isLocalFile: true);
                else if (nr < top50) cardCreator.AddImage("../../../Resources/img/FIFA_Card_effect_50.png", 0, 0, 735, 1211, isLocalFile: true);
                else if (nr < top100) cardCreator.AddImage("../../../Resources/img/FIFA_Card_effect_100.png", 0, 0, 735, 1211, isLocalFile: true);

                //EVENT ZONE -------------
                //var startDate = DateTime.ParseExact("2022-12-05", "yyyy-MM-dd", CultureInfo.InvariantCulture);
                //var endDate = DateTime.ParseExact("2022-12-27", "yyyy-MM-dd", CultureInfo.InvariantCulture);
                //if (DateTime.UtcNow > startDate && DateTime.UtcNow < endDate)
                //{
                //    var eventNr = random.Next(0, 100);
                //    if (eventNr <= 10) //Chance
                //    {
                //        //Give event specials
                //        var bordertypenr = random.Next(1, 4);
                //        if (bordertypenr == 1) cardCreator.AddImage("../../../Resources/img/christmas_border_2022_0.png", 0, 0, 735, 1211, isLocalFile: true);
                //        else if (bordertypenr == 2) cardCreator.AddImage("../../../Resources/img/christmas_border_2022_1.png", 0, 0, 735, 1211, isLocalFile: true);
                //        else if (bordertypenr == 3) cardCreator.AddImage("../../../Resources/img/christmas_border_2022_2.png", 0, 0, 735, 1211, isLocalFile: true);
                //    }
                //}

                //------------------------

                cardCreator.AddTextCenter("Drawn by: " + command.User.Username, Color.Black, 22, 380 + 5, 1170 + 5);
                var size = cardCreator.AddTextCenter("Drawn by: " + command.User.Username, Color.FromArgb(103, 90, 55), 22, 380, 1170);
                //cardCreator.DrawRectangle(380 / 2 - 10, 1170, Convert.ToInt32(size.Width), Convert.ToInt32(size.Height), Color.FromArgb(80, 123, 90, 55));

                var today = DateTime.Now;
                var creationTime = DateTime.UtcNow;

                //Updates user collection vote list username                 
                var json = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/CardCollectionVotes.json");
                var collectionVotes = JsonConvert.DeserializeObject<List<CardCollectionVotes>>(json);
                if (collectionVotes != null)
                {
                    if (collectionVotes.FirstOrDefault(x => x.DiscordID == command.User.Id.ToString()) == null)
                    {
                        var voted = new List<string>();
                        voted.Add(command.User.Id.ToString());
                        collectionVotes.Add(new CardCollectionVotes { DiscordID = command.User.Id.ToString(), Username = command.User.Username, AmountOfVotes = 1, DiscordIDsWhoVoted = voted });

                    }
                    else
                    {
                        var collection = collectionVotes.FirstOrDefault(x => x.DiscordID == command.User.Id.ToString());
                        collection.Username = command.User.Username;
                    }
                    var newJson = JsonConvert.SerializeObject(collectionVotes);
                    File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/CardCollectionVotes.json", newJson);
                }

                //Create gif when shiny chance 


                //Create the card
                await cardCreator.Create($"F:\\BeatSaberTradingCards/BeatSaber_Card-{command.User.Id}-{command.User.Id}-{player.Id}-{total}-{player.Rank}-{nr}-{creationTime.ToShortDateString().Replace("-", "_") + "_" + creationTime.ToShortTimeString().Replace(":", "_")}.png");

                //Make components for on the message
                var componentBuilder = new ComponentBuilder();
                var beatshardAmount = (1000 - player.Rank) * total / 1000;
                var multiplier = 1;
                if (player.Rank <= 100) multiplier = 5;
                if (player.Rank <= 50) multiplier = 10;
                if (player.Rank <= 10) multiplier = 30;
                beatshardAmount = beatshardAmount * multiplier;
                componentBuilder.WithButton(label: $"Convert to Beat Shards ({beatshardAmount})", customId: $"{command.User.Id}_ConvertCardButton_{beatshardAmount}", style: ButtonStyle.Secondary);

                //send card in discord
                await command.Channel.SendFileAsync($"F:\\BeatSaberTradingCards/BeatSaber_Card-{command.User.Id}-{command.User.Id}-{player.Id}-{total}-{player.Rank}-{nr}-{creationTime.ToShortDateString().Replace("-", "_") + "_" + creationTime.ToShortTimeString().Replace(":", "_")}.png", components: componentBuilder.Build());
                return true;
            }
            catch (Exception ex)
            {
                var ohoh = ex;
                await ErrorCase(command, "An unexpected error occurred.");
                return false;
            }
        }

        public static async Task<bool> DrawBeatSaberTradingCard(DiscordSocketClient discord, SocketSlashCommand command)
        {
            try
            {
                //Send opening message to channel
                var playerTimeout = GetPlayerTimeout(command.User.Id.ToString());
                if (playerTimeout.PacksLeft == 0) await command.Channel.SendMessageAsync($"Opening a pack... this is your last pack!");
                else await command.Channel.SendMessageAsync($"Opening a pack... Remaining packs: {playerTimeout.PacksLeft}");

                //Get all top 1000 players and pick out a random one
                var players = new List<ScoreSaberLib.Models.PlayerInfoModel.Player>();
                var amount = 20;
                for (var i = 1; i <= amount; i++)
                {
                    var playersData = await new ScoreSaberClient().Api.Players.GetPlayers(page: i);
                    if (playersData != null) players.AddRange(playersData.Players);
                }
                if (players.Count != 1000)
                {
                    await ErrorCase(command, "An unexpected error with Scoresaber occurred.");
                    return false;
                }
                players = players.OrderBy(x => x.Rank).ToList();
                var top1 = 5;
                var top10 = 50;
                var top50 = 500;
                var top100 = 2600;
                var top250 = 15000;
                var top500 = 50000;
                var top1000 = 100000;
                var nr = new Random().Next(0, 100000);
                var rangeBegin = 0;
                var rangeEnd = 0;

                if (nr <= top1)
                {
                    rangeBegin = 0;
                    rangeEnd = 1;
                }
                else if (nr < top10)
                {
                    rangeBegin = 1;
                    rangeEnd = 10;
                }
                else if (nr < top50)
                {
                    rangeBegin = 10;
                    rangeEnd = 50;
                }
                else if (nr < top100)
                {
                    rangeBegin = 50;
                    rangeEnd = 100;
                }
                else if (nr < top250)
                {
                    rangeBegin = 100;
                    rangeEnd = 250;
                }
                else if (nr < top500)
                {
                    rangeBegin = 250;
                    rangeEnd = 500;
                }
                else if (nr <= top1000)
                {
                    rangeBegin = 500;
                    rangeEnd = 1000;
                }

                var random = new Random();
                var player = players[random.Next(rangeBegin, rangeEnd)];

                //TEST ZONE
                //player = players.FirstOrDefault(x => x.Id == "76561198186151129");
                //nr = 2000;

                //Prevent banned people from showing up. 
                if (BanList.Contains(player.Id))
                {
                    do
                    {
                        player = players[random.Next(rangeBegin, rangeEnd)];
                    } while (BanList.Contains(player.Id));
                }

                //Get the player scoresaber info

                var playerFull = await new ScoreSaberClient().Api.Players.GetPlayer(Convert.ToInt64(player.Id));
                if (playerFull == null)
                {
                    await ErrorCase(command, "An unexpected error with Scoresaber occurred.");
                    return false;
                }

                //Get all scores data needed from the player
                var hashList = new List<string>();
                var scores = new List<ScoreSaberLib.Models.PlayerScoresModel.PlayerScore>();
                var scores100 = await new ScoreSaberClient().Api.Players.GetPlayerScores(Convert.ToInt64(player.Id), 100, sort: Players.sort.top, page: 1);
                var scores200 = await new ScoreSaberClient().Api.Players.GetPlayerScores(Convert.ToInt64(player.Id), 100, sort: Players.sort.top, page: 2);
                var scores100recent = await new ScoreSaberClient().Api.Players.GetPlayerScores(Convert.ToInt64(player.Id), 100, sort: Players.sort.recent, page: 1);
                scores.AddRange(scores100);
                scores.AddRange(scores200);
                scores.AddRange(scores100recent);
                foreach (var score in scores100) hashList.Add(score.Leaderboard.SongHash + "_" + score.Leaderboard.Difficulty.DifficultyRaw);
                foreach (var score in scores200) hashList.Add(score.Leaderboard.SongHash + "_" + score.Leaderboard.Difficulty.DifficultyRaw);
                foreach (var score in scores100recent) hashList.Add(score.Leaderboard.SongHash + "_" + score.Leaderboard.Difficulty.DifficultyRaw);

                //Get all scores that qualify for being looked at their stats
                var beatSaverMaps = new List<Maps>();

                if (hashList.Count >= 50)
                {
                    var maps = await new BeatSaverApi("").GetMapsByHashes(hashList.GetRange(0, 49));
                    beatSaverMaps.AddRange(maps);
                }
                if (hashList.Count >= 100)
                {
                    var maps = await new BeatSaverApi("").GetMapsByHashes(hashList.GetRange(51, 49));
                    beatSaverMaps.AddRange(maps);
                }
                if (hashList.Count >= 150)
                {
                    var maps = await new BeatSaverApi("").GetMapsByHashes(hashList.GetRange(101, 49));
                    beatSaverMaps.AddRange(maps);
                }
                if (hashList.Count >= 200)
                {
                    var maps = await new BeatSaverApi("").GetMapsByHashes(hashList.GetRange(151, 49));
                    beatSaverMaps.AddRange(maps);
                }
                if (hashList.Count >= 250)
                {
                    var maps = await new BeatSaverApi("").GetMapsByHashes(hashList.GetRange(201, 49));
                    beatSaverMaps.AddRange(maps);
                }
                if (hashList.Count >= 300)
                {
                    var maps = await new BeatSaverApi("").GetMapsByHashes(hashList.GetRange(251, 49));
                    beatSaverMaps.AddRange(maps);
                }

                scores = scores.Where(x => x.Score.Modifiers == "").ToList();

                //Calculate every skill based on the qualified scores from the player 
                var speed = 0;
                var tech = 0;
                var stamina = 0;
                var accuracy = 0;
                var total = 0;

                var mapsWithTags = beatSaverMaps.Where(x => x != null && x.Tags != null && x.Tags.Count > 0).ToList();

                var speedMaps = mapsWithTags.Where(x => x.Versions.First().Diffs.FirstOrDefault(y => x.DifficultyRaw == y.Difficulty).Nps > 10).Where(x => x.Metadata.Bpm >= 300).ToList();
                var accuracyMaps = mapsWithTags.Where(x => x.Versions.First().Diffs.FirstOrDefault(y => x.DifficultyRaw == y.Difficulty).Nps < 5).ToList();
                var staminaMaps = mapsWithTags.Where(x => x.Versions.First().Diffs.FirstOrDefault(y => x.DifficultyRaw == y.Difficulty).Notes > 1800).Where(x => x.Versions.First().Diffs.FirstOrDefault(y => x.DifficultyRaw == y.Difficulty).Seconds > 60 * 5).ToList();
                var techMaps = mapsWithTags.Where(x => x.Tags.Contains("tech")).Where(x => x.Versions.First().Diffs.FirstOrDefault(y => x.DifficultyRaw == y.Difficulty).Nps > 6).Where(x => x.Versions.First().Diffs.FirstOrDefault(y => x.DifficultyRaw == y.Difficulty).Nps < 10).ToList();

                speed = CalculateStatPoints(speedMaps);
                accuracy = CalculateStatPoints(accuracyMaps);
                stamina = CalculateStatPoints(staminaMaps);
                tech = CalculateStatPoints(techMaps);

                var speedNormalised = CalculateStatPoints(speedMaps, true);
                var accuracyNormalised = CalculateStatPoints(accuracyMaps, true);
                var staminaNormalised = CalculateStatPoints(staminaMaps, true);
                var techNormalised = CalculateStatPoints(techMaps, true);

                int CalculateStatPoints(List<Maps> statHashes, bool normalize = false)
                {
                    if (statHashes.Count < 3) return 0;

                    var count = 0;
                    var stat = 0;
                    foreach (var stathash in statHashes)
                    {
                        if (stathash.Versions != null)
                        {
                            var score = scores.FirstOrDefault(x => x.Leaderboard.SongHash.ToLower() == stathash.Versions.First().Hash.ToLower() && x.Leaderboard.Plays > 150);
                            if (score != null)
                            {
                                var totalPlays = score.Leaderboard.Plays;
                                var playerRank = score.Score.Rank;
                                var points = ((double)totalPlays - playerRank) * 100 / (totalPlays - 1);
                                if (normalize)
                                {
                                    var pointloss = (double)player.Rank / 2000;
                                    var newpoints = (double)points - pointloss * (double)points;
                                    points = (int)newpoints;
                                }
                                count++;
                                stat += (int) points;
                            }
                        }
                    }
                    if (count > 0) stat = stat / count;

                    return stat;
                }

                if(speed != 0) speed = normalise(speed);
                if (accuracy != 0) accuracy = normalise(accuracy);
                if (stamina != 0) stamina = normalise(stamina);
                if (tech != 0) tech = normalise(tech);

                int normalise(int stat)
                {
                    var diff = (double)(100 - stat);
                    var min = (diff / 100 * 30);
                    var final = stat - min;
                    return (int) Math.Round(final);
                }

                var amountOfStats = 4;
                if (speed == 0) amountOfStats--;
                if (accuracy == 0) amountOfStats--;
                if (tech == 0) amountOfStats--;
                if (stamina == 0) amountOfStats--;

                total = amountOfStats == 0 ? 0 : (speedNormalised + accuracyNormalised + techNormalised + staminaNormalised) / amountOfStats;

                //speed = expandStat(speed, total);
                //stamina = expandStat(stamina, total);
                //accuracy = expandStat(accuracy, total);
                //tech = expandStat(tech, total);

                //int expandStat(int stat, int statTotal)
                //{
                //    if(stat != 0) {
                //        if (statTotal < stat)
                //        {
                //            var diff = statTotal - stat;
                //            diff += 1;
                //            var extra = diff * 2;
                //            stat = stat + extra;
                //        }
                //        else if(statTotal > stat)
                //        {
                //            var diff = stat - statTotal;
                //            diff -= 1;
                //            var extra = diff * 2;
                //            stat = stat - extra;
                //        }
                //    }
                //    if (stat > 99) stat = 99;
                //     return stat;
                //}

                if (nr <= 5) total += 1;

                var isShiny = false;
                if (random.Next(0, 100000) <= 750) isShiny = true;

                //All data is finished. Create the card.
                var cardCreator = new ImageCreator("../../../Resources/img/TradingCardsv2_template.png");
                cardCreator.AddImage(player.ProfilePicture.ToString(), 0, -0, 735 * 6, (int) (1211 * 1.3), blurItensity: 12);
                if (isShiny) cardCreator.ApplyShinyEffect();
                var backgroundImage = cardCreator.AddImage(player.ProfilePicture.ToString(), 285, 250, 400, 400, cornerRadius: 25);
                //cardCreator.AddImage("../../../Resources/img/TradingCardsv2_template.png", 0, 0, 735, 1211, isLocalFile: true);

                //Calculate the average image pixel


                var fontType = "Poppins";
                var averageColor = cardCreator.GetAverageImagePixel(backgroundImage);
                var frontcolor = averageColor;
                var backcolor = cardCreator.GetBackgroundContrast(averageColor);
                var highlightColor = cardCreator.GetHighlightColor(frontcolor);
                var shadowDistance = 5;

                cardCreator.AddMask("../../../Resources/img/TradingCardsv2_mask.png", isLocalFile: true);

                if (nr <= top1) cardCreator.AddImage($"../../../Resources/img/TradingCardsv2_1{(isShiny ? "_noglow": "")}.png", 0, 0, 735, 1211, isLocalFile: true);
                else if (nr < top10) cardCreator.AddImage($"../../../Resources/img/TradingCardsv2_10{(isShiny ? "_noglow" : "")}.png", 0, 0, 735, 1211, isLocalFile: true);
                else if (nr < top50) cardCreator.AddImage($"../../../Resources/img/TradingCardsv2_50{(isShiny ? "_noglow" : "")}.png", 0, 0, 735, 1211, isLocalFile: true);
                else if (nr < top100) cardCreator.AddImage($"../../../Resources/img/TradingCardsv2_100{(isShiny ? "_noglow" : "")}.png", 0, 0, 735, 1211, isLocalFile: true);
                else if (nr < top250) cardCreator.AddImage("../../../Resources/img/TradingCardsv2_250.png", 0, 0, 735, 1211, isLocalFile: true);
                else if (nr < top500) cardCreator.AddImage("../../../Resources/img/TradingCardsv2_500.png", 0, 0, 735, 1211, isLocalFile: true);
                else if (nr < top1000) cardCreator.AddImage("../../../Resources/img/TradingCardsv2_1000.png", 0, 0, 735, 1211, isLocalFile: true);

                if (playerFull.Badges != null)
                {
                    var badges = (JArray)playerFull.Badges;
                    if (badges.Count > 0)
                    {
                        var url = badges.Children().Last().Children().Last().Children().First().ToString();
                        cardCreator.AddImage(url, 150, 485, 100, 35);
                    }
                }

                cardCreator.AddTextCenter(total.ToString(), backcolor, 72, 200 + shadowDistance, 175 + shadowDistance, fontstyle: fontType, useAntiAlias: true);
                cardCreator.AddTextCenter(total.ToString(), frontcolor, 72, 200, 175, fontstyle: fontType, textStrokeColor: highlightColor, useAntiAlias: true);

                cardCreator.AddTextCenter(player.Country, backcolor, 48, 200 + shadowDistance, 285 + shadowDistance, fontstyle: fontType, useAntiAlias: true);
                cardCreator.AddTextCenter(player.Country, frontcolor, 48, 200, 285, fontstyle: fontType, textStrokeColor: highlightColor, useAntiAlias: true);

                cardCreator.AddImageRounded($"https://flagpedia.net/data/flags/w580/{player.Country.ToLower()}.png", 150, 390, 100, 60);

                cardCreator.AddTextCenter(speed == 0 ? "?" : speed.ToString(), backcolor, 42, 200 + shadowDistance, 825 + shadowDistance, fontstyle: fontType, useAntiAlias: true);
                cardCreator.AddTextCenter(stamina == 0 ? "?" : stamina.ToString(), backcolor, 42, 200 + shadowDistance, 945 + shadowDistance, fontstyle: fontType, useAntiAlias: true);
                cardCreator.AddTextCenter(tech == 0 ? "?" : tech.ToString(), backcolor, 42, 530 + shadowDistance, 825 + shadowDistance, fontstyle: fontType, useAntiAlias: true);
                cardCreator.AddTextCenter(accuracy == 0 ? "?" : accuracy.ToString(), backcolor, 42, 530 + shadowDistance, 945 + shadowDistance, fontstyle: fontType, useAntiAlias: true);

                cardCreator.AddTextCenter(speed == 0 ? "?" : speed.ToString(), frontcolor, 42, 200, 825, fontstyle: fontType, textStrokeColor: highlightColor, useAntiAlias: true);
                cardCreator.AddTextCenter(stamina == 0 ? "?" : stamina.ToString(), frontcolor, 42, 200, 945, fontstyle: fontType, textStrokeColor: highlightColor, useAntiAlias: true);
                cardCreator.AddTextCenter(tech == 0 ? "?" : tech.ToString(), frontcolor, 42, 530, 825, fontstyle: fontType, textStrokeColor: highlightColor, useAntiAlias: true);
                cardCreator.AddTextCenter(accuracy == 0 ? "?" : accuracy.ToString(), frontcolor, 42, 530, 945, fontstyle: fontType, textStrokeColor: highlightColor, useAntiAlias: true);

                cardCreator.AddTextCenter("SPEED", frontcolor, 27, 200 + 3, 825 - 40 + 3, fontstyle: fontType, useAntiAlias: true);
                cardCreator.AddTextCenter("STAMINA", frontcolor, 27, 202 + 3, 945 - 40 + 3, fontstyle: fontType, useAntiAlias: true);
                cardCreator.AddTextCenter("TECH", frontcolor, 27, 530 + 3, 825 - 40 + 3, fontstyle: fontType, useAntiAlias: true);
                cardCreator.AddTextCenter("ACCURACY", frontcolor, 27, 530 + 3, 945 - 40 + 3, fontstyle: fontType, useAntiAlias: true);

                cardCreator.AddTextCenter("SPEED", backcolor, 27, 200, 825 - 40, fontstyle: fontType, textStrokeColor: highlightColor, useAntiAlias: true);
                cardCreator.AddTextCenter("STAMINA", backcolor, 27, 202, 945 - 40, fontstyle: fontType, textStrokeColor: highlightColor, useAntiAlias: true);
                cardCreator.AddTextCenter("TECH", backcolor, 27, 530, 825 - 40, fontstyle: fontType, textStrokeColor: highlightColor, useAntiAlias: true);
                cardCreator.AddTextCenter("ACCURACY", backcolor, 27, 530, 945 - 40, fontstyle: fontType, textStrokeColor: highlightColor, useAntiAlias: true);

                cardCreator.AddTextCenter(player.Name.ToUpper(), backcolor, 58, 370 + shadowDistance, 655 + shadowDistance, fontstyle: fontType, maxWidth: 500, useAntiAlias: true);
                cardCreator.AddTextCenter(player.Name.ToUpper(), frontcolor, 58, 370, 655, fontstyle: fontType, textStrokeColor: highlightColor, maxWidth: 500, useAntiAlias: true);              

                cardCreator.AddTextCenter("Drawn by: " + command.User.Username, Color.Black, 22, 380 + 5, 1170 + 5, fontstyle: fontType);
                var size = cardCreator.AddTextCenter("Drawn by: " + command.User.Username, Color.FromArgb(103, 90, 55), 22, 380, 1170, fontstyle: fontType);

                //EVENT ZONE -------------
                //var startDate = DateTime.ParseExact("2022-12-05", "yyyy-MM-dd", CultureInfo.InvariantCulture);
                //var endDate = DateTime.ParseExact("2022-12-27", "yyyy-MM-dd", CultureInfo.InvariantCulture);
                //if (DateTime.UtcNow > startDate && DateTime.UtcNow < endDate)
                //{
                //    var eventNr = random.Next(0, 100);
                //    if (eventNr <= 10) //Chance
                //    {
                //        //Give event specials
                //        var bordertypenr = random.Next(1, 4);
                //        if (bordertypenr == 1) cardCreator.AddImage("../../../Resources/img/christmas_border_2022_0.png", 0, 0, 735, 1211, isLocalFile: true);
                //        else if (bordertypenr == 2) cardCreator.AddImage("../../../Resources/img/christmas_border_2022_1.png", 0, 0, 735, 1211, isLocalFile: true);
                //        else if (bordertypenr == 3) cardCreator.AddImage("../../../Resources/img/christmas_border_2022_2.png", 0, 0, 735, 1211, isLocalFile: true);
                //    }
                //}

                //------------------------

                //cardCreator.DrawRectangle(380 / 2 - 10, 1170, Convert.ToInt32(size.Width), Convert.ToInt32(size.Height), Color.FromArgb(80, 123, 90, 55));

                var today = DateTime.Now;
                var creationTime = DateTime.UtcNow;

                //Updates user collection vote list and usernames                 
                var json = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/CardCollectionVotes.json");
                var collectionVotes = JsonConvert.DeserializeObject<List<CardCollectionVotes>>(json);
                if (collectionVotes != null)
                {
                    if (collectionVotes.FirstOrDefault(x => x.DiscordID == command.User.Id.ToString()) == null)
                    {
                        var voted = new List<string>();
                        voted.Add(command.User.Id.ToString());
                        collectionVotes.Add(new CardCollectionVotes { DiscordID = command.User.Id.ToString(), Username = command.User.Username, AmountOfVotes = 1, DiscordIDsWhoVoted = voted });

                    }
                    else
                    {
                        var collection = collectionVotes.FirstOrDefault(x => x.DiscordID == command.User.Id.ToString());
                        collection.Username = command.User.Username;
                    }
                    var newJson = JsonConvert.SerializeObject(collectionVotes);
                    File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/CardCollectionVotes.json", newJson);
                }

                //Create gif when shiny chance 

                //Store the card in the database
                if(isShiny) await cardCreator.CreateAsGifWithShine($"F:\\Test/BeatSaber_Card-{command.User.Id}-{command.User.Id}-{player.Id}-{total}-{player.Rank}-{nr}-{creationTime.ToShortDateString().Replace("-", "_") + "_" + creationTime.ToShortTimeString().Replace(":", "_")}.gif", frontcolor);
                else await cardCreator.Create($"F:\\Test/BeatSaber_Card-{command.User.Id}-{command.User.Id}-{player.Id}-{total}-{player.Rank}-{nr}-{creationTime.ToShortDateString().Replace("-", "_") + "_" + creationTime.ToShortTimeString().Replace(":", "_")}.png");


                //Make component buttons for on the message
                var componentBuilder = new ComponentBuilder();
                var beatshardAmount = (1000 - player.Rank) * total / 1000;
                double multiplier = 1;
                if (player.Rank <= 500) multiplier = 1.5;
                if (player.Rank <= 250) multiplier = 1.8;
                if (player.Rank <= 100) multiplier = 5;
                if (player.Rank <= 50) multiplier = 10;
                if (player.Rank <= 10) multiplier = 30;
                if (player.Rank <= 1) multiplier = 60;
                beatshardAmount = beatshardAmount * (int)multiplier;
                if (beatshardAmount <= 3) beatshardAmount = 3;

                componentBuilder.WithButton(label: $"Convert to Beat Shards ({beatshardAmount})", customId: $"{command.User.Id}_ConvertCardButton_{beatshardAmount}", style: ButtonStyle.Secondary);

                //send card in discord
                if(isShiny) await command.Channel.SendFileAsync($"F:\\Test/BeatSaber_Card-{command.User.Id}-{command.User.Id}-{player.Id}-{total}-{player.Rank}-{nr}-{creationTime.ToShortDateString().Replace("-", "_") + "_" + creationTime.ToShortTimeString().Replace(":", "_")}.gif", components: componentBuilder.Build());
                else await command.Channel.SendFileAsync($"F:\\Test/BeatSaber_Card-{command.User.Id}-{command.User.Id}-{player.Id}-{total}-{player.Rank}-{nr}-{creationTime.ToShortDateString().Replace("-", "_") + "_" + creationTime.ToShortTimeString().Replace(":", "_")}.png", components: componentBuilder.Build());

                return true;
            }
            catch (Exception ex)
            {
                var ohoh = ex;
                await ErrorCase(command, "An unexpected error occurred.");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private async Task DiscordButtonConvert(SocketMessageComponent button)
        {
            if (button.Data.CustomId.Contains("ConvertCardButton"))
            {
                try
                {
                    if (button.User.Id.ToString() == button.Message.Components.First().Components.First().CustomId.Split("_").First()) //Check if user is the one that got the card
                    {
                        var cardName = button.Message.Attachments.FirstOrDefault().Filename;
                        var allCards = GetAllCards();
                        var card = allCards.FirstOrDefault(x => x.Name == "/BeatSaberTradingCards/" + cardName);

                        if (card != null)
                        {
                            if (card.OwnerDiscordID == button.User.Id.ToString())
                            {
                                var shards = button.Data.CustomId.Split("_").Last();
                                var beatShardsJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/BSTCBeatShards.json");
                                var playersBeatShardsList = JsonConvert.DeserializeObject<Dictionary<string, long>>(beatShardsJson);

                                if (playersBeatShardsList != null)
                                {
                                    if (playersBeatShardsList.ContainsKey(button.User.Id.ToString()))
                                    {
                                        //Get and add points
                                        var playerShards = playersBeatShardsList[button.User.Id.ToString()];
                                        playersBeatShardsList[button.User.Id.ToString()] = playerShards + Convert.ToInt64(shards);
                                        //Delete card
                                        System.IO.File.Delete("F://" + card.Name);

                                    }
                                    else //First time converting
                                    {
                                        playersBeatShardsList.Add(button.User.Id.ToString(), Convert.ToInt64(shards));
                                        //Delete card
                                        System.IO.File.Delete("F://" + card.Name);
                                    }

                                    var newJson = JsonConvert.SerializeObject(playersBeatShardsList);
                                    System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/BSTCBeatShards.json", newJson);
                                }
                                else
                                {
                                    return;
                                }

                                await button.UpdateAsync(x => x.Content = $"This card has been converted into **{shards}** beat shards. You now have **{playersBeatShardsList[button.User.Id.ToString()]}** Beat Shards. `/Shop`");
                                await button.ModifyOriginalResponseAsync(x => x.Components = new ComponentBuilder().Build());
                            }
                        }
                        else
                        {
                            await button.UpdateAsync(x => x.Content = $"This card doesn't exist anymore");
                            await button.ModifyOriginalResponseAsync(x => x.Components = new ComponentBuilder().Build());
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Card Convert Button Handle error!!!");
                    return;
                }
            }
            return;
        }

        public async static Task ErrorCase(SocketSlashCommand command, string message)
        {
            var jsonGlobal = System.IO.File.ReadAllText($"../../../Resources/DrawCardTimeOut.json");
            var playerTimeOuts = JsonConvert.DeserializeObject<List<PLayerTimeOut>>(jsonGlobal);
            var user = playerTimeOuts.First(x => x.DiscordID == command.User.Id.ToString());
            user.PacksLeft += 1;
            user.TimeOutTill = null;
            System.IO.File.WriteAllText($"../../../Resources/DrawCardTimeOut.json", JsonConvert.SerializeObject(playerTimeOuts));
            await command.Channel.SendMessageAsync(message + "**\nyou got 1 pack back in return.**");
        }

        public static int CheckPlayerPackAmount(string discordID)
        {
            var jsonGlobal = System.IO.File.ReadAllText($"../../../Resources/DrawCardTimeOut.json");
            var playerTimeOuts = JsonConvert.DeserializeObject<List<PLayerTimeOut>>(jsonGlobal);

            var player = playerTimeOuts.FirstOrDefault(x => x.DiscordID == discordID);
            if (player == null) return 0;
            return player.PacksLeft;
        }

        public static bool IsPlayerTimedOut(DateTime timeOfRequest, SocketSlashCommand command)
        {
            var jsonGlobal = System.IO.File.ReadAllText($"../../../Resources/DrawCardTimeOut.json");
            var playerTimeOuts = JsonConvert.DeserializeObject<List<PLayerTimeOut>>(jsonGlobal);

            if (playerTimeOuts == null)
            {
                playerTimeOuts = new List<PLayerTimeOut>();
                var player = new PLayerTimeOut() { DiscordID = command.User.Id.ToString(), PacksLeft = 3, TimeOutTill = null };
                player.PacksLeft--;
                playerTimeOuts.Add(player);
                System.IO.File.WriteAllText($"../../../Resources/DrawCardTimeOut.json", JsonConvert.SerializeObject(playerTimeOuts));
                return false;
            }

            if (playerTimeOuts.FirstOrDefault(x => x.DiscordID == command.User.Id.ToString()) == null)
            {
                var player = new PLayerTimeOut() { DiscordID = command.User.Id.ToString(), PacksLeft = 3, TimeOutTill = null };
                player.PacksLeft--;
                playerTimeOuts.Add(player);
                System.IO.File.WriteAllText($"../../../Resources/DrawCardTimeOut.json", JsonConvert.SerializeObject(playerTimeOuts));
                return false;
            }

            if (playerTimeOuts.FirstOrDefault(x => x.DiscordID == command.User.Id.ToString()) != null)
            {
                var player = playerTimeOuts.FirstOrDefault(x => x.DiscordID == command.User.Id.ToString());
                if (player.PacksLeft > 1) // Has packs, no timer
                {
                    player.PacksLeft--;
                    System.IO.File.WriteAllText($"../../../Resources/DrawCardTimeOut.json", JsonConvert.SerializeObject(playerTimeOuts));
                    return false;
                }
                if (player.PacksLeft == 1) // Last pack, add timer
                {
                    player.PacksLeft--;
                    if (/*player.TimeOutTill < DateTime.Now ||*/ player.TimeOutTill == null)
                    {
                        var timeTillTimeOut = timeOfRequest.AddHours(23);
                        player.TimeOutTill = timeTillTimeOut;
                    }
                    System.IO.File.WriteAllText($"../../../Resources/DrawCardTimeOut.json", JsonConvert.SerializeObject(playerTimeOuts));
                    return false;
                }
                if (player.PacksLeft == 0) //No packs 
                {
                    if (player.TimeOutTill == null) // No timer
                    {
                        player.PacksLeft += 3;
                        System.IO.File.WriteAllText($"../../../Resources/DrawCardTimeOut.json", JsonConvert.SerializeObject(playerTimeOuts));
                        return false;
                    }
                    TimeSpan timeToWait = (DateTime)player.TimeOutTill - timeOfRequest;
                    if (timeToWait.TotalSeconds < 0) //Has timer but ran out
                    {
                        player.TimeOutTill = null;
                        player.PacksLeft += 3;
                        System.IO.File.WriteAllText($"../../../Resources/DrawCardTimeOut.json", JsonConvert.SerializeObject(playerTimeOuts));
                        return false;
                    }
                    else // Has timer
                    {
                        command.Channel.SendMessageAsync($"No remaining packs. You will get new packs in {timeToWait.Hours} hours, {timeToWait.Minutes} minutes and {timeToWait.Seconds} seconds.");
                        if (timeToWait.TotalHours > 7) command.Channel.SendMessageAsync("Reminder: you can get pack notifications in DM by using the `/tradingcards settings` command. \nYou could also compete in the Cup of the day (https://beatsaberbot.com/CupOfTheDay) and earn free packs. Additionally, you can convert cards and buy new packs in `/shop`");
                        return true;
                    }
                }
            }
            return true;
        }

        public static PLayerTimeOut GetPlayerTimeout(string discordID)
        {
            var jsonGlobal = System.IO.File.ReadAllText($"../../../Resources/DrawCardTimeOut.json");
            var playerTimeOuts = JsonConvert.DeserializeObject<List<PLayerTimeOut>>(jsonGlobal);
            var player = playerTimeOuts.FirstOrDefault(x => x.DiscordID == discordID);

            return player;
        }

        public class PLayerTimeOut
        {
            public string DiscordID { get; set; }
            public DateTime? TimeOutTill { get; set; }
            public int PacksLeft { get; set; }
        }

        public static List<Card> GetAllCards()
        {
            string[] files = Directory.GetFiles(@$"F:\\BeatSaberTradingCards");
            var cards = new List<Card>();
            foreach (var file in files)
            {
                var parameters = file.Split("-");
                try
                {
                    var card = new Card()
                    {
                        Name = file.Replace("F:\\", "").Replace("\\", "/"),
                        DiscordID = parameters[1],
                        OwnerDiscordID = parameters[2],
                        ScoresaberID = parameters[3],
                        Score = Convert.ToInt32(parameters[4]),
                        rank = Convert.ToInt32(parameters[5]),
                        luckNumber = Convert.ToInt32(parameters[6].Replace(".png", "").Replace(".gif", ""))
                    };
                    cards.Add(card);
                }
                catch (Exception ex)
                {
                }
            }

            return cards;
        }

        public async void ShowInventory(SocketSlashCommand command)
        {
            _command = command;

            var cards = GetAllCards();

            if (cards.Where(x => x.OwnerDiscordID == command.User.Id.ToString()).Count() > 0)
            {
                var json = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/CardCollectionVotes.json");
                var collectionVotes = JsonConvert.DeserializeObject<List<CardCollectionVotes>>(json);

                var hasVotes = false;
                if (collectionVotes != null) hasVotes = collectionVotes.Where(x => x.DiscordID == command.User.Id.ToString()).Count() > 0;

                var inventoryLink = CreateInventoryImage(cards, command.User.Id.ToString());

                var embed = EmbedBuilderExtension.NullEmbed($"{command.User.Username}'s Card Inventory",
                    $"Amount of cards: **{cards.Where(x => x.OwnerDiscordID == command.User.Id.ToString()).Count()}**\n" +
                    $"Amount of top 10 cards: **{cards.Where(x => x.OwnerDiscordID == command.User.Id.ToString() && x.rank <= 10).Count()}**\n" +
                    $"Amount of top 50 cards: **{cards.Where(x => x.OwnerDiscordID == command.User.Id.ToString() && x.rank <= 50).Count()}**\n" +
                    $"Amount of top 100 cards: **{cards.Where(x => x.OwnerDiscordID == command.User.Id.ToString() && x.rank <= 100).Count()}**\n" +
                    $"Best Luck Number: **{cards.OrderBy(x => x.luckNumber).Where(x => x.DiscordID == command.User.Id.ToString()).First().luckNumber}** (*Lower is better)\n" +
                    $"Highest Score Card: **{cards.OrderByDescending(x => x.Score).Where(x => x.OwnerDiscordID == command.User.Id.ToString()).First().Score}**\n " +
                    $"Total Collection value: **{cards.Where(x => x.OwnerDiscordID == command.User.Id.ToString()).Sum(x => (1000 - x.rank) * x.Score)} points**\n" +
                    $"Best Rank Card: **{cards.OrderBy(x => x.rank).Where(x => x.OwnerDiscordID == command.User.Id.ToString()).First().rank}**\n" +
                    $"Amount of collection votes: **{(hasVotes ? collectionVotes.FirstOrDefault(x => x.DiscordID == command.User.Id.ToString()).AmountOfVotes : 0)}**" +
                    $"\n\n\n" +
                    $"-Best card-");
                embed.Url = $"http://beatsaberbot.com/BeatSaberCards?rankingPageSelectRanking=OwnedDiscordID&searchinput={command.User.Id}#inventory";
                embed.ImageUrl = inventoryLink;
                embed.ThumbnailUrl = command.User.GetAvatarUrl();

                var componentBuilder = new ComponentBuilder();
                componentBuilder.WithButton("Vote 👍", customId: "CardCollectionVoteButton", style: ButtonStyle.Secondary);
                componentBuilder.WithButton("Trade 🔄", customId: "CardCollectionTradeButton", style: ButtonStyle.Secondary);
                componentBuilder.WithButton("Stake 🎲", customId: "CardCollectionStakeButton", style: ButtonStyle.Secondary);

                _discord.ButtonExecuted += VoteButtonExecute;

                try
                {
                    var message = await command.Channel.SendMessageAsync("", false, embed.Build(), components: componentBuilder.Build());
                    dmMessageID = message.Id;
                }
                catch (Exception)
                {

                }
            }
            else
            {
                await command.Channel.SendMessageAsync("You don't have any cards yet");
            }
        }

        public string CreateInventoryImage(List<Card> cards, string ownerDiscordId)
        {
            var json = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/HighlightedCardCollection.json");
            var highlighted = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);

            var cardCreator = new ImageCreator("../../../Resources/img/InventoryTemplate.png");


            try
            {
                var userCards = cards.Where(x => x.OwnerDiscordID == ownerDiscordId);
                var bestCard = "F://" + cards.Where(x => x.OwnerDiscordID == ownerDiscordId).OrderByDescending(x => (1000 - x.rank) / 2 * x.Score).First().Name;
                cardCreator.AddImage(bestCard, 380, 100, 735 * 2, 1211 * 2, 1, true);

                var hasHighlightedCards = false;
                if (highlighted.Keys.Contains(ownerDiscordId))
                {
                    var allHighlightedCards = highlighted.FirstOrDefault(x => x.Key == ownerDiscordId);
                    var allHighlightedCardsFinal = allHighlightedCards.Value.Where(x => userCards.FirstOrDefault(z => z.Name.Replace("/", "").Replace(@"\", "") == x.Replace("/", "").Replace(@"\", "")) != null);
                    if (allHighlightedCardsFinal.Count() > 0) hasHighlightedCards = true;
                    else hasHighlightedCards = false;
                }

                if (hasHighlightedCards)
                {
                    var ownedCards = cards.Where(x => x.OwnerDiscordID == ownerDiscordId);
                    var ownendHighlighted = highlighted.FirstOrDefault(x => x.Key == ownerDiscordId);
                    cards = ownedCards.Where(x => ownendHighlighted.Value.FirstOrDefault(y => y.Split("BeatSaber_Card").Last() == x.Name.Split("BeatSaber_Card").Last()) != null).OrderByDescending(x => (1000 - x.rank) * x.Score).ToList();
                }
                else
                {
                    cards = cards.Where(x => x.OwnerDiscordID == ownerDiscordId).OrderByDescending(x => (1000 - x.rank) * x.Score).ToList();
                }

                var count = 0;
                for (var i = 0; i < 3; i++)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        cardCreator.AddImage("F://" + cards[count].Name, 2300 + 600 * i, 400 + 780 * j, Convert.ToInt32(735 / 1.45), Convert.ToInt32(1211 / 1.45), 1, true);
                        count++;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            var guid = Guid.NewGuid();
            cardCreator.Create(GlobalConfiguration.WebsiteRoot + $"img/BeatSaberBot/{ownerDiscordId}-{guid}.png");

            return $"https://beatsaberbot.com/img/BeatSaberBot/{ownerDiscordId}-{guid}.png";
        }

        private async Task VoteButtonExecute(SocketMessageComponent arg)
        {
            try
            {
                if (arg.Message.Id == dmMessageID)
                {
                    if (arg.Data.CustomId == "CardCollectionVoteButton")
                    {
                        var json = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/CardCollectionVotes.json");
                        var collectionVotes = JsonConvert.DeserializeObject<List<CardCollectionVotes>>(json);

                        if (collectionVotes == null)
                        {
                            var cv = new List<CardCollectionVotes>();
                            var voted = new List<string>();
                            voted.Add(arg.User.Id.ToString());
                            cv.Add(new CardCollectionVotes() { DiscordID = arg.Message.Embeds.First().Url.Split("=").Last(), Username = _discord.GetUser(Convert.ToUInt64(arg.Message.Embeds.First().Url.Split("=").Last())).Username, AmountOfVotes = 1, DiscordIDsWhoVoted = voted });
                            collectionVotes = cv;
                        }
                        else
                        {
                            //Check if collection id is in the list already
                            if (collectionVotes.FirstOrDefault(x => x.DiscordID == arg.Message.Embeds.First().Url.Split("=").Last()) == null)
                            {
                                var voted = new List<string>();
                                voted.Add(arg.User.Id.ToString());
                                collectionVotes.Add(new CardCollectionVotes { DiscordID = arg.Message.Embeds.First().Url.Split("=").Last(), Username = _discord.GetUser(Convert.ToUInt64(arg.Message.Embeds.First().Url.Split("=").Last())).Username, AmountOfVotes = 1, DiscordIDsWhoVoted = voted });
                            }
                            else // user is in the list
                            {
                                var collection = collectionVotes.FirstOrDefault(x => x.DiscordID == arg.Message.Embeds.First().Url.Split("=").Last());
                                if (!collection.DiscordIDsWhoVoted.Contains(arg.User.Id.ToString()))
                                {
                                    collection.AmountOfVotes += 1;
                                    collection.DiscordIDsWhoVoted.Add(arg.User.Id.ToString());
                                }
                                collection.Username = _discord.GetUser(Convert.ToUInt64(arg.Message.Embeds.First().Url.Split("=").Last())).Username;
                            }
                        }

                        var newJson = JsonConvert.SerializeObject(collectionVotes);
                        File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/CardCollectionVotes.json", newJson);
                    }
                    if (arg.Data.CustomId == "CardCollectionTradeButton")
                    {
                        new BeatSaberCardCollection(_discord).StartTradeProcess(_command, _command.User, arg.User);
                    }
                    if (arg.Data.CustomId == "CardCollectionStakeButton")
                    {
                        new BeatSaberCardCollection(_discord).StartStakeProcess(_command, _command.User, arg.User);
                    }

                    _discord.ButtonExecuted -= VoteButtonExecute;
                }
                return;
            }
            catch (Exception ex)
            {
                _discord.ButtonExecuted -= VoteButtonExecute;
                return;
            }
        }

        public class CardCollectionVotes
        {
            public string DiscordID { get; set; }
            public string Username { get; set; }
            public int AmountOfVotes { get; set; }
            public List<string> DiscordIDsWhoVoted { get; set; }
        }

        public async Task StartStakeProcess(SocketSlashCommand command, SocketUser userToStakeWith, SocketUser stakingUser)
        {
            try
            {
                this.userToStakeWith = userToStakeWith;
                var tradelimits = JsonConvert.DeserializeObject<List<TradeLimit>>(System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/TradeLimit.json"));
                if (tradelimits == null) tradelimits = new List<TradeLimit>();

                if (userToStakeWith.Id == stakingUser.Id && stakingUser.Id != 138439306774577152)
                {
                    var cantTradeMessage = await command.Channel.SendMessageAsync("Can't stake with yourself");
                    await Task.Delay(5000);
                    await cantTradeMessage.DeleteAsync();
                    return;
                }

                //Trade limit check
                if (tradelimits.FirstOrDefault(x => x.DiscordID == userToStakeWith.Id.ToString()) == null) tradelimits.Add(new TradeLimit() { DiscordID = userToStakeWith.Id.ToString(), CardsLeftToTrade = 5 });
                if (tradelimits.FirstOrDefault(x => x.DiscordID == stakingUser.Id.ToString()) == null) tradelimits.Add(new TradeLimit() { DiscordID = stakingUser.Id.ToString(), CardsLeftToTrade = 5 });
                if (tradelimits.FirstOrDefault(x => x.DiscordID == userToStakeWith.Id.ToString()).CardsLeftToTrade <= 0)
                {
                    await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Staking is not possible (Trade Limit)", $"You are not able to trade with this user because this user reached its daily trade limit").Build());
                    return;
                }
                if (tradelimits.FirstOrDefault(x => x.DiscordID == stakingUser.Id.ToString()).CardsLeftToTrade <= 0)
                {
                    await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Staking is not possible (Trade Limit)", $"You are not able to trade because you reached your daily trade limit").Build());
                    return;
                }
                var tradeJson = JsonConvert.SerializeObject(tradelimits);
                File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/TradeLimit.json", tradeJson);

                //linked check
                if (!await new RoleAssignment(_discord).CheckIfDiscordIdIsLinked(stakingUser.Id.ToString()))
                {
                    await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Staking is not possible (Not Linked)", $"You are not able to stake because you need to link your Scoresaber account with the `/link` command").Build());
                    return;
                }

                if (!await new RoleAssignment(_discord).CheckIfDiscordIdIsLinked(userToStakeWith.Id.ToString()))
                {
                    await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Staking is not possible (Not Linked)", $"{userToStakeWith.Username} is not able to stake because its account need to be linked with Scoresaber with the `/link` command").Build());
                    return;
                }

                if (userToStakeWith.CreatedAt > DateTime.Now.AddDays(-14))
                {
                    await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Staking is not possible (New Account)", $"You are not able to stake with this user because the account is too new.").Build());
                    return;
                }
                if (stakingUser.CreatedAt > DateTime.Now.AddDays(-14))
                {
                    await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Staking is not possible (New Account)", $"You are not able to stake because your account is too new.").Build());
                    return;
                }


                //CHECK 2FA (OUTDATED?)
                //var json = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/DiscordUsersAuthStatus.json");
                //var authStatusList = JsonConvert.DeserializeObject<Dictionary<string, bool>>(json);

                //if (authStatusList.Keys.Contains(command.User.Id.ToString()) && authStatusList.Keys.Contains(userToStakeWith.Id.ToString()))
                //{
                //    if (authStatusList.FirstOrDefault(x => x.Key == command.User.Id.ToString()).Value)
                //    {
                //        if (authStatusList.FirstOrDefault(x => x.Key == userToStakeWith.Id.ToString()).Value)
                //        {

                //        }
                //        else
                //        {
                //            await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Staking is not possible (No 2FA)", $"{userToStakeWith.Username} does not have 2FA activated on its account. This is a must for being able to trade cards. The reason is because it prevents people from creating lots of alt discord accounts to bypass the daily pack limit. \n\nIt is also better for your own account safety tho =D. \n\nplease tell the user to activate 2FA if you want to stake. After activating 2FA, please visit https://beatsaberbot.com/BeatSaberCards#inventory again once when logged in").Build());
                //            return;
                //        }
                //    }
                //    else
                //    {
                //        await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Staking is not possible (No 2FA)", $"You do not have 2FA activated on your account. This is a must for being able to trade cards. The reason is because it prevents people from creating lots of alt discord accounts to bypass the daily pack limit. \n\nIt is also better for your own account safety tho =D. \n\nAfter activating 2FA, please visit https://beatsaberbot.com/BeatSaberCards#inventory again once when logged in").Build());
                //        return;
                //    }
                //}
                //else
                //{
                //    var userString = "";
                //    if (!authStatusList.Keys.Contains(command.User.Id.ToString())) userString += "\n" + command.User.Username + " is not known in the system.\n";
                //    if (!authStatusList.Keys.Contains(userToStakeWith.Id.ToString())) userString += "\n" + userToStakeWith.Username + " is not known in the system.\n";
                //    await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Staking is not possible (Not known)", $"One of the users is not known in the system yet. {userString} \nPlease login into the website https://beatsaberbot.com/BeatSaberCards#inventory with your discord account to be able to stake. If you are logged in already, just refresh the site to get into the system.").Build());
                //    return;
                //}

                var dmChannel = await stakingUser.CreateDMChannelAsync();
                var embed = EmbedBuilderExtension.NullEmbed($"Staking offer towards {userToStakeWith.Username}", "You are about to create a staking offer. Do you want to proceed? \n\n\n Staking is an competition where your staked cards will be on the line. It is a 1vs1. Both players have a limited time to set their best possible score on a chosen map. The player who wins the map at the end of the time limit, will get all the cards that were on stake. staked cards can not be traded in the meantime.");
                var componentBuilder = new ComponentBuilder();
                componentBuilder.WithButton("YES", customId: "stakingYesButton", style: ButtonStyle.Success);
                componentBuilder.WithButton("NO", customId: "stakingNoButton", style: ButtonStyle.Danger);

                var message = await dmChannel.SendMessageAsync("", false, embed.Build(), components: componentBuilder.Build());
                dmMessageID = message.Id;
                _discord.ButtonExecuted += Discord_ButtonExecuted;
            }
            catch (Exception ex)
            {

            }
        }

        public async Task StartTradeProcess(SocketSlashCommand command, SocketUser userToTradeWith, SocketUser tradingUser)
        {
            try
            {
                this.userToTradeWith = userToTradeWith;
                var tradelimits = JsonConvert.DeserializeObject<List<TradeLimit>>(System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/TradeLimit.json"));
                if (tradelimits == null) tradelimits = new List<TradeLimit>();

                if (userToTradeWith.Id == tradingUser.Id && tradingUser.Id != 138439306774577152)
                {
                    var cantTradeMessage = await command.Channel.SendMessageAsync("Can't trade with yourself");
                    await Task.Delay(5000);
                    await cantTradeMessage.DeleteAsync();
                    return;
                }

                //Check trade limit
                if (tradelimits.FirstOrDefault(x => x.DiscordID == userToTradeWith.Id.ToString()) == null) tradelimits.Add(new TradeLimit() { DiscordID = userToTradeWith.Id.ToString(), CardsLeftToTrade = 5 });
                if (tradelimits.FirstOrDefault(x => x.DiscordID == tradingUser.Id.ToString()) == null) tradelimits.Add(new TradeLimit() { DiscordID = tradingUser.Id.ToString(), CardsLeftToTrade = 5 });
                if (tradelimits.FirstOrDefault(x => x.DiscordID == userToTradeWith.Id.ToString()).CardsLeftToTrade <= 0)
                {
                    await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Trading is not possible (Trade Limit)", $"You are not able to trade with this user because this user reached its daily trade limit").Build());
                    return;
                }
                if (tradelimits.FirstOrDefault(x => x.DiscordID == tradingUser.Id.ToString()).CardsLeftToTrade <= 0)
                {
                    await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Trading is not possible (Trade Limit)", $"You are not able to trade because you reached your daily trade limit").Build());
                    return;
                }
                var tradeJson = JsonConvert.SerializeObject(tradelimits);
                File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/TradeLimit.json", tradeJson);


                if (userToTradeWith.CreatedAt > DateTime.Now.AddDays(-14))
                {
                    await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Trading is not possible (New Account)", $"You are not able to trade with this user because the account is too new.").Build());
                    return;
                }
                if (command.User.CreatedAt > DateTime.Now.AddDays(-14))
                {
                    await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Trading is not possible (New Account)", $"You are not able to trade because your account is too new.").Build());
                    return;
                }

                //2FA CHECK (OUTDATED?)
                //var json = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/DiscordUsersAuthStatus.json");
                //var authStatusList = JsonConvert.DeserializeObject<Dictionary<string, bool>>(json);

                //if (authStatusList.Keys.Contains(command.User.Id.ToString()) && authStatusList.Keys.Contains(userToTradeWith.Id.ToString()))
                //{
                //    if (authStatusList.FirstOrDefault(x => x.Key == command.User.Id.ToString()).Value)
                //    {
                //        if (authStatusList.FirstOrDefault(x => x.Key == userToTradeWith.Id.ToString()).Value)
                //        {

                //        }
                //        else
                //        {
                //            await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Trading is not possible (No 2FA)", $"{userToTradeWith.Username} does not have 2FA activated on its account. This is a must for being able to trade cards. The reason is because it prevents people from creating lots of alt discord accounts to bypass the daily pack limit. \n\nIt is also better for your own account safety tho =D. \n\nplease tell the user to activate 2FA if you want to trade. After activating 2FA, please visit https://beatsaberbot.com/BeatSaberCards#inventory again once when logged in").Build());
                //            return;
                //        }
                //    }
                //    else
                //    {
                //        await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Trading is not possible (No 2FA)", $"You do not have 2FA activated on your account. This is a must for being able to trade cards. The reason is because it prevents people from creating lots of alt discord accounts to bypass the daily pack limit. \n\nIt is also better for your own account safety tho =D. \n\nAfter activating 2FA, please visit https://beatsaberbot.com/BeatSaberCards#inventory again once when logged in").Build());
                //        return;
                //    }
                //}
                //else
                //{
                //    var userString = "";
                //    if (!authStatusList.Keys.Contains(command.User.Id.ToString())) userString += "\n" + command.User.Username + " is not known in the system.\n";
                //    if (!authStatusList.Keys.Contains(userToTradeWith.Id.ToString())) userString += "\n" + userToTradeWith.Username + " is not known in the system.\n";
                //    await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Trading is not possible (Not known)", $"One of the users is not known in the system yet. {userString} \nPlease login into the website https://beatsaberbot.com/BeatSaberCards#inventory with your discord account to be able to trade. If you are logged in already, just refresh the site to get into the system.").Build());
                //    return;
                //}

                var dmChannel = await tradingUser.CreateDMChannelAsync();
                var embed = EmbedBuilderExtension.NullEmbed($"Trading offer towards {userToTradeWith.Username} ({userToTradeWith.Id})", "You are about to create a trading offer. Do you want to proceed?");
                var componentBuilder = new ComponentBuilder();
                componentBuilder.WithButton("YES", customId: "tradingYesButton", style: ButtonStyle.Success);
                componentBuilder.WithButton("NO", customId: "tradingNoButton", style: ButtonStyle.Danger);

                var message = await dmChannel.SendMessageAsync("", false, embed.Build(), components: componentBuilder.Build());
                dmMessageID = message.Id;
                _discord.ButtonExecuted += Discord_ButtonExecuted;
            }
            catch (Exception ex)
            {

            }
        }

        private async System.Threading.Tasks.Task Discord_ButtonExecuted(SocketMessageComponent arg)
        {
            if (arg.Message.Id == dmMessageID)
            {
                if (arg.Data.CustomId == "tradingYesButton")
                {
                    StartTradeOfferProcess(arg);
                }
                if (arg.Data.CustomId == "tradingNoButton" || arg.Data.CustomId == "tradingAbortButton")
                {
                    arg.Message.DeleteAsync();
                }
                if (arg.Data.CustomId == "tradingNextStepOneButton" || arg.Data.CustomId == "tradingSendOfferButton")
                {
                    hasNextBeenPressed = true;
                }
                if (arg.Data.CustomId == "tradingAcceptOfferButton")
                {
                    tradeHasBeenAccepted = true;
                }

                if (arg.Data.CustomId == "stakingYesButton")
                {
                    StartStakeOfferProcess(arg);
                }
                if (arg.Data.CustomId == "stakingNoButton" || arg.Data.CustomId == "stakingAbortButton")
                {
                    arg.Message.DeleteAsync();
                }
                if (arg.Data.CustomId == "stakingNextStepOneButton" || arg.Data.CustomId == "stakingSendOfferButton")
                {
                    hasNextBeenPressed = true;
                }
                if (arg.Data.CustomId == "stakingAcceptOfferButton")
                {
                    tradeHasBeenAccepted = true;
                }
                if (arg.Data.CustomId == "easy" || arg.Data.CustomId == "normal" || arg.Data.CustomId == "hard" || arg.Data.CustomId == "expert" || arg.Data.CustomId == "expertplus")
                {
                    stakeDifficulty = arg.Data.CustomId;
                    hasNextBeenPressed = true;
                }
            }
            return;
        }

        public async Task StartStakeOfferProcess(SocketMessageComponent arguments)
        {
            //Start stake option process
            try
            {
                var tradelimits = JsonConvert.DeserializeObject<List<TradeLimit>>(System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/TradeLimit.json"));

                var embed = EmbedBuilderExtension.NullEmbed(arguments.Message.Embeds.First().Title, $"**Click on the card you want to stake from your [Inventory](http://beatsaberbot.com/BeatSaberCards?rankingPageSelectRanking=OwnedDiscordID&searchinput={arguments.User.Id}#inventory) and enter the card url here in the chat** \n\nInput card url's one by one\n\n----------CARDS----------");
                embed.WithImageUrl("http://beatsaberbot.com/img/howtotrade.gif");
                var componentBuilder = new ComponentBuilder();
                componentBuilder.WithButton("NEXT", customId: "stakingNextStepOneButton", style: ButtonStyle.Success, disabled: true);
                componentBuilder.WithButton("ABORT", customId: "stakingAbortButton", style: ButtonStyle.Danger, disabled: false);

                var currentStakeMatches = JsonConvert.DeserializeObject<List<StakeMatch>>(System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/StakeMatches.json"));
                if (currentStakeMatches == null) currentStakeMatches = new List<StakeMatch>();

                await arguments.Message.ModifyAsync(msg => msg.Embed = embed.Build());
                await arguments.Message.ModifyAsync(msg => msg.Components = componentBuilder.Build());


                var cardListUserOne = new List<string>();
                var cardListUserTwo = new List<string>();

                var cards = GetAllCards();

                var startTime = DateTime.Now;
                var endTime = startTime.AddMinutes(10);
                var content = "";
                do
                {
                    await Task.Delay(1000);
                    var message = await arguments.Channel.GetMessagesAsync(1).FlattenAsync();
                    if (message.First().CreatedAt > startTime && message.First().Author.IsBot == false)
                    {
                        content = message.First().Content;
                        content = content.Replace("https://beatsaberbot.com/BeatSaberTradingCards/", "/BeatSaberTradingCards/");
                        if (cards.Where(x => x.OwnerDiscordID == arguments.User.Id.ToString()).FirstOrDefault(x => x.Name == content) != null && !cardListUserOne.Contains(content) && currentStakeMatches.FirstOrDefault(x => x.PlayerOneCardsStaked.FirstOrDefault(x => x == content) != null) == null && currentStakeMatches.FirstOrDefault(x => x.PlayerTwoCardsStaked.FirstOrDefault(x => x == content) != null) == null && tradelimits.FirstOrDefault(x => x.DiscordID == arguments.User.Id.ToString()).CardsLeftToTrade >= cardListUserOne.Count)
                        {
                            embed.Description = embed.Description + $"\n\n [Card](http://beatsaberbot.com{content})";
                            var newEmbed = EmbedBuilderExtension.NullEmbed(embed.Title, embed.Description);

                            var componentBuilderAfterCard = new ComponentBuilder();
                            componentBuilderAfterCard.WithButton("NEXT", customId: "stakingNextStepOneButton", style: ButtonStyle.Success, disabled: false);
                            componentBuilderAfterCard.WithButton("ABORT", customId: "stakingAbortButton", style: ButtonStyle.Danger, disabled: false);

                            await arguments.Message.ModifyAsync(msg => msg.Embed = newEmbed.Build());
                            await arguments.Message.ModifyAsync(msg => msg.Components = componentBuilderAfterCard.Build());
                            cardListUserOne.Add(content);
                        }
                    }

                } while (endTime > startTime && hasNextBeenPressed == false);

                hasNextBeenPressed = false;

                if (content == "")
                {
                    await arguments.Message.DeleteAsync();
                    return;
                }

                var embedTwo = EmbedBuilderExtension.NullEmbed(arguments.Message.Embeds.First().Title, $"**Click on the card you want your opponent to stake from {userToStakeWith.Username}'s [Inventory](http://beatsaberbot.com/BeatSaberCards?rankingPageSelectRanking=OwnedDiscordID&searchinput={userToStakeWith.Id}#inventory) and enter the card url here in the chat** \n\nInput card url's one by one\n\n----------CARDS----------");
                embedTwo.WithImageUrl("http://beatsaberbot.com/img/howtotrade.gif");
                await arguments.Message.ModifyAsync(x => x.Embed = embedTwo.Build());

                startTime = DateTime.Now;
                endTime = startTime.AddMinutes(10);
                content = "";
                do
                {
                    await Task.Delay(1000);
                    var message = await arguments.Channel.GetMessagesAsync(1).FlattenAsync();
                    if (message.First().CreatedAt > startTime && message.First().Author.IsBot == false)
                    {
                        content = message.First().Content;
                        content = content.Replace("https://beatsaberbot.com/BeatSaberTradingCards/", "/BeatSaberTradingCards/");
                        if (cards.Where(x => x.OwnerDiscordID == userToStakeWith.Id.ToString()).FirstOrDefault(x => x.Name == content) != null && !cardListUserTwo.Contains(content) && currentStakeMatches.FirstOrDefault(x => x.PlayerOneCardsStaked.FirstOrDefault(x => x == content) != null) == null && currentStakeMatches.FirstOrDefault(x => x.PlayerTwoCardsStaked.FirstOrDefault(x => x == content) != null) == null && tradelimits.FirstOrDefault(x => x.DiscordID == arguments.User.Id.ToString()).CardsLeftToTrade >= cardListUserOne.Count)
                        {
                            embedTwo.Description = embedTwo.Description + $"\n\n [Card](http://beatsaberbot.com{content})";
                            var newEmbed = EmbedBuilderExtension.NullEmbed(embedTwo.Title, embedTwo.Description);

                            var componentBuilderAfterCard = new ComponentBuilder();
                            componentBuilderAfterCard.WithButton("NEXT", customId: "stakingNextStepOneButton", style: ButtonStyle.Success, disabled: false);
                            componentBuilderAfterCard.WithButton("ABORT", customId: "stakingAbortButton", style: ButtonStyle.Danger, disabled: false);

                            await arguments.Message.ModifyAsync(msg => msg.Embed = newEmbed.Build());
                            await arguments.Message.ModifyAsync(msg => msg.Components = componentBuilderAfterCard.Build());
                            cardListUserTwo.Add(content);
                        }
                    }

                } while (endTime > startTime && hasNextBeenPressed == false);

                hasNextBeenPressed = false;

                if (content == "")
                {
                    await arguments.Message.DeleteAsync();
                    return;
                }

                if (cardListUserOne.Count == 0 || cardListUserTwo.Count == 0)
                {
                    await arguments.Channel.SendMessageAsync("One of the sides didn't have any cards, stake offer has been aborted");
                    await arguments.Message.DeleteAsync();
                    return;
                }

                //Create a question for what the stake map will be and how long the time will be 
                var embedThree = EmbedBuilderExtension.NullEmbed(arguments.Message.Embeds.First().Title, $"**Please insert the bsr key of the map you want to compete at**");
                await arguments.Message.ModifyAsync(x => x.Embed = embedThree.Build());
                BeatSaverMapModelNew mapToPlay = null;

                startTime = DateTime.Now;
                endTime = startTime.AddMinutes(10);
                content = "";
                var mapHasBeenGiven = false;
                do
                {
                    await Task.Delay(1000);
                    var message = await arguments.Channel.GetMessagesAsync(1).FlattenAsync();
                    if (message.First().CreatedAt > startTime && message.First().Author.IsBot == false)
                    {
                        content = message.First().Content;
                        var map = await BeatSaverApi.GetMapByKey(content);
                        if (map != null && mapHasBeenGiven == false)
                        {
                            embedThree.Description = embedThree.Description + $"\n\n bsr key: {content}\n\n Name: {map.Name}";
                            var newEmbed = EmbedBuilderExtension.NullEmbed(embedThree.Title, embedThree.Description);

                            var componentBuilderAfterCard = new ComponentBuilder();
                            componentBuilderAfterCard.WithButton("EASY", customId: "easy", style: ButtonStyle.Success, disabled: false);
                            componentBuilderAfterCard.WithButton("NORMAL", customId: "normal", style: ButtonStyle.Success, disabled: false);
                            componentBuilderAfterCard.WithButton("HARD", customId: "hard", style: ButtonStyle.Success, disabled: false);
                            componentBuilderAfterCard.WithButton("EXPERT", customId: "expert", style: ButtonStyle.Success, disabled: false);
                            componentBuilderAfterCard.WithButton("EXPERTPLUS", customId: "expertplus", style: ButtonStyle.Success, disabled: false);
                            componentBuilderAfterCard.WithButton("ABORT", customId: "stakingAbortButton", style: ButtonStyle.Danger, disabled: false);

                            await arguments.Message.ModifyAsync(msg => msg.Embed = newEmbed.Build());
                            await arguments.Message.ModifyAsync(msg => msg.Components = componentBuilderAfterCard.Build());

                            mapHasBeenGiven = true;

                            mapToPlay = map;
                        }
                    }

                } while (endTime > startTime && hasNextBeenPressed == false);

                mapToPlay.Versions.FirstOrDefault(x => x.Diffs.FirstOrDefault(x => x.Difficulty.ToLower() == stakeDifficulty.ToLower()) != null);
                if (mapToPlay == null)
                {
                    await arguments.Channel.SendMessageAsync("Difficulty does not exist. Stake offer got aborted");
                    await arguments.Message.DeleteAsync();
                    return;
                }

                hasNextBeenPressed = false;

                await Task.Delay(3000);

                if (content == "")
                {
                    await arguments.Message.DeleteAsync();
                    return;
                }

                //Create a question for what the stake time will be
                var embedFour = EmbedBuilderExtension.NullEmbed(arguments.Message.Embeds.First().Title, $"**Please insert the time limit of the map you want to compete at. Insert the end date in the following format `HH` example: 48 will make the stake end in 2 days, 0 hours and 0 minutes**");
                var componentBuilderBeforeCard = new ComponentBuilder();
                componentBuilderBeforeCard.WithButton("NEXT", customId: "stakingNextStepOneButton", style: ButtonStyle.Success, disabled: false);
                componentBuilderBeforeCard.WithButton("ABORT", customId: "stakingAbortButton", style: ButtonStyle.Danger, disabled: false);

                await arguments.Message.ModifyAsync(msg => msg.Components = componentBuilderBeforeCard.Build());
                await arguments.Message.ModifyAsync(x => x.Embed = embedFour.Build());
                DateTime endDate = DateTime.UtcNow;

                startTime = DateTime.Now;
                endTime = startTime.AddMinutes(10);
                content = "";
                do
                {
                    await Task.Delay(1000);
                    var message = await arguments.Channel.GetMessagesAsync(1).FlattenAsync();
                    if (message.First().CreatedAt > startTime && message.First().Author.IsBot == false)
                    {
                        content = message.First().Content;
                        TimeSpan timespan;
                        if (TimeSpan.TryParse(content, out timespan))
                        {
                            embedFour.Description = embedFour.Description + $"\n\n hours: {content})";
                            var newEmbed = EmbedBuilderExtension.NullEmbed(embedFour.Title, embedFour.Description);

                            var componentBuilderAfterCard = new ComponentBuilder();
                            componentBuilderAfterCard.WithButton("NEXT", customId: "stakingNextStepOneButton", style: ButtonStyle.Success, disabled: false);
                            componentBuilderAfterCard.WithButton("ABORT", customId: "stakingAbortButton", style: ButtonStyle.Danger, disabled: false);

                            await arguments.Message.ModifyAsync(msg => msg.Embed = newEmbed.Build());
                            await arguments.Message.ModifyAsync(msg => msg.Components = componentBuilderAfterCard.Build());
                            endDate = DateTime.UtcNow.AddHours(Convert.ToInt64(content));

                            hasNextBeenPressed = true;
                        }
                    }

                } while (endTime > startTime && hasNextBeenPressed == false);

                hasNextBeenPressed = false;

                if (content == "")
                {
                    await arguments.Message.DeleteAsync();
                    return;
                }


                await arguments.Message.DeleteAsync();

                var cardListOneString = "";
                foreach (var card in cardListUserOne)
                {
                    var drawnUser = await _discord.GetUserAsync(Convert.ToUInt64(card.Split("-")[1]));
                    cardListOneString += $"[Card](http://beatsaberbot.com{card}) [Drawn By: **{ drawnUser.Username}#{drawnUser.Discriminator}**, Account created at: **{drawnUser.CreatedAt.UtcDateTime.ToShortDateString()}**]\n";
                }

                var cardListTwoString = "";

                foreach (var card in cardListUserTwo)
                {
                    var drawnUser = await _discord.GetUserAsync(Convert.ToUInt64(card.Split("-")[1]));
                    cardListTwoString += $"[Card](http://beatsaberbot.com{card}) [Drawn By: **{ drawnUser.Username}#{drawnUser.Discriminator}**, Account created at: **{drawnUser.CreatedAt.UtcDateTime.ToShortDateString()}**] \n";
                }

                var finalEmbed = EmbedBuilderExtension.NullEmbed($"Do you want to sent the following stake offer towards {userToStakeWith.Username}?", $"**The map to compete in:**\n**{mapToPlay.Name}({mapToPlay.Id})** \n\n **Stake end time:** \n**{endDate.ToShortDateString()} {endDate.ToShortTimeString()}**\n\n**You will stake:**\n{cardListOneString} \n\n**Your opponent will stake:**\n{cardListTwoString}\n\n");
                var componentBuilderSendOffer = new ComponentBuilder();
                componentBuilderSendOffer.WithButton("SEND OFFER", customId: "stakingSendOfferButton", style: ButtonStyle.Success);
                componentBuilderSendOffer.WithButton("ABORT OFFER", customId: "stakingAbortButton", style: ButtonStyle.Danger);
                var finalMessage = await arguments.Channel.SendMessageAsync("", false, finalEmbed.Build(), components: componentBuilderSendOffer.Build());
                dmMessageID = finalMessage.Id;

                do
                {
                    await Task.Delay(2000);

                } while (hasNextBeenPressed == false);
                await finalMessage.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Offer has been sent!", "You will get reaction here soon!").Build());
                await finalMessage.ModifyAsync(x => x.Components = new ComponentBuilder().Build());

                //Send message to the user to trade with and ask if the offer will ge accepted or denied
                var userToStakeWithDM = await userToStakeWith.CreateDMChannelAsync();
                var offerEmbed = EmbedBuilderExtension.NullEmbed($"You got a stake offer from {arguments.User.Username}!", $"**The map to compete in:**\n**{mapToPlay.Name} [map](https://beatsaver.com/maps/{mapToPlay.Id})** \n\n **Stake end time:** \n**{endDate.ToShortDateString()} {endDate.ToShortTimeString()}**\n\n**You will stake:**\n{cardListOneString} \n\n**Your opponent will stake:**\n{cardListTwoString}\n\n\n\n **What is staking?**\n\nStaking is an competition where your staked cards will be on the line. It is a 1vs1. Both players have a limited time to set their best possible score on a chosen map. The player who wins the map at the end of the time limit, will get all the cards that were on stake. staked cards can not be traded in the meantime.");
                var componentBuilderAcceptOffer = new ComponentBuilder();
                componentBuilderAcceptOffer.WithButton("ACCEPT OFFER", customId: "stakingAcceptOfferButton", style: ButtonStyle.Success);
                componentBuilderAcceptOffer.WithButton("DENY OFFER", customId: "stakingAbortButton", style: ButtonStyle.Danger);

                var toTradeWithMsg = await userToStakeWithDM.SendMessageAsync("", false, offerEmbed.Build(), components: componentBuilderAcceptOffer.Build());
                dmMessageID = toTradeWithMsg.Id;

                startTime = DateTime.Now;
                endTime = startTime.AddMinutes(20);
                do
                {
                    await Task.Delay(2000);

                } while (endTime > startTime && tradeHasBeenAccepted == false);

                if (tradeHasBeenAccepted)
                {
                    //Adjust both players trade limit
                    tradelimits.FirstOrDefault(x => x.DiscordID == arguments.User.Id.ToString()).CardsLeftToTrade -= cardListUserOne.Count;
                    tradelimits.FirstOrDefault(x => x.DiscordID == userToStakeWith.Id.ToString()).CardsLeftToTrade -= cardListUserTwo.Count;
                    var tradeJson = JsonConvert.SerializeObject(tradelimits);
                    File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/TradeLimit.json", tradeJson);

                    var scoresaberIDPlayerOne = await RoleAssignment.GetScoresaberIdWithDiscordId(arguments.User.Id.ToString());
                    var scoresaberIDPlayerTwo = await RoleAssignment.GetScoresaberIdWithDiscordId(userToStakeWith.Id.ToString());

                    var scoresaberPlayerOne = await new ScoreSaberClient().Api.Players.GetPlayer(Convert.ToInt64(scoresaberIDPlayerOne));
                    var scoresaberPlayerTwo = await new ScoreSaberClient().Api.Players.GetPlayer(Convert.ToInt64(scoresaberIDPlayerTwo));
                    long playerOneRank = 0;
                    long playerTwoRank = 0;
                    if (scoresaberPlayerOne != null) playerOneRank = scoresaberPlayerOne.Rank;
                    if (scoresaberPlayerTwo != null) playerTwoRank = scoresaberPlayerTwo.Rank;

                    //Create stake in database 
                    var stakeMatch = new StakeMatch()
                    {
                        PlayerOneDiscordID = arguments.User.Id.ToString(),
                        PlayerOneUsername = arguments.User.Username,
                        PlayerOneProfileUrl = arguments.User.GetAvatarUrl(),
                        PlayerOneScoresaberID = scoresaberIDPlayerOne,
                        PlayerOneCardsStaked = cardListUserOne,
                        PlayerOneCurrentScore = 0,
                        PlayerOneRank = playerOneRank,
                        PlayerTwoDiscordID = userToStakeWith.Id.ToString(),
                        PlayerTwoUsername = userToStakeWith.Username,
                        PlayerTwoProfileUrl = userToStakeWith.GetAvatarUrl(),
                        PlayerTwoScoresaberID = scoresaberIDPlayerTwo,
                        PlayerTwoCardsStaked = cardListUserTwo,
                        PlayerTwoCurrentScore = 0,
                        PlayerTwoRank = playerTwoRank,
                        EndDate = endDate,
                        mapDiff = stakeDifficulty,
                        mapKey = mapToPlay.Id,
                        mapUrl = mapToPlay.Versions.First().CoverUrl.OriginalString,
                        mapName = mapToPlay.Name,
                        mapHash = mapToPlay.Versions.First().Hash,
                        mapMaxScore = mapToPlay.Versions.First().Diffs.FirstOrDefault(x => x.Difficulty.ToLower() == stakeDifficulty).MaxScore,
                        matchHasEnded = false,
                        matchID = Guid.NewGuid()
                    };

                    var json = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/StakeMatches.json");
                    var stakeMatches = JsonConvert.DeserializeObject<List<StakeMatch>>(json);

                    if (stakeMatches != null)
                    {
                        stakeMatches.Add(stakeMatch);
                    }
                    else
                    {
                        var newmatches = new List<StakeMatch>();
                        newmatches.Add(stakeMatch);
                        stakeMatches = newmatches;
                    }

                    var newJson = JsonConvert.SerializeObject(stakeMatches);
                    File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/StakeMatches.json", newJson);

                    await userToStakeWithDM.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("You have accepted the stake match. Goodluck!", $"- Make sure to get the highest score. You can play over and over again.\n\n- You won't be able to trade the cards in the meantime.\n\n- You can download the map here [download](https://beatsaver.com/maps/{mapToPlay.Id})\n\n- You can find the progress on the stake match space on your own inventory [here](https://beatsaberbot.com/BeatSaberCards?rankingPageSelectRanking=OwnedDiscordID&searchinput={userToStakeWith.Id}#stakematches)").Build());
                    await arguments.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed($"{userToStakeWith.Username} has accepted the stake match. Goodluck!", $"- Make sure to get the highest score. You can play over and over again.\n\n- You won't be able to trade the cards in the meantime.\n\n- You can download the map here [download](https://beatsaver.com/maps/{mapToPlay.Id})\n\n- You can find the progress on the stake match space on your own inventory [here](https://beatsaberbot.com/BeatSaberCards?rankingPageSelectRanking=OwnedDiscordID&searchinput={arguments.User.Id}#stakematches)").Build());

                }
                else
                {
                    await userToStakeWithDM.SendMessageAsync("You have denied the stake match offer");
                    await arguments.Channel.SendMessageAsync($"Your stake match offer with {userToStakeWith.Username} has been declined");
                }

                await toTradeWithMsg.DeleteAsync();
            }
            catch (Exception ex)
            {

            }
        }

        public async Task StartTradeOfferProcess(SocketMessageComponent arguments)
        {
            try
            {
                var tradelimits = JsonConvert.DeserializeObject<List<TradeLimit>>(System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/TradeLimit.json"));

                var embed = EmbedBuilderExtension.NullEmbed(arguments.Message.Embeds.First().Title, $"**Click on the card you want to trade from your [Inventory](http://beatsaberbot.com/BeatSaberCards?rankingPageSelectRanking=OwnedDiscordID&searchinput={arguments.User.Id}#inventory) and enter the card url here in the chat** \n\nInput card url's one by one\n\n----------CARDS----------");
                embed.WithImageUrl("http://beatsaberbot.com/img/howtotrade.gif");
                var componentBuilder = new ComponentBuilder();
                componentBuilder.WithButton("NEXT", customId: "tradingNextStepOneButton", style: ButtonStyle.Success, disabled: true);
                componentBuilder.WithButton("ABORT", customId: "tradingAbortButton", style: ButtonStyle.Danger, disabled: false);


                var currentStakeMatches = JsonConvert.DeserializeObject<List<StakeMatch>>(System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/StakeMatches.json"));

                await arguments.Message.ModifyAsync(msg => msg.Embed = embed.Build());
                await arguments.Message.ModifyAsync(msg => msg.Components = componentBuilder.Build());


                var cardListUserOne = new List<string>();
                var cardListUserTwo = new List<string>();

                string[] files = Directory.GetFiles(@$"F:\\BeatSaberTradingCards");
                var cards = GetAllCards();

                var startTime = DateTime.Now;
                var endTime = startTime.AddMinutes(10);
                var content = "";
                do
                {
                    await Task.Delay(1000);
                    var message = await arguments.Channel.GetMessagesAsync(1).FlattenAsync();
                    if (message.First().CreatedAt > startTime && message.First().Author.IsBot == false)
                    {
                        content = message.First().Content;
                        content = content.Replace("https://beatsaberbot.com/BeatSaberTradingCards/", "/BeatSaberTradingCards/");
                        if (cards.Where(x => x.OwnerDiscordID == arguments.User.Id.ToString()).FirstOrDefault(x => x.Name == content) != null && !cardListUserOne.Contains(content) && currentStakeMatches.FirstOrDefault(x => x.PlayerOneCardsStaked.FirstOrDefault(x => x == content) != null) == null && currentStakeMatches.FirstOrDefault(x => x.PlayerTwoCardsStaked.FirstOrDefault(x => x == content) != null) == null && tradelimits.FirstOrDefault(x => x.DiscordID == arguments.User.Id.ToString()).CardsLeftToTrade >= cardListUserOne.Count)
                        {
                            embed.Description = embed.Description + $"\n\n [Card](http://beatsaberbot.com{content})";
                            var newEmbed = EmbedBuilderExtension.NullEmbed(embed.Title, embed.Description);

                            var componentBuilderAfterCard = new ComponentBuilder();
                            componentBuilderAfterCard.WithButton("NEXT", customId: "tradingNextStepOneButton", style: ButtonStyle.Success, disabled: false);
                            componentBuilderAfterCard.WithButton("ABORT", customId: "tradingAbortButton", style: ButtonStyle.Danger, disabled: false);

                            await arguments.Message.ModifyAsync(msg => msg.Embed = newEmbed.Build());
                            await arguments.Message.ModifyAsync(msg => msg.Components = componentBuilderAfterCard.Build());
                            cardListUserOne.Add(content);
                        }
                    }

                } while (endTime > startTime && hasNextBeenPressed == false);

                hasNextBeenPressed = false;

                if (content == "")
                {
                    await arguments.Message.DeleteAsync();
                    return;
                }

                var embedTwo = EmbedBuilderExtension.NullEmbed(arguments.Message.Embeds.First().Title, $"**Click on the card you want to have from {userToTradeWith.Username}'s [Inventory](http://beatsaberbot.com/BeatSaberCards?rankingPageSelectRanking=OwnedDiscordID&searchinput={userToTradeWith.Id}#inventory) and enter the card url here in the chat** \n\nInput card url's one by one\n\n----------CARDS----------");
                embedTwo.WithImageUrl("http://beatsaberbot.com/img/howtotrade.gif");
                await arguments.Message.ModifyAsync(x => x.Embed = embedTwo.Build());

                startTime = DateTime.Now;
                endTime = startTime.AddMinutes(10);
                content = "";
                do
                {
                    await Task.Delay(1000);
                    var message = await arguments.Channel.GetMessagesAsync(1).FlattenAsync();
                    if (message.First().CreatedAt > startTime && message.First().Author.IsBot == false)
                    {
                        content = message.First().Content;
                        content = content.Replace("https://beatsaberbot.com/BeatSaberTradingCards/", "/BeatSaberTradingCards/");
                        if (cards.Where(x => x.OwnerDiscordID == userToTradeWith.Id.ToString()).FirstOrDefault(x => x.Name == content) != null && !cardListUserTwo.Contains(content) && currentStakeMatches.FirstOrDefault(x => x.PlayerOneCardsStaked.FirstOrDefault(x => x == content) != null) == null && currentStakeMatches.FirstOrDefault(x => x.PlayerTwoCardsStaked.FirstOrDefault(x => x == content) != null) == null && tradelimits.FirstOrDefault(x => x.DiscordID == userToTradeWith.Id.ToString()).CardsLeftToTrade >= cardListUserTwo.Count)
                        {
                            embedTwo.Description = embedTwo.Description + $"\n\n [Card](http://beatsaberbot.com{content})";
                            var newEmbed = EmbedBuilderExtension.NullEmbed(embedTwo.Title, embedTwo.Description);

                            var componentBuilderAfterCard = new ComponentBuilder();
                            componentBuilderAfterCard.WithButton("NEXT", customId: "tradingNextStepOneButton", style: ButtonStyle.Success, disabled: false);
                            componentBuilderAfterCard.WithButton("ABORT", customId: "tradingAbortButton", style: ButtonStyle.Danger, disabled: false);

                            await arguments.Message.ModifyAsync(msg => msg.Embed = newEmbed.Build());
                            await arguments.Message.ModifyAsync(msg => msg.Components = componentBuilderAfterCard.Build());
                            cardListUserTwo.Add(content);
                        }
                    }

                } while (endTime > startTime && hasNextBeenPressed == false);

                hasNextBeenPressed = false;

                if (content == "")
                {
                    await arguments.Message.DeleteAsync();
                    return;
                }

                if (cardListUserOne.Count == 0 || cardListUserTwo.Count == 0)
                {
                    await arguments.Channel.SendMessageAsync("One of the sides didn't have any cards, trade offer has been aborted");
                    await arguments.Message.DeleteAsync();
                    return;
                }


                await arguments.Message.DeleteAsync();

                var cardListOneString = "";
                foreach (var card in cardListUserOne)
                {
                    var drawnUser = await _discord.GetUserAsync(Convert.ToUInt64(card.Split("-")[1]));
                    cardListOneString += $"[Card](http://beatsaberbot.com{card}) [Drawn By: **{ drawnUser.Username}#{drawnUser.Discriminator}**, Account created at: **{drawnUser.CreatedAt.UtcDateTime.ToShortDateString()}**]\n";
                }

                var cardListTwoString = "";

                foreach (var card in cardListUserTwo)
                {
                    var drawnUser = await _discord.GetUserAsync(Convert.ToUInt64(card.Split("-")[1]));
                    cardListTwoString += $"[Card](http://beatsaberbot.com{card}) [Drawn By: **{ drawnUser.Username}#{drawnUser.Discriminator}**, Account created at: **{drawnUser.CreatedAt.UtcDateTime.ToShortDateString()}**] \n";
                }

                var finalEmbed = EmbedBuilderExtension.NullEmbed($"Do you want to sent the following trade offer towards {userToTradeWith.Username}?", $"**You will give:**\n{cardListOneString} \n\n**You will get:**\n{cardListTwoString}");
                var componentBuilderSendOffer = new ComponentBuilder();
                componentBuilderSendOffer.WithButton("SEND OFFER", customId: "tradingSendOfferButton", style: ButtonStyle.Success);
                componentBuilderSendOffer.WithButton("ABORT OFFER", customId: "tradingAbortButton", style: ButtonStyle.Danger);
                var finalMessage = await arguments.Channel.SendMessageAsync("", false, finalEmbed.Build(), components: componentBuilderSendOffer.Build());
                dmMessageID = finalMessage.Id;

                do
                {
                    await Task.Delay(2000);

                } while (hasNextBeenPressed == false);
                await finalMessage.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Offer has been sent!", "You will get reaction here soon!").Build());
                await finalMessage.ModifyAsync(x => x.Components = new ComponentBuilder().Build());

                //Send message to the user to trade with and ask if the offer will ge accepted or denied
                var userToTradeWithDM = await userToTradeWith.CreateDMChannelAsync();
                var offerEmbed = EmbedBuilderExtension.NullEmbed($"You got a trade offer from {arguments.User.Username}!", $"**You will get:**\n{cardListOneString} \n\n**You will give away:**\n{cardListTwoString} \n\n***Make sure the trade is as expected!\nCheck if the drawn by user is as expected\nYou have 20 minutes to decide if you want to accept or deny the trade.***");
                var componentBuilderAcceptOffer = new ComponentBuilder();
                componentBuilderAcceptOffer.WithButton("ACCEPT OFFER", customId: "tradingAcceptOfferButton", style: ButtonStyle.Success);
                componentBuilderAcceptOffer.WithButton("DENY OFFER", customId: "tradingAbortButton", style: ButtonStyle.Danger);

                var toTradeWithMsg = await userToTradeWithDM.SendMessageAsync("", false, offerEmbed.Build(), components: componentBuilderAcceptOffer.Build());
                dmMessageID = toTradeWithMsg.Id;

                startTime = DateTime.Now;
                endTime = startTime.AddMinutes(20);
                do
                {
                    await Task.Delay(2000);

                } while (endTime > startTime && tradeHasBeenAccepted == false);

                if (tradeHasBeenAccepted)
                {
                    //Adjust both players trade limit
                    tradelimits.FirstOrDefault(x => x.DiscordID == arguments.User.Id.ToString()).CardsLeftToTrade -= cardListUserOne.Count;
                    tradelimits.FirstOrDefault(x => x.DiscordID == userToTradeWith.Id.ToString()).CardsLeftToTrade -= cardListUserTwo.Count;
                    var newJson = JsonConvert.SerializeObject(tradelimits);
                    File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/TradeLimit.json", newJson);


                    //card file name --- new card file name
                    var tradeList = new Dictionary<string, string>();
                    //Make the trade happen
                    foreach (var tradingCardString in cardListUserOne)
                    {
                        var cardFile = files.FirstOrDefault(x => x == "F:\\" + tradingCardString.Replace("/", "\\"));
                        var ownerDiscordID = cardFile.Split("-")[2];

                        var cardName = cardFile.Split("-");
                        cardName[2] = userToTradeWith.Id.ToString();
                        var newName = "";
                        foreach (var part in cardName) newName += part + "-";
                        newName = newName.Remove(newName.Length - 1);

                        tradeList.Add(cardFile, newName);
                    }

                    foreach (var tradingCardString in cardListUserTwo)
                    {
                        var cardFile = files.FirstOrDefault(x => x == "F:\\" + tradingCardString.Replace("/", "\\"));
                        var ownerDiscordID = cardFile.Split("-")[2];

                        var cardName = cardFile.Split("-");
                        cardName[2] = arguments.User.Id.ToString();
                        var newName = "";
                        foreach (var part in cardName) newName += part + "-";
                        newName = newName.Remove(newName.Length - 1);

                        tradeList.Add(cardFile, newName);
                    }

                    //if all card file names still exists, execute the trade
                    var validTrade = true;
                    foreach (var trade in tradeList)
                    {
                        if (!File.Exists(trade.Key)) validTrade = false;
                    }
                    if (validTrade)
                    {
                        foreach (var trade in tradeList)
                        {
                            File.Move(trade.Key, trade.Value);
                        }

                        await userToTradeWithDM.SendMessageAsync("You have accepted the trade offer and the trade has been completed!");
                        await arguments.Channel.SendMessageAsync($"Your trade offer for {userToTradeWith.Username} has been accepted! Your cards have been traded!");
                    }
                    else
                    {
                        await userToTradeWithDM.SendMessageAsync("Your trade was marked as invalid and has been aborted. One of the cards has been traded already.");
                        await arguments.Channel.SendMessageAsync($"Your trade offer for {userToTradeWith.Username} has been marked as invalid and has been aborted. One of the cards has been traded already.");
                    }
                }
                else
                {
                    await userToTradeWithDM.SendMessageAsync("You have denied the trade offer and the trade has been aborted!");
                    await arguments.Channel.SendMessageAsync($"Your trade offer for {userToTradeWith.Username} has been denied :c Your cards have not been traded.");
                }

                await toTradeWithMsg.DeleteAsync();
            }
            catch (Exception ex)
            {

            }
        }

        public static async Task GiveAllUsersDailyTrades()
        {
            var tradelimits = JsonConvert.DeserializeObject<List<TradeLimit>>(System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/TradeLimit.json"));

            foreach (var limit in tradelimits)
            {
                limit.CardsLeftToTrade = 5;
            }

            var newJson = JsonConvert.SerializeObject(tradelimits);
            File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/TradeLimit.json", newJson);
        }

        public async Task CheckFinishedMatches()
        {
            do
            {
                var currentStakeMatches = JsonConvert.DeserializeObject<List<StakeMatch>>(System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/StakeMatches.json"));
                if (currentStakeMatches == null)
                {
                    await Task.Delay(1000 * 60 * 5);
                    continue;
                }

                var matchesToMark = currentStakeMatches.Where(x => x.matchHasEnded == false && x.EndDate < DateTime.UtcNow);

                foreach (var finishedMatch in matchesToMark)
                {
                    currentStakeMatches.FirstOrDefault(x => x.matchID == finishedMatch.matchID).matchHasEnded = true;
                    //Trade cards 
                    if (finishedMatch.PlayerOneCurrentScore == finishedMatch.PlayerTwoCurrentScore)
                    {
                        var p1 = await _discord.GetUserAsync(Convert.ToUInt64(finishedMatch.PlayerOneDiscordID));
                        var p2 = await _discord.GetUserAsync(Convert.ToUInt64(finishedMatch.PlayerTwoDiscordID));
                        var dm1 = await p1.CreateDMChannelAsync();
                        var dm2 = await p2.CreateDMChannelAsync();
                        await dm1.SendMessageAsync("You stake match has ended in a draw! you won't gain or lose anything");
                        await dm2.SendMessageAsync("You stake match has ended in a draw! you won't gain or lose anything");
                        var json = JsonConvert.SerializeObject(currentStakeMatches);
                        File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/StakeMatches.json", json);
                        continue;
                    }

                    var playerDiscordIDWhoWon = finishedMatch.PlayerOneCurrentScore > finishedMatch.PlayerTwoCurrentScore ? finishedMatch.PlayerOneDiscordID : finishedMatch.PlayerTwoDiscordID;
                    var playerDiscordIDWhoLost = finishedMatch.PlayerOneCurrentScore > finishedMatch.PlayerTwoCurrentScore ? finishedMatch.PlayerTwoDiscordID : finishedMatch.PlayerOneDiscordID;

                    var lostCards = new List<string>();
                    lostCards = finishedMatch.PlayerOneDiscordID == playerDiscordIDWhoWon ? finishedMatch.PlayerTwoCardsStaked : finishedMatch.PlayerOneCardsStaked;

                    foreach (var card in lostCards)
                    {
                        var path = "F://" + card;
                        var ownerDiscordID = path.Split("-")[2];

                        var cardName = path.Split("-");
                        cardName[2] = playerDiscordIDWhoWon;
                        var newName = "";
                        foreach (var part in cardName) newName += part + "-";
                        newName = newName.Remove(newName.Length - 1);

                        File.Move(path, newName);
                    }

                    var userWin = await _discord.GetUserAsync(Convert.ToUInt64(playerDiscordIDWhoWon));
                    var userLose = await _discord.GetUserAsync(Convert.ToUInt64(playerDiscordIDWhoLost));
                    var winDM = await userWin.CreateDMChannelAsync();
                    var loseDM = await userLose.CreateDMChannelAsync();
                    await winDM.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed($"Congrats! You have won your stake match against {userLose.Username}", $"You gained all of his staked cards in your inventory [inventory](https://beatsaberbot.com/BeatSaberCards?rankingPageSelectRanking=OwnedDiscordID&searchinput={userWin.Id}#inventory)").Build());
                    await loseDM.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed($"Im sorry to tell you, but you have lost your stake match against {userWin.Username}", $"You lost all of your staked cards [inventory](https://beatsaberbot.com/BeatSaberCards?rankingPageSelectRanking=OwnedDiscordID&searchinput={userLose.Id}#inventory)").Build());

                    var newJson = JsonConvert.SerializeObject(currentStakeMatches);
                    File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/StakeMatches.json", newJson);
                }

                await Task.Delay(1000 * 60 * 5);
            } while (true);
        }

        public static void ToggleCardDrawDMNotification(SocketSlashCommand command)
        {
            var jsonGlobal = System.IO.File.ReadAllText($"../../../Resources/NewCardPacksNotification.json");
            var CardPackNotificationList = JsonConvert.DeserializeObject<List<string>>(jsonGlobal);

            if (CardPackNotificationList == null)
            {
                var list = new List<string>();
                list.Add(command.User.Id.ToString());
                CardPackNotificationList = list;
                command.Channel.SendMessageAsync("You will now receive DM notfications when new packs are available");
            }
            else
            {

                if (CardPackNotificationList.Contains(command.User.Id.ToString()))
                {
                    CardPackNotificationList.Remove(command.User.Id.ToString());
                    command.Channel.SendMessageAsync("You will now stop receiving DM notfications when new packs are available");
                }
                else
                {
                    CardPackNotificationList.Add(command.User.Id.ToString());
                    command.Channel.SendMessageAsync("You will now receive DM notfications when new packs are available");
                }
            }

            System.IO.File.WriteAllText($"../../../Resources/NewCardPacksNotification.json", JsonConvert.SerializeObject(CardPackNotificationList));
        }

        public async Task NotifyUsersOnNewCardPacks()
        {
            do
            {
                var json = System.IO.File.ReadAllText($"../../../Resources/NewCardPacksNotification.json");
                var CardPackNotificationList = JsonConvert.DeserializeObject<List<string>>(json);

                var json2 = System.IO.File.ReadAllText($"../../../Resources/DrawCardTimeOut.json");
                var playerTimeOuts = JsonConvert.DeserializeObject<List<PLayerTimeOut>>(json2);

                var playerTimeOutsWhoWantNotficiations = playerTimeOuts.Where(x => CardPackNotificationList.Contains(x.DiscordID) && x.TimeOutTill != null && x.TimeOutTill > DateTime.Now).OrderBy(x => x.TimeOutTill).ToList();


                var player = playerTimeOutsWhoWantNotficiations.First();
                var timeLeft = (DateTime)player.TimeOutTill - DateTime.Now;
                if (timeLeft.TotalSeconds > 60 * 60)
                {
                    await Task.Delay(1000 * 60 * 60);

                }
                else
                {
                    await Task.Delay(Convert.ToInt32(timeLeft.TotalSeconds) * 1000);
                    try
                    {
                        var user = await _discord.GetUserAsync(Convert.ToUInt64(player.DiscordID));
                        var dm = await user.CreateDMChannelAsync();
                        await dm.SendMessageAsync("You received new Beat Saber card packs!");
                    }
                    catch (Exception x)
                    {

                    }
                }
            } while (true);

            return;
        }

        public async Task SignCard(SocketSlashCommand command, dynamic cardfile, string discordID, string scoresaberID, string score, string rank)
        {
            //Create card 
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(cardfile.Url), "../../../Resources/img/signedcard.png");
            }
            var cardCreator = new ImageCreator("../../../Resources/img/signedcard.png");
            cardCreator.ResizeImage(735, 1211);

            var user = await _discord.GetUserAsync(Convert.ToUInt64(discordID));
            cardCreator.AddTextCenter("Drawn by: " + user.Username, Color.Black, 22, 380 + 5, 1170 + 5);
            var size = cardCreator.AddTextCenter("Drawn by: " + user.Username, Color.FromArgb(103, 90, 55), 22, 380, 1170);

            //Create the card
            await cardCreator.Create($"F:\\BeatSaberTradingCards/BeatSaber_Card-{discordID}-{discordID}-{scoresaberID}-{score}-{rank}-99999-{DateTime.UtcNow.ToShortDateString().Replace("-", "_") + "_" + DateTime.UtcNow.ToShortTimeString().Replace(":", "_")}.png");

            //send card in discord
            await command.Channel.SendFileAsync($"F:\\BeatSaberTradingCards/BeatSaber_Card-{discordID}-{discordID}-{scoresaberID}-{score}-{rank}-99999-{DateTime.UtcNow.ToShortDateString().Replace("-", "_") + "_" + DateTime.UtcNow.ToShortTimeString().Replace(":", "_")}.png");

        }

        public async Task<bool> GivePacks(long discordID, int packAmount, string message = null)
        {
            try
            {
                var jsonGlobal = System.IO.File.ReadAllText($"../../../Resources/DrawCardTimeOut.json");
                var playerTimeOuts = JsonConvert.DeserializeObject<List<PLayerTimeOut>>(jsonGlobal);

                if (playerTimeOuts != null)
                {
                    if (packAmount <= 0) return false;

                    var playerToGive = playerTimeOuts.FirstOrDefault(x => x.DiscordID.ToLower() == discordID.ToString().ToLower());

                    if (playerToGive.TimeOutTill != null && playerToGive.TimeOutTill < DateTime.Now)
                    {
                        playerToGive.PacksLeft = 4;
                        var timeTill = (DateTime)playerToGive.TimeOutTill;
                        playerToGive.TimeOutTill = timeTill.AddHours(23);
                    }

                    playerToGive.PacksLeft += packAmount;
                    System.IO.File.WriteAllText($"../../../Resources/DrawCardTimeOut.json", JsonConvert.SerializeObject(playerTimeOuts));

                    var user = await _discord.GetUserAsync(Convert.ToUInt64(discordID));
                    var dm = await user.CreateDMChannelAsync();

                    if (message == null) message = $"For somem magical reason.";

                    await dm.SendMessageAsync(message + $" You gained **{packAmount}** extra card packs.");

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async void TransferAllCards(string userFrom, string userTo)
        {
            var cards = GetAllCards();

            var userCards = cards.Where(x => x.OwnerDiscordID == userFrom).ToList();

            foreach (var card in userCards)
            {
                var cardFile = "F://" + card.Name;
                var ownerDiscordID = cardFile.Split("-")[2];

                var cardName = cardFile.Split("-");
                cardName[2] = userTo;
                var newName = "";
                foreach (var part in cardName) newName += part + "-";
                newName = newName.Remove(newName.Length - 1);

                File.Move(cardFile, newName);
            }

        }

        public static void RemoveCheaterCards(string discordID)
        {
            var cards = GetAllCards();

            var userCards = cards.Where(x => x.OwnerDiscordID == discordID);

            var allCheatCards = userCards.Where(x => x.DiscordID == "312319770550861876");

            //var cardsGrouped = new Dictionary<string, List<Card>>();
            //foreach (var card in userCards)
            //{
            //    if (!cardsGrouped.Keys.Contains(card.DiscordID))
            //    {
            //        var v = new List<Card>();
            //        v.Add(card);
            //        cardsGrouped.Add(card.DiscordID, v);
            //    }
            //    else
            //    {
            //        cardsGrouped[card.DiscordID].Add(card);
            //    }
            //}

            //var cheatCards = new List<Card>();
            //var cheatGroup = cardsGrouped.Where(x => x.Value.Count > 20 && cards.Where(y => y.OwnerDiscordID == x.Key).Count() < 10);
            //var count = cheatGroup.Count();

            //var allCheatCards = new List<Card>();

            //foreach (var cg in cheatGroup)
            //{
            //    allCheatCards.AddRange(cg.Value);
            //}

            foreach (var c in allCheatCards)
            {
                File.Delete("F://" + c.Name);
            }
        }

        public class StakeMatch
        {
            public string PlayerOneDiscordID { get; set; }
            public string PlayerOneUsername { get; set; }
            public string PlayerOneProfileUrl { get; set; }
            public string PlayerOneScoresaberID { get; set; }
            public double PlayerOneCurrentScore { get; set; }
            public long PlayerOneRank { get; set; }
            public List<string> PlayerOneCardsStaked { get; set; }
            public string PlayerTwoDiscordID { get; set; }
            public string PlayerTwoUsername { get; set; }
            public string PlayerTwoProfileUrl { get; set; }
            public string PlayerTwoScoresaberID { get; set; }
            public double PlayerTwoCurrentScore { get; set; }
            public long PlayerTwoRank { get; set; }
            public List<string> PlayerTwoCardsStaked { get; set; }
            public string mapKey { get; set; }
            public string mapDiff { get; set; }
            public string mapUrl { get; set; }

            public bool matchHasEnded { get; set; }

            public string mapName { get; set; }
            public string mapHash { get; set; }
            public long mapMaxScore { get; set; }
            public DateTime EndDate { get; set; }

            public Guid matchID { get; set; }

        }

        public class Card
        {
            public string Name { get; set; }
            public string DiscordID { get; set; }

            public string OwnerDiscordID { get; set; }
            public string ScoresaberID { get; set; }
            public int Score { get; set; }
            public int rank { get; set; }
            public int luckNumber { get; set; }

            public DateTime CreationDate { get; set; }
        }

        public class TradeLimit
        {
            public string DiscordID { get; set; }
            public int CardsLeftToTrade { get; set; }
        }
    }
}
