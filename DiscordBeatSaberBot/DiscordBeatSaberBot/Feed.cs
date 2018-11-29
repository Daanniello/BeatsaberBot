using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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