using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBeatSaberBot.Api.BeatSaverApi;
using DiscordBeatSaberBot.Api.BeatSaverApi.Models.New;
using DiscordBeatSaberBot.Api.BeatSaverApi.Models.v2;
using Newtonsoft.Json;
using ScoreSaberLib;
using ScoreSaberLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using static DiscordBeatSaberBot.BeatSaberCardCollection;

namespace DiscordBeatSaberBot.Handlers
{
    public class CupOfTheDayHandler
    {
        private List<LeaderboardInfoModel.Leaderboard> serverDailyMaps = null;
        private ScoreSaberClient scoresaberClient;
        private DiscordSocketClient _discord;

        public CupOfTheDayHandler(DiscordSocketClient discord, bool startScoresaberWebsocket = true)
        {
            _discord = discord;

            scoresaberClient = new ScoreSaberClient();

            RefreshDailyMaps();

            if (startScoresaberWebsocket)
            {
                scoresaberClient.Api.ScoreFeed.WebSocket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                scoresaberClient.Api.ScoreFeed.Connect();
                WebsocketTimer();
                scoresaberClient.Api.ScoreFeed.OnPlayReceived += Feed_OnPlayReceived;
            }
        }

        private void Feed_OnDisconnect(object sender, EventArgs e)
        {
            scoresaberClient.Api.ScoreFeed.WebSocket.Close();
            scoresaberClient.Api.ScoreFeed.Connect();
            scoresaberClient.Api.ScoreFeed.OnPlayReceived += Feed_OnPlayReceived;
            scoresaberClient.Api.ScoreFeed.OnDisconnect += Feed_OnDisconnect;
        }

        private async void RefreshDailyMaps()
        {
            while (true)
            {
                var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
                var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);
                serverDailyMaps = new List<LeaderboardInfoModel.Leaderboard>();
                foreach (var server in cotdServers)
                {
                    RefreshLiveMMRPoints(server);
                }
                var cotdServersNewJson = JsonConvert.SerializeObject(cotdServers);
                System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersNewJson);

                setAllDailyMaps();

