using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace DiscordBeatSaberBot.Api.BeatSaverApi.Models
{
    public partial class MapsBySearchModel
    {
        [JsonProperty("docs")]
        public Doc[] Docs { get; set; }

        [JsonProperty("totalDocs")]
        public long TotalDocs { get; set; }

        [JsonProperty("lastPage")]
        public long LastPage { get; set; }

        [JsonProperty("prevPage")]
        public object PrevPage { get; set; }

        [JsonProperty("nextPage")]
        public long NextPage { get; set; }
    }

    public partial class Doc
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

        [JsonProperty("automapper")]
        public string Automapper { get; set; }

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
        public double Bpm { get; set; }
    }

    public partial class Characteristic
    {
        [JsonProperty("difficulties")]
        public CharacteristicDifficulties Difficulties { get; set; }

       
    }

    public partial class CharacteristicDifficulties
    {
        [JsonProperty("easy")]
        public Easy Easy { get; set; }

        [JsonProperty("expert")]
        public Easy Expert { get; set; }

        [JsonProperty("expertPlus")]
        public Easy ExpertPlus { get; set; }

        [JsonProperty("hard")]
        public Easy Hard { get; set; }

        [JsonProperty("normal")]
        public Easy Normal { get; set; }
    }

    public partial class Easy
    {
        [JsonProperty("duration")]
        public double Duration { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }

        [JsonProperty("njs")]
        public double Njs { get; set; }

        [JsonProperty("njsOffset")]
        public double NjsOffset { get; set; }

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

    public enum Name { Lawless, NoArrows, OneSaber, Standard, The90Degree };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                NameConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class NameConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Name) || t == typeof(Name?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "90Degree":
                    return Name.The90Degree;
                case "Lawless":
                    return Name.Lawless;
                case "NoArrows":
                    return Name.NoArrows;
                case "OneSaber":
                    return Name.OneSaber;
                case "Standard":
                    return Name.Standard;
            }
            throw new Exception("Cannot unmarshal type Name");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Name)untypedValue;
            switch (value)
            {
                case Name.The90Degree:
                    serializer.Serialize(writer, "90Degree");
                    return;
                case Name.Lawless:
                    serializer.Serialize(writer, "Lawless");
                    return;
                case Name.NoArrows:
                    serializer.Serialize(writer, "NoArrows");
                    return;
                case Name.OneSaber:
                    serializer.Serialize(writer, "OneSaber");
                    return;
                case Name.Standard:
                    serializer.Serialize(writer, "Standard");
                    return;
            }
            throw new Exception("Cannot marshal type Name");
        }

        public static readonly NameConverter Singleton = new NameConverter();
    }
}



