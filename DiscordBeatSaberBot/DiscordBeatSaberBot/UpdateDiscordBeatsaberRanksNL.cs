using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace DiscordBeatSaberBot
{
    static class UpdateDiscordBeatsaberRanksNL
    {
        public static async System.Threading.Tasks.Task UpdateNLAsync(DiscordSocketClient discord)
        {
            Console.WriteLine("Starting updating roles from linked NL accounts");
            var accounts = new List<string[]>();
            var filePath = "../../../account.txt";

            using (var r = new StreamReader(filePath))
            {
                var json = r.ReadToEnd();
                accounts = JsonConvert.DeserializeObject<List<string[]>>(json);
            }

            if (accounts == null || accounts.Count == 0)
            {
                accounts = new List<string[]>();

            }

            var client = new HttpClient();

            var staticUrl = "https://scoresaber.com/u/";
            foreach (var account in accounts)
            {
                var url = staticUrl + account[1];
                var rank = 0;
               
                    var html = await client.GetStringAsync(url);
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);
                    try
                    {
                        var rankUnfixed = doc.DocumentNode.SelectSingleNode("//a[@href='/global?country=nl']").InnerText;
                        rank = int.Parse(rankUnfixed.Replace("(", "").Replace(")", "").Replace("#", "").Replace(",", "").Trim());
                    }
                    catch
                    {
                        var Logger = new Logger(discord);
                        Logger.Log(Logger.LogCode.debug, "Cant get rank info from: " + url);
                    }
                 
                try
                {
                    if (rank == 0)
                    {
                        await DutchRankFeed.GiveRole(account[1], "Koos Rankloos", discord);
                        continue;
                    }
                    if (rank == 1)
                    {
                        await DutchRankFeed.GiveRole(account[1], "Nummer 1", discord);

                    }
                    else if (rank <= 3)
                    {
                        await DutchRankFeed.GiveRole(account[1], "Top 3", discord);

                    }
                    else if (rank <= 10)
                    {
                        await DutchRankFeed.GiveRole(account[1], "Top 10", discord);

                    }
                    else if (rank <= 25)
                    {
                        await DutchRankFeed.GiveRole(account[1], "Top 25", discord);

                    }
                    else if (rank <= 50)
                    {
                        await DutchRankFeed.GiveRole(account[1], "Top 50", discord);

                    }
                    else if (rank <= 100)
                    {
                        await DutchRankFeed.GiveRole(account[1], "Top 100", discord);

                    }
                    else if (rank <= 250)
                    {

                        await DutchRankFeed.GiveRole(account[1], "Top 250", discord);
                    }
                }
                catch
                {
                    Console.WriteLine("Delete " + account[1] + "He left the discord");
                }

            }

            client.Dispose();

            Console.WriteLine("Done updating accounts NL");
        }
    }
}
