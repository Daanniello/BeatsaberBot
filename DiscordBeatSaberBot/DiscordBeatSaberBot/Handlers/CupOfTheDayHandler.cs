using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBeatSaberBot.Api.BeatSaverApi;
using DiscordBeatSaberBot.Api.BeatSaverApi.Models.New;
using DiscordBeatSaberBot.Api.BeatSaverApi.Models.v2;
using Newtonsoft.Json;
using ScoreSaberLib;
using ScoreSaberLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using static DiscordBeatSaberBot.BeatSaberCardCollection;

namespace DiscordBeatSaberBot.Handlers
{
    public class CupOfTheDayHandler
    {
        private List<LeaderboardInfoModel.Leaderboard> serverDailyMaps = null;
        private ScoreSaberClient scoresaberClient;
        private DiscordSocketClient _discord;

        public CupOfTheDayHandler(DiscordSocketClient discord)
        {
            _discord = discord;

            scoresaberClient = new ScoreSaberClient();
            scoresaberClient.Api.ScoreFeed.WebSocket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            scoresaberClient.Api.ScoreFeed.Connect();
            WebsocketTimer();
            RefreshDailyMaps();
            scoresaberClient.Api.ScoreFeed.OnPlayReceived += Feed_OnPlayReceived;
        }

        private void Feed_OnDisconnect(object sender, EventArgs e)
        {
            scoresaberClient.Api.ScoreFeed.WebSocket.Close();
            scoresaberClient.Api.ScoreFeed.Connect();
            scoresaberClient.Api.ScoreFeed.OnPlayReceived += Feed_OnPlayReceived;
            scoresaberClient.Api.ScoreFeed.OnDisconnect += Feed_OnDisconnect;
        }

        private async void RefreshDailyMaps()
        {
            while (true)
            {
                var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
                var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);
                serverDailyMaps = new List<LeaderboardInfoModel.Leaderboard>();
                foreach (var server in cotdServers)
                {
                    RefreshLiveMMRPoints(server);
                }
                var cotdServersNewJson = JsonConvert.SerializeObject(cotdServers);
                System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersNewJson);

                setAllDailyMaps();

