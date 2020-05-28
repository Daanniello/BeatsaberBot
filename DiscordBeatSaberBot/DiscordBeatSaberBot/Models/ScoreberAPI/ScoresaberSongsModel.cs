using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBeatSaberBot.Models.ScoreberAPI
{

    public partial class ScoresaberSongsModel
    {
        [JsonProperty("scores")]
        public Score[] Scores { get; set; }
    }
    public partial class Score
        {
            [JsonProperty("scoreId")]
            public long ScoreId { get; set; }

            [JsonProperty("leaderboardId")]
            public long LeaderboardId { get; set; }

            [JsonProperty("score")]
            public long ScoreScore { get; set; }

            [JsonProperty("uScore")]
            public long UScore { get; set; }

            [JsonProperty("mods")]
            public string Mods { get; set; }

            [JsonProperty("playerId")]
            public string PlayerId { get; set; }

            [JsonProperty("timeset")]
            public DateTimeOffset Timeset { get; set; }

            //[JsonProperty("pp")]
            //public long Pp { get; set; }

            [JsonProperty(PropertyName = "pp")]
            private double _pp { get; set; }
            public double Pp { get { return Math.Round(_pp, 3); } set { this._pp = Math.Round(value, 3); } }

            [JsonProperty(PropertyName = "weight")]
            private double _weight { get; set; }
            public double Weight { get { return Math.Round(_weight, 14); } set { this._weight = Math.Round(value, 14); } }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("songSubName")]
            public string SongSubName { get; set; }

            [JsonProperty("songAuthorName")]
            public string SongAuthorName { get; set; }

            [JsonProperty("levelAuthorName")]
            public string LevelAuthorName { get; set; }

            [JsonProperty("diff")]
            public string Diff { get; set; }

            [JsonProperty("maxScoreEx")]
            public long MaxScoreEx { get; set; }

            [JsonProperty("rank")]
            public long Rank { get; set; }
        
    }
}
