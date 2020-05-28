using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DiscordBeatSaberBot.Models.ScoreberAPI
{
    public partial class ScoresaberSearchPlayerModel
    {
        [JsonProperty("playerid")]
        public string Playerid { get; set; }

        [JsonProperty("pp")]
        public double Pp { get; set; }

        [JsonProperty("banned")]
        public long Banned { get; set; }

        [JsonProperty("inactive")]
        public long Inactive { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("history")]
        public string History { get; set; }

        [JsonProperty("rank")]
        public long Rank { get; set; }

        [JsonProperty("difference")]
        public long Difference { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }
    }
}
