using Discord.WebSocket;
using DiscordBeatSaberBot.Api.ScoreberAPI.Models;
using DiscordBeatSaberBot.Api.ScoreberAPI.Models.ScoresaberRankedRequestModel;
using DiscordBeatSaberBot.Api.ScoreberAPI.Models.ScoresaberRankedTopRequestsModel;
using DiscordBeatSaberBot.Models.ScoreberAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public static async Task<ScoresaberRankedRequestModel> GetRankedRequests(long requestId)
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

        public async Task<ScoresaberPlayerFullModel> GetPlayerFull()
        {
            Console.WriteLine("Scoresaber request for player full data");
            var scoresaberPlayerFullModel = new ScoresaberPlayerFullModel();
            var apiType = "";
            var endpoint = "/full";
            var result = await GetData(apiType, endpoint);

            scoresaberPlayerFullModel = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(result);

            return scoresaberPlayerFullModel;
        }

        public async Task<ScoresaberSongsModel> GetScoresRecent()
        {
            Console.WriteLine("Scoresaber request for recent scores");
            var RecentScores = new ScoresaberSongsModel();
            var apiType = "/scores";
            var endpoint = "/recent";
            var result = await GetData(apiType, endpoint);
            try
            {
                RecentScores = JsonConvert.DeserializeObject<ScoresaberSongsModel>(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return RecentScores;
        }

        public async Task<ScoresaberSongsModel> GetTopScores()
        {
            Console.WriteLine("Scoresaber request for top scores");
            var topScores = new ScoresaberSongsModel();
            var apiType = "/scores";
            var endpoint = "/top";
            var result = await GetData(apiType, endpoint);
            try
            {
                topScores = JsonConvert.DeserializeObject<ScoresaberSongsModel>(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return topScores;
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

        private async Task<string> GetData(string type, string endpoint)
        {
            string result = null;
            using (var Client = new HttpClient())
            {
                var url = new Uri(_baseUrl + _playerId + type + endpoint);
                var httpResponseMessage = await Client.GetAsync(url);
                if (httpResponseMessage.StatusCode != HttpStatusCode.OK && _message != null)
                {
                    await _message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Scoresaber Error", $"Status code: {httpResponseMessage.StatusCode}").Build());
                    return null;
                }

                httpResponseMessage.EnsureSuccessStatusCode();
                result = await httpResponseMessage.Content.ReadAsStringAsync();
            }
            return result;
        }

    }
}
