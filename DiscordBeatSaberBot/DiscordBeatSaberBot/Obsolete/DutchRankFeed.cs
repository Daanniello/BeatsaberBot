using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Discord;
using Discord.WebSocket;
using DiscordBeatSaberBot.Models.ScoreberAPI;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace DiscordBeatSaberBot
{
    
    public static class DutchRankFeed
    {
        private static DiscordSocketClient _discord;
        private static List<Dictionary<ulong, long>> recentPlays = new List<Dictionary<ulong, long>>();
        private static List<Dictionary<ulong, long>> recentPlaysSoonRemoved = new List<Dictionary<ulong, long>>();

        public static async Task<Dictionary<double, Dictionary<ScoreSaberLiveFeedModel, ScoreSaberSongsModel>>> GetScoreSaberLiveFeed(DiscordSocketClient discord)
        {

            var rankRanked = 100;
            var rankUnranked = 1;

            var liveFeedInfo = await ScoreSaberAPI.GetLiveFeed();
            if (liveFeedInfo == null) return null;

            var playerListToPost = new List<PlayerListToPost>();

            var ScoreSaberIds = await DatabaseContext.ExecuteSelectQuery($"Select * from ServerSilverhazeAchievementFeed");
            if (ScoreSaberIds == null) return null;

            try
            {
                foreach (var playerData in liveFeedInfo)
                {
                    var silverhazesChannel = false;
                    foreach (var id in ScoreSaberIds)
                    {
                        if (id.First().ToString() == playerData.PlayerId.ToString()) silverhazesChannel = true;
                    }

                    if (playerData.Flag.ToLower() == "nl.png" || silverhazesChannel)
                    {
                        var ScoreSaber = new ScoreSaberAPI(playerData.PlayerId.ToString());
                        var recentScores = await ScoreSaber.GetScoresRecent();
                        var topScores = await ScoreSaber.GetTopScores();

                        //Check if player should be called out again or if its the first time

                        bool shouldBeSkipped = false;
                        foreach (var d in recentPlays)
                        {
                            //Checks if the play is not in recentplays (if yes, it means that its already posted)
                            //true -> skip
                            //false -> add to list to not post again 
                            if(d.Keys.First() == playerData.PlayerId && d.Values.First() == playerData.LeaderboardId)
                            {
                                var t = new Dictionary<ulong, long>();
                                t.Add(playerData.PlayerId, playerData.LeaderboardId);
                                recentPlaysSoonRemoved.Add(t);
                                shouldBeSkipped = true;
                            }
                        }
                        if(shouldBeSkipped) continue;
                  

                        //Checks if the player should be called out
                        //Adds the player to the post list if qualified
                        if (recentScores.Scores[0].Rank <= rankRanked && playerData.Info.Ranked == Ranked.Ranked)
                        {                      
                            var player = new PlayerListToPost() { PlayerId = playerData.PlayerId, ScoreSaberLive = playerData, ScoreSaberSongsData = recentScores };
                            playerListToPost.Add(player);
                        }
                        if (recentScores.Scores[0].Rank <= rankUnranked && playerData.Info.Ranked == Ranked.Unranked)
                        {
                            var player = new PlayerListToPost() { PlayerId = playerData.PlayerId, ScoreSaberLive = playerData, ScoreSaberSongsData = recentScores };
                            playerListToPost.Add(player);
                        }
                        if (recentScores.Scores.First().LeaderboardId == topScores.Scores.First().LeaderboardId && playerData.Info.Ranked == Ranked.Ranked)
                        {
                            var player = new PlayerListToPost() { PlayerId = playerData.PlayerId, ScoreSaberLive = playerData, ScoreSaberSongsData = recentScores, IsTopPlay = true };
                            playerListToPost.Add(player);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }           
            
            if(playerListToPost.Count != 0)await SendPostInAchievementFeed(playerListToPost, discord, liveFeedInfo, ScoreSaberIds);
            if (recentPlays.Count != 0) Console.WriteLine($"Recentplays count: {recentPlays.Count}");
            if (playerListToPost.Count != 0) Console.WriteLine($"PlayerListToPost count: {playerListToPost.Count}");
            if (recentPlaysSoonRemoved.Count != 0) Console.WriteLine($"RecentplaysSoonRemoved count: {recentPlaysSoonRemoved.Count}");

            return null;
        }


        private static async Task SendPostInAchievementFeed(List<PlayerListToPost> playerListToPost, DiscordSocketClient discord, List<ScoreSaberLiveFeedModel> liveFeedInfo, List<List<object>> ScoreSaberIds)
        {
            if (playerListToPost.Count != 0)
            {
                //Goes through every post and send it to the discord channel 
                foreach (var playerData in playerListToPost)
                {
                    var playerInfo = playerData.ScoreSaberLive;
                    var songInfo = playerData.ScoreSaberSongsData.Scores.First();

                    var color = Color.Blue;
                    if (songInfo.Rank == 1) color = Color.Gold;
                    if (songInfo.Rank == 2) color = Color.LighterGrey;
                    if (songInfo.Rank == 3) color = Color.DarkRed;


                    var img = "https://scoresaber.com/imports/images/oculus.png";
                    if (playerInfo.Image != null)
                    {
                        if (!playerInfo.Image.OriginalString.Contains("oculus")) img = playerInfo.Image.OriginalString.Replace(".jpg", "_full.jpg");
                    }     

                    var title = $":flag_{playerInfo.Flag.Replace(".png", "")}: {playerInfo.Name} just got a #{songInfo.Rank} play!";
                    if (playerData.IsTopPlay)
                    {
                        title = $":flag_{playerInfo.Flag.Replace(".png", "")}: Congrats on your new top play {playerInfo.Name}! with rank #{songInfo.Rank}";
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
                        Description = $"**Song name:** {playerInfo.Info.Title} \n" +
                        $"**Difficulty**: {songInfo.GetDifficulty()}\n" +
                        $"**Ranked type:** {playerInfo.Info.Ranked}\n" +
                        $"**PP:** {playerInfo.Pp}\n" +
                        acc +
                        $"**Total plays:** {playerInfo.Info.Scores}",
                        ThumbnailUrl = $"{img}",
                        ImageUrl = $"{playerInfo.Info.Image}",
                        Url = $"https://scoresaber.com/u/{playerInfo.PlayerId}",

                    };

                    //Adds the play to the recentplays so its not posted again.
                    var t = new Dictionary<ulong, long>();
                    t.Add(playerInfo.PlayerId, playerInfo.LeaderboardId);
                    recentPlays.Add(t);

                    //Checks what channel the post needs to be
                    if (playerInfo.Flag.Contains("nl"))
                    {
                        var textChannel = discord.GetGuild(505485680344956928).GetTextChannel(767552138879434772);
                        await textChannel.SendMessageAsync("", false, embed.Build());
                    }
                    foreach (var id in ScoreSaberIds)
                    {
                        if (id[0].ToString() == playerInfo.PlayerId.ToString())
                        {
                            var textChannel = discord.GetGuild(627156958880858113).GetTextChannel(768520962206990396);
                            await textChannel.SendMessageAsync("", false, embed.Build());
                        }
                    }

                }

                EmptyRecentPlays(recentPlaysSoonRemoved, liveFeedInfo[liveFeedInfo.Count - 1]);
                if (recentPlays.Count > 100) recentPlays.Clear();
                playerListToPost.Clear();
            }
        }

        /// <summary>
        /// Add player with leaderboard id and score to not let the play post agaian.
        /// </summary>
        /// <param name="recentPlaysToRemove"></param>
        /// <param name="lastPlayOnFeed"></param>
        /// <returns></returns>
        static public async Task EmptyRecentPlays(List<Dictionary<ulong, long>> recentPlaysToRemove, ScoreSaberLiveFeedModel lastPlayOnFeed)
        {
            if (recentPlaysToRemove.Count == 0) return;

            //Checks what the time needs to be to wait for.
            var timeLeftForRemoving = 90000;
            if (lastPlayOnFeed.Timeset.Contains("minutes"))
            {
                var temp = lastPlayOnFeed.Timeset;
                timeLeftForRemoving = int.Parse(temp.Replace(" minutes ago", "")) * 60 * 1000;
            }
            if (lastPlayOnFeed.Timeset.Contains("seconds"))
            {
                var temp = lastPlayOnFeed.Timeset;
                timeLeftForRemoving = int.Parse(temp.Replace(" seconds ago", "")) * 1000;
            }

            //Waits for the time to be over to remove the plays that needs to be removed.
            await Task.Delay(timeLeftForRemoving);

            var recentPlaysTemp = new List<Dictionary<ulong, long>>();

            //Checks for every play to be removed if it containts one in recentplays.  
            foreach (var toRemove in recentPlaysToRemove)
            {
                recentPlays.Remove(toRemove);
                //foreach (var r in recentPlays)
                //{
                //    // ScoreSaberID & LeaderboardID
                //    if (r.Keys.First() == toRemove.Keys.First() && r.Values.First() == toRemove.Values.First()) continue;
                //    var t = new Dictionary<ulong, long>();
                //    t.Add(r.Keys.First(), r.Values.First());
                //    recentPlaysTemp.Add(t);
                //}
                
            }

            
            //recentPlays.Clear();
            //recentPlays = recentPlaysTemp;
        }

        class PlayerListToPost
        {
            public ulong PlayerId;
            public ScoreSaberLiveFeedModel ScoreSaberLive;
            public ScoreSaberSongsModel ScoreSaberSongsData;
            public bool IsTopPlay = false;
        }

        public static async Task GiveRole(string ScoreSaberId, string roleName, DiscordSocketClient discord)
        {
            _discord = discord;
            await GiveRole(ScoreSaberId, roleName);
        }

        public static async Task GiveRoleWithRank(int rank, string ScoreSaberId, DiscordSocketClient discord = null)
        {
            if (_discord == null) _discord = discord;

            if (rank == 0) return;
            if (rank <= 1) await GiveRole(ScoreSaberId, "Nummer 1");
            else if (rank <= 3) await GiveRole(ScoreSaberId, "Top 3");
            else if (rank <= 10) await GiveRole(ScoreSaberId, "Top 10");
            else if (rank <= 25) await GiveRole(ScoreSaberId, "Top 25");
            else if (rank <= 50) await GiveRole(ScoreSaberId, "Top 50");
            else if (rank <= 100) await GiveRole(ScoreSaberId, "Top 100");
            else if (rank <= 250) await GiveRole(ScoreSaberId, "Top 250");
            else if (rank <= 500) await GiveRole(ScoreSaberId, "Top 500");
            else if (rank > 500) await GiveRole(ScoreSaberId, "Top 501+");
        }

        public static async Task GiveRole(string ScoreSaberId, string roleName)
        {
            var rolenames = new[]
            {
                "Nummer 1",
                "Top 3",
                "Top 10",
                "Top 25",
                "Top 50",
                "Top 100",
                "Top 250",
                "Top 500",
                "Top 501+"
            };
            var r = new RoleAssignment(_discord);
            ulong userDiscordId = await r.GetDiscordIdWithScoreSaberId(ScoreSaberId);
            if (userDiscordId != 0)
            {
                var guild = _discord.GetGuild(505485680344956928);
                var user = guild.GetUser(userDiscordId);
                if (user == null)
                {
                    Console.WriteLine($"User {userDiscordId} is not in the server");
                    //Console.WriteLine($"Removing User...");
                    //await DatabaseContext.ExecuteRemoveQuery($"Delete from Player where discordid={userDiscordId}");
                    //await DatabaseContext.ExecuteRemoveQuery($"Delete from PlayerInCountry where discordid={userDiscordId}");
                    //Console.WriteLine($"User removed.");

                    return;
                }

                var role = guild.Roles.FirstOrDefault(x => x.Name == roleName);


                if (user.Id != 221373638979485696)
                    foreach (var userRole in user.Roles)
                    {
                        if (rolenames.Contains(userRole.Name))
                            await user.RemoveRoleAsync(userRole);
                    }

                await user.AddRoleAsync(role);
                Console.WriteLine("User: " + user.Username + "| Added: " + role.Name);
            }
        }
    }
}