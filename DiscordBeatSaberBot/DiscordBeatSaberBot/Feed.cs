using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace DiscordBeatSaberBot
{
    internal static class Feed
    {
        //<a href="https://t.co/yXKcqIn8wu" class="twitter-timeline-link u-hidden" data-pre-embedded="true" dir="ltr">pic.twitter.com/yXKcqIn8wu</a>

        public static async Task<string> TwitterInfo()
        {
            var Link = "";
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync("https://twitter.com/beatsaber");
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//a[@class='twitter-timeline-link u-hidden']");
                Link = table.GetAttributeValue("href", "");
            }

            return Link;
        }

        private static async Task MessagePlayer(DiscordSocketClient discord,
            ulong discordId,
            string content)
        {
            var guilds = discord.Guilds;
            var members = new List<SocketUser>();
            SocketUser userToSendTo = null;
            foreach (var guild in guilds)
            {
                foreach (var channel in guild.Channels)
                {
                    foreach (var user in channel.Users)
                        if (user.Id == discordId)
                            userToSendTo = user;
                }
            }

            await userToSendTo.SendMessageAsync(content);
        }

        public static async Task RanksInfoFeed(DiscordSocketClient discord)
        {
            var filePath = "RankFeedPlayers.txt";
            var content = "";
            var _data = new Dictionary<ulong, string[]>();
            using (var r = new StreamReader(filePath))
            {
                var json = r.ReadToEnd();
                _data = JsonConvert.DeserializeObject<Dictionary<ulong, string[]>>(json);
            }

            if (_data == null) return;

            foreach (var keyValue in _data)
            {
                var rankValue = await RankCheck(keyValue.Value[0], keyValue.Value[1]);
                var currentRankPlayer = new List<Player>();

                try { currentRankPlayer = await BeatSaberInfoExtension.GetPlayerInfo(keyValue.Value[0]); }
                catch { return; }

                var rank = currentRankPlayer.First().rank;
                var threshold = RankThresHold(rank);


                if (rankValue.Item1 < threshold * -1) content = "Ohhh noo!you have been deranked.Your rank is now " + rank;
                else if (rankValue.Item1 > threshold) content = "Congrats! you have a better rank. Your rank is now " + rank;
                else continue;

                try
                {
                    await MessagePlayer(discord, keyValue.Key, content);

                    var rankInput = new Dictionary<ulong, string[]>();
                    using (var r = new StreamReader(filePath))
                    {
                        var json = r.ReadToEnd();
                        rankInput = JsonConvert.DeserializeObject<Dictionary<ulong, string[]>>(json);
                    }

                    foreach (var player in rankInput)
                        if (player.Key == keyValue.Key)
                        {
                            var info = new List<Player>();
                            try { info = await BeatSaberInfoExtension.GetPlayerInfo(player.Value[0]); }
                            catch { return; }

                            player.Value[1] = info.First().rank.ToString();
                        }

                    using (var file = File.CreateText(filePath))
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(file, rankInput);
                    }
                }
                catch
                {

                }
            }
        }

        public static async Task<(int, int)> RankCheck(string username,
            string oldRank)
        {
            var playerInfo = new List<Player>();
            try { playerInfo = await BeatSaberInfoExtension.GetPlayerInfo(username); }
            catch { return (0, 0); }

            var rank = playerInfo.First().rank;
            var rankValue = int.Parse(oldRank) - rank;
            return (rankValue, rank);
        }

        public static async Task<EmbedBuilder> UpdateCheck(DiscordSocketClient discordSocketClient)
        {
            var filePath = @"TwitterLinks.txt";
            var jsonData = File.ReadAllText(filePath);
            if (await TwitterInfo() == jsonData) return null;

            var _data = new List<ulong[]>();

            using (var r = new StreamReader("FeedChannels.txt"))
            {
                var json = r.ReadToEnd();
                _data = JsonConvert.DeserializeObject<List<ulong[]>>(json);
            }

            foreach (var ids in _data)
            {
                var guild_id = ids[1];
                var channel_id = ids[0];

                var guild = discordSocketClient.Guilds.FirstOrDefault(x => x.Id == guild_id);
                var channel = guild.Channels.First(x => x.Id == channel_id) as IMessageChannel;
                await channel.SendMessageAsync(await TwitterInfo());
            }

            File.WriteAllText(filePath, await TwitterInfo());
            return null;
        }

        private static int RankThresHold(int rank)
        {
            //#1-50: per 1 rank
            //#51-100: per 5 ranks
            //#101-250: per 10 ranks
            //#251-500: per 20 ranks
            //#500-1000: per 50 ranks
            //#1000+: per 100 ranks
            var threshold = 0;
            if (rank < 50) threshold = 1;
            else if (rank < 100) threshold = 5;
            else if (rank < 250) threshold = 10;
            else if (rank < 500) threshold = 20;
            else if (rank < 1000) threshold = 50;
            else if (rank < 10000) threshold = 100;
            else if (rank < 50000) threshold = 500;
            return threshold;
        }
    }
}