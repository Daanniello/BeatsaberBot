using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DiscordBeatSaberBot.Api.BeatSaviourApi.Models
{
    public class BeatSaviourLivedataModel
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("songDataType")]
        public long SongDataType { get; set; }

        [JsonProperty("playerID")]
        public string PlayerId { get; set; }

        [JsonProperty("songID")]
        public string SongId { get; set; }

        [JsonProperty("songDifficulty")]
        public SongDifficulty SongDifficulty { get; set; }

        [JsonProperty("songName")]
        public string SongName { get; set; }

        [JsonProperty("songArtist")]
        public string SongArtist { get; set; }

        [JsonProperty("songMapper")]
        public string SongMapper { get; set; }

        [JsonProperty("songSpeed")]
        public long SongSpeed { get; set; }

        [JsonProperty("songStartTime")]
        public long SongStartTime { get; set; }

        [JsonProperty("songDuration")]
        public double SongDuration { get; set; }

        [JsonProperty("trackers")]
        public Trackers Trackers { get; set; }

        [JsonProperty("deepTrackers")]
        public object DeepTrackers { get; set; }

        [JsonProperty("timeSet")]
        public DateTimeOffset TimeSet { get; set; }
    }

    public partial class Trackers
    {
        [JsonProperty("hitTracker")]
        public HitTracker HitTracker { get; set; }

        [JsonProperty("accuracyTracker")]
        public AccuracyTracker AccuracyTracker { get; set; }

        [JsonProperty("scoreTracker")]
        public ScoreTracker ScoreTracker { get; set; }

        [JsonProperty("winTracker")]
        public WinTracker WinTracker { get; set; }

        [JsonProperty("distanceTracker")]
        public DistanceTracker DistanceTracker { get; set; }

        [JsonProperty("scoreGraphTracker")]
        public ScoreGraphTracker ScoreGraphTracker { get; set; }
    }

    public partial class AccuracyTracker
    {
        [JsonProperty("accRight")]
        public double AccRight { get; set; }

        [JsonProperty("accLeft")]
        public double AccLeft { get; set; }

        [JsonProperty("averageAcc")]
        public double AverageAcc { get; set; }

        [JsonProperty("leftSpeed")]
        public double LeftSpeed { get; set; }

        [JsonProperty("rightSpeed")]
        public double RightSpeed { get; set; }

        [JsonProperty("averageSpeed")]
        public double AverageSpeed { get; set; }

        [JsonProperty("leftAverageCut")]
        public List<double> LeftAverageCut { get; set; }

        [JsonProperty("rightAverageCut")]
        public List<double> RightAverageCut { get; set; }

        [JsonProperty("averageCut")]
        public List<double> AverageCut { get; set; }

        [JsonProperty("gridAcc")]
        public List<GridAccElement> GridAcc { get; set; }

        [JsonProperty("gridCut")]
        public List<long> GridCut { get; set; }

        [JsonProperty("leftHighestSpeed")]
        public double LeftHighestSpeed { get; set; }

        [JsonProperty("RightHighestSpeed")]
        public double RightHighestSpeed { get; set; }

        [JsonProperty("averageTimeDependence")]
        public double AverageTimeDependence { get; set; }

        [JsonProperty("rightTimeDependence")]
        public double RightTimeDependence { get; set; }

        [JsonProperty("leftTimeDependence")]
        public double rightTimeDependence { get; set; }

        [JsonProperty("averagePostswing")]
        public double AveragePostswing { get; set; }

        [JsonProperty("rightPostswing")]
        public double RightPostswing { get; set; }

        [JsonProperty("leftPostswing")]
        public double LeftPostswing { get; set; }

        [JsonProperty("averagePreswing")]
        public double AveragePreswing { get; set; }

        [JsonProperty("rightPreswing")]
        public double RightPreswing { get; set; }
        [JsonProperty("leftPreswing")]
        public double LeftPreswing { get; set; }
    }

    public partial class DistanceTracker
    {
        [JsonProperty("rightSaber")]
        public double RightSaber { get; set; }

        [JsonProperty("leftSaber")]
        public double LeftSaber { get; set; }

        [JsonProperty("rightHand")]
        public double RightHand { get; set; }

        [JsonProperty("leftHand")]
        public double LeftHand { get; set; }
    }

    public partial class HitTracker
    {
        [JsonProperty("leftNoteHit")]
        public long LeftNoteHit { get; set; }

        [JsonProperty("rightNoteHit")]
        public long RightNoteHit { get; set; }

        [JsonProperty("bombHit")]
        public long BombHit { get; set; }

        [JsonProperty("miss")]
        public long Miss { get; set; }

        [JsonProperty("maxCombo")]
        public long MaxCombo { get; set; }
    }

    public partial class ScoreGraphTracker
    {
        [JsonProperty("graph")]
        public Dictionary<string, double> Graph { get; set; }
    }

    public partial class ScoreTracker
    {
        [JsonProperty("rawScore")]
        public long RawScore { get; set; }

        [JsonProperty("score")]
        public long Score { get; set; }

        [JsonProperty("personalBest")]
        public long PersonalBest { get; set; }

        [JsonProperty("rawRatio")]
        public double RawRatio { get; set; }

        [JsonProperty("modifiedRatio")]
        public double ModifiedRatio { get; set; }

        [JsonProperty("personalBestRawRatio")]
        public double PersonalBestRawRatio { get; set; }

        [JsonProperty("personalBestModifiedRatio")]
        public double PersonalBestModifiedRatio { get; set; }

        [JsonProperty("modifiersMultiplier")]
        public long ModifiersMultiplier { get; set; }

        [JsonProperty("modifiers")]
        public List<object> Modifiers { get; set; }
    }

    public partial class WinTracker
    {
        [JsonProperty("won")]
        public bool Won { get; set; }

        [JsonProperty("rank")]
        public Rank Rank { get; set; }

        [JsonProperty("endTime")]
        public double EndTime { get; set; }

        [JsonProperty("nbOfPause")]
        public long NbOfPause { get; set; }
    }

    public enum SongDifficulty { Easy, normal, Expert, Expertplus, Hard };

    public enum GridAccEnum { NaN };

    public enum Rank { A, B, C, D, E, S, Ss, Sss };

    public partial struct GridAccElement
    {
        public double? Double;
        public string? String;

        public static implicit operator GridAccElement(double Double) => new GridAccElement { Double = Double };
        public static implicit operator GridAccElement(string Enum) => new GridAccElement { String = Enum };
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                SongDifficultyConverter.Singleton,
                GridAccElementConverter.Singleton,
                GridAccEnumConverter.Singleton,
                RankConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class SongDifficultyConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(SongDifficulty) || t == typeof(SongDifficulty?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "easy":
                    return SongDifficulty.Easy;
                case "normal":
                    return SongDifficulty.normal;
                case "expert":
                    return SongDifficulty.Expert;
                case "expertplus":
                    return SongDifficulty.Expertplus;
                case "hard":
                    return SongDifficulty.Hard;
            }
            throw new Exception("Cannot unmarshal type SongDifficulty");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (SongDifficulty)untypedValue;
            switch (value)
            {
                case SongDifficulty.Easy:
                    serializer.Serialize(writer, "easy");
                    return;
                case SongDifficulty.normal:
                    serializer.Serialize(writer, "normal");
                    return;
                case SongDifficulty.Expert:
                    serializer.Serialize(writer, "expert");
                    return;
                case SongDifficulty.Expertplus:
                    serializer.Serialize(writer, "expertplus");
                    return;
                case SongDifficulty.Hard:
                    serializer.Serialize(writer, "hard");
                    return;
            }
            throw new Exception("Cannot marshal type SongDifficulty");
        }

        public static readonly SongDifficultyConverter Singleton = new SongDifficultyConverter();
    }

    internal class GridAccElementConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(GridAccElement) || t == typeof(GridAccElement?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                    var doubleValue = serializer.Deserialize<double>(reader);
                    return new GridAccElement { Double = doubleValue };
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    if (stringValue == "NaN")
                    {
                        return new GridAccElement { String = "NaN" };
                    }
                    break;
            }
            throw new Exception("Cannot unmarshal type GridAccElement");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (GridAccElement)untypedValue;
            if (value.Double != null)
            {
                serializer.Serialize(writer, value.Double.Value);
                return;
            }
            if (value.String != null)
            {
                if (value.String == "NaN")
                {
                    serializer.Serialize(writer, "NaN");
                    return;
                }
            }
            throw new Exception("Cannot marshal type GridAccElement");
        }

        public static readonly GridAccElementConverter Singleton = new GridAccElementConverter();
    }

    internal class GridAccEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(GridAccEnum) || t == typeof(GridAccEnum?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "NaN")
            {
                return GridAccEnum.NaN;
            }
            throw new Exception("Cannot unmarshal type GridAccEnum");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (GridAccEnum)untypedValue;
            if (value == GridAccEnum.NaN)
            {
                serializer.Serialize(writer, "NaN");
                return;
            }
            throw new Exception("Cannot marshal type GridAccEnum");
        }

        public static readonly GridAccEnumConverter Singleton = new GridAccEnumConverter();
    }

    internal class RankConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Rank) || t == typeof(Rank?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "A":
                    return Rank.A;
                case "B":
                    return Rank.B;
                case "C":
                    return Rank.C;
                case "D":
                    return Rank.D;
                case "E":
                    return Rank.E;
                case "S":
                    return Rank.S;
                case "SS":
                    return Rank.Ss;
                case "SSS":
                    return Rank.Sss;
            }
            throw new Exception("Cannot unmarshal type Rank");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Rank)untypedValue;
            switch (value)
            {
                case Rank.A:
                    serializer.Serialize(writer, "A");
                    return;
                case Rank.B:
                    serializer.Serialize(writer, "B");
                    return;
                case Rank.C:
                    serializer.Serialize(writer, "C");
                    return;
                case Rank.D:
                    serializer.Serialize(writer, "D");
                    return;
                case Rank.E:
                    serializer.Serialize(writer, "E");
                    return;
                case Rank.S:
                    serializer.Serialize(writer, "S");
                    return;
                case Rank.Ss:
                    serializer.Serialize(writer, "SS");
                    return;
                case Rank.Sss:
                    serializer.Serialize(writer, "SSS");
                    return;
            }
            throw new Exception("Cannot marshal type Rank");
        }

        public static readonly RankConverter Singleton = new RankConverter();
    }
}
