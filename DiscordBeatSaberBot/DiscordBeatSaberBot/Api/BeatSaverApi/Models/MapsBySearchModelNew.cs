using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DiscordBeatSaberBot.Api.BeatSaverApi.Models.New2
{

        public partial class MapsBySearchModel
        {
            [JsonProperty("docs")]
            public Doc[] Docs { get; set; }
        }

        public partial class Doc
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
            public double Bpm { get; set; }

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
            public TypeEnum Type { get; set; }
        }

        public partial class Version
        {
            [JsonProperty("hash")]
            public string Hash { get; set; }

            [JsonProperty("key")]
            public string Key { get; set; }

            [JsonProperty("state")]
            public State State { get; set; }

            [JsonProperty("createdAt")]
            public DateTimeOffset CreatedAt { get; set; }

            [JsonProperty("sageScore", NullValueHandling = NullValueHandling.Ignore)]
            public long? SageScore { get; set; }

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
            public long Offset { get; set; }

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
            public Characteristic Characteristic { get; set; }

            [JsonProperty("difficulty")]
            public Difficulty Difficulty { get; set; }

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

        public enum TypeEnum { Discord, Simple };

        public enum Characteristic { Standard };

        public enum Difficulty { Easy, Expert, ExpertPlus, Hard, Normal };

        public enum State { Published };

        internal static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters =
            {
                TypeEnumConverter.Singleton,
                CharacteristicConverter.Singleton,
                DifficultyConverter.Singleton,
                StateConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
            };
        }

        internal class TypeEnumConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(TypeEnum) || t == typeof(TypeEnum?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                switch (value)
                {
                    case "DISCORD":
                        return TypeEnum.Discord;
                    case "SIMPLE":
                        return TypeEnum.Simple;
                }
                throw new Exception("Cannot unmarshal type TypeEnum");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (TypeEnum)untypedValue;
                switch (value)
                {
                    case TypeEnum.Discord:
                        serializer.Serialize(writer, "DISCORD");
                        return;
                    case TypeEnum.Simple:
                        serializer.Serialize(writer, "SIMPLE");
                        return;
                }
                throw new Exception("Cannot marshal type TypeEnum");
            }

            public static readonly TypeEnumConverter Singleton = new TypeEnumConverter();
        }

        internal class CharacteristicConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(Characteristic) || t == typeof(Characteristic?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                if (value == "Standard")
                {
                    return Characteristic.Standard;
                }
                throw new Exception("Cannot unmarshal type Characteristic");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (Characteristic)untypedValue;
                if (value == Characteristic.Standard)
                {
                    serializer.Serialize(writer, "Standard");
                    return;
                }
                throw new Exception("Cannot marshal type Characteristic");
            }

            public static readonly CharacteristicConverter Singleton = new CharacteristicConverter();
        }

        internal class DifficultyConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(Difficulty) || t == typeof(Difficulty?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                switch (value)
                {
                    case "Easy":
                        return Difficulty.Easy;
                    case "Expert":
                        return Difficulty.Expert;
                    case "ExpertPlus":
                        return Difficulty.ExpertPlus;
                    case "Hard":
                        return Difficulty.Hard;
                    case "Normal":
                        return Difficulty.Normal;
                }
                throw new Exception("Cannot unmarshal type Difficulty");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (Difficulty)untypedValue;
                switch (value)
                {
                    case Difficulty.Easy:
                        serializer.Serialize(writer, "Easy");
                        return;
                    case Difficulty.Expert:
                        serializer.Serialize(writer, "Expert");
                        return;
                    case Difficulty.ExpertPlus:
                        serializer.Serialize(writer, "ExpertPlus");
                        return;
                    case Difficulty.Hard:
                        serializer.Serialize(writer, "Hard");
                        return;
                    case Difficulty.Normal:
                        serializer.Serialize(writer, "Normal");
                        return;
                }
                throw new Exception("Cannot marshal type Difficulty");
            }

            public static readonly DifficultyConverter Singleton = new DifficultyConverter();
        }

        internal class StateConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(State) || t == typeof(State?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                if (value == "Published")
                {
                    return State.Published;
                }
                throw new Exception("Cannot unmarshal type State");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (State)untypedValue;
                if (value == State.Published)
                {
                    serializer.Serialize(writer, "Published");
                    return;
                }
                throw new Exception("Cannot marshal type State");
            }

            public static readonly StateConverter Singleton = new StateConverter();
        }
    }

