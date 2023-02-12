using DiscordBeatSaberBot.Api.BeatSaverApi.Models;
using DiscordBeatSaberBot.Api.BeatSaverApi.Models.NewMaps;
using DiscordBeatSaberBot.Api.BeatSaverApi.Models.v2;
using DiscordBeatSaberBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Api.BeatSaverApi
{
    public class BeatSaverApi
    {
        private string _songId;
        public int apiCallCount;
        private static string _baseURL = "https://api.beatsaver.com/";

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
                try
                {
                    var recentSongsInfoBeatSaver = JsonConvert.DeserializeObject<BeatSaverMapInfoModel>(recentSongsJsonDataBeatSaver);
                    return recentSongsInfoBeatSaver;
                }
                catch
                {
                    return null;
                }
            }
        }

        public static async Task<Api.BeatSaverApi.Models.New.BeatSaverMapModelNew> GetMapByKey(string key)
        {
            var mapJsonDataBeatSaver = await Get($"maps/id/{key}");
            if (mapJsonDataBeatSaver == null) return null;
            var mapInfoBeatSaver = JsonConvert.DeserializeObject<Api.BeatSaverApi.Models.New.BeatSaverMapModelNew>(mapJsonDataBeatSaver);
            return mapInfoBeatSaver;
        }

        public static async Task<Api.BeatSaverApi.Models.New.BeatSaverMapModelNew> GetMapByHash(string hash)
        {
            var mapJsonDataBeatSaver = await Get($"maps/hash/{hash}");
            if (mapJsonDataBeatSaver == null) return null;
            var mapInfoBeatSaver = JsonConvert.DeserializeObject<Api.BeatSaverApi.Models.New.BeatSaverMapModelNew>(mapJsonDataBeatSaver);
            return mapInfoBeatSaver;
        }

        public async Task<dynamic> GetMapsByHash(List<string> hashes)
        {
            var hashesString = "";
            foreach (var hash in hashes)
            {
                hashesString += hash + ",";
            }

            var mapJsonDataBeatSaver = await Get($"maps/hash/{hashesString}");

            //foreach (var hash in hashes)
            //{
            //    mapJsonDataBeatSaver = mapJsonDataBeatSaver.Replace($"\"{hash.ToLower()}\":","\"map\":");
            //}

            if (mapJsonDataBeatSaver == null) return null;
            var mapInfoBeatSaver = JsonConvert.DeserializeObject<dynamic>(mapJsonDataBeatSaver);
            return mapInfoBeatSaver;
        }

        public async Task<List<Maps>> GetMapsByHashes(List<string> hasheswithdiffs)
        {
            var hashesString = "";
            foreach (var hash in hasheswithdiffs)
            {
                hashesString += hash.Split("_")[0] + ",";
            }

            var mapJsonDataBeatSaver = await Get($"maps/hash/{hashesString}");
            JObject tokens = JsonConvert.DeserializeObject<dynamic>(mapJsonDataBeatSaver);
            var childs = tokens.Children();

            if (mapJsonDataBeatSaver == null) return null;

            var maps = new List<Maps>();
            var count = 0;
            foreach(var map in childs)
            {
                var json = map.Children().First().ToString();
                var obj = JsonConvert.DeserializeObject<Maps>(json);
                if (obj != null)
                {
                    if(obj.Versions.First() != null)
                    {                                          
                        var hashdiff = hasheswithdiffs.FirstOrDefault(x => x.Split("_")[0].ToLower() == obj.Versions.First().Hash.ToLower());
                        if(hashdiff != null)
                        {
                            obj.DifficultyRaw = hashdiff.Split("__")[1].Split("_")[0];
                            obj.HashKey = obj.Versions.First().Hash;
                            if (obj.Versions.First().Diffs != null)
                            {
                                if (obj.Versions.First().Diffs.FirstOrDefault(x => obj.DifficultyRaw == x.Difficulty) != null) maps.Add(obj);
                            }
                        }
                    }
                }
                count++;
            }

            return maps;
        }

        public static async Task<Api.BeatSaverApi.Models.New2.MapsBySearchModel> GetMapsBySearch(string searchText)
        {
            var data = await Get($"search/text/0?q={searchText}&?automapper=1");
            if (data == null) return null;
            try
            {
                var recentSongsInfoBeatSaver = JsonConvert.DeserializeObject<Api.BeatSaverApi.Models.New2.MapsBySearchModel>(data);
                return recentSongsInfoBeatSaver;
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public static async Task<BeatSaverMapSearchModelv2> GetMapForCupOfTheDayFocus(int page)
        {
            var data = await Get($"search/text/{page}?sortOrder=Relevance&curated=true&verified=true&tags=accuracy");
            if (data == null) return null;
            try
            {
                var cotdMaps = JsonConvert.DeserializeObject<BeatSaverMapSearchModelv2>(data);
                return cotdMaps;
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public static async Task<BeatSaverMapSearchModelv2> GetMapForCupOfTheDayStandard(int page)
        {
            var data = await Get($"search/text/{page}?sortOrder=Relevance&curated=true&verified=true&maxNps=9&minNps=5.5&tags=alternative|ambient|classical-orchestral|comedy-meme|drum-and-bass|dubstep|hardcore|funk-disco|folk-acoustic|electronic|hip-hop-rap|holiday|house|indie|instrumental|jazz|pop|nightcore|punk|rb|rock|soul|speedcore|video-game-soundtrack|trance|techno|tv-movie-soundtrack|swing");
            if (data == null) return null;
            try
            {
                var cotdMaps = JsonConvert.DeserializeObject<BeatSaverMapSearchModelv2>(data);
                return cotdMaps;
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public static async Task<BeatSaverMapSearchModelv2> GetMapForCupOfTheDayHardcore(int page)
        {
            var data = await Get($"search/text/{page}?sortOrder=Relevance&curated=true&verified=true&minNps=8&tags=challenge|speed|tech");
            if (data == null) return null;
            try
            {
                var cotdMaps = JsonConvert.DeserializeObject<BeatSaverMapSearchModelv2>(data);
                return cotdMaps;
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public static async Task<string> Get(string endpoint)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(_baseURL + endpoint),
                    Method = HttpMethod.Get,
                };

                var productValue = new ProductInfoHeaderValue("ScraperBot", "1.0");
                var commentValue = new ProductInfoHeaderValue("(+http://www.example.com/ScraperBot.html)");

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
                client.DefaultRequestHeaders.UserAgent.Add(productValue);
                client.DefaultRequestHeaders.UserAgent.Add(commentValue);

                var httpResponseMessage = await client.SendAsync(request);

                if (httpResponseMessage.StatusCode != HttpStatusCode.OK) return null;

                var data = await httpResponseMessage.Content.ReadAsStringAsync();
                return data;
            }
        }
    }
}
