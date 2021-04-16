using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBeatSaberBot.Api.ScoreberAPI.Models
{
    public partial class QualifiedMapsModel
    {
        [JsonProperty("songs")]
        public Song[] Songs { get; set; }
    }

    public partial class Song
    {
        [JsonProperty("uid")]
        public long Uid { get; set; }

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

        [JsonProperty("bpm")]
        public long Bpm { get; set; }

        [JsonProperty("scores_day")]
        public long ScoresDay { get; set; }

        [JsonProperty("ranked")]
        public long Ranked { get; set; }

        [JsonProperty("stars")]
        public long Stars { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }
    }
}
