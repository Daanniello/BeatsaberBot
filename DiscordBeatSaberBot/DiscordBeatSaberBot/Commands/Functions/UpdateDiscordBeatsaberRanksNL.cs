using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBeatSaberBot.Models.ScoreberAPI;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace DiscordBeatSaberBot
{
    internal static class UpdateDiscordBeatsaberRanksNL
    {
        public static async Task UpdateNLAsync(DiscordSocketClient discord, SocketMessage message)
        {
            

            var embed = await message.Channel.SendMessageAsync("Starting Roles update");
            await Task.Delay(1000);

            Console.WriteLine("Starting updating roles from linked NL accounts");
            var accounts = new Dictionary<string, object>();
            string DutchAccountsPath = "../../../DutchAccounts.txt";

            accounts = JsonExtension.GetJsonData(DutchAccountsPath);

            if (accounts == null || accounts.Count == 0)
                accounts = new Dictionary<string, object>();

            var LoadingSpaceCount = 0;
            var spaceCount = accounts.Count;
            var accountsProcessCount = 0;

            using (HttpClient client = new HttpClient())
            {
                foreach (var account in accounts)
                {
                    string url = $"https://new.scoresaber.com/api/player/{account.Value.ToString()}/full";
                    int rank = 0;

                    var playerInfoRaw = await client.GetAsync(url);
                    if (playerInfoRaw.StatusCode != HttpStatusCode.OK)
                    {
                        Console.WriteLine("Delete " + account.Value.ToString() + "He left the discord");
                        continue;
                    }
                    else
                    {
                        var playerInfo = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(playerInfoRaw.Content.ReadAsStringAsync().Result);
                        rank = playerInfo.playerInfo.CountryRank;
                    }
                    
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

                    await embed.ModifyAsync(x =>
                        x.Content = "Loading... \n*" + accountsProcessCount + "* ||  " + GiveSpaces(accountsProcessCount).Item1 + "||" + GiveSpaces(accountsProcessCount).Item2 + "*" + accounts.Count.ToString() + "*");
                    accountsProcessCount += 1;

                    (string, string) GiveSpaces(int count)
                    {
                        var l = "";
                        var s = "";

                        for (var i = 0; i < count; i++) l += " ";

                        for (var i = 0; i < accounts.Count - count; i++) s += " ";

                        return (l, s);
                    }

                    //wait 2 sec for each call
                    await Task.Delay(2000);
                }
            }

            Console.WriteLine("Done updating accounts NL");
        }
    }
}