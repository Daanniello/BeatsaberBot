using Newtonsoft.Json;
using System;

namespace DiscordBeatSaberBot.Models.ScoreberAPI
{
    public class ScoresaberPlayerFullModel
    {
        

        [JsonProperty(PropertyName = "playerinfo")]
        public PlayerInfoModel playerInfo { get; set; }

        [JsonProperty(PropertyName = "scoreStats")]
        public ScoreStats scoreStats { get; set; }

        public class PlayerInfoModel
        {
            [JsonProperty(PropertyName = "playerId")]
            public ulong PlayerId { get; set; }

            [JsonProperty(PropertyName = "pp")]
            private double _pp { get; set; }
            public double Pp { get { return Math.Round(_pp, 2); } set { this._pp = Math.Round(value, 2); } }

            [JsonProperty(PropertyName = "banned")]
            public bool Banned { get; set; }

            [JsonProperty(PropertyName = "inactive")]
            public bool Inactive { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "country")]
            public string Country { get; set; }

            [JsonProperty(PropertyName = "role")]
            public string Role { get; set; }


            [JsonProperty(PropertyName = "badges")]
            public BadgesModel[] Badges { get; set; }

            [JsonProperty(PropertyName = "history")]
            public string History { get; set; }

            [JsonProperty(PropertyName = "avatar")]
            public string Avatar { get; set; }

            [JsonProperty(PropertyName = "rank")]
            public int rank { get; set; }

            [JsonProperty(PropertyName = "countryRank")]
            public int CountryRank { get; set; }

            public class BadgesModel
            {
                [JsonProperty(PropertyName = "image")]
                public string Image { get; set; }

                [JsonProperty(PropertyName = "description")]
                public string Description { get; set; }
            }
        }

        public class ScoreStats
        {
            [JsonProperty(PropertyName = "totalScore")]
            public ulong TotalScore { get; set; }

            [JsonProperty(PropertyName = "totalRankedScore")]
            public ulong TotalRankedScore { get; set; }

            [JsonProperty(PropertyName = "averageRankedAccuracy")]
            private double _avarageRankedAccuracy { get; set; }
            public double AvarageRankedAccuracy { get { return Math.Round(_avarageRankedAccuracy,2); } set { this._avarageRankedAccuracy = Math.Round(value, 2); } }

            [JsonProperty(PropertyName = "totalPlayCount")]
            public int TotalPlayCount { get; set; }

            [JsonProperty(PropertyName = "rankedPlayCount")]
            public int RankedPlayerCount { get; set; }
        }
    }
}
