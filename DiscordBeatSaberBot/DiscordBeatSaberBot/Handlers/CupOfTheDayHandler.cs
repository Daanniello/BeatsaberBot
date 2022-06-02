using DiscordBeatSaberBot.Api.BeatSaverApi;
using DiscordBeatSaberBot.Api.BeatSaverApi.Models.New;
using Newtonsoft.Json;
using ScoreSaberLib;
using ScoreSaberLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Handlers
{
    public class CupOfTheDayHandler
    {
        private LeaderboardInfoModel.Leaderboard currentLeaderboard = null;
        private ScoreSaberClient scoresaberClient;

        public CupOfTheDayHandler()
        {
            scoresaberClient = new ScoreSaberClient();
            scoresaberClient.Api.ScoreFeed.Connect();
            scoresaberClient.Api.ScoreFeed.OnPlayReceived += Feed_OnPlayReceived;
            scoresaberClient.Api.ScoreFeed.OnDisconnect += Feed_OnDisconnect;

            var json = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/CupOfTheDayMapInfo.json");
            var map = JsonConvert.DeserializeObject<ScoreSaberLib.Models.LeaderboardInfoModel.Leaderboard>(json);
            if (map != null) currentLeaderboard = map;
        }

        private void Feed_OnDisconnect(object sender, EventArgs e)
        {
            scoresaberClient.Api.ScoreFeed.WebSocket.Close();
            scoresaberClient.Api.ScoreFeed.Connect();
            scoresaberClient.Api.ScoreFeed.OnPlayReceived += Feed_OnPlayReceived;
            scoresaberClient.Api.ScoreFeed.OnDisconnect += Feed_OnDisconnect;
        }

        private void Feed_OnPlayReceived(object sender, ScoreSaberLib.Models.ScoreFeedModel e)
        {
            if (currentLeaderboard != null)
            {
                if (e.CommandData.Leaderboard.SongHash == currentLeaderboard.SongHash && e.CommandData.Leaderboard.Difficulty.DifficultyRaw == currentLeaderboard.Difficulty.DifficultyRaw)
                {
                    //Replace score value 
                    var json = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/TodaysCupOfTheDayPlayers.json");
                    List<Player> players = null;
                    if (json != "" && json != null) players = JsonConvert.DeserializeObject<List<Player>>(json);
                    if (players != null && players.Count > 0)
                    {
                        var player = players.FirstOrDefault(x => x.ScoreSaberID == e.CommandData.Score.LeaderboardPlayerInfo.Id);
                        if (player != null)
                        {
                            player.TodaysScore = e.CommandData.Score.BaseScore;
                            var newJson = JsonConvert.SerializeObject(players);
                            System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/TodaysCupOfTheDayPlayers.json", newJson);
                        }
                    }
                }
            }
        }

        public async Task ResetDailyMap()
        {

            //Pick a new map and replace it            
            var leaderboards = await new ScoreSaberClient().Api.Leaderboards.GetLeaderboardsByFilter(category: Leaderboards.Category.trending);
            var withStar = await new ScoreSaberClient().Api.Leaderboards.GetLeaderboardsByFilter(category: Leaderboards.Category.trending, minStar: 5);
            leaderboards.Leaderboards.AddRange(withStar.Leaderboards);
            var random = new Random();
            leaderboards.Leaderboards = leaderboards.Leaderboards.OrderBy(x => random.Next(0, leaderboards.Leaderboards.Count)).ToList();
            LeaderboardInfoModel.Leaderboard board;
            BeatSaverMapModelNew result;
            var count = 0;
            do
            {
                board = leaderboards.Leaderboards[count];
                result = await BeatSaverApi.GetMapByHash(board.SongHash);
                var diff = result.Versions.First().Diffs.FirstOrDefault(x => x.Difficulty == "ExpertPlus");
                if (diff == null) continue;
                board.MaxScore = diff.MaxScore;
                count++;
            } while (result.Stats.Upvotes < 20 || (result.Stats.Upvotes * 100 / result.Stats.Upvotes + result.Stats.Downvotes) < 80 || result.Metadata.Duration < 30 || currentLeaderboard.SongHash == board.SongHash && count < leaderboards.Leaderboards.Count);       
            
            var json = JsonConvert.SerializeObject(board);
            File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/CupOfTheDayMapInfo.json", json);

            currentLeaderboard = board;

            var jsonGlobal = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/AllCupOfTheDayPlayers.json");
            List<Player> playersGlobal = null;
            if (json != "" && json != null) playersGlobal = JsonConvert.DeserializeObject<List<Player>>(jsonGlobal);

            //Reward the top player with the win
            var jsonDaily = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/TodaysCupOfTheDayPlayers.json");
            List<Player> playersDaily = null;
            if (json != "" && json != null) playersDaily = JsonConvert.DeserializeObject<List<Player>>(jsonDaily);
            if (playersDaily != null && playersDaily.Count > 0)
            {
                //Give top player a win
                if (playersGlobal != null && playersGlobal.Count > 0) playersGlobal.FirstOrDefault(x => x.ScoreSaberID == playersDaily.OrderByDescending(x => x.TodaysScore).First().ScoreSaberID).TotalWins += 1;

                //Give Everyone their MMR
                foreach(var player in playersDaily)
                {
                    //TODO: give MMR
                    if (player.TodaysScore == 0) continue;
                    var playerCurrentMMR = playersGlobal.FirstOrDefault(x => x.ScoreSaberID == player.ScoreSaberID).MMR;
                    var avgMMRFromAll = playersGlobal.Average(x => x.MMR);
                    var avgMMRFromBelowPlayer = playersDaily.OrderByDescending(x => x.TodaysScore).Where(x => x.TodaysScore <= player.TodaysScore).Average(x => x.MMR);
                    var avgMMRFromAbovePlayer = playersDaily.OrderByDescending(x => x.TodaysScore).Where(x => x.TodaysScore >= player.TodaysScore).Average(x => x.MMR);
                    double mmrWin = 20;
                    var mmrDiff = (200 * avgMMRFromAbovePlayer / playerCurrentMMR) - 100;
                    mmrWin += mmrDiff;
                    playersGlobal.FirstOrDefault(x => x.ScoreSaberID == player.ScoreSaberID).MMR += (int) Math.Round(mmrWin);
                }
                var newJson = JsonConvert.SerializeObject(playersGlobal);
                File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/AllCupOfTheDayPlayers.json", newJson);
            }


            //Remove all players from the daily leaderboard
            File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/TodaysCupOfTheDayPlayers.json", "");
        }

        public class Player
        {
            public string Name { get; set; }
            public string ScoreSaberID { get; set; }

            public int MMR { get; set; }

            public long TodaysScore { get; set; }

            public int TotalWins { get; set; }
        }
    }
}
