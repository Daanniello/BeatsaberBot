using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using Discord;
using System.IO;

namespace DiscordBeatSaberBot
{
    class  MemeFeed
    {
        private DiscordSocketClient _discord;

        public MemeFeed(DiscordSocketClient Discord)
        {
            _discord = Discord;
        }

        public async void TimerRunning(CancellationToken token)
        {
            var watch = Stopwatch.StartNew();
            while (!token.IsCancellationRequested)
                try
                {
                    // 60000 = 1 min
                    // 600000 = 10 min
                    // 3600000 = 60 min
                    var Hour = 60000 * 60 * 8;
                    await Task.Delay(Hour - (int)(watch.ElapsedMilliseconds % 1000), token);
                    await GetLatestMeme();
                }
                catch
                {
                }
        }

        public async Task GetLatestMeme()
        {
            var url = "https://www.reddit.com/r/dankmemes/hot/";
            var img = "";
            var text = "";
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                //https://preview.redd.it/60bywofgjkd21.jpg?width=640&crop=smart&auto=webp&s=37c92a299ca5f91d3d6242c566dc52b2013b37a9
                var table = doc.DocumentNode.SelectNodes("//img[@class='_2_tDEnGMLxpM6uOa2kaDB3 media-element']");
                img = WebUtility.HtmlDecode(table.First().GetAttributeValue("src", ""));
                text = doc.DocumentNode.SelectNodes("//h2[@class='s1okktje-0 kVQyNs']").Skip(2).First().InnerText;

            }

            

            // guild test: 247188513672396802
            // channel test: 502202979060023297
            // guild: 505485680344956928
            // channel: 505503157258944512
            var guild = _discord.GetGuild(505485680344956928);
            var channel = guild.GetTextChannel(505503157258944512);

            var embed = new EmbedBuilder {
                ThumbnailUrl = img
            };

            using (var client = new WebClient())
            {
                
                client.DownloadFile(img, @"img.jpg");
                var t = client.BaseAddress;
                
                //C:\Users\Daan\Documents\GitHub\BeatsaberBot\DiscordBeatSaberBot\DiscordBeatSaberBot\img.jpg
                string r = AppDomain.CurrentDomain.BaseDirectory;
                await channel.SendFileAsync(r + "\\img.jpg", "**" + text + "**");
            }

            
        }
    }
}
