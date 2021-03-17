using DiscordBeatSaberBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Api.BeatSaverApi
{
    public class BeatSaverApi
    {
        private string _songId;
        public int apiCallCount;
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

                var productValue = new ProductInfoHeaderValue("ScraperBot", "1.0");
                var commentValue = new ProductInfoHeaderValue("(+http://www.example.com/ScraperBot.html)");

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
                client.DefaultRequestHeaders.UserAgent.Add(productValue);
                client.DefaultRequestHeaders.UserAgent.Add(commentValue);

                var httpResponseMessage2 = await client.SendAsync(request);
                apiCallCount++;
                if (httpResponseMessage2.StatusCode != HttpStatusCode.OK) return null;

                var recentSongsJsonDataBeatSaver = await httpResponseMessage2.Content.ReadAsStringAsync();
                var recentSongsInfoBeatSaver = JsonConvert.DeserializeObject<BeatSaverMapInfoModel>(recentSongsJsonDataBeatSaver);
                return recentSongsInfoBeatSaver;
            }            
        }

        public static async Task<BeatSaverMapInfoModel> GetMapByKey(string key)
        {
            var beatsaverUrl = $"https://beatsaver.com/api/maps/detail/{key}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(beatsaverUrl),
                    Method = HttpMethod.Get,
                };

                var productValue = new ProductInfoHeaderValue("ScraperBot", "1.0");
                var commentValue = new ProductInfoHeaderValue("(+http://www.example.com/ScraperBot.html)");

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
                client.DefaultRequestHeaders.UserAgent.Add(productValue);
                client.DefaultRequestHeaders.UserAgent.Add(commentValue);

                var httpResponseMessage2 = await client.SendAsync(request);

                if (httpResponseMessage2.StatusCode != HttpStatusCode.OK) return null;

                var mapJsonDataBeatSaver = await httpResponseMessage2.Content.ReadAsStringAsync();
                var mapInfoBeatSaver = JsonConvert.DeserializeObject<BeatSaverMapInfoModel>(mapJsonDataBeatSaver);
                return mapInfoBeatSaver;
            }
        }
    }
}
