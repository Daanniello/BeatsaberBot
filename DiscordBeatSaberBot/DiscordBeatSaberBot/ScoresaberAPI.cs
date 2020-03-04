using DiscordBeatSaberBot.Models.ScoreberAPI;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    class ScoresaberAPI
    {

        private string _baseUrl = "https://new.scoresaber.com/api/player/";
        private string _playerId;

        public ScoresaberAPI (string playerId)
        {
            _playerId = playerId;
        }

        public async Task<ScoresaberPlayerFullModel> GetPlayerFull()
        {
            var scoresaberPlayerFullModel = new ScoresaberPlayerFullModel();
            var apiType = "";
            var endpoint = "/full";
            var result = await GetData(apiType, endpoint);
            
            scoresaberPlayerFullModel = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(result);

            return scoresaberPlayerFullModel;
        }

        public async Task<ScoresaberScoresRecentModel> GetScoresRecent()
        {
            var RecentScores = new ScoresaberScoresRecentModel();
            var apiType = "/scores";
            var endpoint = "/recent";
            var result = await GetData(apiType, endpoint);
            try
            {
                RecentScores = JsonConvert.DeserializeObject<ScoresaberScoresRecentModel>(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return RecentScores;
        }

        private async Task<string> GetData(string type, string endpoint)
        {
            string result = null;
            try
            {
                using (var Client = new HttpClient())
                {
                    var url = new Uri(_baseUrl + _playerId + type + endpoint);
                    var httpResponseMessage = await Client.GetAsync(url);
                    httpResponseMessage.EnsureSuccessStatusCode();
                    result = await httpResponseMessage.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
        
    }
}
