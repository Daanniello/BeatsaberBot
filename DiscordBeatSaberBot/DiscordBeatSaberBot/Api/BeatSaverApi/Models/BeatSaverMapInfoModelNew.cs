using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBeatSaberBot.Api.BeatSaverApi.Models.New
{
    public partial class BeatSaverMapModelNew
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("uploader")]
        public Uploader Uploader { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("stats")]
        public Stats Stats { get; set; }

        [JsonProperty("uploaded")]
        public DateTimeOffset Uploaded { get; set; }

        [JsonProperty("automapper")]
        public bool Automapper { get; set; }

        [JsonProperty("ranked")]
        public bool Ranked { get; set; }

        [JsonProperty("qualified")]
        public bool Qualified { get; set; }

        [JsonProperty("versions")]
        public Version[] Versions { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("lastPublishedAt")]
        public DateTimeOffset LastPublishedAt { get; set; }
    }

    public partial class Metadata
    {
        [JsonProperty("bpm")]
        public long Bpm { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("songName")]
        public string SongName { get; set; }

        [JsonProperty("songSubName")]
        public string SongSubName { get; set; }

        [JsonProperty("songAuthorName")]
        public string SongAuthorName { get; set; }

        [JsonProperty("levelAuthorName")]
        public string LevelAuthorName { get; set; }
    }

    public partial class Stats
    {
        [JsonProperty("plays")]
        public long Plays { get; set; }

        [JsonProperty("downloads")]
        public long Downloads { get; set; }

        [JsonProperty("upvotes")]
        public long Upvotes { get; set; }

        [JsonProperty("downvotes")]
        public long Downvotes { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }
    }

    public partial class Uploader
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uniqueSet")]
        public bool UniqueSet { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("avatar")]
        public Uri Avatar { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public partial class Version
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("sageScore")]
        public long SageScore { get; set; }

        [JsonProperty("diffs")]
        public Diff[] Diffs { get; set; }

        [JsonProperty("downloadURL")]
        public Uri DownloadUrl { get; set; }

        [JsonProperty("coverURL")]
        public Uri CoverUrl { get; set; }

        [JsonProperty("previewURL")]
        public Uri PreviewUrl { get; set; }
    }

    public partial class Diff
    {
        [JsonProperty("njs")]
        public long Njs { get; set; }

        [JsonProperty("offset")]
        public double Offset { get; set; }

        [JsonProperty("notes")]
        public long Notes { get; set; }

        [JsonProperty("bombs")]
        public long Bombs { get; set; }

        [JsonProperty("obstacles")]
        public long Obstacles { get; set; }

        [JsonProperty("nps")]
        public double Nps { get; set; }

        [JsonProperty("length")]
        public double Length { get; set; }

        [JsonProperty("characteristic")]
        public string Characteristic { get; set; }

        [JsonProperty("difficulty")]
        public string Difficulty { get; set; }

        [JsonProperty("events")]
        public long Events { get; set; }

        [JsonProperty("chroma")]
        public bool Chroma { get; set; }

        [JsonProperty("me")]
        public bool Me { get; set; }

        [JsonProperty("ne")]
        public bool Ne { get; set; }

        [JsonProperty("cinema")]
        public bool Cinema { get; set; }

        [JsonProperty("seconds")]
        public double Seconds { get; set; }

        [JsonProperty("paritySummary")]
        public ParitySummary ParitySummary { get; set; }
        [JsonProperty("maxScore")]
        public long MaxScore { get; set; }

    }

    public partial class ParitySummary
    {
        [JsonProperty("errors")]
        public long Errors { get; set; }

        [JsonProperty("warns")]
        public long Warns { get; set; }

        [JsonProperty("resets")]
        public long Resets { get; set; }
    }
}

