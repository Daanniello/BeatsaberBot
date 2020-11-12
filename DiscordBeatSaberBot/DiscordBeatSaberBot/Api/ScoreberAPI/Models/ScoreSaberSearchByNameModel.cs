using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;


namespace DiscordBeatSaberBot.Models.ScoreberAPI
{

    public partial class ScoreSaberSearchByNameModel
    {
        [JsonProperty("players")]
        public List<Player> Players { get; set; }
    }

    public partial class Player
    {
        [JsonProperty("playerId")]
        public string PlayerId { get; set; }

        [JsonProperty("playerName")]
        public string PlayerName { get; set; }

        [JsonProperty("pp")]
        public double Pp { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("history")]
        public string History { get; set; }

        [JsonProperty("difference")]
        public long Difference { get; set; }
    }

}
