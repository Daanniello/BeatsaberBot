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
    }
}

