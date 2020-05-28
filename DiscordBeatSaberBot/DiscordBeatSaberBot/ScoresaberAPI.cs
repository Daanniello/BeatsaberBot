using Discord.WebSocket;
using DiscordBeatSaberBot.Models.ScoreberAPI;
using Newtonsoft.Json;
using System;
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

        public ScoresaberAPI (string playerId, SocketMessage message = null)
        {
            _playerId = playerId;
            _message = message;
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
