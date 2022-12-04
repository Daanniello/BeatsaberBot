using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBeatSaberBot.Api.BeatSaverApi.Models.v2
{
    public partial class BeatSaverMapSearchModelv2
    {
        [JsonProperty("docs")]
        public List<Maps> Docs { get; set; }
    }

    public partial class Maps
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("uploader")]
        public Curator Uploader { get; set; }

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
        public List<Version> Versions { get; set; }

        [JsonProperty("curator")]
        public Curator Curator { get; set; }

        [JsonProperty("curatedAt")]
        public DateTimeOffset CuratedAt { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("lastPublishedAt")]
        public DateTimeOffset LastPublishedAt { get; set; }

        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Tags { get; set; }
    }

    public partial class Curator
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("avatar")]
        public Uri Avatar { get; set; }


        [JsonProperty("admin")]
        public bool Admin { get; set; }

        [JsonProperty("curator")]
        public bool CuratorCurator { get; set; }

        [JsonProperty("verifiedMapper", NullValueHandling = NullValueHandling.Ignore)]
        public bool? VerifiedMapper { get; set; }

        [JsonProperty("hash", NullValueHandling = NullValueHandling.Ignore)]
        public string Hash { get; set; }
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

        [JsonProperty("upvotes")]
        public long Upvotes { get; set; }

        [JsonProperty("downvotes")]
        public long Downvotes { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("reviews", NullValueHandling = NullValueHandling.Ignore)]
        public long? Reviews { get; set; }
    }

    public partial class Version
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("sageScore", NullValueHandling = NullValueHandling.Ignore)]
        public long? SageScore { get; set; }

        [JsonProperty("diffs")]
        public List<Diff> Diffs { get; set; }

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
        public double Njs { get; set; }

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

        [JsonProperty("maxScore")]
        public long MaxScore { get; set; }
    }
}
