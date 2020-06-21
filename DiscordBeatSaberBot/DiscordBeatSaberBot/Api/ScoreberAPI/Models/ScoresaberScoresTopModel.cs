using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBeatSaberBot.Api.ScoreberAPI.Models
{
    public class ScoresaberScoresTopModel
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

        [JsonProperty("unmodififiedScore")]
        public long ScoreScore { get; set; }

        [JsonProperty("uScore")]
        public long UScore { get; set; }

        [JsonProperty("mods")]
        public string Mods { get; set; }

        [JsonProperty("playerId")]
        public string PlayerId { get; set; }

        [JsonProperty("timeset")]
        public DateTimeOffset Timeset { get; set; }

        [JsonProperty("pp")]
        public double Pp { get; set; }

        [JsonProperty("weight")]
        public double Weight { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("songName")]
        public string Name { get; set; }

        [JsonProperty("songSubName")]
        public string SongSubName { get; set; }

        [JsonProperty("songAuthorName")]
        public string SongAuthorName { get; set; }

        [JsonProperty("levelAuthorName")]
        public string LevelAuthorName { get; set; }

        [JsonProperty("diff")]
        public string Diff { get; set; }

        [JsonProperty("maxScore")]
        public long MaxScoreEx { get; set; }

        [JsonProperty("rank")]
        public long Rank { get; set; }
    }
    }
