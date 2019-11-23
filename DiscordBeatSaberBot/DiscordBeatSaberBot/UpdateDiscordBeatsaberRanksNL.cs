using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.WebSocket;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace DiscordBeatSaberBot
{
    internal static class UpdateDiscordBeatsaberRanksNL
    {
        public static async Task UpdateNLAsync(DiscordSocketClient discord)
        {
            Console.WriteLine("Starting updating roles from linked NL accounts");
            var accounts = new Dictionary<string, object>();
            string DutchAccountsPath = "../../../DutchAccounts.txt";

            accounts = JsonExtension.GetJsonData(DutchAccountsPath);

            if (accounts == null || accounts.Count == 0)
                accounts = new Dictionary<string, object>();

            var client = new HttpClient();

            string staticUrl = "https://scoresaber.com/u/";
            foreach (var account in accounts)
            {
                string url = staticUrl + account.Value.ToString();
                int rank = 0;

                string html = await client.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                try
                {
                    string rankUnfixed = doc.DocumentNode.SelectSingleNode("//a[@href='/global?country=nl']").InnerText;
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
                        await DutchRankFeed.GiveRole(account.Value.ToString(), "Koos Rankloos", discord);
                        continue;
                    }

                    if (rank == 1)
                        await DutchRankFeed.GiveRole(account.Value.ToString(), "Nummer 1", discord);
                    else if (rank <= 3)
                        await DutchRankFeed.GiveRole(account.Value.ToString(), "Top 3", discord);
                    else if (rank <= 10)
                        await DutchRankFeed.GiveRole(account.Value.ToString(), "Top 10", discord);
                    else if (rank <= 25)
                        await DutchRankFeed.GiveRole(account.Value.ToString(), "Top 25", discord);
                    else if (rank <= 50)
                        await DutchRankFeed.GiveRole(account.Value.ToString(), "Top 50", discord);
                    else if (rank <= 100)
                        await DutchRankFeed.GiveRole(account.Value.ToString(), "Top 100", discord);
                    else if (rank <= 250)
                        await DutchRankFeed.GiveRole(account.Value.ToString(), "Top 250", discord);
                    else if (rank <= 500)
                        await DutchRankFeed.GiveRole(account.Value.ToString(), "Top 500", discord);
                    else if (rank > 500)
                        await DutchRankFeed.GiveRole(account.Value.ToString(), "Top 501+", discord);

                }
                catch
                {
                    Console.WriteLine("Delete " + account.Value.ToString() + "He left the discord");
                }
            }

            client.Dispose();

            Console.WriteLine("Done updating accounts NL");
        }
    }
}