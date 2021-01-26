using Discord;
using DiscordBeatSaberBot.Models.ScoreberAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    class DiscordScoreSaberLiveFeed
    {
        private List<ScoreSaberLiveFeedModel> _latestPostablePlays = new List<ScoreSaberLiveFeedModel>();
        private Discord.WebSocket.DiscordSocketClient _discord;
        private ILogger _logger;

        private int _rankForPostableRanked = 100;
        private int _rankForPostableUnranked = 1;

        public DiscordScoreSaberLiveFeed(Discord.WebSocket.DiscordSocketClient discord)
        {
            _discord = discord;
            _logger = new Logger(discord);
        }

        public async Task Start()
        {
            var liveFeedData = await GetLiveFeedData();
            var postablePlays = await GetPostablePlays(liveFeedData);
            var playsToPost = await CheckForDoublePost(postablePlays);
            StoreLatestPlays(playsToPost);
            PostPlaysInDiscord(playsToPost);
            await CleanUpLatestPosts();
        }

        private async Task<List<ScoreSaberLiveFeedModel>> GetLiveFeedData()
        {
            Console.WriteLine("Getting LiveFeedData...");
            var liveFeedInfo = await ScoreSaberAPI.GetLiveFeed();
            if (liveFeedInfo == null) return null;
            return liveFeedInfo;
        }

        private async Task CleanUpLatestPosts()
        {
            if (_latestPostablePlays == null) return;
            if (_latestPostablePlays.Count() > 15)
            {
                Console.WriteLine("Cleaning up plays...");
                _latestPostablePlays.RemoveRange(0, 15);
            }
        }

        private async Task<List<ScoreSaberLiveFeedModel>> GetPostablePlays(List<ScoreSaberLiveFeedModel> liveFeedPlays)
        {
            if (liveFeedPlays == null) return null;
            Console.WriteLine("Checking for potential plays...");
            var postablePlays = new List<ScoreSaberLiveFeedModel>();
            var ScoreSaberIds = await DatabaseContext.ExecuteSelectQuery($"Select * from ServerSilverhazeAchievementFeed");
            if (ScoreSaberIds == null) return null;

            foreach (var play in liveFeedPlays)
            {
                var isInDatabase = false;
                //Checks if the play contains a ID that needs to be posted
                foreach (var id in ScoreSaberIds)
                {
                    if (play.PlayerId.ToString() != id.First().ToString()) continue;
                    isInDatabase = true;
                }
                if (play.Flag.ToLower() == "nl.png" || isInDatabase)
                {
                    postablePlays.Add(play);
                }
            }
            return await CheckForPostablePlays(postablePlays);
        }

        private async Task<List<ScoreSaberLiveFeedModel>> CheckForPostablePlays(List<ScoreSaberLiveFeedModel> liveFeedPlays)
        {
            if (liveFeedPlays == null || liveFeedPlays.Count == 0) return null;
            Console.WriteLine($"Checking for postable plays... potential play count: {liveFeedPlays.Count}");
            var postablePlays = new List<ScoreSaberLiveFeedModel>();

            foreach (var play in liveFeedPlays)
            {
                var ScoreSaber = new ScoreSaberAPI(play.PlayerId.ToString());
                var recentScores = await ScoreSaber.GetScoresRecent();
                var topScores = await ScoreSaber.GetTopScores();

                //Checks if the player should be called out
                //Adds the player to the postlist if qualified                               
                if (recentScores.Scores[0].Rank <= _rankForPostableRanked && play.Info.Ranked == Ranked.Ranked)
                {
                    postablePlays.Add(play);
                }
                if (recentScores.Scores[0].Rank <= _rankForPostableUnranked && play.Info.Ranked == Ranked.Unranked)
                {
                    postablePlays.Add(play);
                }
                if (recentScores.Scores.First().LeaderboardId == topScores.Scores.First().LeaderboardId && play.Info.Ranked == Ranked.Ranked)
                {
                    postablePlays.Add(play);
                }
            }
            return postablePlays;
        }

        private void StoreLatestPlays(List<ScoreSaberLiveFeedModel> postablePlays)
        {
            if (postablePlays == null) return;
            Console.WriteLine($"Storing Latest postable plays... {postablePlays.Count} plays");
            _latestPostablePlays = postablePlays;
        }

        private async Task<List<ScoreSaberLiveFeedModel>> CheckForDoublePost(List<ScoreSaberLiveFeedModel> postablePlays)
        {
            if (postablePlays == null || postablePlays.Count == 0) return null;
            Console.WriteLine("Removing doubles from plays...");

            var playsToPost = new List<ScoreSaberLiveFeedModel>();

            foreach (var play in postablePlays)
            {
                if (_latestPostablePlays == null)
                {
                    playsToPost.Add(play);
                    continue;
                }
                var commonPlays = _latestPostablePlays.Where(x => x.PlayerId == play.PlayerId && x.LeaderboardId == play.LeaderboardId); 
                if (commonPlays.Count() > 0)
                {
                    continue;
                }
                else
                {
                    playsToPost.Add(play);
                }
            }

            return playsToPost;
        }

        private async void PostPlaysInDiscord(List<ScoreSaberLiveFeedModel> playsToPost)
        {
            if (playsToPost == null || playsToPost.Count == 0) return;
            foreach (var play in playsToPost)
            {
                var channelsToPostIn = await GetGuildAndChannelIDToPostIn(play);
                var embed = await CreateEmbedBuilder(play);
                foreach (var GuildAndChannel in channelsToPostIn)
                {
                    var textChannel = _discord.GetGuild(GuildAndChannel[0]).GetTextChannel(GuildAndChannel[1]);
                    await textChannel.SendMessageAsync("", false, embed.Build());
                }
            }

        }

        private async Task<Discord.EmbedBuilder> CreateEmbedBuilder(ScoreSaberLiveFeedModel play)
        {
            var ScoreSaber = new ScoreSaberAPI(play.PlayerId.ToString());
            var recentScores = await ScoreSaber.GetScoresRecent();
            var topScores = await ScoreSaber.GetTopScores();
            var songInfo = recentScores.Scores.First();

            var color = Color.Blue;
            if (songInfo.Rank == 1) color = Color.Gold;
            if (songInfo.Rank == 2) color = Color.LighterGrey;
            if (songInfo.Rank == 3) color = Color.DarkRed;


            var img = "https://scoresaber.com/imports/images/oculus.png";
            if (play.Image != null)
            {
                if (!play.Image.OriginalString.Contains("oculus")) img = play.Image.OriginalString.Replace(".jpg", "_full.jpg");
            }

            var title = $":flag_{play.Flag.Replace(".png", "")}: {play.Name} just got a #{songInfo.Rank} play!";
            if (recentScores.Scores.First().LeaderboardId == topScores.Scores.First().LeaderboardId && play.Info.Ranked == Ranked.Ranked)
            {
                title = $":flag_{play.Flag.Replace(".png", "")}: Congrats on your new top play {play.Name}! with rank #{songInfo.Rank}";
            }

            var acc = "";
            if (songInfo.MaxScoreEx != 0)
            {
                double percentage = Convert.ToDouble(songInfo.UScore) / Convert.ToDouble(songInfo.MaxScoreEx) * 100;
                acc = $"**Accuracy:** { Math.Round(percentage, 2)}% \n";
            }

            var embed = new EmbedBuilder()
            {
                Color = color,
                Title = title,
                Description = $"**Song name:** {play.Info.Title} \n" +
                $"**Difficulty**: {songInfo.GetDifficulty()}\n" +
                $"**Ranked type:** {play.Info.Ranked}\n" +
                $"**PP:** {play.Pp}\n" +
                acc +
                $"**Total plays:** {play.Info.Scores}",
                ThumbnailUrl = $"{img}",
                ImageUrl = $"{play.Info.Image}",
                Url = $"https://scoresaber.com/u/{play.PlayerId}",

            };
            return embed;
        }

        private async Task<List<ulong[]>> GetGuildAndChannelIDToPostIn(ScoreSaberLiveFeedModel play)
        {
            var channelsToPostIn = new List<ulong[]>();
            if (play.Flag.ToLower() == "nl.png") channelsToPostIn.Add(new ulong[] { 505485680344956928, 767552138879434772 });

            var ScoreSaberIds = await DatabaseContext.ExecuteSelectQuery($"Select * from ServerSilverhazeAchievementFeed");
            var isInDatabase = false;

            //Checks if the play contains a ID that needs to be posted
            foreach (var id in ScoreSaberIds)
            {
                if (play.PlayerId.ToString() != id.First().ToString()) continue;
                isInDatabase = true;
            }
            if (isInDatabase) channelsToPostIn.Add(new ulong[] { 627156958880858113, 768520962206990396 });

            return channelsToPostIn;
        }
    }
}
