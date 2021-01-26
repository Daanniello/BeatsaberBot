using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBeatSaberBot.Api.ScoreberAPI.Models.ScoreSaberRankedTopRequestsModel
{
    public partial class ScoreSaberRankedTopRequestsModel
    {
        [JsonProperty("requests")]
        public Request[] Requests { get; set; }
    }

    public partial class Request
    {
        [JsonProperty("request")]
        public long RequestRequest { get; set; }

        [JsonProperty("songId")]
        public long SongId { get; set; }

        [JsonProperty("priority")]
        public long Priority { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("levelAuthorName")]
        public string LevelAuthorName { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("rankVotes")]
        public Votes RankVotes { get; set; }

        [JsonProperty("qatVotes")]
        public Votes QatVotes { get; set; }

        [JsonProperty("difficulties")]
        public long Difficulties { get; set; }
    }

    public partial class Votes
    {
        [JsonProperty("upvotes")]
        public long Upvotes { get; set; }

        [JsonProperty("downvotes")]
        public long Downvotes { get; set; }

        [JsonProperty("myVote")]
        public bool MyVote { get; set; }
    }
}
