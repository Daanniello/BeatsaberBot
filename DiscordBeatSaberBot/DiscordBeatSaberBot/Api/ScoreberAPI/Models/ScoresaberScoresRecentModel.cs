using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DiscordBeatSaberBot.Models.ScoreberAPI
{
    class ScoresaberScoresRecentModel
    {
        [JsonProperty(PropertyName = "scores")]
        public ScoresaberScoreRecentModel[] RecentScore { get; set; }

        public class ScoresaberScoreRecentModel
        {
            [JsonProperty(PropertyName = "scoreId")]
            public ulong ScoreId { get; set; }

            [JsonProperty(PropertyName = "leaderboardId")]
            public int Leaderboard { get; set; }

            [JsonProperty(PropertyName = "score")]
            public ulong Score { get; set; }

            [JsonProperty(PropertyName = "uScore")]
            public ulong UScore { get; set; }

            [JsonProperty(PropertyName = "mods")]
            public string Mods { get; set; }

            [JsonProperty(PropertyName = "playerId")]
            public ulong PlayerId { get; set; }

            [JsonIgnore]
            public DateTime Timeset { get; set; }

            [JsonProperty(PropertyName = "pp")]
            public double Pp { get; set; }

            [JsonProperty(PropertyName = "weight")]
            public float Weight { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "songSubName")]
            public string SongSubName { get; set; }

            [JsonProperty(PropertyName = "songAuthorName")]
            public string SongAutherName { get; set; }

            [JsonProperty(PropertyName = "levelAuthorName")]
            public string LevelAuthorName { get; set; }

            [JsonProperty(PropertyName = "diff")]
            public string Diff { get; set; }

            [JsonProperty(PropertyName = "maxScoreEx")]
            public ulong MaxScoreEx { get; set; }

            [JsonProperty(PropertyName = "rank")]
            public int Rank { get; set; }
        }

      
    }
}
