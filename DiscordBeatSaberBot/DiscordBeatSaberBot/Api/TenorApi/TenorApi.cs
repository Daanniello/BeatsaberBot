using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tenor;
using Tenor.Schema;

namespace DiscordBeatSaberBot.Api.TenorApi
{
    public class TenorApi
    {       
        public static async Task<string> GetGif(ContentFilter filter, string parameter = null)
        {
            var random = new Random();
            var loginCode = await DatabaseContext.ExecuteSelectQuery("Select * from Settings");

            var config = new TenorConfiguration
            {
                ApiKey = (string)loginCode[0][5],                
                ContentFilter = filter,
                MediaFilter = MediaFilter.Minimal,
                AspectRatio = AspectRatio.All
            };
            var client = new TenorClient(config);            

            if (parameter == null) parameter = "meme";
            var gifInfo = await client.GetRandomPostsAsync(parameter, 20, "5");            
            

            if (gifInfo == null || gifInfo.Results.Count() <= 0) return null;
            if (random.Next(0, 80) > 60)
            {
                var trending = await client.SearchAsync(parameter);
                var link = trending.Results.ElementAt(random.Next(0,19)).FullUrl.AbsoluteUri;
                return link;
            }
            else
            {                
                var link = gifInfo.Results.First().FullUrl.AbsoluteUri;
                return link;
            }            
        }
    }
}