                await Task.Delay(1000 * 60 * 30);
            }
        }

        private void setAllDailyMaps()
        {
            var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
            var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);

            foreach (var server in cotdServers)
            {
                if (server.TodaysMap != null) serverDailyMaps.Add(server.TodaysMap);
                if (server.ServerID == "0")
                {
                    if (server.TodaysMapFocus != null) serverDailyMaps.Add(server.TodaysMapFocus);
                    if (server.TodaysMapHardcore != null) serverDailyMaps.Add(server.TodaysMapHardcore);
                }
            }
        }

        private async void WebsocketTimer()
        {
            while (true)
            {
                await Task.Delay(10000);
                if (!scoresaberClient.Api.ScoreFeed.WebSocket.IsAlive)
                {
                    scoresaberClient.Api.ScoreFeed.WebSocket.Connect();
                }
            }
        }

        private async void Feed_OnPlayReceived(object sender, ScoreSaberLib.Models.ScoreFeedModel e)
        {
            //COTD
            if (serverDailyMaps != null)
            {
                var playedMap = e.CommandData;
                try
                {
                    if (serverDailyMaps.FirstOrDefault(x => x.SongHash == playedMap.Leaderboard.SongHash && x.Difficulty.DifficultyRaw == playedMap.Leaderboard.Difficulty.DifficultyRaw) != null)
                    {
                        var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
                        var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);

                        var todaysMaps = serverDailyMaps.FirstOrDefault(x => x.SongHash == playedMap.Leaderboard.SongHash);

                        var serversThatHasThisMap = new List<COTDServer>();

                        var focus = cotdServers.Where(x => x.TodaysPlayersFocus != null && x.TodaysMapFocus != null && x.TodaysMapFocus.SongHash == todaysMaps.SongHash && x.TodaysPlayersFocus.FirstOrDefault(i => i.ScoreSaberID == playedMap.Score.LeaderboardPlayerInfo.Id.ToString()) != null).ToList();
                        var standard = cotdServers.Where(x => x.TodaysPlayers != null && x.TodaysMap != null && x.TodaysMap.SongHash == todaysMaps.SongHash && x.TodaysPlayers.FirstOrDefault(i => i.ScoreSaberID == playedMap.Score.LeaderboardPlayerInfo.Id.ToString()) != null).ToList();
                        var hardcore = cotdServers.Where(x => x.TodaysPlayersHardcore != null && x.TodaysMapHardcore != null && x.TodaysMapHardcore.SongHash == todaysMaps.SongHash && x.TodaysPlayersHardcore.FirstOrDefault(i => i.ScoreSaberID == playedMap.Score.LeaderboardPlayerInfo.Id.ToString()) != null).ToList();

                        if (focus.Count() > 0) serversThatHasThisMap.AddRange(focus);
                        if (standard.Count() > 0) serversThatHasThisMap.AddRange(standard);
                        if (hardcore.Count() > 0) serversThatHasThisMap.AddRange(hardcore);

                        foreach (var serverMapPlayed in serversThatHasThisMap)
                        {
                            COTDPlayer player = null;

                            if (serverMapPlayed.ServerID == "0")
                            {
                                
                                if(serverMapPlayed.TodaysMapFocus.Id == playedMap.Leaderboard.Id) player = serverMapPlayed.TodaysPlayersFocus.FirstOrDefault(x => x.ScoreSaberID == playedMap.Score.LeaderboardPlayerInfo.Id.ToString());
                                if (serverMapPlayed.TodaysMap.Id == playedMap.Leaderboard.Id) player = serverMapPlayed.TodaysPlayers.FirstOrDefault(x => x.ScoreSaberID == playedMap.Score.LeaderboardPlayerInfo.Id.ToString());
                                if (serverMapPlayed.TodaysMapHardcore.Id == playedMap.Leaderboard.Id) player = serverMapPlayed.TodaysPlayersHardcore.FirstOrDefault(x => x.ScoreSaberID == playedMap.Score.LeaderboardPlayerInfo.Id.ToString());
                                if (player.TodaysScore < e.CommandData.Score.BaseScore && player != null)
                                {
                                    player.TodaysScore = e.CommandData.Score.BaseScore;
                                }
                            }
                            else
                            {
                                player = serverMapPlayed.TodaysPlayers.FirstOrDefault(x => x.ScoreSaberID == playedMap.Score.LeaderboardPlayerInfo.Id.ToString());
                                if (player.TodaysScore < e.CommandData.Score.BaseScore)
                                {
                                    player.TodaysScore = e.CommandData.Score.BaseScore;
                                }
                            }                           


                            RefreshLiveMMRPoints(serverMapPlayed);

                            try
                            {
                                if (serverMapPlayed.DiscordFeedChannelID != null)
                                {
                                    var channel = (ITextChannel)await _discord.GetChannelAsync(Convert.ToUInt64(serverMapPlayed.DiscordFeedChannelID));
                                    if (channel == null)
                                    {
                                        serverMapPlayed.DiscordFeedChannelID = null;
                                    }
                                    else
                                    {
                                        var messages = await channel.GetMessagesAsync(10).FlattenAsync();
                                        if (messages != null && messages.Count() >= 2)
                                        {
                                            var leaderboardRaw = messages.FirstOrDefault(x => x.Embeds.First().Footer.Value.Text == "001");
                                            var newPlayRaw = messages.FirstOrDefault(x => x.Embeds.First().Footer.Value.Text == "002");
                                            var leaderboard = (RestUserMessage)leaderboardRaw;
                                            var newPlay = (RestUserMessage)newPlayRaw;

                                            await leaderboard.ModifyAsync(x => x.Embed = GetMapFeed(serverMapPlayed).Build());
                                            await newPlay.ModifyAsync(x => x.Embed = GetNewPlayFeed(player.Name, Math.Round((double)player.TodaysScore * 100 / serverMapPlayed.TodaysMap.MaxScore, 2).ToString(), player.MMR.ToString(), player.TotalWins.ToString(), player.ScoreSaberID).Build());

                                            var fakeMsg = await channel.SendMessageAsync("yeet");
                                            await fakeMsg.DeleteAsync();
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var f = ex;
                            }
                        }

                        var cotdServersNewJson = JsonConvert.SerializeObject(cotdServers);
                        System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersNewJson);
                    }
                }
                catch (Exception ex)
                {
                    var f = ex;
                }
            }


            //STAKING
            var currentStakeMatches = JsonConvert.DeserializeObject<List<StakeMatch>>(System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/StakeMatches.json"));

            if (currentStakeMatches != null)
            {
                var playerOneList = currentStakeMatches.Where(x => x.PlayerOneScoresaberID == e.CommandData.Score.LeaderboardPlayerInfo.Id.ToString());
                var playerTwoList = currentStakeMatches.Where(x => x.PlayerTwoScoresaberID == e.CommandData.Score.LeaderboardPlayerInfo.Id.ToString());
                if (playerOneList.Count() > 0 || playerTwoList.Count() > 0)
                {
                    var diff = e.CommandData.Leaderboard.Difficulty.DifficultyRaw.Replace("_", " ").Trim().Split(' ')[0].ToLower();
                    var maphash = e.CommandData.Leaderboard.SongHash.ToLower();
                    var playerOne = playerOneList.FirstOrDefault(x => x.mapDiff.ToLower() == diff && x.mapHash.ToLower() == maphash && x.EndDate > DateTime.UtcNow);
                    var playerTwo = playerTwoList.FirstOrDefault(x => x.mapDiff.ToLower() == diff && x.mapHash.ToLower() == maphash && x.EndDate > DateTime.UtcNow);

                    if (playerOne != null)
                    {
                        double percentage = Convert.ToDouble(e.CommandData.Score.BaseScore) / playerOne.mapMaxScore * 100;
                        if (percentage > playerOne.PlayerOneCurrentScore)
                        {
                            //overwrite score
                            playerOne.PlayerOneCurrentScore = percentage;
                            var newJson = JsonConvert.SerializeObject(currentStakeMatches);
                            File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/StakeMatches.json", newJson);
                        }

                    }

                    if (playerTwo != null)
                    {

                        double percentage = Convert.ToDouble(e.CommandData.Score.BaseScore) / playerTwo.mapMaxScore * 100;
                        if (percentage > playerTwo.PlayerTwoCurrentScore)
                        {
                            //overwrite score
                            playerTwo.PlayerTwoCurrentScore = percentage;
                            var newJson = JsonConvert.SerializeObject(currentStakeMatches);
                            File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/StakeMatches.json", newJson);
                        }

                    }
                }
            }
        }

        public enum DailyMapMode
        {
            Focus,
            Standard,
            Hardcore
        }
        public static async Task<LeaderboardInfoModel.Leaderboard> GenerateGlobalDailyMap(string hashFromMapBefore, DailyMapMode mode)
        {
            if (hashFromMapBefore == null) hashFromMapBefore = "";
            var maps = new List<Maps>();
            var validMaps = new List<Maps>();

            if (mode == DailyMapMode.Focus) //Focus pool
            {
                var maps1 = await BeatSaverApi.GetMapForCupOfTheDayFocus(0);
                var maps2 = await BeatSaverApi.GetMapForCupOfTheDayFocus(1);
                maps.AddRange(maps1.Docs);
                maps.AddRange(maps2.Docs);

                validMaps = maps.Where(x => x.Stats.Score > 0.75 && x.Metadata.Duration < 2000 && x.Stats.Upvotes > 20 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Count() > 0 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Last().Nps < 3.5).ToList();
            }
            if (mode == DailyMapMode.Standard) //Standard pool
            {
                var maps1 = await BeatSaverApi.GetMapForCupOfTheDayStandard(0);
                var maps2 = await BeatSaverApi.GetMapForCupOfTheDayStandard(1);
                maps.AddRange(maps1.Docs);
                maps.AddRange(maps2.Docs);

                validMaps = maps.Where(x => x.Stats.Score > 0.70 && x.Metadata.Duration < 2000 && x.Stats.Upvotes > 20 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Count() > 0 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Last().Nps > 5.5 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Last().Nps < 9).ToList();
            }
            if (mode == DailyMapMode.Hardcore) //Hardcore pool
            {
                var maps1 = await BeatSaverApi.GetMapForCupOfTheDayHardcore(0);
                var maps2 = await BeatSaverApi.GetMapForCupOfTheDayHardcore(1);
                maps.AddRange(maps1.Docs);
                maps.AddRange(maps2.Docs);

                validMaps = maps.Where(x => x.Stats.Score > 0.40 && x.Metadata.Duration < 2000 && x.Stats.Upvotes > 3 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Count() > 0 && x.Versions.First().Diffs.Where(x => x.Characteristic == "Standard").Last().Nps > 8).ToList();
            }

            LeaderboardInfoModel.Leaderboard chosenMap;
            var random = new Random();
            do
            {
                var mapInfo = validMaps.ToList()[random.Next(0, validMaps.Count())].Versions.First();

                var diffnr = 9;
                switch (mapInfo.Diffs.Where(x => x.Characteristic == "Standard").Last().Difficulty.ToString())
                {
                    case "Easy":
                        diffnr = 1;
                        break;
                    case "Normal":
                        diffnr = 3;
                        break;
                    case "Hard":
                        diffnr = 5;
                        break;
                    case "Expert":
                        diffnr = 7;
                        break;
                    case "ExpertPlus":
                        diffnr = 9;
                        break;
                    default:
                        diffnr = 9;
                        break;
                }
                chosenMap = await new ScoreSaberClient().Api.Leaderboards.GetLeaderboardInfoByHashcode(mapInfo.Hash.ToUpper(), (Leaderboards.Difficulty)diffnr);

                if (chosenMap != null) chosenMap.MaxScore = mapInfo.Diffs.Where(x => x.Characteristic == "Standard").Last().MaxScore;

            } while (chosenMap == null || hashFromMapBefore.ToLower() == chosenMap.SongHash.ToLower());

            return chosenMap;
        }

        private void RefreshLiveMMRPoints(COTDServer server)
        {
            if (server.TodaysPlayers != null)
            {
                foreach (var player in server.TodaysPlayers)
                {
                    //TODO Make mmr change for focus mode and hardcore mode
                    player.TodaysMMRChange = CalculateMMRPoints(player, server, server.TodaysPlayers);
                }
            }
            if (server.TodaysPlayersFocus != null)
            {
                foreach (var player in server.TodaysPlayersFocus)
                {
                    //TODO Make mmr change for focus mode and hardcore mode
                    player.TodaysMMRChange = CalculateMMRPoints(player, server, server.TodaysPlayersFocus);
                }
            }
            if (server.TodaysPlayersHardcore != null)
            {
                foreach (var player in server.TodaysPlayersHardcore)
                {
                    //TODO Make mmr change for focus mode and hardcore mode
                    player.TodaysMMRChange = CalculateMMRPoints(player, server, server.TodaysPlayersHardcore);
                }
            }
        }

        private double CalculateMMRPoints(COTDPlayer player, COTDServer server, List<COTDPlayer> todaysPlayers)
        {
            if (player.TodaysScore <= 0 || todaysPlayers.Where(x => x.TodaysScore > 0).Count() <= 1) return 0;
            var mmr = player.MMR;
            var playersAbove = todaysPlayers.Where(x => x.TodaysScore >= player.TodaysScore && x.TodaysScore > 0 && x.MMR <= player.MMR);
            var playersBelow = todaysPlayers.Where(x => x.TodaysScore <= player.TodaysScore && x.TodaysScore > 0 && x.MMR >= player.MMR);
            double avgMmrAboveWhereMmrIsLower = player.MMR;
            double avgMmrBelowWhereMmrIsHigher = player.MMR;
            if (playersAbove.Count() > 0) avgMmrAboveWhereMmrIsLower = playersAbove.Average(x => x.MMR);
            if (playersBelow.Count() > 0) avgMmrBelowWhereMmrIsHigher = playersBelow.Average(x => x.MMR);

            var mmrPointsplus = avgMmrAboveWhereMmrIsLower - player.MMR;
            var mmrPointsMin = avgMmrBelowWhereMmrIsHigher - player.MMR;

            var mmrPointsRaw = mmrPointsplus + mmrPointsMin;
            var mmrPoints = mmrPointsRaw * 100 / player.MMR;
            if (mmrPoints == 0) mmrPoints = 5;

            return mmrPoints;
        }

        public async Task ResetDailyMap(DiscordSocketClient discord)
        {
            var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
            var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);            

            foreach (var server in cotdServers)
            {
                if (server.TodaysPlayers == null) continue;
                foreach (var player in server.TodaysPlayers)
                {
                    var allTimePlayer = server.AllTimePlayers.FirstOrDefault(x => x.ScoreSaberID == player.ScoreSaberID && x.DiscordID == player.DiscordID);

                    var mmrPointsStandard = CalculateMMRPoints(player, server, server.TodaysPlayers);
                    allTimePlayer.MMR += mmrPointsStandard;

                    if (server.ServerID == "0")
                    {
                        if (server.TodaysPlayersFocus != null)
                        {
                            var mmrPointsFocus = CalculateMMRPoints(server.TodaysPlayersFocus.FirstOrDefault(x => x.ScoreSaberID == player.ScoreSaberID && x.DiscordID == player.DiscordID), server, server.TodaysPlayersFocus);
                            allTimePlayer.MMR += mmrPointsFocus;
                            if (mmrPointsFocus > 0) await new BeatSaberCardCollection(discord).GivePacks(Convert.ToInt64(allTimePlayer.DiscordID), 1, $"Congrats on gaining +{mmrPointsFocus} mmr points on the cup of the day in Focus Mode! here is a reward.");
                        }

                        if (server.TodaysPlayersHardcore != null)
                        {
                            var mmrPointsHardcore = CalculateMMRPoints(server.TodaysPlayersHardcore.FirstOrDefault(x => x.ScoreSaberID == player.ScoreSaberID && x.DiscordID == player.DiscordID), server, server.TodaysPlayersFocus);
                            allTimePlayer.MMR += mmrPointsHardcore;
                            if (mmrPointsHardcore > 0) await new BeatSaberCardCollection(discord).GivePacks(Convert.ToInt64(allTimePlayer.DiscordID), 1, $"Congrats on gaining +{mmrPointsHardcore} mmr points on the cup of the day in Hardcore Mode! here is a reward.");
                        }

                        if (mmrPointsStandard > 0) await new BeatSaberCardCollection(discord).GivePacks(Convert.ToInt64(allTimePlayer.DiscordID), 1, $"Congrats on gaining +{mmrPointsStandard} mmr points on the cup of the day in Standard Mode! here is a reward.");
                    }
                }

                //Give winner points 
                var winner = server.TodaysPlayers.OrderByDescending(x => x.TodaysScore).First();
                var allTimePlayerWinner = server.AllTimePlayers.FirstOrDefault(x => x.ScoreSaberID == winner.ScoreSaberID && x.DiscordID == winner.DiscordID);
                if (winner.TodaysScore != 0)
                {
                    if (server.TodaysPlayers.Where(x => x.TodaysScore > 0).Count() > 1)
                        allTimePlayerWinner.TotalWins += 1;
                }
            }

            foreach (var server in cotdServers)
            {
                server.TodaysPlayersFocus = null;
                server.TodaysPlayers = null;
                server.TodaysPlayersHardcore = null;
            }

            var cotdServersNewJson = JsonConvert.SerializeObject(cotdServers);
            System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersNewJson);

            await ResetDailyMapFromAllServer();
            setAllDailyMaps();
            await ResetDiscordServerFeeds(discord);
        }

        private async Task ResetDiscordServerFeeds(DiscordSocketClient discord)
        {
            var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
            var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);

            foreach (var server in cotdServers)
            {
                //Reset discord feed 
                try
                {
                    if (server.DiscordFeedChannelID != null)
                    {
                        var channel = (ITextChannel)await _discord.GetChannelAsync(Convert.ToUInt64(server.DiscordFeedChannelID));
                        if (channel == null)
                        {
                            server.DiscordFeedChannelID = null;
                        }
                        else
                        {
                            var messages = await channel.GetMessagesAsync(10).FlattenAsync();
                            if (messages != null && messages.Count() >= 2)
                            {
                                var leaderboardRaw = messages.FirstOrDefault(x => x.Embeds.First().Footer.Value.Text == "001");
                                var newPlayRaw = messages.FirstOrDefault(x => x.Embeds.First().Footer.Value.Text == "002");
                                var leaderboard = (RestUserMessage)leaderboardRaw;
                                var newPlay = (RestUserMessage)newPlayRaw;

                                await leaderboard.ModifyAsync(x => x.Embed = GetMapFeed(server).Build());
                                await newPlay.ModifyAsync(x => x.Embed = GetNewPlayFeed("", "", "", "", "").Build());

                                var fakeMsg = await channel.SendMessageAsync("yeet");
                                await fakeMsg.DeleteAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var f = ex;
                }
            }
            var cotdServersNewJson = JsonConvert.SerializeObject(cotdServers);
            System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersNewJson);
        }

        public static async void CreateGlobalServer()
        {
            var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
            var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);
            if (cotdServers.FirstOrDefault(x => x.ServerID == "0" && x.ServerName == "Global") == null)
            {
                var map = await GenerateGlobalDailyMap("", DailyMapMode.Standard);
                cotdServers.Add(new COTDServer() { ServerID = "0", ServerName = "Global", IsPublic = true, TodaysMap = map });
            }
            var cotdServersNewJson = JsonConvert.SerializeObject(cotdServers);
            System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersNewJson);
        }

        public static async void MakeServerPrivateOrPublic(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {
            //Make server private / public 
            var json = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
            if (json != "")
            {
                var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(json);
                var server = cotdServers.FirstOrDefault(x => x.ServerID == command.GuildId.ToString());
                if (server != null)
                {
                    if (server.IsPublic) server.IsPublic = false;
                    else server.IsPublic = true;
                }
                else
                {
                    var guild = discordSocketClient.GetGuild((ulong)command.GuildId);
                    server = new COTDServer() { ServerID = command.GuildId.ToString(), ServerName = guild.Name, IsPublic = true, TodaysMap = cotdServers.FirstOrDefault(x => x.ServerName == "Global" && x.ServerID == "0").TodaysMap };
                    cotdServers.Add(server);
                }

                var cotdServersJson = JsonConvert.SerializeObject(cotdServers);
                System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersJson);
                await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed($"This server is now {(server.IsPublic ? "public" : "private")}", $"{(server.IsPublic ? "Everyone can now see the leaderboard on" : "This server has been removed from")} https://beatsaberbot.com/CupOfTheDay").Build());
            }
            else
            {
                var cotdServers = new List<COTDServer>();
                var guild = discordSocketClient.GetGuild((ulong)command.GuildId);
                cotdServers.Add(new COTDServer() { ServerID = command.GuildId.ToString(), ServerName = guild.Name, IsPublic = true, TodaysMap = cotdServers.FirstOrDefault(x => x.ServerName == "Global" && x.ServerID == "0").TodaysMap });
                var cotdServersJson = JsonConvert.SerializeObject(cotdServers);
                System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersJson);
                await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("This server is now public", "Everyone can now see the leaderboard on https://beatsaberbot.com/CupOfTheDay").Build());
            }
        }

        public static async Task SetFeedChannel(SocketSlashCommand command, ITextChannel channel)
        {
            var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
            var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);

            var channelID = channel.Id;
            var server = cotdServers.FirstOrDefault(x => x.ServerID == command.GuildId.ToString());
            if (server != null)
            {
                server.DiscordFeedChannelID = channelID.ToString();

                //Send Main message + new score message

                var leaderboardEmbed = GetMapFeed(server);
                await channel.SendMessageAsync("", false, leaderboardEmbed.Build());

                await channel.SendMessageAsync("", false, GetNewPlayFeed("", "", "", "", "").Build());

                var cotdServersJsonNew = JsonConvert.SerializeObject(cotdServers);
                System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersJsonNew);
            }
            else await command.Channel.SendMessageAsync("Server is not made yet, First use the command `/cupoftheday settings make public`");
        }

        public static EmbedBuilder GetNewPlayFeed(string username, string acc, string mmr, string wins, string id)
        {
            var newPlayEmbed = new EmbedBuilder();

            var title = "No one has set a score yet.";
            if (username != "") title = $"{username} set a new score! ({acc}%)";

            newPlayEmbed.Title = title;

            var desc = "be the first one to set a score";
            if (username != "") desc = $"MMR: {mmr}\nWins: {wins}";

            newPlayEmbed.Description = desc;

            var thumbnail = "https://cdn.scoresaber.com/avatars/steam.png";
            if (id != "") thumbnail = $"https://cdn.scoresaber.com/avatars/{id}.jpg";
            newPlayEmbed.ThumbnailUrl = thumbnail;

            newPlayEmbed.Footer = new EmbedFooterBuilder() { Text = "002" };

            return newPlayEmbed;
        }

        public static EmbedBuilder GetMapFeed(COTDServer server)
        {
            var desc = "";
            for (var x = 1; x <= 8; x++)
            {
                if (server.TodaysPlayers != null && x <= server.TodaysPlayers.Count())
                {
                    var player = server.TodaysPlayers[x - 1];
                    desc += $"#{x}  **{player.Name}**         Acc: **{Math.Round((double)player.TodaysScore * 100 / server.TodaysMap.MaxScore, 2)}%** - MMR: **{player.MMR}** \n\n";
                }
                else
                {
                    desc += $"#{x} ...\n\n";
                }
            }

            var leaderboardEmbed = new EmbedBuilder();
            leaderboardEmbed.Title = "Cup Of The Day (Daily Leaderboard)";
            leaderboardEmbed.Url = "https://beatsaberbot.com/CupOfTheDay";
            leaderboardEmbed.Author = new EmbedAuthorBuilder { Name = server.ServerName };
            leaderboardEmbed.ThumbnailUrl = $"https://cdn.scoresaber.com/covers/{server.TodaysMap.SongHash}.png";
            leaderboardEmbed.Footer = new EmbedFooterBuilder() { Text = "001" };
            leaderboardEmbed.Fields.Add(new EmbedFieldBuilder() { Name = server.TodaysMap.SongName + " by " + server.TodaysMap.SongAuthorName + $"({server.TodaysMap.Difficulty.DifficultyRaw.Replace("_", " ")})", Value = "Made by: " + server.TodaysMap.LevelAuthorName });
            leaderboardEmbed.Description = desc;

            return leaderboardEmbed;
        }

        public static async void UploadServerPlaylist(IAttachment attachment, string serverID)
        {
            try
            {
                // Create scoresaber map list 
                var json = "";
                using (WebClient wc = new WebClient())
                {
                    json = wc.DownloadString(attachment.Url);
                }

                var playlist = JsonConvert.DeserializeObject<Models.PlaylistMapsModel>(json);
                if (playlist == null) return;

                var maps = new List<LeaderboardInfoModel.Leaderboard>();
                var scoresaberClient = new ScoreSaberClient();
                foreach (var song in playlist.Songs)
                {
                    var map = await BeatSaverApi.GetMapByKey(song.Key);
                    if (map == null) map = await BeatSaverApi.GetMapByHash(song.Hash);
                    if (map == null) return;
                    var diff = map.Versions.First().Diffs.Last().Difficulty;
                    var diffnr = 9;
                    switch (diff)
                    {
                        case "Easy":
                            diffnr = 1;
                            break;
                        case "Normal":
                            diffnr = 3;
                            break;
                        case "Hard":
                            diffnr = 5;
                            break;
                        case "Expert":
                            diffnr = 7;
                            break;
                        case "ExpertPlus":
                            diffnr = 9;
                            break;
                        default:
                            diffnr = 9;
                            break;
                    }
                    var leaderboard = await scoresaberClient.Api.Leaderboards.GetLeaderboardInfoByHashcode(song.Hash, (Leaderboards.Difficulty)diffnr);
                    maps.Add(leaderboard);
                }

                if (maps.Count <= 0) return;

                //Add playlist to server
                var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
                var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);

                if (cotdServers.FirstOrDefault(x => x.ServerID == serverID) == null) return;
                else
                {
                    var server = cotdServers.FirstOrDefault(x => x.ServerID == serverID);
                    server.Playlist = maps;
                    server.TodaysMap = maps.First();
                    foreach (var player in server.TodaysPlayers)
                    {
                        player.TodaysScore = 0;
                    }

                    var cotdServersNewJson = JsonConvert.SerializeObject(cotdServers);
                    System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersNewJson);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }           
        }

        private async Task ResetDailyMapFromAllServer()
        {
            var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
            if (cotdServersJson != "")
            {
                var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);
                foreach (var server in cotdServers)
                {
                    if (server.ServerName == "Global" && server.ServerID == "0")
                    {
                        var standardFocus = await GenerateGlobalDailyMap(server.TodaysMapFocus.SongHash, DailyMapMode.Focus);
                        var standardMap = await GenerateGlobalDailyMap(server.TodaysMap.SongHash, DailyMapMode.Standard); //TODO replace todaysmap with specific
                        var standardHardcore = await GenerateGlobalDailyMap(server.TodaysMapHardcore.SongHash, DailyMapMode.Hardcore);

                        server.TodaysMapFocus = standardFocus;
                        server.TodaysMap = standardMap;
                        server.TodaysMapHardcore = standardHardcore;

                        continue;
                    }

                    var playlist = server.Playlist.Where(x => x != null).ToList();
                    var index = 0;
                    if (server.TodaysMap != null) index = playlist.IndexOf(playlist.FirstOrDefault(x => x.SongHash == server.TodaysMap.SongHash));
                    if (index + 1 >= playlist.Count) index = 0;
                    else index += 1;
                    server.TodaysMap = playlist[index];
                }
                var newJson = JsonConvert.SerializeObject(cotdServers);
                File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/COTDServerPlayer.json", newJson);
            }
        }

        public static void ServerPlayerJoin(Player player, string discordID, string serverID)
        {
            var cotdServersJson = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json");
            var cotdServers = JsonConvert.DeserializeObject<List<COTDServer>>(cotdServersJson);
            var server = cotdServers.FirstOrDefault(x => x.ServerID == serverID);
            if (server == null) return;
            else
            {
                if (server.AllTimePlayers == null) server.AllTimePlayers = new List<COTDPlayer>();
                if (server.TodaysPlayers == null) server.TodaysPlayers = new List<COTDPlayer>();
                if (server.TodaysPlayersFocus == null) server.TodaysPlayersFocus = new List<COTDPlayer>();
                if (server.TodaysPlayersHardcore == null) server.TodaysPlayersHardcore = new List<COTDPlayer>();

                if (server.AllTimePlayers.FirstOrDefault(x => x.ScoreSaberID == player.ScoreSaberID && x.DiscordID == discordID) == null)//First time join
                {
                    var newPlayer = new COTDPlayer { DiscordID = discordID, ScoreSaberID = player.ScoreSaberID, MMR = 600, Name = player.Name, TodaysScore = 0, TotalWins = 0 };
                    server.AllTimePlayers.Add(newPlayer);
                    server.TodaysPlayers.Add(newPlayer);

                    if (server.ServerID == "0")
                    {
                        server.TodaysPlayersFocus.Add(newPlayer);
                        server.TodaysPlayersHardcore.Add(newPlayer);
                    }
                }
                else
                {
                    if (server.TodaysPlayers.FirstOrDefault(x => x.ScoreSaberID == player.ScoreSaberID && x.DiscordID == discordID) == null) //already existing
                    {
                        var allTimePlayer = server.AllTimePlayers.FirstOrDefault(x => x.ScoreSaberID == player.ScoreSaberID && x.DiscordID == discordID);
                        server.TodaysPlayers.Add(allTimePlayer);

                        if (server.ServerID == "0")
                        {
                            server.TodaysPlayersFocus.Add(allTimePlayer);
                            server.TodaysPlayersHardcore.Add(allTimePlayer);
                        }
                    }
                }

                var cotdServersNewJson = JsonConvert.SerializeObject(cotdServers);
                System.IO.File.WriteAllText(GlobalConfiguration.WebsiteRoot + @"DataCollection\COTDServerPlayer.json", cotdServersNewJson);
            }
        }

        public class Player
        {
            public string Name { get; set; }
            public string ScoreSaberID { get; set; }

            public int MMRGlobal { get; set; }
            public long TodaysScore { get; set; }

            public int TotalWinsGlobal { get; set; }

            public List<ServerStats> Servers { get; set; }

            public class ServerStats
            {
                public string id { get; set; }
                public string Name { get; set; }

                public int MMR { get; set; }

                public int TotalWins { get; set; }
            }
        }

        public class COTDServer
        {
            public string ServerName;
            public string ServerID;
            public bool IsPublic;
            public string DiscordFeedChannelID;
            public LeaderboardInfoModel.Leaderboard TodaysMap;
            public LeaderboardInfoModel.Leaderboard TodaysMapFocus;
            public LeaderboardInfoModel.Leaderboard TodaysMapHardcore;
            public List<LeaderboardInfoModel.Leaderboard> Playlist;
            public List<COTDPlayer> TodaysPlayers;
            public List<COTDPlayer> TodaysPlayersFocus;
            public List<COTDPlayer> TodaysPlayersHardcore;
            public List<COTDPlayer> AllTimePlayers;
        }

        public class COTDPlayer
        {
            public string Name { get; set; }
            public string ScoreSaberID { get; set; }
            public string DiscordID { get; set; }
            public double MMR { get; set; }
            public double TodaysMMRChange { get; set; }
            public int TotalWins { get; set; }
            public long TodaysScore { get; set; }
        }
    }
}
