using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace DiscordBeatSaberBot
{
    static class Feed
    {
        //<a href="https://t.co/yXKcqIn8wu" class="twitter-timeline-link u-hidden" data-pre-embedded="true" dir="ltr">pic.twitter.com/yXKcqIn8wu</a>


        public static async Task<string> TwitterInfo()
        {
            var Link = "";
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync("https://twitter.com/beatsaber");
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                
                var table = doc.DocumentNode.SelectSingleNode("//a[@class='twitter-timeline-link u-hidden']");
                Link = table.GetAttributeValue("href", "");
            }

            return Link;
        }

        private static async Task MessagePlayer(DiscordSocketClient discord, ulong discordId, string content)
        {
            var guilds = discord.Guilds;
            var members = new List<SocketUser>();
            SocketUser userToSendTo = null;
            foreach (var guild in guilds)
            {
                foreach (var channel in guild.Channels)
                {
                    foreach (var user in channel.Users)
                    {
                        if (user.Id == discordId) { userToSendTo = user; }
                    }
                }
            }

            await Discord.UserExtensions.SendMessageAsync(userToSendTo, content);
        }

        public static async Task RanksInfoFeed(DiscordSocketClient discord)
        {
            var filePath = "RankFeedPlayers.txt";
            var content = "";
            var _data = new Dictionary<ulong, string[]>();
            using (StreamReader r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                _data = JsonConvert.DeserializeObject<Dictionary<ulong, string[]>>(json);
            }

            if (_data == null) { return; }

            foreach (var keyValue in _data)
            {
                var rankValue = await RankCheck(keyValue.Value[0], keyValue.Value[1]);
                if (rankValue.Item1 == 0) { return; }
                else if(rankValue.Item1 > 0) { content = "Congrats! you have a new rank. Your rank is now " + rankValue.Item2 + " and was " + (rankValue.Item2 + rankValue.Item1); }
                else
                {
                    content = "Same on you! you have a new rank. Your rank is now " + rankValue.Item2 + " and was " + (rankValue.Item2 - rankValue.Item1);
                }

                try
                {
                    await MessagePlayer(discord, keyValue.Key, content); 

                }
                catch
                {

                }
                
            }

        }

        public static async Task<(int,int)> RankCheck(string username, string oldRank)
        {
            var playerInfo = await BeatSaberInfoExtension.GetPlayerInfo(username);
            var rank = playerInfo.First().rank;
            var rankValue = int.Parse(oldRank) - rank;
            return (rankValue,rank);
        }

        public static async Task<EmbedBuilder> UpdateCheck(DiscordSocketClient discordSocketClient)
        {
            var filePath = @"TwitterLinks.txt";
            var jsonData = System.IO.File.ReadAllText(filePath);
            if (await TwitterInfo() == jsonData) return null;

            

            List<ulong[]> _data = new List<ulong[]>();

            using (StreamReader r = new StreamReader("FeedChannels.txt"))
            {
                string json = r.ReadToEnd();
                _data = JsonConvert.DeserializeObject<List<ulong[]>>(json);
            }

            foreach (var ids in _data)
            {

                ulong guild_id = ids[1];
                ulong channel_id = ids[0];

                var guild = discordSocketClient.Guilds.FirstOrDefault(x => x.Id == guild_id);
                var channel = guild.Channels.First(x => x.Id == channel_id) as IMessageChannel;
                await channel.SendMessageAsync(await TwitterInfo());
            }

            

            System.IO.File.WriteAllText(filePath, await TwitterInfo());
            return null;
        }

    }
}