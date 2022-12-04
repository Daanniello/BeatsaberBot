using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBeatSaberBot.Models
{
    public class PlaylistMapsModel
    {
        [JsonProperty("playlistTitle")]
        public string PlaylistTitle { get; set; }

        [JsonProperty("playlistAuthor")]
        public string PlaylistAuthor { get; set; }

        [JsonProperty("playlistDescription")]
        public string PlaylistDescription { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("customData")]
        public CustomData CustomData { get; set; }

        [JsonProperty("songs")]
        public List<Song> Songs { get; set; }
    }

    public partial class CustomData
    {
        [JsonProperty("syncURL")]
        public Uri SyncUrl { get; set; }
    }

    public partial class Song
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("songName")]
        public string SongName { get; set; }
    }
}
