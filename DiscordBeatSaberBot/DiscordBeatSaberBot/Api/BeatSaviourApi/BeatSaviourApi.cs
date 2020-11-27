using DiscordBeatSaberBot.Api.BeatSaviourApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Api.BeatSaviourApi
{
    class BeatSaviourApi
    {

        private string _baseUrl = "https://www.beatsavior.io/api/livescores/player/";
        private string _scoresaberID;
        public BeatSaviourApi(string scoresaberId)
        {
            _scoresaberID = scoresaberId;
        }

        public async Task<List<GridAccElement>> GetMostRecentGridAccAsync()
        {
            using (var client = new HttpClient())
            {
                var httpResponseMessage = await client.GetAsync(_baseUrl + _scoresaberID);

                if (httpResponseMessage.StatusCode != HttpStatusCode.OK) return null;

                var LiveDataJsonData = await httpResponseMessage.Content.ReadAsStringAsync();

                var LiveData = JsonConvert.DeserializeObject<List<BeatSaviourLivedataModel>>(LiveDataJsonData);
                var accgrid = LiveData.First().Trackers.AccuracyTracker.GridAcc;
                return accgrid;
            }
        }

        public async Task<List<BeatSaviourLivedataModel>> GetLiveDataFull()
        {
            using (var client = new HttpClient())
            {
                var httpResponseMessage = await client.GetAsync(_baseUrl + _scoresaberID);

                if (httpResponseMessage.StatusCode != HttpStatusCode.OK) return null;

                var LiveDataJsonData = await httpResponseMessage.Content.ReadAsStringAsync();

                var LiveData = JsonConvert.DeserializeObject<List<BeatSaviourLivedataModel>>(LiveDataJsonData);      
                return LiveData;
            }
        }

        public async Task<BeatSaviourLivedataModel> GetMostRecentLiveData(string playid)
        {
            using (var client = new HttpClient())
            {
                var httpResponseMessage = await client.GetAsync(_baseUrl + _scoresaberID);

                if (httpResponseMessage.StatusCode != HttpStatusCode.OK) return null;

                var LiveDataJsonData = await httpResponseMessage.Content.ReadAsStringAsync();
                try
                {
                    var LiveData = JsonConvert.DeserializeObject<List<BeatSaviourLivedataModel>>(LiveDataJsonData);
                    if (LiveData.Count == 0) return null;
                    var mostRecentPlay = LiveData.Where(x => x.SongId == playid);
                    if (mostRecentPlay.Count() == 0) return null;
                    return mostRecentPlay.Last();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    return null;
                }
                
            }
        }
    }
}
