using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBeatSaberBot.Models.ScoreberAPI
{

    public class ScoreSaberSongsModel
    {
        [JsonProperty("scores")]
        public Score[] Scores { get; set; }
    }
    public class Score
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
        public string _id { get; set; }
        public string Id { get { return _id.ToUpper(); } set { this._id = value; } }

        [JsonProperty("songName")]
        public string Name { get; set; }

        [JsonProperty("songSubName")]
        public string SongSubName { get; set; }

        [JsonProperty("songAuthorName")]
        public string SongAuthorName { get; set; }

        [JsonProperty("levelAuthorName")]
        public string LevelAuthorName { get; set; }

        [JsonProperty("difficulty")]
        private double _diff { get; set; }

        public string GetDifficulty()
        {
            var returnItem = "owo";
            if (_diff == 1) returnItem = "Easy";
            else if (_diff == 3) returnItem = "Normal";
            else if (_diff == 5) returnItem = "Hard";
            else if (_diff == 7) returnItem = "Expert";
            else if (_diff == 9) returnItem = "ExpertPlus";
            return returnItem;
        }     

        [JsonProperty("maxScore")]
        public long MaxScoreEx { get; set; }

        [JsonProperty("rank")]
        public long Rank { get; set; }

    }
}
