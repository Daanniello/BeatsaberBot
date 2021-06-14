using Discord.WebSocket;
using DiscordBeatSaberBot.Api.ScoreberAPI.Models;
using DiscordBeatSaberBot.Api.ScoreberAPI.Models.ScoresaberRankedRequestModel;
using DiscordBeatSaberBot.Api.ScoreberAPI.Models.ScoresaberRankedTopRequestsModel;
using DiscordBeatSaberBot.Models.ScoreberAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    class ScoresaberAPI
    {

        private string _baseUrl = "https://new.scoresaber.com/api/player/";
        private string _playerId;
        private SocketMessage _message;

        public ScoresaberAPI(string playerId, SocketMessage message = null)
        {
            _playerId = playerId;
            _message = message;
        }

        public static async Task<ScoresaberRankedTopRequestsModel> GetTopRankedRequests()
        {
            try
            {
                Console.WriteLine("Scoresaber request for top ranked requests");
                var url = "https://new.scoresaber.com/api/ranking/requests/top";
                var rankedRequests = new ScoresaberRankedTopRequestsModel();

                using (var client = new HttpClient())
                {
                    var httpResponseMessage = await client.GetAsync(url);

                    if (httpResponseMessage.StatusCode != HttpStatusCode.OK) return null;

                    var rankedRequestsJsonData = await httpResponseMessage.Content.ReadAsStringAsync();
                    rankedRequests = JsonConvert.DeserializeObject<ScoresaberRankedTopRequestsModel>(rankedRequestsJsonData);
                }
                return rankedRequests;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public static async Task<ScoreSaberSearchByNameModel> GetPlayerByName(string search)
        {
            using (var client = new HttpClient())
            {
                var httpResponseMessage = await client.GetAsync($"https://new.scoresaber.com/api/players/by-name/{search}");

                if (httpResponseMessage.StatusCode != HttpStatusCode.OK) return null;

                var playerInfoJsonData = await httpResponseMessage.Content.ReadAsStringAsync();
                if (playerInfoJsonData == null) return null;
                var playerInfo = JsonConvert.DeserializeObject<ScoreSaberSearchByNameModel>(playerInfoJsonData);
                return playerInfo;
            }          
        }

        public static async Task<ScoreSaberSearchByNameModel> GetTop50Global()
        {
            try
            {
                Console.WriteLine("Scoresaber request getting top 50 players");
                var url = "https://new.scoresaber.com/api/players/1";

                using (var client = new HttpClient())
                {
                    var httpResponseMessage = await client.GetAsync(url);

                    if (httpResponseMessage.StatusCode != HttpStatusCode.OK) return null;

                    var rankedRequestsJsonData = await httpResponseMessage.Content.ReadAsStringAsync();
                    var top100Players = JsonConvert.DeserializeObject<ScoreSaberSearchByNameModel>(rankedRequestsJsonData);
                    return top100Players;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public static async Task<ScoresaberRankedRequestModel> GetRankedRequests(long requestId)
        {
            try
            {
                Console.WriteLine("Scoresaber request for ranked requests");
                var url = $"https://new.scoresaber.com/api/ranking/request/{requestId}";
                var rankedRequests = new ScoresaberRankedRequestModel();

                using (var client = new HttpClient())
                {
                    var httpResponseMessage = await client.GetAsync(url);

                    if (httpResponseMessage.StatusCode != HttpStatusCode.OK) return null;

                    var rankedRequestsJsonData = await httpResponseMessage.Content.ReadAsStringAsync();
                    rankedRequests = JsonConvert.DeserializeObject<ScoresaberRankedRequestModel>(rankedRequestsJsonData);
                }
                return rankedRequests;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<ScoresaberPlayerFullModel> GetPlayerFull()
        {
            try
            {
                Console.WriteLine("Scoresaber request for player full data");
                var scoresaberPlayerFullModel = new ScoresaberPlayerFullModel();
                var apiType = "";
                var endpoint = "/full";
                var result = await GetData(apiType, endpoint);

                scoresaberPlayerFullModel = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(result);

                return scoresaberPlayerFullModel;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<ScoresaberSongsModel> GetScoresRecent()
        {
            try
            {
                Console.WriteLine("Scoresaber request for recent scores");
                var RecentScores = new ScoresaberSongsModel();
                var apiType = "/scores";
                var endpoint = "/recent";
                var result = await GetData(apiType, endpoint);
                RecentScores = JsonConvert.DeserializeObject<ScoresaberSongsModel>(result);
                return RecentScores;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<Score> GetScoresRecent(int recentSongNr = 1)
        {
            try
            {
                Console.WriteLine("Scoresaber request for recent scores");
                var RecentScores = new ScoresaberSongsModel();
                var apiType = "/scores";
                var page = Math.DivRem(recentSongNr, 8, out int nr);
                if (nr == 0)
                {
                    page -= 1;
                    nr += 8;
                }
                var endpoint = $"/recent/{page + 1}";

                var result = await GetData(apiType, endpoint);

                RecentScores = JsonConvert.DeserializeObject<ScoresaberSongsModel>(result);
                return RecentScores.Scores[nr - 1];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<ScoresaberSongsModel> GetTopScores()
        {
            try
            {
                Console.WriteLine("Scoresaber request for top scores");
                var topScores = new ScoresaberSongsModel();
                var apiType = "/scores";
                var endpoint = "/top";
                var result = await GetData(apiType, endpoint);

                topScores = JsonConvert.DeserializeObject<ScoresaberSongsModel>(result);
                return topScores;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<Score> GetTopScores(int topSongNr = 1)
        {
            try
            {
                Console.WriteLine("Scoresaber request for top scores");
                var topScores = new ScoresaberSongsModel();
                var apiType = "/scores";
                var page = Math.DivRem(topSongNr, 8, out int nr);
                if (nr == 0)
                {
                    page -= 1;
                    nr += 8;
                }
                var endpoint = $"/top/{page + 1}";
                var result = await GetData(apiType, endpoint);

                topScores = JsonConvert.DeserializeObject<ScoresaberSongsModel>(result);
                return topScores.Scores[nr - 1];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async static Task<List<ScoresaberLiveFeedModel>> GetLiveFeed()
        {
            using (var client = new HttpClient())
            {
                var url = "https://scoresaber.com/scripts/feed.php";
                var httpResponseMessage = await client.GetAsync(url);

                if (httpResponseMessage.StatusCode != HttpStatusCode.OK) return null;

                var liveFeedJsonData = await httpResponseMessage.Content.ReadAsStringAsync();
                var liveFeedInfo = JsonConvert.DeserializeObject<List<ScoresaberLiveFeedModel>>(liveFeedJsonData);
                return liveFeedInfo;
            }
        }

        public async static Task<QualifiedMapsModel> GetQualifiedMaps()
        {
            using (var client = new HttpClient())
            {
                var url = "http://scoresaber.com/api.php?function=get-leaderboards&cat=5&limit=100&page=1&unique=1";
                var httpResponseMessage = await client.GetAsync(url);

                if (httpResponseMessage.StatusCode != HttpStatusCode.OK) return null;

                var liveFeedJsonData = await httpResponseMessage.Content.ReadAsStringAsync();
                var liveFeedInfo = JsonConvert.DeserializeObject<QualifiedMapsModel>(liveFeedJsonData);
                return liveFeedInfo;
            }
        }

        private async Task<string> GetData(string type, string endpoint)
        {
            string result = null;
            using (var Client = new HttpClient())
            {
                var url = new Uri(_baseUrl + _playerId + type + endpoint);
                try
                {
                    var httpResponseMessage = await Client.GetAsync(url);
                    if (httpResponseMessage.StatusCode != HttpStatusCode.OK && _message != null)
                    {
                        await _message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Scoresaber Error", $"Status code: {httpResponseMessage.StatusCode}").Build());
                        return null;
                    }

                    httpResponseMessage.EnsureSuccessStatusCode();
                    result = await httpResponseMessage.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    return null;
                }

            }
            return result;
        }

    }
}
