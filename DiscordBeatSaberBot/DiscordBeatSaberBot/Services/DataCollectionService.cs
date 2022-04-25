using DiscordBeatSaberBot.Api.BeatSaverApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Services
{
    public class DataCollectionService
    {
        private static string JsonSavePath = "../../../Resources/DataCollection.json";

        public static async Task UpdateData()
        {
            StoreData(await CollectData());
        }

        private static async Task<List<DataModel>> CollectData()
        {
            //Get Playerbase 
            var players = await new ScoreSaberLib.ScoreSaberClient().Api.Players.GetPlayers();
            var rankedMaps = await new ScoreSaberLib.ScoreSaberClient().Api.Leaderboards.GetLeaderboardsByFilter(ranked: true);
            var currentTopPlayer = await new ScoreSaberLib.ScoreSaberClient().Api.Players.GetPlayers(page: 0);

            //Get Old data 
            var oldData = GetData();
            if (oldData == null) oldData = new List<DataModel>();

            //Set Playerbase
            var oldDataCatPlayerbase = oldData.FirstOrDefault(x => x.Name == "PlayerBase");
            if (oldDataCatPlayerbase != null) oldDataCatPlayerbase.DataPoints.Add(DateTime.Now, players.Metadata.Total);
            else oldData.Add(new DataModel() { Name = "PlayerBase", DataPoints = new Dictionary<DateTime, object>() });

            //Set RankedMaps            
            var oldDataCatRankedmaps = oldData.FirstOrDefault(x => x.Name == "RankedMaps");
            if (oldDataCatRankedmaps != null) oldDataCatRankedmaps.DataPoints.Add(DateTime.Now, rankedMaps.Metadata.Total);
            else oldData.Add(new DataModel() { Name = "RankedMaps", DataPoints = new Dictionary<DateTime, object>() });

            //Set #1 players
            var oldDataCatPlayerOne = oldData.FirstOrDefault(x => x.Name == "PlayerOne");
            if (oldDataCatPlayerOne != null) oldDataCatPlayerOne.DataPoints.Add(DateTime.Now, Convert.ToInt64(currentTopPlayer.Players.First().Id));
            else oldData.Add(new DataModel() { Name = "PlayerOne", DataPoints = new Dictionary<DateTime, object>() });


            return oldData;
        }

        private static void StoreData(List<DataModel> data)
        {
            var json = JsonConvert.SerializeObject(data);
            MoveDataForWebsiteAccess();
            File.WriteAllText(JsonSavePath, json);
        }

        private static void MoveDataForWebsiteAccess()
        {
            try
            {
                var websitePath = @"C:\Users\DaanS\source\repos\BeatSaberBotWeb\BeatSaberBotWeb\wwwroot\DataCollection\DataCollection.json";
                if (File.Exists(websitePath)) File.Delete(websitePath);
                File.Copy(JsonSavePath, websitePath);
            }
            catch (Exception ex)
            {

            }
        }

        public static List<DataModel> GetData()
        {
            var json = File.ReadAllText(JsonSavePath);
            var data = JsonConvert.DeserializeObject<List<DataModel>>(json);
            return data;
        }

        public class DataModel
        {
            public Dictionary<DateTime, object> DataPoints { get; set; }
            public string Name { get; set; }
        }
    }
}