                await Task.Delay(1000 * 60 * 30);
            }
        }

        private void setAllDailyMaps()
        {
            var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
            var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);

            foreach (var server in cotdServers)
            {
                if (server.TodaysMap != null) serverDailyMaps.Add(server.TodaysMap);
                if (server.ServerID == "0")
                {
                    if (server.TodaysMapFocus != null) serverDailyMaps.Add(server.TodaysMapFocus);
                    if (server.TodaysMapHardcore != null) serverDailyMaps.Add(server.TodaysMapHardcore);
                }
            }
        }

        private async void WebsocketTimer()
        {
            while (true)
            {
                await Task.Delay(10000);
                if (!scoresaberClient.Api.ScoreFeed.WebSocket.IsAlive)
                {
                    scoresaberClient.Api.ScoreFeed.WebSocket.Connect();
                }
            }
        }

        private async void Feed_OnPlayReceived(object sender, ScoreSaberLib.Models.ScoreFeedModel e)
        {
            //COTD
            if (serverDailyMaps != null)
            {
                var playedMap = e.CommandData;
                await UploadDailyMapScore(playedMap.Leaderboard.SongHash, playedMap.Leaderboard.Difficulty.DifficultyRaw, playedMap.Leaderboard.Id, playedMap.Score.LeaderboardPlayerInfo.Id, playedMap.Score.BaseScore);
            }


            //STAKING
            var currentStakeMatches = JsonConvert.DeserializeObject<List<StakeMatch>>(System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/StakeMatches.json"));

            if (currentStakeMatches != null)
            {
                var playerOneList = currentStakeMatches.Where(x => x.PlayerOneScoresaberID == e.CommandData.Score.LeaderboardPlayerInfo.Id.ToString());
                var playerTwoList = currentStakeMatches.Where(x => x.PlayerTwoScoresaberID == e.CommandData.Score.LeaderboardPlayerInfo.Id.ToString());
                if (playerOneList.Count() > 0 || playerTwoList.Count() > 0)
                {
                    var diff = e.CommandData.Leaderboard.Difficulty.DifficultyRaw.Replace("_", " ").Trim().Split(' ')[0].ToLower();
                    var maphash = e.CommandData.Leaderboard.SongHash.ToLower();
                    var playerOne = playerOneList.FirstOrDefault(x => x.mapDiff.ToLower() == diff && x.mapHash.ToLower() == maphash && x.EndDate > DateTime.UtcNow);
                    var playerTwo = playerTwoList.FirstOrDefault(x => x.mapDiff.ToLower() == diff && x.mapHash.ToLower() == maphash && x.EndDate > DateTime.UtcNow);

                    if (playerOne != null)
                    {
                        double percentage = Convert.ToDouble(e.CommandData.Score.BaseScore) / playerOne.mapMaxScore * 100;
                        if (percentage > playerOne.PlayerOneCurrentScore)
                        {
                            //overwrite score
                            playerOne.PlayerOneCurrentScore = percentage;
                            var newJson = JsonConvert.SerializeObject(currentStakeMatches);
                            File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/StakeMatches.json", newJson);
                        }

                    }

                    if (playerTwo != null)
                    {

                        double percentage = Convert.ToDouble(e.CommandData.Score.BaseScore) / playerTwo.mapMaxScore * 100;
                        if (percentage > playerTwo.PlayerTwoCurrentScore)
                        {
                            //overwrite score
                            playerTwo.PlayerTwoCurrentScore = percentage;
                            var newJson = JsonConvert.SerializeObject(currentStakeMatches);
                            File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/StakeMatches.json", newJson);
                        }

                    }
                }
            }
        }

        public async Task UploadDailyMapScore(string songHash, string difficultyRaw, long leaderboardID, string playerID, long score)
        {
            try
            {
                if (serverDailyMaps.FirstOrDefault(x => x.SongHash == songHash && x.Difficulty.DifficultyRaw == difficultyRaw) != null)
                {
                    var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
                    var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);

                    var todaysMaps = serverDailyMaps.FirstOrDefault(x => x.SongHash == songHash);

                    var serversThatHasThisMap = new List<COTDServer>();

                    var focus = cotdServers.Where(x => x.TodaysPlayersFocus != null && x.TodaysMapFocus != null && x.TodaysMapFocus.SongHash == todaysMaps.SongHash && x.TodaysPlayersFocus.FirstOrDefault(i => i.ScoreSaberID == playerID) != null).ToList();
                    var standard = cotdServers.Where(x => x.TodaysPlayers != null && x.TodaysMap != null && x.TodaysMap.SongHash == todaysMaps.SongHash && x.TodaysPlayers.FirstOrDefault(i => i.ScoreSaberID == playerID) != null).ToList();
                    var hardcore = cotdServers.Where(x => x.TodaysPlayersHardcore != null && x.TodaysMapHardcore != null && x.TodaysMapHardcore.SongHash == todaysMaps.SongHash && x.TodaysPlayersHardcore.FirstOrDefault(i => i.ScoreSaberID == playerID) != null).ToList();

                    if (focus.Count() > 0) serversThatHasThisMap.AddRange(focus);
                    if (standard.Count() > 0) serversThatHasThisMap.AddRange(standard);
                    if (hardcore.Count() > 0) serversThatHasThisMap.AddRange(hardcore);

                    foreach (var serverMapPlayed in serversThatHasThisMap)
                    {
                        COTDPlayer player = null;

                        if (serverMapPlayed.ServerID == "0")
                        {

                            if (serverMapPlayed.TodaysMapFocus.Id == leaderboardID) player = serverMapPlayed.TodaysPlayersFocus.FirstOrDefault(x => x.ScoreSaberID == playerID);
                            if (serverMapPlayed.TodaysMap.Id == leaderboardID) player = serverMapPlayed.TodaysPlayers.FirstOrDefault(x => x.ScoreSaberID == playerID);
                            if (serverMapPlayed.TodaysMapHardcore.Id == leaderboardID) player = serverMapPlayed.TodaysPlayersHardcore.FirstOrDefault(x => x.ScoreSaberID == playerID);
                            if (player.TodaysScore < score && player != null)
                            {
                                player.TodaysScore = score;
                            }
                        }
                        else
                        {
                            player = serverMapPlayed.TodaysPlayers.FirstOrDefault(x => x.ScoreSaberID == playerID);
                            if (player.TodaysScore < score)
                            {
                                player.TodaysScore = score;
                            }
                        }

                        //Refresh MMR
                        RefreshLiveMMRPoints(serverMapPlayed);

                        //Send score in feed channel
                        try
                        {
                            if (serverMapPlayed.DiscordFeedChannelID != null)
                            {
                                var channel = (ITextChannel)await _discord.GetChannelAsync(Convert.ToUInt64(serverMapPlayed.DiscordFeedChannelID));
                                if (channel == null)
                                {
                                    serverMapPlayed.DiscordFeedChannelID = null;
                                }
                                else
                                {
                                    var messages = await channel.GetMessagesAsync(10).FlattenAsync();
                                    if (messages != null && messages.Count() >= 2)
                                    {
                                        var leaderboardRaw = messages.FirstOrDefault(x => x.Embeds.First().Footer.Value.Text == "001");
                                        var newPlayRaw = messages.FirstOrDefault(x => x.Embeds.First().Footer.Value.Text == "002");
                                        var leaderboard = (RestUserMessage)leaderboardRaw;
                                        var newPlay = (RestUserMessage)newPlayRaw;

                                        await leaderboard.ModifyAsync(x => x.Embed = GetMapFeed(serverMapPlayed).Build());
                                        await newPlay.ModifyAsync(x => x.Embed = GetNewPlayFeed(player.Name, Math.Round((double)player.TodaysScore * 100 / serverMapPlayed.TodaysMap.MaxScore, 2).ToString(), player.MMR.ToString(), player.TotalWins.ToString(), player.ScoreSaberID).Build());

                                        var fakeMsg = await channel.SendMessageAsync("yeet");
                                        await fakeMsg.DeleteAsync();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var f = ex;
                        }
                    }

                    var cotdServersNewJson = JsonConvert.SerializeObject(cotdServers);
                    System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersNewJson);
                }
            }
            catch (Exception ex)
            {
                var f = ex;
            }
        }

        public List<LeaderboardInfoModel.Leaderboard> GetDailyMaps()
        {
            return serverDailyMaps;
        }

        public async Task<bool> ReuploadDailyMapScores(string scoresaberId)
        {
            var recentPlays = await new ScoreSaberClient().Api.Players.GetPlayerScores(Convert.ToInt64(scoresaberId), 50, Players.sort.recent);
            var recentPlaysInTime = recentPlays.Where(x => x.Score.TimeSet > DateTime.Now.Date);
            var hasUploaded = false;
            foreach (var recentplay in recentPlaysInTime)
            {
                if (serverDailyMaps.FirstOrDefault(x => x.SongHash == recentplay.Leaderboard.SongHash.ToString() && x.Difficulty.DifficultyRaw == recentplay.Leaderboard.Difficulty.DifficultyRaw) != null)
                {
                    await UploadDailyMapScore(recentplay.Leaderboard.SongHash, recentplay.Leaderboard.Difficulty.DifficultyRaw, recentplay.Leaderboard.Id, scoresaberId, recentplay.Score.BaseScore);
                    hasUploaded = true;
                }
            }

            return hasUploaded;
        }

        public enum DailyMapMode
        {
            Focus,
            Standard,
            Hardcore
        }
        public static async Task<LeaderboardInfoModel.Leaderboard> GenerateGlobalDailyMap(string hashFromMapBefore, DailyMapMode mode)
        {
            if (hashFromMapBefore == null) hashFromMapBefore = "";
            var maps = new List<Maps>();
            var mapsBackup = new List<Maps>();
            var validMaps = new List<Maps>();
            var validBackupMaps = new List<Maps>();

            if (mode == DailyMapMode.Focus) //Focus pool
            {
                var maps1 = await BeatSaverApi.GetMapForCupOfTheDayFocus(0);
                var maps2 = await BeatSaverApi.GetMapForCupOfTheDayFocus(1);
                maps.AddRange(maps1.Docs);
                maps.AddRange(maps2.Docs);

                var mapsBackUp1 = await BeatSaverApi.GetMapForCupOfTheDayFocus(5);
                var mapsBackUp2 = await BeatSaverApi.GetMapForCupOfTheDayFocus(6);
                var mapsBackUp3 = await BeatSaverApi.GetMapForCupOfTheDayFocus(7);
                var mapsBackUp4 = await BeatSaverApi.GetMapForCupOfTheDayFocus(8);
                mapsBackup.AddRange(mapsBackUp1.Docs);
                mapsBackup.AddRange(mapsBackUp2.Docs);
                mapsBackup.AddRange(mapsBackUp3.Docs);
                mapsBackup.AddRange(mapsBackUp4.Docs);

                validMaps = maps.Where(x => x.Stats.Score > 0.75 && x.Metadata.Duration < 2000 && x.Stats.Upvotes > 20 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Count() > 0 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Last().Nps < 3.5).ToList();
                validBackupMaps = mapsBackup.Where(x => x.Stats.Score > 0.75 && x.Metadata.Duration < 2000 && x.Stats.Upvotes > 20 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Count() > 0 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Last().Nps < 3.5).ToList();
            }
            if (mode == DailyMapMode.Standard) //Standard pool
            {
                var maps1 = await BeatSaverApi.GetMapForCupOfTheDayStandard(0);
                var maps2 = await BeatSaverApi.GetMapForCupOfTheDayStandard(1);
                maps.AddRange(maps1.Docs);
                maps.AddRange(maps2.Docs);

                var mapsBackUp1 = await BeatSaverApi.GetMapForCupOfTheDayStandard(5);
                var mapsBackUp2 = await BeatSaverApi.GetMapForCupOfTheDayStandard(6);
                var mapsBackUp3 = await BeatSaverApi.GetMapForCupOfTheDayStandard(7);
                var mapsBackUp4 = await BeatSaverApi.GetMapForCupOfTheDayStandard(8);
                mapsBackup.AddRange(mapsBackUp1.Docs);
                mapsBackup.AddRange(mapsBackUp2.Docs);
                mapsBackup.AddRange(mapsBackUp3.Docs);
                mapsBackup.AddRange(mapsBackUp4.Docs);

                validMaps = maps.Where(x => x.Stats.Score > 0.70 && x.Metadata.Duration < 2000 && x.Stats.Upvotes > 20 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Count() > 0 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Last().Nps > 5.5 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Last().Nps < 9).ToList();
                validBackupMaps = mapsBackup.Where(x => x.Stats.Score > 0.70 && x.Metadata.Duration < 2000 && x.Stats.Upvotes > 20 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Count() > 0 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Last().Nps > 5.5 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Last().Nps < 9).ToList();
            }
            if (mode == DailyMapMode.Hardcore) //Hardcore pool
            {
                var maps1 = await BeatSaverApi.GetMapForCupOfTheDayHardcore(0);
                var maps2 = await BeatSaverApi.GetMapForCupOfTheDayHardcore(1);
                maps.AddRange(maps1.Docs);
                maps.AddRange(maps2.Docs);

                var mapsBackUp1 = await BeatSaverApi.GetMapForCupOfTheDayHardcore(5);
                var mapsBackUp2 = await BeatSaverApi.GetMapForCupOfTheDayHardcore(6);
                var mapsBackUp3 = await BeatSaverApi.GetMapForCupOfTheDayHardcore(7);
                var mapsBackUp4 = await BeatSaverApi.GetMapForCupOfTheDayHardcore(8);
                mapsBackup.AddRange(mapsBackUp1.Docs);
                mapsBackup.AddRange(mapsBackUp2.Docs);
                mapsBackup.AddRange(mapsBackUp3.Docs);
                mapsBackup.AddRange(mapsBackUp4.Docs);

                validMaps = maps.Where(x => x.Stats.Score > 0.40 && x.Metadata.Duration < 2000 && x.Stats.Upvotes > 3 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Count() > 0 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Last().Nps > 8).ToList();
                validBackupMaps = mapsBackup.Where(x => x.Stats.Score > 0.40 && x.Metadata.Duration < 2000 && x.Stats.Upvotes > 3 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Count() > 0 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Last().Nps > 8).ToList();
            }

            //Get the last 20 map days of map hashes from all modes
            var historyData = GetHistoryData();
            var mapsThatHaveBeenPlayedRecently = new List<string>();
            for (var i = 0; i > -30; i--)
            {
                var day = DateTime.Now.AddDays(i);
                if (historyData.ContainsKey(day.Date))
                {
                    var historyDay = historyData[day.Date];
                    if (historyData != null)
                    {
                        var globalServer = historyDay.FirstOrDefault(x => x.ServerID == "0");
                        mapsThatHaveBeenPlayedRecently.Add(globalServer.TodaysMap.SongHash.ToLower());
                        mapsThatHaveBeenPlayedRecently.Add(globalServer.TodaysMapFocus.SongHash.ToLower());
                        mapsThatHaveBeenPlayedRecently.Add(globalServer.TodaysMapHardcore.SongHash.ToLower());
                    }
                }
            }


            LeaderboardInfoModel.Leaderboard chosenMap;
            var random = new Random();
            var count = 0;
            do
            {
                count++;

                var mapInfo = validMaps.ToList()[random.Next(0, validMaps.Count())].Versions.First();
                if (count > 20) mapInfo = validBackupMaps.ToList()[random.Next(0, validBackupMaps.Count())].Versions.First(); //Grab the backup list when count is past mapHistoryCount

                var diffnr = 9;
                switch (mapInfo.Diffs.Where(x => x.Characteristic == "Standard").Last().Difficulty.ToString())
                {
                    case "Easy":
                        diffnr = 1;
                        break;
                    case "Normal":
                        diffnr = 3;
                        break;
                    case "Hard":
                        diffnr = 5;
                        break;
                    case "Expert":
                        diffnr = 7;
                        break;
                    case "ExpertPlus":
                        diffnr = 9;
                        break;
                    default:
                        diffnr = 9;
                        break;
                }
                chosenMap = await new ScoreSaberClient().Api.Leaderboards.GetLeaderboardInfoByHashcode(mapInfo.Hash.ToUpper(), (Leaderboards.Difficulty)diffnr);

                if (chosenMap != null) chosenMap.MaxScore = mapInfo.Diffs.Where(x => x.Characteristic == "Standard").Last().MaxScore;

            } while (chosenMap == null || hashFromMapBefore.ToLower() == chosenMap.SongHash.ToLower() || mapsThatHaveBeenPlayedRecently.Contains(chosenMap.SongHash.ToLower()));

            return chosenMap;
        }

        private void RefreshLiveMMRPoints(COTDServer server)
        {
            if (server.TodaysPlayers != null)
            {
                foreach (var player in server.TodaysPlayers)
                {
                    //TODO Make mmr change for focus mode and hardcore mode
                    player.TodaysMMRChange = CalculateMMRPoints(player, server, server.TodaysPlayers, "standard", calculateSubstractPoints: true);
                }
            }
            if (server.TodaysPlayersFocus != null)
            {
                foreach (var player in server.TodaysPlayersFocus)
                {
                    //TODO Make mmr change for focus mode and hardcore mode
                    player.TodaysMMRChange = CalculateMMRPoints(player, server, server.TodaysPlayersFocus, "focus", calculateSubstractPoints: true);
                }
            }
            if (server.TodaysPlayersHardcore != null)
            {
                foreach (var player in server.TodaysPlayersHardcore)
                {
                    //TODO Make mmr change for focus mode and hardcore mode
                    player.TodaysMMRChange = CalculateMMRPoints(player, server, server.TodaysPlayersHardcore, "hardcore", calculateSubstractPoints: true);
                }
            }
        }

        private double CalculateMMRPoints(COTDPlayer player, COTDServer server, List<COTDPlayer> todaysPlayers, string mode = "standard", bool calculateSubstractPoints = false)
        {
            //Dont calculate mmr when no score or when there is only 1 player 
            if (player.TodaysScore <= 0 || todaysPlayers.Where(x => x.TodaysScore > 0).Count() <= 1)
            {
                //If there is only 1 player, give it the passed mmr
                if (todaysPlayers.Where(x => x.TodaysScore > 0).Count() == 1)
                {
                    if (mode == "focus" && player.TodaysScore > 0) return 1;
                    if (mode == "standard" && player.TodaysScore > 0) return 3;
                    if (mode == "hardcore" && player.TodaysScore > 0) return 5;
                }

                return 0;
            }

            var todaysPlayersWhoHaveScores = todaysPlayers.Where(x => x.TodaysScore > 0 && x.MMR != 600).ToList();

            //Give player MMR based on worse player above them or better persons below them
            var mmr = player.MMR;
            var todaysPlayerRank = todaysPlayersWhoHaveScores.OrderByDescending(x => x.TodaysScore).ToList().IndexOf(player);
            var playersAbove = todaysPlayersWhoHaveScores.Where(x => x.TodaysScore >= player.TodaysScore && x.MMR <= player.MMR);
            var playersBelow = todaysPlayersWhoHaveScores.Where(x => x.TodaysScore <= player.TodaysScore && x.MMR >= player.MMR);
            double avgMmrAboveWhereMmrIsLower = player.MMR;
            double avgMmrBelowWhereMmrIsHigher = player.MMR;
            if (playersAbove.Count() > 0) avgMmrAboveWhereMmrIsLower = playersAbove.Average(x => x.MMR);
            if (playersBelow.Count() > 0) avgMmrBelowWhereMmrIsHigher = playersBelow.Average(x => x.MMR);

            var mmrPointsplus = avgMmrAboveWhereMmrIsLower - player.MMR;
            var mmrPointsMin = avgMmrBelowWhereMmrIsHigher - player.MMR;

            var mmrPointsRaw = mmrPointsplus + mmrPointsMin;
            var mmrPoints = mmrPointsRaw * 100 / player.MMR;


            //Give everyone mmr based on their todays rank relative to their all time rank
            //Depends on the all time rank of the player. The lower the rank the more mmr if placed higher.
            var basePoints = 20;
            var negativePoints = -2;
            var negativeRange = 0.2;
            var allTimePlayer = server.AllTimePlayers.FirstOrDefault(x => x.ScoreSaberID == player.ScoreSaberID && x.DiscordID == player.DiscordID);
            if (allTimePlayer != null)
            {
                double rank = server.AllTimePlayers.OrderByDescending(x => x.MMR).ToList().IndexOf(allTimePlayer);
                var totalPlayerCount = Convert.ToDouble(server.AllTimePlayers.Count);

                var relativeTotalPlayerCountPercentage = todaysPlayersWhoHaveScores.Count * 100 / totalPlayerCount / 100;

                var rankOneBasePoints = (((totalPlayerCount * relativeTotalPlayerCountPercentage - todaysPlayerRank) * basePoints) / totalPlayerCount) - negativePoints;
                var extraRelativePoints = (rank * rankOneBasePoints) / (totalPlayerCount * negativeRange);

                mmrPoints += extraRelativePoints;
            }

            //Give #1 +5 static
            if (todaysPlayers.OrderByDescending(x => x.TodaysScore).First().TodaysScore == player.TodaysScore) mmrPoints += 5;

            //Give mmr for passing if its the public server
            if (mode == "focus" && player.TodaysScore > 0) mmrPoints += 1;
            if (mode == "standard" && player.TodaysScore > 0) mmrPoints += 3;
            if (mode == "hardcore" && player.TodaysScore > 0) mmrPoints += 5;

            //Multiplier
            mmrPoints = mmrPoints * 1.5;

            //If its a players first time, cap the mmr points at 30
            if (mmrPoints > 30 && allTimePlayer.MMR == 600) mmrPoints = 30;

            //standarize points
            if (calculateSubstractPoints)
            {
                var totalMmr = 0.0;
                foreach (var p in todaysPlayersWhoHaveScores)
                {
                    totalMmr += CalculateMMRPoints(p, server, todaysPlayersWhoHaveScores, mode);
                }

                var pointsToSubstract = totalMmr / todaysPlayersWhoHaveScores.Count() / 3;
                mmrPoints -= pointsToSubstract;
            }

            //Make everyone gain more or less depending on their divison.
            if(mmrPoints > 0)
            {
                var currentMmr = allTimePlayer.MMR;
                var reduction = 1.0;                
                if (currentMmr < 600) reduction = 0.9;
                if (currentMmr > 600) reduction = 0.8;
                if (currentMmr > 700) reduction = 0.6;
                if (currentMmr > 800) reduction = 0.3;
                if (currentMmr > 900) reduction = 0.2;
                if (currentMmr > 1100) reduction = 0.1;

                mmrPoints = mmrPoints * reduction;
            }

            return mmrPoints;
        }

        //public async Task NotifyGlobalPlayersWhoHasNotSetAScoreYet(DiscordSocketClient discord)
        //{
        //    var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
        //    var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);
        //    var globalServer = cotdServers.FirstOrDefault(x => x.ServerID == "0");

        //    var listToMessage = new List<string>();
        //    if (globalServer.TodaysPlayersFocus != null) foreach (var player in globalServer.TodaysPlayersFocus) if (player.TodaysScore <= 0) if (!listToMessage.Contains(player.DiscordID)) listToMessage.Add(player.DiscordID);
        //    if (globalServer.TodaysPlayers != null) foreach (var player in globalServer.TodaysPlayersFocus) if (player.TodaysScore <= 0) if (!listToMessage.Contains(player.DiscordID)) listToMessage.Add(player.DiscordID);
        //    if (globalServer.TodaysPlayersHardcore != null) foreach (var player in globalServer.TodaysPlayersFocus) if (player.TodaysScore <= 0) if (!listToMessage.Contains(player.DiscordID)) listToMessage.Add(player.DiscordID);

        //    foreach(var id in listToMessage)
        //    {
        //        try
        //        {
        //            var user = await discord.GetUserAsync(Convert.ToUInt64(id));
        //            var dm = await user.CreateDMChannelAsync();
        //            dm.SendMessageAsync("Reminder: You have 1 hour left on the cup of the day ");
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    }
        //}

        public async Task ResetDailyMap(DiscordSocketClient discord)
        {
            var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
            var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);

            foreach (var server in cotdServers)
            {
                if (server.TodaysPlayers == null || server.TodaysPlayers.Count() <= 0) continue;
                if (server.AllTimePlayers == null || server.AllTimePlayers.Count() <= 0) continue;

                foreach (var player in server.TodaysPlayers)
                {
                    var allTimePlayer = server.AllTimePlayers.FirstOrDefault(x => x.ScoreSaberID == player.ScoreSaberID && x.DiscordID == player.DiscordID);
                    var mmrBefore = Math.Round(allTimePlayer.MMR, 2);

                    var mmrPointsStandard = CalculateMMRPoints(player, server, server.TodaysPlayers, "standard", calculateSubstractPoints: true);
                    allTimePlayer.MMR += mmrPointsStandard;

                    if (server.ServerID == "0")
                    {
                        if (server.TodaysPlayersFocus != null && server.TodaysPlayersFocus.Count() >= 2)
                        {
                            var mmrPointsFocus = CalculateMMRPoints(server.TodaysPlayersFocus.FirstOrDefault(x => x.ScoreSaberID == player.ScoreSaberID && x.DiscordID == player.DiscordID), server, server.TodaysPlayersFocus, "focus", calculateSubstractPoints: true);
                            allTimePlayer.MMR += mmrPointsFocus;
                            if (mmrPointsFocus > 0) await new BeatSaberCardCollection(discord).GivePacks(Convert.ToInt64(allTimePlayer.DiscordID), 1, $"You gained a free card pack for gaining +{Math.Round(mmrPointsFocus, 2)} mmr points on Focus Mode!");
                        }

                        if (server.TodaysPlayersHardcore != null && server.TodaysPlayersHardcore.Count() >= 2)
                        {
                            var mmrPointsHardcore = CalculateMMRPoints(server.TodaysPlayersHardcore.FirstOrDefault(x => x.ScoreSaberID == player.ScoreSaberID && x.DiscordID == player.DiscordID), server, server.TodaysPlayersHardcore, "hardcore", calculateSubstractPoints: true);
                            allTimePlayer.MMR += mmrPointsHardcore;
                            if (mmrPointsHardcore > 0) await new BeatSaberCardCollection(discord).GivePacks(Convert.ToInt64(allTimePlayer.DiscordID), 1, $"You gained a free card pack for gaining +{Math.Round(mmrPointsHardcore, 2)} mmr points on Hardcore Mode!");
                        }

                        if (mmrPointsStandard > 0) await new BeatSaberCardCollection(discord).GivePacks(Convert.ToInt64(allTimePlayer.DiscordID), 1, $"You gained a free card pack for gaining +{Math.Round(mmrPointsStandard, 2)} mmr points on Standard Mode!");

                        var mmrAfter = Math.Round(allTimePlayer.MMR, 2);

                        try
                        {
                            //DM
                            var user = await _discord.GetUserAsync(Convert.ToUInt64(allTimePlayer.DiscordID));

                            var muteDataJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTD_DiscordID_hasMutedDM.json");
                            var muteData = new Dictionary<string, bool>();
                            bool shouldSend = true;
                            if (muteData.ContainsKey(user.Id.ToString())) shouldSend = muteData[user.Id.ToString()];

                            if (shouldSend)
                            {
                                var dm = await user.CreateDMChannelAsync();
                                await dm.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Cup of the day ended", $"You went from **{mmrBefore}** mmr points to **{mmrAfter}**. Check out your progress on your [profile](https://beatsaberbot.com/COTDProfile?id={player.DiscordID}). A new [cup of the day](https://beatsaberbot.com/CupOfTheDay) has started.").Build());

                                if (mmrBefore < 600 && mmrAfter > 600) SendPromotionOrDemotion(true, "Silver", Color.LighterGrey, 600);
                                if (mmrBefore > 600 && mmrAfter < 600) SendPromotionOrDemotion(false, "Bronze", Color.DarkRed, 500);

                                if (mmrBefore < 700 && mmrAfter > 700) SendPromotionOrDemotion(true, "Gold", Color.Gold, 700);
                                if (mmrBefore > 700 && mmrAfter < 700) SendPromotionOrDemotion(false, "Silver", Color.LighterGrey, 600);

                                if (mmrBefore < 800 && mmrAfter > 800) SendPromotionOrDemotion(true, "Platinum", Color.Green, 800);
                                if (mmrBefore > 800 && mmrAfter < 800) SendPromotionOrDemotion(false, "Gold", Color.Gold, 700);

                                if (mmrBefore < 900 && mmrAfter > 900) SendPromotionOrDemotion(true, "Diamond", Color.Blue, 900);
                                if (mmrBefore > 900 && mmrAfter < 900) SendPromotionOrDemotion(false, "Platinum", Color.Green, 800);

                                if (mmrBefore < 1100 && mmrAfter > 1100) SendPromotionOrDemotion(true, "Grand Master", Color.Red, 1100);
                                if (mmrBefore > 1100 && mmrAfter < 900) SendPromotionOrDemotion(false, "Diamond", Color.Blue, 900);

                                async void SendPromotionOrDemotion(bool isPromotion, string division, Color color, int divisionStartPoint)
                                {
                                    var percentage = 0.0;
                                    var amountOfPlayersInDivision = server.AllTimePlayers.Where(x => x.MMR > divisionStartPoint && x.MMR < divisionStartPoint + 100);
                                    percentage = amountOfPlayersInDivision.Count() * 100 / server.AllTimePlayers.Count();

                                    var title = "";
                                    if (isPromotion) title = $"Congratulations! You have been promoted to {division}!";
                                    else title = $"Sadly, you have been demoted to {division}";

                                    var description = $"You are now in the top **{percentage.ToString("0.00")}%.** ";
                                    if (isPromotion) description += "Keep it going!";
                                    else description += $"Luckly, tomorrow is another day.";

                                    var embed = EmbedBuilderExtension.NullEmbed(title, $"{description}");
                                    embed.Color = color;
                                    await dm.SendMessageAsync("", false, embed.Build());
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }

                //Give standard winner points 
                if (server.TodaysPlayers != null && server.TodaysPlayers.Count() >= 2)
                {
                    var winner = server.TodaysPlayers.OrderByDescending(x => x.TodaysScore).First();
                    var allTimePlayerWinner = server.AllTimePlayers.FirstOrDefault(x => x.ScoreSaberID == winner.ScoreSaberID && x.DiscordID == winner.DiscordID);
                    if (winner.TodaysScore != 0)
                    {
                        if (server.TodaysPlayers.Where(x => x.TodaysScore > 0).Count() > 1) allTimePlayerWinner.TotalWins += 1;
                    }
                }

                //Give Focus winner points 
                if (server.TodaysPlayersFocus != null && server.TodaysPlayersFocus.Count() >= 2)
                {
                    var winner = server.TodaysPlayersFocus.OrderByDescending(x => x.TodaysScore).First();
                    var allTimePlayerWinner = server.AllTimePlayers.FirstOrDefault(x => x.ScoreSaberID == winner.ScoreSaberID && x.DiscordID == winner.DiscordID);
                    if (winner.TodaysScore != 0)
                    {
                        if (server.TodaysPlayersFocus.Where(x => x.TodaysScore > 0).Count() > 1) allTimePlayerWinner.TotalWins += 1;
                    }
                }

                //Give Hardcore winner points 
                if (server.TodaysPlayersHardcore != null && server.TodaysPlayersHardcore.Count() >= 2)
                {
                    var winner = server.TodaysPlayersHardcore.OrderByDescending(x => x.TodaysScore).First();
                    var allTimePlayerWinner = server.AllTimePlayers.FirstOrDefault(x => x.ScoreSaberID == winner.ScoreSaberID && x.DiscordID == winner.DiscordID);
                    if (winner.TodaysScore != 0)
                    {
                        if (server.TodaysPlayersHardcore.Where(x => x.TodaysScore > 0).Count() > 1) allTimePlayerWinner.TotalWins += 1;
                    }
                }
            }

            //store the new data
            StoreHistoryData(cotdServers);

            foreach (var server in cotdServers)
            {
                server.TodaysPlayersFocus = null;
                server.TodaysPlayers = null;
                server.TodaysPlayersHardcore = null;
            }

            var cotdServersNewJson = JsonConvert.SerializeObject(cotdServers);
            System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersNewJson);

            //Reset cycle
            await ResetDailyMapFromAllServer();
            setAllDailyMaps();
            await ResetDiscordServerFeeds(discord);
            await AutomaticallyLetUsersJoin();
        }

        public async Task AutomaticallyLetUsersJoin()
        {
            var automaticJoinDataJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTD_DiscordID_hasAutomaticJoin.json");
            var automaticJoinData = new Dictionary<string, bool>();
            
            foreach(var user in automaticJoinData)
            {
                try
                {
                    if (user.Value)
                    {
                        //Join
                        var scoresaberID = await RoleAssignment.GetScoresaberIdWithDiscordId(user.Key);
                        var player = new CupOfTheDayHandler.Player() { ScoreSaberID = scoresaberID, Name = "" };
                        CupOfTheDayHandler.ServerPlayerJoin(player, user.Key, "0");
                    }
                }
                catch
                {

                }
            }
        }

        public void StoreHistoryData(List<COTDServer> serverDataToday)
        {
            var cotdHistoryRaw = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDHistoryData.json");
            var cotdHistory = JsonConvert.DeserializeObject<Dictionary<DateTime, List<COTDServer>>>(cotdHistoryRaw);

            var cotdServers = serverDataToday;

            var cotdHistoryNew = cotdHistory;
            if (cotdHistory == null)
            {
                cotdHistoryNew = new Dictionary<DateTime, List<COTDServer>>();
                cotdHistoryNew.Add(DateTime.Now.Date, cotdServers);
            }
            if (cotdHistory != null)
            {
                if (!cotdHistoryNew.ContainsKey(DateTime.Now.Date)) cotdHistoryNew.Add(DateTime.Now.Date, cotdServers);
                else cotdHistoryNew[DateTime.Now.Date] = cotdServers;
            }

            var cotdServersNewJson = JsonConvert.SerializeObject(cotdHistoryNew);
            System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDHistoryData.json", cotdServersNewJson);
        }

        public static Dictionary<DateTime, List<COTDServer>> GetHistoryData()
        {
            var cotdHistoryRaw = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDHistoryData.json");
            var cotdHistory = JsonConvert.DeserializeObject<Dictionary<DateTime, List<COTDServer>>>(cotdHistoryRaw);
            return cotdHistory;
        }

        private async Task ResetDiscordServerFeeds(DiscordSocketClient discord)
        {
            var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
            var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);

            foreach (var server in cotdServers)
            {
                //Reset discord feed 
                try
                {
                    if (server.DiscordFeedChannelID != null)
                    {
                        var channel = (ITextChannel)await _discord.GetChannelAsync(Convert.ToUInt64(server.DiscordFeedChannelID));
                        if (channel == null)
                        {
                            server.DiscordFeedChannelID = null;
                        }
                        else
                        {
                            var messages = await channel.GetMessagesAsync(10).FlattenAsync();
                            if (messages != null && messages.Count() >= 2)
                            {
                                var leaderboardRaw = messages.FirstOrDefault(x => x.Embeds.First().Footer.Value.Text == "001");
                                var newPlayRaw = messages.FirstOrDefault(x => x.Embeds.First().Footer.Value.Text == "002");
                                var leaderboard = (RestUserMessage)leaderboardRaw;
                                var newPlay = (RestUserMessage)newPlayRaw;

                                await leaderboard.ModifyAsync(x => x.Embed = GetMapFeed(server).Build());
                                await newPlay.ModifyAsync(x => x.Embed = GetNewPlayFeed("", "", "", "", "").Build());

                                var fakeMsg = await channel.SendMessageAsync("yeet");
                                await fakeMsg.DeleteAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var f = ex;
                }
            }
            var cotdServersNewJson = JsonConvert.SerializeObject(cotdServers);
            System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersNewJson);
        }

        public static async void CreateGlobalServer()
        {
            var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
            var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);
            if (cotdServers.FirstOrDefault(x => x.ServerID == "0" && x.ServerName == "Global") == null)
            {
                var map = await GenerateGlobalDailyMap("", DailyMapMode.Standard);
                cotdServers.Add(new COTDServer() { ServerID = "0", ServerName = "Global", IsPublic = true, TodaysMap = map });
            }
            var cotdServersNewJson = JsonConvert.SerializeObject(cotdServers);
            System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersNewJson);
        }

        public static async void MakeServerPrivateOrPublic(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {
            //Make server private / public 
            var json = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
            if (json != "")
            {
                var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(json);
                var server = cotdServers.FirstOrDefault(x => x.ServerID == command.GuildId.ToString());
                if (server != null)
                {
                    if (server.IsPublic) server.IsPublic = false;
                    else server.IsPublic = true;
                }
                else
                {
                    var guild = discordSocketClient.GetGuild((ulong)command.GuildId);
                    server = new COTDServer() { ServerID = command.GuildId.ToString(), ServerName = guild.Name, IsPublic = true, TodaysMap = cotdServers.FirstOrDefault(x => x.ServerName == "Global" && x.ServerID == "0").TodaysMap };
                    cotdServers.Add(server);
                }

                var cotdServersJson = JsonConvert.SerializeObject(cotdServers);
                System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersJson);
                await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed($"This server is now {(server.IsPublic ? "public" : "private")}", $"{(server.IsPublic ? "Everyone can now see the leaderboard on" : "This server has been removed from")} https://beatsaberbot.com/CupOfTheDay").Build());
            }
            else
            {
                var cotdServers = new List<COTDServer>();
                var guild = discordSocketClient.GetGuild((ulong)command.GuildId);
                cotdServers.Add(new COTDServer() { ServerID = command.GuildId.ToString(), ServerName = guild.Name, IsPublic = true, TodaysMap = cotdServers.FirstOrDefault(x => x.ServerName == "Global" && x.ServerID == "0").TodaysMap });
                var cotdServersJson = JsonConvert.SerializeObject(cotdServers);
                System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersJson);
                await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("This server is now public", "Everyone can now see the leaderboard on https://beatsaberbot.com/CupOfTheDay").Build());
            }
        }

        public static async Task SetFeedChannel(SocketSlashCommand command, ITextChannel channel)
        {
            var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
            var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);

            var channelID = channel.Id;
            var server = cotdServers.FirstOrDefault(x => x.ServerID == command.GuildId.ToString());
            if (server != null)
            {
                server.DiscordFeedChannelID = channelID.ToString();

                //Send Main message + new score message

                var leaderboardEmbed = GetMapFeed(server);
                await channel.SendMessageAsync("", false, leaderboardEmbed.Build());

                await channel.SendMessageAsync("", false, GetNewPlayFeed("", "", "", "", "").Build());

                var cotdServersJsonNew = JsonConvert.SerializeObject(cotdServers);
                System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersJsonNew);
            }
            else await command.Channel.SendMessageAsync("Server is not made yet, First use the command `/cupoftheday settings make public`");
        }

        public static EmbedBuilder GetNewPlayFeed(string username, string acc, string mmr, string wins, string id)
        {
            var newPlayEmbed = new EmbedBuilder();

            var title = "No one has set a score yet.";
            if (username != "") title = $"{username} set a new score! ({acc}%)";

            newPlayEmbed.Title = title;

            var desc = "be the first one to set a score";
            if (username != "") desc = $"MMR: {mmr}\nWins: {wins}";

            newPlayEmbed.Description = desc;

            var thumbnail = "https://cdn.scoresaber.com/avatars/steam.png";
            if (id != "") thumbnail = $"https://cdn.scoresaber.com/avatars/{id}.jpg";
            newPlayEmbed.ThumbnailUrl = thumbnail;

            newPlayEmbed.Footer = new EmbedFooterBuilder() { Text = "002" };

            return newPlayEmbed;
        }

        public static EmbedBuilder GetMapFeed(COTDServer server)
        {
            var desc = "";
            for (var x = 1; x <= 8; x++)
            {
                if (server.TodaysPlayers != null && x <= server.TodaysPlayers.Count())
                {
                    var player = server.TodaysPlayers[x - 1];
                    desc += $"#{x}  **{player.Name}**         Acc: **{Math.Round((double)player.TodaysScore * 100 / server.TodaysMap.MaxScore, 2)}%** - MMR: **{player.MMR}** \n\n";
                }
                else
                {
                    desc += $"#{x} ...\n\n";
                }
            }

            var leaderboardEmbed = new EmbedBuilder();
            leaderboardEmbed.Title = "Cup Of The Day (Daily Leaderboard)";
            leaderboardEmbed.Url = "https://beatsaberbot.com/CupOfTheDay";
            leaderboardEmbed.Author = new EmbedAuthorBuilder { Name = server.ServerName };
            leaderboardEmbed.ThumbnailUrl = $"https://cdn.scoresaber.com/covers/{server.TodaysMap.SongHash}.png";
            leaderboardEmbed.Footer = new EmbedFooterBuilder() { Text = "001" };
            leaderboardEmbed.Fields.Add(new EmbedFieldBuilder() { Name = server.TodaysMap.SongName + " by " + server.TodaysMap.SongAuthorName + $"({server.TodaysMap.Difficulty.DifficultyRaw.Replace("_", " ")})", Value = "Made by: " + server.TodaysMap.LevelAuthorName });
            leaderboardEmbed.Description = desc;

            return leaderboardEmbed;
        }

        public static async void UploadServerPlaylist(IAttachment attachment, string serverID)
        {
            try
            {
                // Create scoresaber map list 
                var json = "";
                using (WebClient wc = new WebClient())
                {
                    json = wc.DownloadString(attachment.Url);
                }

                var playlist = JsonConvert.DeserializeObject<Models.PlaylistMapsModel>(json);
                if (playlist == null) return;

                var maps = new List<LeaderboardInfoModel.Leaderboard>();
                var scoresaberClient = new ScoreSaberClient();
                foreach (var song in playlist.Songs)
                {
                    var map = await BeatSaverApi.GetMapByKey(song.Key);
                    if (map == null) map = await BeatSaverApi.GetMapByHash(song.Hash);
                    if (map == null) return;
                    var diff = map.Versions.First().Diffs.Last().Difficulty;
                    var diffnr = 9;
                    switch (diff)
                    {
                        case "Easy":
                            diffnr = 1;
                            break;
                        case "Normal":
                            diffnr = 3;
                            break;
                        case "Hard":
                            diffnr = 5;
                            break;
                        case "Expert":
                            diffnr = 7;
                            break;
                        case "ExpertPlus":
                            diffnr = 9;
                            break;
                        default:
                            diffnr = 9;
                            break;
                    }
                    var leaderboard = await scoresaberClient.Api.Leaderboards.GetLeaderboardInfoByHashcode(song.Hash, (Leaderboards.Difficulty)diffnr);
                    maps.Add(leaderboard);
                }

                if (maps.Count <= 0) return;

                //Add playlist to server
                var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
                var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);

                if (cotdServers.FirstOrDefault(x => x.ServerID == serverID) == null) return;
                else
                {
                    var server = cotdServers.FirstOrDefault(x => x.ServerID == serverID);
                    server.Playlist = maps;
                    server.TodaysMap = maps.First();
                    foreach (var player in server.TodaysPlayers)
                    {
                        player.TodaysScore = 0;
                    }

                    var cotdServersNewJson = JsonConvert.SerializeObject(cotdServers);
                    System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersNewJson);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task ResetDailyMapFromAllServer()
        {
            var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
            if (cotdServersJson != "")
            {
                var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);
                foreach (var server in cotdServers)
                {
                    if (server.ServerName == "Global" && server.ServerID == "0")
                    {
                        var standardFocus = await GenerateGlobalDailyMap(server.TodaysMapFocus.SongHash, DailyMapMode.Focus);
                        var standardMap = await GenerateGlobalDailyMap(server.TodaysMap.SongHash, DailyMapMode.Standard); //TODO replace todaysmap with specific
                        var standardHardcore = await GenerateGlobalDailyMap(server.TodaysMapHardcore.SongHash, DailyMapMode.Hardcore);

                        server.TodaysMapFocus = standardFocus;
                        server.TodaysMap = standardMap;
                        server.TodaysMapHardcore = standardHardcore;

                        continue;
                    }

                    if (server.Playlist != null)
                    {
                        var playlist = server.Playlist.Where(x => x != null).ToList();
                        var index = 0;
                        if (server.TodaysMap != null) index = playlist.IndexOf(playlist.FirstOrDefault(x => x.SongHash == server.TodaysMap.SongHash));
                        if (index + 1 >= playlist.Count) index = 0;
                        else index += 1;
                        server.TodaysMap = playlist[index];
                    }
                    else
                    {
                        server.TodaysMap = cotdServers.FirstOrDefault(x => x.ServerID == "0").TodaysMap;
                    }
                }
                var newJson = JsonConvert.SerializeObject(cotdServers);
                File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/COTDServerPlayer.json", newJson);
            }
        }

        public static async Task ServerPlayerJoin(Player player, string discordID, string serverID)
        {
            var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
            var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);
            var server = cotdServers.FirstOrDefault(x => x.ServerID == serverID);
            if (server == null) return;
            else
            {
                if (server.AllTimePlayers == null) server.AllTimePlayers = new List<COTDPlayer>();
                if (server.TodaysPlayers == null) server.TodaysPlayers = new List<COTDPlayer>();
                if (server.TodaysPlayersFocus == null) server.TodaysPlayersFocus = new List<COTDPlayer>();
                if (server.TodaysPlayersHardcore == null) server.TodaysPlayersHardcore = new List<COTDPlayer>();

                if (server.AllTimePlayers.FirstOrDefault(x => x.ScoreSaberID == player.ScoreSaberID && x.DiscordID == discordID) == null)//First time join
                {
                    var scoresaberplayer = await new ScoreSaberClient().Api.Players.GetPlayer(Convert.ToInt64(player.ScoreSaberID));
                    var newPlayer = new COTDPlayer { DiscordID = discordID, ScoreSaberID = player.ScoreSaberID, MMR = 600, Name = scoresaberplayer.Name, TodaysScore = 0, TotalWins = 0 };
                    server.AllTimePlayers.Add(newPlayer);
                    server.TodaysPlayers.Add(newPlayer);

                    if (server.ServerID == "0")
                    {
                        server.TodaysPlayersFocus.Add(newPlayer);
                        server.TodaysPlayersHardcore.Add(newPlayer);
                    }
                }
                else
                {
                    if (server.TodaysPlayers.FirstOrDefault(x => x.ScoreSaberID == player.ScoreSaberID && x.DiscordID == discordID) == null) //already existing
                    {
                        var allTimePlayer = server.AllTimePlayers.FirstOrDefault(x => x.ScoreSaberID == player.ScoreSaberID && x.DiscordID == discordID);
                        server.TodaysPlayers.Add(allTimePlayer);

                        if (server.ServerID == "0")
                        {
                            server.TodaysPlayersFocus.Add(allTimePlayer);
                            server.TodaysPlayersHardcore.Add(allTimePlayer);
                        }
                    }
                }

                var cotdServersNewJson = JsonConvert.SerializeObject(cotdServers);
                System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersNewJson);
            }
        }

        public class Player
        {
            public string Name { get; set; }
            public string ScoreSaberID { get; set; }

            public int MMRGlobal { get; set; }
            public long TodaysScore { get; set; }

            public int TotalWinsGlobal { get; set; }

            public List<ServerStats> Servers { get; set; }

            public class ServerStats
            {
                public string id { get; set; }
                public string Name { get; set; }

                public int MMR { get; set; }

                public int TotalWins { get; set; }
            }
        }

        public class COTDServer
        {
            public string ServerName;
            public string ServerID;
            public bool IsPublic;
            public string DiscordFeedChannelID;
            public LeaderboardInfoModel.Leaderboard TodaysMap;
            public LeaderboardInfoModel.Leaderboard TodaysMapFocus;
            public LeaderboardInfoModel.Leaderboard TodaysMapHardcore;
            public List<LeaderboardInfoModel.Leaderboard> Playlist;
            public List<COTDPlayer> TodaysPlayers;
            public List<COTDPlayer> TodaysPlayersFocus;
            public List<COTDPlayer> TodaysPlayersHardcore;
            public List<COTDPlayer> AllTimePlayers;
        }

        public class COTDPlayer
        {
            public string Name { get; set; }
            public string ScoreSaberID { get; set; }
            public string DiscordID { get; set; }
            public double MMR { get; set; }
            public double TodaysMMRChange { get; set; }
            public int TotalWins { get; set; }
            public long TodaysScore { get; set; }
        }
    }
}
