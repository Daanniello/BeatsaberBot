using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using DiscordBeatSaberBot;

namespace UnitTest
{
    [TestClass]
    public class DutchRankFeedTest
    {
        [TestMethod]
        public async Task UsingUpdateRankList()
        {
            var filePath = "../../../DutchRankList.txt";
            var newData = new Dictionary<int, List<string>>();
            newData.Add(999, new List<string>());

            using (var file = File.CreateText(filePath))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, newData);
            }

            var collectedList = await DutchRankFeed.UpdateDutchRankList();

            Assert.IsTrue(collectedList.Count > 5);
        }

        [TestMethod]
        public async Task MessagesToSend()
        {
            var testPath = "../../../DutchRankListTest.txt";
            var testPath2 = "../../../DutchRankListTest2.txt";
            var newData = new Dictionary<int, List<string>>();
            using (var r = new StreamReader(testPath))
            {
                var json = r.ReadToEnd();
                newData = JsonConvert.DeserializeObject<Dictionary<int, List<string>>>(json);
            }

            using (var file = File.CreateText(testPath2))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, newData);
            }

            var builder = await DutchRankFeed.MessagesToSend();
        }
    }
}
