using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBeatSaberBot.Api.BeatSaverApi.Models.NewMaps
{
    public partial class BeatSaverMapsModel
    {
        [JsonProperty("map")]
        public Map Map { get; set; }
    }

    public partial class Map
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("uploader")]
        public Uploader Uploader { get; set; }

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

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("lastPublishedAt")]
        public DateTimeOffset LastPublishedAt { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }
    }
}

