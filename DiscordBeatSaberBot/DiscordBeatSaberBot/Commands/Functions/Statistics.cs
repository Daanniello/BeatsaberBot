//using Discord;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Commands.Functions
{
    public class Statistics
    {
        public enum category
        {
            Error,
            Hardware,
            PersonalInfo,
            Settings,
            Ranking
        }

        public enum type
        {
            Error,
            Headset,
            Controllers,
            Gender,
            Age,
            FavMods,
            Platform,
            //TotalPlayCount
        }

        private SocketMessage _message;

        public Statistics(SocketMessage message)
        {
            _message = message;
        }

        public async Task<type> Start(SocketMessage message)
        {
            var types = Enum.GetValues(typeof(type));
            var typesString = "";
            foreach (type type in types)
            {
                if (type != type.Error) typesString += $"*{type.ToString()}* \n";
            }
            var endMessage = await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("What kind of statistics do you want to see?", $"Type one of the following categories: \n\n {typesString}").Build());

            var endTime = DateTime.Now.AddSeconds(30);
            var startTime = DateTime.Now;
            do
            {
                Task.Delay(1000).Wait();
                var possibleReaction = await message.Channel.GetMessagesAsync(1).Flatten().FirstAsync();
                if (possibleReaction.Author == message.Author && possibleReaction.CreatedAt > startTime)
                {
                    var content = possibleReaction.Content;
                    await possibleReaction.DeleteAsync();

                    try
                    {
                        type enumType = (type)Enum.Parse(typeof(type), content, true);
                        await endMessage.DeleteAsync();
                        return enumType;
                    }
                    catch
                    {
                        await endMessage.DeleteAsync();
                        return type.Error;
                    }
                }

            } while (DateTime.Now < endTime);

            return type.Error;
        }

        public async Task<(category, type)> CreateSelectedType(type type)
        {
            //TODO ADD MORE STATISTICS
            if (type == type.Headset) return await CreateHeadsetStatistics();
            if (type == type.Controllers) return await CreateControllersStatistics();
            if (type == type.Gender) return await CreateGenderStatistics();
            if (type == type.Age) return await CreateAgeStatistics();
            if (type == type.FavMods) return await CreateFavModsStatistics();
            if (type == type.Platform) return await CreatePlatformStatistics();
            //if (type == type.TotalPlayCount) return await CreateTotalPlayCountStatistics();

            return (category.Error, type.Error);
        }

        public async Task<(category, type)> CreateHeadsetStatistics()
        {
            var results = await DatabaseContext.ExecuteSelectQuery("SELECT Headset from UserBeatSaberSettings");

            var ItemDictionary = new Dictionary<string, float>();
            ItemDictionary.Add("Index", results.Where(x => x.Where(x => x.ToString().ToLower().Contains("index")).Count() > 0).Count());
            ItemDictionary.Add("CV1", results.Where(x => x.Where(x => x.ToString().ToLower().Contains("cv1")).Count() > 0).Count());
            ItemDictionary.Add("Quest 1", results.Where(x => x.Where(x => x.ToString().ToLower().Contains("quest 1")).Count() > 0).Count());
            ItemDictionary.Add("Quest 2", results.Where(x => x.Where(x => x.ToString().ToLower().Contains("quest 2")).Count() > 0).Count());
            ItemDictionary.Add("Rift S", results.Where(x => x.Where(x => x.ToString().ToLower().Contains("rift s")).Count() > 0).Count());
            ItemDictionary.Add("Vive", results.Where(x => x.Where(x => x.ToString().ToLower().Contains("vive")).Count() > 0).Count());

            var headsetPath = CreatePieChart(category.Hardware, type.Headset, ItemDictionary, "Most used headsets");

            return (category.Hardware, type.Headset);
        }

        public async Task<(category, type)> CreateControllersStatistics()
        {
            var results = await DatabaseContext.ExecuteSelectQuery("SELECT Controllers from UserBeatSaberSettings");

            var ItemDictionary = new Dictionary<string, float>();
            ItemDictionary.Add("Index", results.Where(x => x.Where(x => x.ToString().ToLower().Contains("index") || x.ToString().ToLower().Contains("knuckle")).Count() > 0).Count());
            ItemDictionary.Add("CV1", results.Where(x => x.Where(x => x.ToString().ToLower().Contains("cv1")).Count() > 0).Count());
            ItemDictionary.Add("Quest 1", results.Where(x => x.Where(x => x.ToString().ToLower().Contains("quest 1")).Count() > 0).Count());
            ItemDictionary.Add("Quest 2", results.Where(x => x.Where(x => x.ToString().ToLower().Contains("quest 2")).Count() > 0).Count());
            ItemDictionary.Add("Rift S", results.Where(x => x.Where(x => x.ToString().ToLower().Contains("rift s")).Count() > 0).Count());
            ItemDictionary.Add("Vive", results.Where(x => x.Where(x => x.ToString().ToLower().Contains("vive")).Count() > 0).Count());

            var headsetPath = CreatePieChart(category.Hardware, type.Controllers, ItemDictionary, "Most used controllers");

            return (category.Hardware, type.Controllers);
        }

        public async Task<(category, type)> CreateAgeStatistics()
        {
            var results = await DatabaseContext.ExecuteSelectQuery("SELECT Age from UserBeatSaberSettings");
            var ItemDictionary = new Dictionary<string, float>();

            var age0to10 = 0;
            var age10to15 = 0;
            var age15to20 = 0;
            var age20to25 = 0;
            var age25to30 = 0;
            var age30to100 = 0;
            foreach (var row in results)
            {
                string term = row.First().ToString().Trim().ToLower();
                if (!term.All(char.IsDigit) || term == "") continue;
                var age = Convert.ToInt32(Regex.Replace(term, "[^0-9.]", ""));
                if (age < 100 && age > 0)
                {
                    if (age > 0 && age < 10) age0to10++;
                    else if (age >= 10 && age < 15) age10to15++;
                    else if (age >= 15 && age < 20) age15to20++;
                    else if (age >= 20 && age < 25) age20to25++;
                    else if (age >= 25 && age < 30) age25to30++;
                    else if (age >= 30 && age < 100) age30to100++;

                }
            }

            ItemDictionary.Add("0 to 10", age0to10);
            ItemDictionary.Add("10 to 15", age10to15);
            ItemDictionary.Add("15 to 20", age15to20);
            ItemDictionary.Add("20 to 25", age20to25);
            ItemDictionary.Add("25 to 30", age25to30);
            ItemDictionary.Add("30 to 100", age30to100);

            var agePath = CreatePieChart(category.PersonalInfo, type.Age, ItemDictionary, "Age distribution");

            return (category.PersonalInfo, type.Age);
        }

        public async Task<(category, type)> CreateFavModsStatistics()
        {
            var results = await DatabaseContext.ExecuteSelectQuery("SELECT FavMods from UserBeatSaberSettings");
            var ItemDictionary = new Dictionary<string, float>();

            var modList = new List<string>();
            foreach (var row in results)
            {
                string cleanup = row.First().ToString().Trim().ToLower();
                var modsMessy = cleanup.Replace(" and ", ",").Replace("-", ",");
                var mods = modsMessy.Split(',');
                foreach(var mod in mods)
                {
                    var modFinal = mod.Trim();
                    if(modFinal != "") modList.Add(modFinal);
                }                
            }

            var q = from x in modList
                    group x by x into g
                    let count = g.Count()
                    orderby count descending
                    select new { Value = g.Key, Count = count };

            for (var x = 0; x < 6; x++)
            {
                ItemDictionary.Add(q.ElementAt(x).Value, q.ElementAt(x).Count);
            }

            var favModsPath = CreatePieChart(category.Settings, type.FavMods, ItemDictionary, "Top 6 Fav Mods");

            return (category.Settings, type.FavMods);
        }

        public async Task<(category, type)> CreatePlatformStatistics()
        {
            var results = await DatabaseContext.ExecuteSelectQuery("SELECT Platform from UserBeatSaberSettings");
            var ItemDictionary = new Dictionary<string, float>();

            var platformList = new List<string>();
            foreach (var row in results)
            {
                string cleanup = row.First().ToString().Trim().ToLower();
                var platformsMessy = cleanup.Replace(" and ", ",").Replace("-", ",").Replace("/", ",");
                var platforms = platformsMessy.Split(',');
                foreach (var platform in platforms)
                {
                    var platformFinal = platform.Trim();
                    if (platformFinal != "" && platformFinal != "steam" && platformFinal != "oculus" && platformFinal != "psvr") platformList.Add(platformFinal);
                }
            }

            var q = from x in platformList
                    group x by x into g
                    let count = g.Count()
                    orderby count descending
                    select new { Value = g.Key, Count = count };

            for (var x = 0; x < 6; x++)
            {
                ItemDictionary.Add(q.ElementAt(x).Value, q.ElementAt(x).Count);
            }

            var favModsPath = CreatePieChart(category.Settings, type.Platform, ItemDictionary, "Top 6 Platforms");

            return (category.Settings, type.Platform);
        }

        public async Task<(category, type)> CreateGenderStatistics()
        {
            var results = await DatabaseContext.ExecuteSelectQuery("SELECT Gender from UserBeatSaberSettings");

            var ItemDictionary = new Dictionary<string, float>();

            var maleCount = 0;
            var femaleCount = 0;
            var otherCount = 0;
            foreach (var row in results)
            {
                string term = row.First().ToString().Trim().ToLower();
                if (term == "male" || term == "man" || term == "boy" || term == "boi" || term == "m") maleCount++;
                else if (term == "female" || term == "woman" || term == "girl" || term == "f") femaleCount++;
                else otherCount++;

            }
            ItemDictionary.Add("Female", femaleCount);
            ItemDictionary.Add("Male", maleCount);
            ItemDictionary.Add("Other", otherCount);


            var headsetPath = CreatePieChart(category.PersonalInfo, type.Gender, ItemDictionary, "Gender distribution");

            return (category.PersonalInfo, type.Gender);
        }

        //public async Task<(category, type)> CreateTotalPlayCountStatistics()
        //{
        //    var top50 = await ScoresaberAPI.GetTop50Global();

        //    var Top5AndRandomDic = new Dictionary<string, float>();
            
        //    for(var i = 0; i < 5; i++)
        //    {
        //        var top5Player = await new ScoresaberAPI(top50.Players[i].PlayerId).GetPlayerFull();
        //        Top5AndRandomDic.Add(top5Player.playerInfo.Name, top5Player.scoreStats.TotalPlayCount);
        //    }

        //    var player = await new ScoresaberAPI(await RoleAssignment.GetScoresaberIdWithDiscordId(_message.Author.Id.ToString())).GetPlayerFull();
        //    Top5AndRandomDic.Add(player.playerInfo.Name, player.scoreStats.TotalPlayCount);

        //    var totalplaycountpath = CreatePieChart(category.Ranking, type.TotalPlayCount, Top5AndRandomDic, "Top 5 Total Play Count");

        //    return (category.Ranking, type.TotalPlayCount);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        /// <param name="values">Key: Name of element | Value: Amount of the element</param>
        /// <param name="title"></param>
        /// <returns></returns>
        public string CreatePieChart(category category, type type, Dictionary<string, float> values, string title)
        {
            using (var bitmap = new Bitmap(1300, 800))
            {
                System.Drawing.Color[] color = { System.Drawing.Color.FromArgb(82, 215, 38), System.Drawing.Color.FromArgb(255, 236, 0), System.Drawing.Color.FromArgb(255, 115, 0), System.Drawing.Color.FromArgb(255, 0, 0), System.Drawing.Color.FromArgb(0, 126, 214), System.Drawing.Color.FromArgb(124, 221, 221) };
                Rectangle rect = new Rectangle(30, 150, 600, 600);

                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.Clear(System.Drawing.Color.FromArgb(36, 36, 36));

                float fDegValue = 0.0f;
                float fDegSum = 0.0f;

                values = values.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                //Draw pie chart 
                for (int iCnt = 0; iCnt < values.Count; iCnt++)
                {
                    fDegValue = (values.Values.ElementAt(iCnt) / values.Values.Sum()) * 360;
                    Brush brush = new SolidBrush(color[iCnt]);
                    graphics.FillPie(brush, rect, fDegSum, fDegValue);
                    fDegSum += fDegValue;
                }


                //Draw text and legenda
                for (var x = 1; x <= values.Count; x++)
                {
                    graphics.DrawString($"{values.Keys.ElementAt(x - 1)}: {values.Values.ElementAt(x - 1)}", new Font("Tourmaline", 36), Brushes.LightGray, 800, 100 * x + 100);
                    graphics.FillRectangle(new SolidBrush(color[x - 1]), 700, 100 * x + 108, 100, 50);
                }

                graphics.DrawString($"{title}", new Font("Tourmaline", 36), Brushes.LightGray, 50, 50);

                //Save file in the correct category and type
                var path = $"../../../Resources/img/piechart-{category}-{type}.png";
                if (File.Exists(path)) File.Delete(path);
                bitmap.Save(path);
                return path;
            }
        }
    }
}
