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
        private List<ScoresaberLiveFeedModel> _latestPostablePlays = new List<ScoresaberLiveFeedModel>();
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

        private async Task<List<ScoresaberLiveFeedModel>> GetLiveFeedData()
        {
            Console.WriteLine("Getting LiveFeedData...");
            var liveFeedInfo = await ScoresaberAPI.GetLiveFeed();
            if (liveFeedInfo == null) return null;
            return liveFeedInfo;
        }

        private async Task CleanUpLatestPosts()
        {
            if (_latestPostablePlays == null) return;
            if (_latestPostablePlays.Count() > 15)
            {
                Console.WriteLine("Cleaning up plays...");
                _latestPostablePlays.RemoveRange(15, _latestPostablePlays.Count - 15);
            }
        }

        private async Task<List<ScoresaberLiveFeedModel>> GetPostablePlays(List<ScoresaberLiveFeedModel> liveFeedPlays)
        {
            if (liveFeedPlays == null) return null;
            Console.WriteLine("Checking for potential plays...");
            var postablePlays = new List<ScoresaberLiveFeedModel>();
            var scoresaberIds = await DatabaseContext.ExecuteSelectQuery($"Select * from ServerSilverhazeAchievementFeed");
            if (scoresaberIds == null) return null;

            foreach (var play in liveFeedPlays)
            {
                var isInDatabase = false;
                //Checks if the play contains a ID that needs to be posted
                foreach (var id in scoresaberIds)
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

        private async Task<List<ScoresaberLiveFeedModel>> CheckForPostablePlays(List<ScoresaberLiveFeedModel> liveFeedPlays)
        {
            if (liveFeedPlays == null || liveFeedPlays.Count == 0) return null;
            Console.WriteLine($"Checking for postable plays... potential play count: {liveFeedPlays.Count}");
            var postablePlays = new List<ScoresaberLiveFeedModel>();

            foreach (var play in liveFeedPlays)
            {
                var scoresaber = new ScoresaberAPI(play.PlayerId.ToString());
                var recentScores = await scoresaber.GetScoresRecent();
                var topScores = await scoresaber.GetTopScores();

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

        private void StoreLatestPlays(List<ScoresaberLiveFeedModel> postablePlays)
        {
            if (postablePlays == null) return;
            Console.WriteLine($"Storing Latest postable plays... {postablePlays.Count} plays");
            _latestPostablePlays = postablePlays;
        }

        private async Task<List<ScoresaberLiveFeedModel>> CheckForDoublePost(List<ScoresaberLiveFeedModel> postablePlays)
        {
            if (postablePlays == null || postablePlays.Count == 0) return null;
            Console.WriteLine("Removing doubles from plays...");

            var playsToPost = new List<ScoresaberLiveFeedModel>();

            foreach (var play in postablePlays)
            {
                if (_latestPostablePlays == null) { }
                else if (_latestPostablePlays.Where(x => x.PlayerId == play.PlayerId && x.LeaderboardId == play.LeaderboardId) != null)
                {
                    _logger.Log(Logger.LogCode.debug, "Skipped a play " + play.LeaderboardId);
                    continue;
                }
                playsToPost.Add(play);
            }

            var latestplaysString = "";
            foreach (var latest in _latestPostablePlays) latestplaysString += latest.LeaderboardId.ToString() + "\n";
            var postablePlaysString = "";
            foreach (var latest in postablePlays) postablePlaysString += latest.LeaderboardId.ToString() + "\n";
            _logger.Log(Logger.LogCode.debug, $"Latestplays(old)\n{latestplaysString} \n\n postablePlays(now) \n{postablePlaysString}");

            return playsToPost;
        }

        private async void PostPlaysInDiscord(List<ScoresaberLiveFeedModel> playsToPost)
        {
            if (playsToPost == null || playsToPost.Count == 0) return;
            foreach (var play in playsToPost)
            {
                var channelsToPostIn = await GetGuildAndChannelIDToPostIn(play);
                var embed = await CreateEmbedBuilder(play);
                foreach (var GuildAndChannel in channelsToPostIn)
                {
                    _logger.Log(Logger.LogCode.debug, "Posting a in an achievementfeed... \n " +
                        $"{play.Name} {play.LeaderboardId} {play.PlayerId}");
                    var textChannel = _discord.GetGuild(GuildAndChannel[0]).GetTextChannel(GuildAndChannel[1]);
                    await textChannel.SendMessageAsync("", false, embed.Build());
                }
            }

        }

        private async Task<Discord.EmbedBuilder> CreateEmbedBuilder(ScoresaberLiveFeedModel play)
        {
            var scoresaber = new ScoresaberAPI(play.PlayerId.ToString());
            var recentScores = await scoresaber.GetScoresRecent();
            var topScores = await scoresaber.GetTopScores();
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

        private async Task<List<ulong[]>> GetGuildAndChannelIDToPostIn(ScoresaberLiveFeedModel play)
        {
            var channelsToPostIn = new List<ulong[]>();
            if (play.Flag.ToLower() == "nl.png") channelsToPostIn.Add(new ulong[] { 505485680344956928, 767552138879434772 });

            var scoresaberIds = await DatabaseContext.ExecuteSelectQuery($"Select * from ServerSilverhazeAchievementFeed");
            var isInDatabase = false;
            //Checks if the play contains a ID that needs to be posted
            foreach (var id in scoresaberIds)
            {
                if (play.PlayerId.ToString() != id.First().ToString()) continue;
                isInDatabase = true;
            }
            if (isInDatabase) channelsToPostIn.Add(new ulong[] { 627156958880858113, 768520962206990396 });

            return channelsToPostIn;
        }
    }
}
