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

        [JsonProperty("unmodififiedScore")]
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

        [JsonProperty("songHash")]
        public string Id { get; set; }

        [JsonProperty("songName")]
        public string Name { get; set; }

        [JsonProperty("songSubName")]
        public string SongSubName { get; set; }

        [JsonProperty("songAuthorName")]
        public string SongAuthorName { get; set; }

        [JsonProperty("levelAuthorName")]
        public string LevelAuthorName { get; set; }

        [JsonProperty("difficulty")]
        private string _diff { get; set; }
        public string Diff
        {
            get
            {
                return _diff;
            }
            set
            {
                if(value.ToString() == "1") this._diff = "easy";
                else if (value.ToString() == "3") this._diff = "normal";
                else if (value.ToString() == "5") this._diff = "hard";
                else if (value.ToString() == "7") this._diff = "expert";
                else if (value.ToString() == "9") this._diff = "expert";
                else this._diff = "owo";           
            }
        }

        [JsonProperty("maxScore")]
        public long MaxScoreEx { get; set; }

        [JsonProperty("rank")]
        public long Rank { get; set; }

    }
}
