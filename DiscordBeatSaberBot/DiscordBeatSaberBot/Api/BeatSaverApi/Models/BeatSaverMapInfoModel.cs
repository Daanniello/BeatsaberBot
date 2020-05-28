using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBeatSaberBot.Models
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class BeatSaverMapInfoModel
    {
        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("stats")]
        public Stats Stats { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("deletedAt")]
        public object DeletedAt { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uploader")]
        public Uploader Uploader { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("uploaded")]
        public DateTimeOffset Uploaded { get; set; }

        [JsonProperty("directDownload")]
        public string DirectDownload { get; set; }

        [JsonProperty("downloadURL")]
        public string DownloadUrl { get; set; }

        [JsonProperty("coverURL")]
        public string CoverUrl { get; set; }
    }

    public partial class Metadata
    {
        [JsonProperty("difficulties")]
        public MetadataDifficulties Difficulties { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("characteristics")]
        public Characteristic[] Characteristics { get; set; }

        [JsonProperty("levelAuthorName")]
        public string LevelAuthorName { get; set; }

        [JsonProperty("songAuthorName")]
        public string SongAuthorName { get; set; }

        [JsonProperty("songName")]
        public string SongName { get; set; }

        [JsonProperty("songSubName")]
        public string SongSubName { get; set; }

        [JsonProperty("bpm")]
        public long Bpm { get; set; }
    }

    public partial class Characteristic
    {
        [JsonProperty("difficulties")]
        public CharacteristicDifficulties Difficulties { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class CharacteristicDifficulties
    {
        [JsonProperty("easy")]
        public object Easy { get; set; }

        [JsonProperty("expert")]
        public object Expert { get; set; }

        [JsonProperty("expertPlus")]
        public ExpertPlus ExpertPlus { get; set; }

        [JsonProperty("hard")]
        public object Hard { get; set; }

        [JsonProperty("normal")]
        public object Normal { get; set; }
    }

    public partial class ExpertPlus
    {
        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }

        [JsonProperty("njs")]
        public long Njs { get; set; }

        [JsonProperty("njsOffset")]
        public long NjsOffset { get; set; }

        [JsonProperty("bombs")]
        public long Bombs { get; set; }

        [JsonProperty("notes")]
        public long Notes { get; set; }

        [JsonProperty("obstacles")]
        public long Obstacles { get; set; }
    }

    public partial class MetadataDifficulties
    {
        [JsonProperty("easy")]
        public bool Easy { get; set; }

        [JsonProperty("expert")]
        public bool Expert { get; set; }

        [JsonProperty("expertPlus")]
        public bool ExpertPlus { get; set; }

        [JsonProperty("hard")]
        public bool Hard { get; set; }

        [JsonProperty("normal")]
        public bool Normal { get; set; }
    }

    public partial class Stats
    {
        [JsonProperty("downloads")]
        public long Downloads { get; set; }

        [JsonProperty("plays")]
        public long Plays { get; set; }

        [JsonProperty("downVotes")]
        public long DownVotes { get; set; }

        [JsonProperty("upVotes")]
        public long UpVotes { get; set; }

        [JsonProperty("heat")]
        public double Heat { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }
    }

    public partial class Uploader
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }
}
