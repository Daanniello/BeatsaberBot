using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBeatSaberBot.Api.ScoreberAPI.Models.ScoresaberRankedRequestModel
{
    public partial class ScoresaberRankedRequestModel
    {
        [JsonProperty("request")]
        public Request Request { get; set; }
    }

    public partial class Request
    {
        [JsonProperty("info")]
        public Info Info { get; set; }

        [JsonProperty("difficulties")]
        public Difficulty[] Difficulties { get; set; }
    }

    public partial class Difficulty
    {
        [JsonProperty("request")]
        public long Request { get; set; }

        [JsonProperty("uid")]
        public long Uid { get; set; }

        [JsonProperty("max_pp")]
        public long MaxPp { get; set; }

        [JsonProperty("difficulty")]
        public long DifficultyDifficulty { get; set; }
    }

    public partial class Info
    {
        [JsonProperty("songId")]
        public long SongId { get; set; }

        [JsonProperty("requestType")]
        public long RequestType { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("approved")]
        public long Approved { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("levelAuthorName")]
        public string LevelAuthorName { get; set; }

        [JsonProperty("difficulty")]
        public long Difficulty { get; set; }

        [JsonProperty("rankVotes")]
        public Votes RankVotes { get; set; }

        [JsonProperty("qatVotes")]
        public Votes QatVotes { get; set; }

        [JsonProperty("rankComments")]
        public object[] RankComments { get; set; }

        [JsonProperty("qatComments")]
        public object[] QatComments { get; set; }
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
