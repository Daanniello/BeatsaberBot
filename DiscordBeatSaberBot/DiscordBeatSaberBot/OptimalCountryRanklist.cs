using DiscordBeatSaberBot.Models.ScoreberAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    class OptimalCountryRanklist
    {
        public OptimalCountryRanklist()
        {

        }

        public async Task CreateAsync()
        {
        

            using (var client = new HttpClient())
            {
            
                    var url = $"https://new.scoresaber.com/api/player/{scoresaberId}/scores/top/{x}";
                    var httpCall = await client.GetAsync(url);
                    if (httpCall.StatusCode != HttpStatusCode.OK) return EmbedBuilderExtension.NullEmbed("Scoresaber Error", $"**Cant find maps on page:** {x}");
                    var f = JsonConvert.DeserializeObject<ScoresaberSongsModel>(httpCall.Content.ReadAsStringAsync().Result);
                
            }
        }
    }
}
