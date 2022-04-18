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

            //Get Old data 
            var oldData = GetData();
            if (oldData == null) oldData = new List<DataModel>();

            //Set Playerbase
            var oldDataCat = oldData.FirstOrDefault(x => x.Name == "PlayerBase");
            if (oldDataCat != null) oldDataCat.DataPoints.Add(DateTime.Now, players.Metadata.Total);
            else oldData.Add(new DataModel() { Name = "PlayerBase", DataPoints = new Dictionary<DateTime, object>() });

            return oldData;
        }

        private static void StoreData(List<DataModel> data)
        {
            var json = JsonConvert.SerializeObject(data);
            File.WriteAllText(JsonSavePath, json);
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
