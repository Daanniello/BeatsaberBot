using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DiscordBeatSaberBot
{
    static class JsonExtension
    {
        public static Dictionary<string, object> GetJsonData(string filePath)
        {
            using (var r = new StreamReader(filePath))
            {
                var json = r.ReadToEnd();
                if (json == "")
                {
                    var empty = new Dictionary<string, object>();
                    empty.Add("empty", "empty");
                    return empty;
                }
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                return data;
            }
        }

        public static void InsertJsonData(string filePath, string key, object data)
        {
            Dictionary<string, object> oldData = GetJsonData(filePath);
            if (oldData.ContainsKey(key))
            {
                oldData.Remove(key);
                oldData.Add(key, data);
            }
            else
            {
                oldData.Add(key, data);
            }
            
            using (var file = File.CreateText(filePath))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, oldData);
            }
        }
    }
}
