using Discord.WebSocket;
using DiscordBeatSaberBot.Api.ScoreberAPI.Models;
using DiscordBeatSaberBot.Api.ScoreberAPI.Models.ScoreSaberRankedRequestModel;
using DiscordBeatSaberBot.Api.ScoreberAPI.Models.ScoreSaberRankedTopRequestsModel;
using DiscordBeatSaberBot.Models.ScoreberAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    class ScoreSaberAPI
    {

        private string _baseUrl = "https://new.ScoreSaber.com/api/player/";
        private string _playerId;
        private SocketMessage _message;

        public ScoreSaberAPI(string playerId, SocketMessage message = null)
        {
            _playerId = playerId;
            _message = message;
        }

        public static async Task<ScoreSaberRankedTopRequestsModel> GetTopRankedRequests()
        {
            Console.WriteLine("ScoreSaber request for top ranked requests");
            var url = "https://new.ScoreSaber.com/api/ranking/requests/top";
            var rankedRequests = new ScoreSaberRankedTopRequestsModel();

            using (var client = new HttpClient())
            {
                var httpResponseMessage = await client.GetAsync(url);

                if (httpResponseMessage.StatusCode != HttpStatusCode.OK) return null;

                var rankedRequestsJsonData = await httpResponseMessage.Content.ReadAsStringAsync();
                rankedRequests = JsonConvert.DeserializeObject<ScoreSaberRankedTopRequestsModel>(rankedRequestsJsonData);
            }
            return rankedRequests;
        }

        public static async Task<ScoreSaberRankedRequestModel> GetRankedRequests(long requestId)
        {
            Console.WriteLine("ScoreSaber request for ranked requests");
            var url = $"https://new.ScoreSaber.com/api/ranking/request/{requestId}";
            var rankedRequests = new ScoreSaberRankedRequestModel();

            using (var client = new HttpClient())
            {
                var httpResponseMessage = await client.GetAsync(url);

                if (httpResponseMessage.StatusCode != HttpStatusCode.OK) return null;

                var rankedRequestsJsonData = await httpResponseMessage.Content.ReadAsStringAsync();
                rankedRequests = JsonConvert.DeserializeObject<ScoreSaberRankedRequestModel>(rankedRequestsJsonData);
            }
            return rankedRequests;
        }

        public async Task<ScoreSaberPlayerFullModel> GetPlayerFull()
        {
            Console.WriteLine("ScoreSaber request for player full data");
            var ScoreSaberPlayerFullModel = new ScoreSaberPlayerFullModel();
            var apiType = "";
            var endpoint = "/full";
            var result = await GetData(apiType, endpoint);

            ScoreSaberPlayerFullModel = JsonConvert.DeserializeObject<ScoreSaberPlayerFullModel>(result);

            return ScoreSaberPlayerFullModel;
        }

        public async Task<ScoreSaberSongsModel> GetScoresRecent()
        {
            Console.WriteLine("ScoreSaber request for recent scores");
            var RecentScores = new ScoreSaberSongsModel();
            var apiType = "/scores";
            var endpoint = "/recent";
            var result = await GetData(apiType, endpoint);
            try
            {
                RecentScores = JsonConvert.DeserializeObject<ScoreSaberSongsModel>(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return RecentScores;
        }

        public async Task<ScoreSaberSongsModel> GetTopScores()
        {
            Console.WriteLine("ScoreSaber request for top scores");
            var topScores = new ScoreSaberSongsModel();
            var apiType = "/scores";
            var endpoint = "/top";
            var result = await GetData(apiType, endpoint);
            try
            {
                topScores = JsonConvert.DeserializeObject<ScoreSaberSongsModel>(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return topScores;
        }

        public async static Task<List<ScoreSaberLiveFeedModel>> GetLiveFeed()
        {
            using (var client = new HttpClient())
            {
                var url = "https://scoresaber.com/scripts/feed.php";
                var httpResponseMessage = await client.GetAsync(url);

                if (httpResponseMessage.StatusCode != HttpStatusCode.OK) return null;

                var liveFeedJsonData = await httpResponseMessage.Content.ReadAsStringAsync();
                var liveFeedInfo = JsonConvert.DeserializeObject<List<ScoreSaberLiveFeedModel>>(liveFeedJsonData);
                return liveFeedInfo;
            }            
        }

        private async Task<string> GetData(string type, string endpoint)
        {
            string result = null;
            using (var Client = new HttpClient())
            {
                var url = new Uri(_baseUrl + _playerId + type + endpoint);
                var httpResponseMessage = await Client.GetAsync(url);
                if (httpResponseMessage.StatusCode != HttpStatusCode.OK && _message != null)
                {
                    await _message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("ScoreSaber Error", $"Status code: {httpResponseMessage.StatusCode}").Build());
                    return null;
                }

                httpResponseMessage.EnsureSuccessStatusCode();
                result = await httpResponseMessage.Content.ReadAsStringAsync();
            }
            return result;
        }

    }
}
