//using Discord.WebSocket;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using PuppeteerSharp;
//using System.Threading.Tasks;

//namespace DiscordBeatSaberBot
//{
//    public class Corona
//    {
//        public Corona()
//        {
//            TakeScreenshotFromCoronaCounter().Wait();
//        }

//        public async Task TakeScreenshotFromCoronaCounter()
//        {
//            try
//            {
//                var outputFilePath = "";
//                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
//                var browser = await Puppeteer.LaunchAsync(new LaunchOptions
//                {
//                    Headless = false
//                });
//                var page = await browser.NewPageAsync();
//                await page.GoToAsync("https://www.worldometers.info/coronavirus/");
//                await page.ScreenshotAsync(outputFilePath);
//            }
//            catch(Exception ex)
//            {

//            }
         
//        }
//    }
//}
