using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace DiscordBeatSaberBot.Models.ScoreberAPI
{

    public partial class ScoreSaberLiveFeedModel
    {
        [JsonProperty("leaderboardId")]
        public long LeaderboardId { get; set; }

        [JsonProperty("score")]
        public string Score { get; set; }

        [JsonProperty("playerId")]
        public ulong PlayerId { get; set; }

        [JsonProperty("pp")]
        public string Pp { get; set; }

        [JsonProperty("timeset")]
        public string Timeset { get; set; }

        [JsonProperty("steam")]
        public bool Steam { get; set; }

        [JsonProperty("percentage")]
        public double Percentage { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("flag")]
        public string Flag { get; set; }

        [JsonProperty("info")]
        public Info Info { get; set; }

        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Image { get; set; }
    }

    public partial class Info
    {
        [JsonProperty("uid")]
        public long Uid { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("subname")]
        public string Subname { get; set; }

        [JsonProperty("difficulty")]
        public string Difficulty { get; set; }

        [JsonProperty("author")]
        public object Author { get; set; }

        [JsonProperty("internalId")]
        public string InternalId { get; set; }

        [JsonProperty("maxScore")]
        public long MaxScore { get; set; }

        [JsonProperty("pp")]
        public double Pp { get; set; }

        [JsonProperty("ranked")]
        public Ranked Ranked { get; set; }

        [JsonProperty("image")]
        public Uri Image { get; set; }

        [JsonProperty("graph")]
        public object Graph { get; set; }

        [JsonProperty("scores")]
        public string Scores { get; set; }

        [JsonProperty("scoresAll")]
        public object ScoresAll { get; set; }

        [JsonProperty("levelAuthorName")]
        public string LevelAuthorName { get; set; }

        [JsonProperty("stars")]
        public Stars Stars { get; set; }
    }

    public enum Ranked { Ranked, Unranked };

    public partial struct Stars
    {
        public double? Double;
        public string String;

        public static implicit operator Stars(double Double) => new Stars { Double = Double };
        public static implicit operator Stars(string String) => new Stars { String = String };
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                RankedConverter.Singleton,
                StarsConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class RankedConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Ranked) || t == typeof(Ranked?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Ranked":
                    return Ranked.Ranked;
                case "Unranked":
                    return Ranked.Unranked;
            }
            throw new Exception("Cannot unmarshal type Ranked");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Ranked)untypedValue;
            switch (value)
            {
                case Ranked.Ranked:
                    serializer.Serialize(writer, "Ranked");
                    return;
                case Ranked.Unranked:
                    serializer.Serialize(writer, "Unranked");
                    return;
            }
            throw new Exception("Cannot marshal type Ranked");
        }

        public static readonly RankedConverter Singleton = new RankedConverter();
    }

    internal class StarsConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Stars) || t == typeof(Stars?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                    var doubleValue = serializer.Deserialize<double>(reader);
                    return new Stars { Double = doubleValue };
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new Stars { String = stringValue };
            }
            throw new Exception("Cannot unmarshal type Stars");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (Stars)untypedValue;
            if (value.Double != null)
            {
                serializer.Serialize(writer, value.Double.Value);
                return;
            }
            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }
            throw new Exception("Cannot marshal type Stars");
        }

        public static readonly StarsConverter Singleton = new StarsConverter();
    }

}
