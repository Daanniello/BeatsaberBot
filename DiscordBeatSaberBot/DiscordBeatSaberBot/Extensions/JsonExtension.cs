using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DiscordBeatSaberBot
{
    internal static class JsonExtension
    {
        /// <summary>
        ///     Get JSON data DICTIONARY. insert file path
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>Dictionary string object</returns>
        public static Dictionary<string, object> GetJsonData(string filePath)
        {
            using (var r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
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

        /// <summary>
        ///     Insert JSON in filepath with key and data
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public static void InsertJsonData(string filePath, string key, object data)
        {
            var oldData = GetJsonData(filePath);
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