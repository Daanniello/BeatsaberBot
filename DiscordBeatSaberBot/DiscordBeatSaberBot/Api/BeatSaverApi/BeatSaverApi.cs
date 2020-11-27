using DiscordBeatSaberBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Api.BeatSaverApi
{
    class BeatSaverApi
    {
        private string _songId;
        public BeatSaverApi(string songId)
        {
            _songId = songId;
        }

        public async Task<BeatSaverMapInfoModel> GetRecentSongData()
        {
            var beatsaverUrl = $"https://beatsaver.com/api/maps/by-hash/{_songId}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(beatsaverUrl),
                    Method = HttpMethod.Get,
                };

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
                client.DefaultRequestHeaders.Add("User-Agent", "C# App");
                client.DefaultRequestHeaders.Connection.Add("keep-alive");

                var httpResponseMessage2 = await client.SendAsync(request);

                if (httpResponseMessage2.StatusCode != HttpStatusCode.OK) return null;

                var recentSongsJsonDataBeatSaver = await httpResponseMessage2.Content.ReadAsStringAsync();
                var recentSongsInfoBeatSaver = JsonConvert.DeserializeObject<BeatSaverMapInfoModel>(recentSongsJsonDataBeatSaver);
                return recentSongsInfoBeatSaver;
            }
        }
    }
}
