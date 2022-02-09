using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBeatSaberBot.Api.BeatSaverApi;
using DiscordBeatSaberBot.Api.BeatSaverApi.Models.New;
using DiscordBeatSaberBot.Api.BeatSaviourApi;
using DiscordBeatSaberBot.Api.BeatSaviourApi.Models;
using DiscordBeatSaberBot.Api.Spotify;
using DiscordBeatSaberBot.Models.ScoreberAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Commands.Functions
{
    public class PlaythroughStats
    {
        private RestUserMessage _msg;
        private DiscordSocketClient _discord;
        private string _playerID;
        private int _recentsongNr;
        private bool _isTopSong;
        private string _mapID;
        private bool _leaderboardToggle;
        private int _leaderboardPage = 0;
        private bool _leaderboardCountryToggle = false;
        private string _countryCode;
        private ulong _msgCreator;
        private Guid _guid;
        private ScoreSaberLib.ScoreSaberClient _scoresaberClient;


        public PlaythroughStats(DiscordSocketClient discord)
        {
            _discord = discord;
        }

        public async Task GetAndPostPlaythroughStatsWithScoresaberId(string playerId, SocketSlashCommand command, int recentsongNr = 1, bool isTopSong = false)
        {
            _msgCreator = command.User.Id;
            _playerID = playerId;
            _recentsongNr = recentsongNr;
            _isTopSong = isTopSong;
            _scoresaberClient = new ScoreSaberLib.ScoreSaberClient();

            var embedBuilder = await CreateCardAndGetPlaythroughStatsEmbed(playerId, recentsongNr, isTopSong);
            if (embedBuilder == null)
            {
                await command.Channel.SendMessageAsync($"This user could not be found id: {playerId}");
                return;
            }
            await PostEmbed(command, playerId, embedBuilder);
        }

        public async Task<EmbedBuilder> CreateCardAndGetPlaythroughStatsEmbed(string playerId, int recentsongNr = 1, bool isTopSong = false)
        {
            //Getting Data from api's
            var scoresaberApi = new ScoresaberAPI(playerId);
            var beatSaviourApi = new BeatSaviourApi(playerId);
            _scoresaberClient = new ScoreSaberLib.ScoreSaberClient();

            Score recentSong;
            if (isTopSong) recentSong = await scoresaberApi.GetTopScore(recentsongNr);
            else recentSong = await scoresaberApi.GetRecentScore(recentsongNr);

            if (recentSong == null) return null;
            _mapID = recentSong.LeaderboardId.ToString();

            var beatSaverMapInfo = await BeatSaverApi.GetMapByHash(recentSong.Id);

            //Download scoresaber full player data
            var playerFullData = await scoresaberApi.GetPlayerFull();
            var playerInfo = playerFullData.playerInfo;
            _countryCode = playerInfo.Country;

            //Download BeatSaviour livedata 
            var playerMostRecentLiveData = await beatSaviourApi.GetMostRecentLiveData(recentSong.Id, recentSong.GetDifficulty());


            await CreateCard(recentSong, beatSaverMapInfo, playerMostRecentLiveData, playerInfo);
            var embedBuilder = await CreateEmbedBuilder(recentSong, playerInfo, beatSaverMapInfo);
            return embedBuilder;
        }

        private async Task CreateCard(Score recentSong, BeatSaverMapModelNew beatSaverMapInfo, BeatSaviourLivedataModel playerMostRecentLiveData, ScoresaberPlayerFullModel.PlayerInfoModel playerInfo)
        {
            var hasBeatSaviour = playerMostRecentLiveData == null ? false : true;
            
            var cardCreator = new ImageCreator("../../../Resources/img/EmbedBackground-Template.png");
            cardCreator.AddImage($"https://scoresaber.com/imports/images/songs/{recentSong.Id}.png", 0, 0, 1080, 720, hasBeatSaviour ? 0.4f : 0.4f);

            var diff = recentSong.GetDifficulty();

            var maxScore = 0;
            dynamic noteCount = 0;
            if (beatSaverMapInfo != null)
            {
                if (hasBeatSaviour)
                {                  

                    noteCount = beatSaverMapInfo.Versions.First().Diffs.First(x => x.Difficulty == diff).Notes;
                    maxScore = (Convert.ToInt32(noteCount) - 13) * 920 + 4715;
                    //maxScore = Convert.ToInt32(noteCount) * 920;
                    if (maxScore < 0) maxScore = 0;


                    cardCreator.AddImage("../../../Resources/img/base-stat-map-template.png", 20, 230, 600, 160, isLocalFile: true);

                    cardCreator.AddText($"{beatSaverMapInfo.Metadata.Duration}", System.Drawing.Color.White, 15, 90, 250);
                    cardCreator.AddText($"{beatSaverMapInfo.Metadata.Bpm}", System.Drawing.Color.White, 15, 90, 300);
                    cardCreator.AddText($"{recentSong.MaxScoreEx}", System.Drawing.Color.White, 15, 90, 350);

                    cardCreator.AddText($"{beatSaverMapInfo.Versions.First().Diffs.First(x => x.Difficulty == diff).Notes}", System.Drawing.Color.White, 15, 275, 250);
                    cardCreator.AddText($"{beatSaverMapInfo.Versions.First().Diffs.First(x => x.Difficulty == diff).Bombs}", System.Drawing.Color.White, 15, 275, 300);
                    cardCreator.AddText($"{beatSaverMapInfo.Versions.First().Diffs.First(x => x.Difficulty == diff).Obstacles}", System.Drawing.Color.White, 15, 275, 350);

                    cardCreator.AddText($"{recentSong.Mods}", System.Drawing.Color.White, 15, 440, 250);
                    cardCreator.AddText($"{beatSaverMapInfo.Versions.First().Diffs.First(x => x.Difficulty == diff).Njs}", System.Drawing.Color.White, 15, 440, 300);
                    cardCreator.AddText($"{beatSaverMapInfo.Versions.First().Diffs.First(x => x.Difficulty == diff).Offset}", System.Drawing.Color.White, 15, 440, 350);

                }
                else
                {
                    cardCreator.AddImage("../../../Resources/img/base-stat-map-template.png", 65, 370, 950, 325, isLocalFile: true);

                    cardCreator.AddText($"{beatSaverMapInfo.Metadata.Duration} sec", System.Drawing.Color.White, 25, 175, 420);
                    cardCreator.AddText($"{beatSaverMapInfo.Metadata.Bpm}", System.Drawing.Color.White, 25, 175, 520);
                    cardCreator.AddText($"{recentSong.MaxScoreEx}", System.Drawing.Color.White, 25, 175, 620);

                    cardCreator.AddText($"{beatSaverMapInfo.Versions.First().Diffs.First(x => x.Difficulty == diff).Notes}", System.Drawing.Color.White, 25, 465, 420);
                    cardCreator.AddText($"{beatSaverMapInfo.Versions.First().Diffs.First(x => x.Difficulty == diff).Bombs}", System.Drawing.Color.White, 25, 465, 520);
                    cardCreator.AddText($"{beatSaverMapInfo.Versions.First().Diffs.First(x => x.Difficulty == diff).Obstacles}", System.Drawing.Color.White, 25, 465, 620);

                    cardCreator.AddText($"{recentSong.Mods}", System.Drawing.Color.White, 25, 730, 420);
                    cardCreator.AddText($"{beatSaverMapInfo.Versions.First().Diffs.First(x => x.Difficulty == diff).Njs}", System.Drawing.Color.White, 25, 730, 520);
                    cardCreator.AddText($"{beatSaverMapInfo.Versions.First().Diffs.First(x => x.Difficulty == diff).Offset}", System.Drawing.Color.White, 25, 730, 620);
                }
            }

            var plays = await _scoresaberClient.Api.Leaderboards.GetLeaderboardInfoByID((int)recentSong.LeaderboardId);
            //Add Base Map stats 
            if (hasBeatSaviour)
            {
                switch (recentSong.Rank)
                {
                    case 1:
                        cardCreator.AddImage("../../../Resources/img/base-stat-beatsavior-template-gold.png", 20, 20, 600, 200, isLocalFile: true);
                        break;
                    case 2:
                        cardCreator.AddImage("../../../Resources/img/base-stat-beatsavior-template-silver.png", 20, 20, 600, 200, isLocalFile: true);
                        break;
                    case 3:
                        cardCreator.AddImage("../../../Resources/img/base-stat-beatsavior-template-bronze.png", 20, 20, 600, 200, isLocalFile: true);
                        break;
                    default:
                        cardCreator.AddImage("../../../Resources/img/base-stat-beatsavior-template.png", 20, 20, 600, 200, isLocalFile: true);
                        break;
                }

                
                
                cardCreator.AddTextCenter($"#{recentSong.Rank}", System.Drawing.Color.White, 25, 320, 90);
                cardCreator.AddTextCenter($"{plays.Plays}", System.Drawing.Color.White, 15, 320, 140);

                cardCreator.AddTextCenter($"{Math.Round(Convert.ToDouble(recentSong.UScore) / Convert.ToDouble(recentSong.MaxScoreEx == 0 ? maxScore : recentSong.MaxScoreEx) * 100, 2)}%", System.Drawing.Color.White, 25, 165, 70);
                var swingloss = (100 - (playerMostRecentLiveData.Trackers.AccuracyTracker.AverageCut[0] + playerMostRecentLiveData.Trackers.AccuracyTracker.AverageCut[2]));
                var pointsWithoutUnderswing = (swingloss * noteCount) * 8;
                var AccWithoutUnderswingAndMisses = (playerMostRecentLiveData.Trackers.AccuracyTracker.AverageCut[1] + 100) * 100 / 115;
                cardCreator.AddTextCenter($"{Math.Round(Convert.ToDouble(recentSong.UScore + pointsWithoutUnderswing) / Convert.ToDouble(recentSong.MaxScoreEx == 0 ? maxScore : recentSong.MaxScoreEx) * 100, 2)}%", System.Drawing.Color.White, 15, 165, 140);
                cardCreator.AddTextCenter($"{Math.Round(AccWithoutUnderswingAndMisses, 2)}%", System.Drawing.Color.White, 15, 165, 185);

                cardCreator.AddTextCenter($"{recentSong.Pp}", System.Drawing.Color.White, 25, 480, 70);
                var misses = "";
                if (playerMostRecentLiveData.Trackers.HitTracker.Miss.ToString() == "0") misses = "FC";
                else misses = playerMostRecentLiveData.Trackers.HitTracker.Miss.ToString();
                cardCreator.AddText($"{misses}", System.Drawing.Color.White, 15, 480, 123);
                cardCreator.AddText($"{playerMostRecentLiveData.Trackers.WinTracker.NbOfPause}", System.Drawing.Color.White, 15, 480, 158);
                cardCreator.AddText($"{playerMostRecentLiveData.Trackers.HitTracker.MaxCombo}", System.Drawing.Color.White, 15, 480, 190);

            }
            else
            {
                cardCreator.AddImage("../../../Resources/img/base-stat-template.png", 90, 40, 900, 300, isLocalFile: true);

                cardCreator.AddTextCenter($"#{recentSong.Rank}", System.Drawing.Color.White, 30, 540, 150);
                cardCreator.AddTextCenter($"{plays.Plays}", System.Drawing.Color.White, 20, 540, 220);

                cardCreator.AddTextCenter($"{Math.Round(Convert.ToDouble(recentSong.UScore) / Convert.ToDouble(recentSong.MaxScoreEx) * 100, 2)}%", System.Drawing.Color.White, 40, 305, 170);
                cardCreator.AddTextCenter($"{recentSong.Pp}", System.Drawing.Color.White, 40, 790, 170);

                cardCreator.AddTextCenter($"Obtain the BeatSavior mod to get more stats", System.Drawing.Color.Gray, 15, 540, 665);
            }

            //Create AccGrid If needed
            if (hasBeatSaviour)
            {

                cardCreator.AddImage("../../../Resources/img/base-stat-extra-template.png", 20, 400, 600, 300, isLocalFile: true);

                // Total acc
                var colorGray = System.Drawing.Color.FromArgb(89, 89, 89);
                var colorWhite = System.Drawing.Color.FromArgb(200, 228, 228, 228);
                var colorBlue = System.Drawing.Color.FromArgb(46, 77, 218);
                var colorRed = System.Drawing.Color.FromArgb(219, 38, 68);

                createStatGraph(playerMostRecentLiveData.Trackers.AccuracyTracker.AccLeft, playerMostRecentLiveData.Trackers.AccuracyTracker.AccRight, playerMostRecentLiveData.Trackers.AccuracyTracker.AverageAcc, 115, 0);
                createStatGraph(playerMostRecentLiveData.Trackers.AccuracyTracker.LeftAverageCut[1], playerMostRecentLiveData.Trackers.AccuracyTracker.RightAverageCut[1], playerMostRecentLiveData.Trackers.AccuracyTracker.AverageCut[1], 15, 51, -50);
                createStatGraph(playerMostRecentLiveData.Trackers.AccuracyTracker.LeftTimeDependence, playerMostRecentLiveData.Trackers.AccuracyTracker.RightTimeDependence, playerMostRecentLiveData.Trackers.AccuracyTracker.AverageTimeDependence, 0.5, 101, 25);
                createStatGraph(playerMostRecentLiveData.Trackers.AccuracyTracker.LeftAverageCut[0], playerMostRecentLiveData.Trackers.AccuracyTracker.RightAverageCut[0], playerMostRecentLiveData.Trackers.AccuracyTracker.AverageCut[0], 70, 153, -40);
                createStatGraph(playerMostRecentLiveData.Trackers.AccuracyTracker.LeftAverageCut[2], playerMostRecentLiveData.Trackers.AccuracyTracker.RightAverageCut[2], playerMostRecentLiveData.Trackers.AccuracyTracker.AverageCut[2], 30, 208, -35);

                createStatGraph(playerMostRecentLiveData.Trackers.AccuracyTracker.LeftSpeed * 3.6, playerMostRecentLiveData.Trackers.AccuracyTracker.RightSpeed * 3.6, playerMostRecentLiveData.Trackers.AccuracyTracker.AverageSpeed * 3.6, 300, 0, 220, 228, 336, "Km/h");
                createStatGraph(playerMostRecentLiveData.Trackers.AccuracyTracker.LeftPostswing * 100, playerMostRecentLiveData.Trackers.AccuracyTracker.RightPostswing * 100, playerMostRecentLiveData.Trackers.AccuracyTracker.AveragePostswing * 100, 360, 51, 260, 228, 336, "°");
                createStatGraph(playerMostRecentLiveData.Trackers.AccuracyTracker.LeftPreswing * 60, playerMostRecentLiveData.Trackers.AccuracyTracker.RightPreswing * 60, playerMostRecentLiveData.Trackers.AccuracyTracker.AveragePreswing * 60, 360, 101, 270, 228, 336, "°");


                void createStatGraph(double accLeft, double accRight, double accBoth, double maxNumber, int offset = 0, int titleOffset = 0, int width = 228, int x = 40, string valueType = "")
                {
                    var Both = (Math.Round(accBoth, 2) * width) / maxNumber;
                    var Right = (Math.Round(accRight, 2) * width) / maxNumber;
                    var Left = (Math.Round(accLeft, 2) * width) / maxNumber;

                    cardCreator.DrawRectangle(x, 437 + offset, width, 23, colorGray);

                    cardCreator.DrawRectangle(x, 437 + offset, (int)Math.Round(Both), 25, colorWhite);
                    cardCreator.DrawRectangle((int)Math.Round(Right) + x, 437 + offset, 2, 25, colorBlue);
                    cardCreator.DrawRectangle((int)Math.Round(Left) + x, 437 + offset, 2, 25, colorRed);

                    cardCreator.AddText($"{maxNumber}", colorGray, 11, width + x - 10, 415 + offset);
                    if(valueType != "") cardCreator.AddText(valueType, colorGray, 8, width + x + 20, 415 + offset + 4);

                    if (accLeft > accRight)
                    {
                        cardCreator.AddText($"{Math.Round(accLeft, 2)}", colorRed, 11, (int)Math.Round(Left) + x + 5, 433 + offset);
                        cardCreator.AddTextFloatRight($"{Math.Round(accRight, 2)}", colorBlue, 11, 1080 - (int)Math.Round(Right) - x + 5, 445 + offset);
                    }
                    else
                    {
                        cardCreator.AddText($"{Math.Round(accRight, 2)}", colorBlue, 11, (int)Math.Round(Right) + x + 5, 433 + offset);
                        cardCreator.AddTextFloatRight($"{Math.Round(accLeft, 2)}", colorRed, 11, 1080 - (int)Math.Round(Left) - x + 5, 445 + offset);
                    }

                    cardCreator.AddText($"{Math.Round(accBoth, 2)}", colorWhite, 18, 180 + titleOffset, 406 + offset);                              
                }


                cardCreator.AddImage("../../../Resources/img/rectangle-template.png", 640, 20, 420, 370, isLocalFile: true);

                cardCreator.AddImage("../../../Resources/img/rectangle-template.png", 640, 400, 420, 300, isLocalFile: true);
                var accGrid = playerMostRecentLiveData.Trackers.AccuracyTracker.GridAcc;
                var hitGrid = playerMostRecentLiveData.Trackers.AccuracyTracker.GridCut;

                var smallestAcc = accGrid.Min(x => x.Double);
                var biggestAcc = accGrid.Max(x => x.Double);


                var gridXReplacement = -35;
                var gridYReplacement = 40;

                //Create colors on the grid acc to see what could be improved.
                cardCreator.DrawRectangle(705 + gridXReplacement, 375 + gridYReplacement, 90, 90, createGridColor(accGrid[8].Double), colorWhite);
                cardCreator.DrawRectangle(795 + gridXReplacement, 375 + gridYReplacement, 90, 90, createGridColor(accGrid[9].Double), colorWhite);
                cardCreator.DrawRectangle(885 + gridXReplacement, 375 + gridYReplacement, 90, 90, createGridColor(accGrid[10].Double), colorWhite);
                cardCreator.DrawRectangle(975 + gridXReplacement, 375 + gridYReplacement, 90, 90, createGridColor(accGrid[11].Double), colorWhite);

                cardCreator.DrawRectangle(705 + gridXReplacement, 465 + gridYReplacement, 90, 90, createGridColor(accGrid[4].Double), colorWhite);
                cardCreator.DrawRectangle(795 + gridXReplacement, 465 + gridYReplacement, 90, 90, createGridColor(accGrid[5].Double), colorWhite);
                cardCreator.DrawRectangle(885 + gridXReplacement, 465 + gridYReplacement, 90, 90, createGridColor(accGrid[6].Double), colorWhite);
                cardCreator.DrawRectangle(975 + gridXReplacement, 465 + gridYReplacement, 90, 90, createGridColor(accGrid[7].Double), colorWhite);

                cardCreator.DrawRectangle(705 + gridXReplacement, 555 + gridYReplacement, 90, 90, createGridColor(accGrid[0].Double), colorWhite);
                cardCreator.DrawRectangle(795 + gridXReplacement, 555 + gridYReplacement, 90, 90, createGridColor(accGrid[1].Double), colorWhite);
                cardCreator.DrawRectangle(885 + gridXReplacement, 555 + gridYReplacement, 90, 90, createGridColor(accGrid[2].Double), colorWhite);
                cardCreator.DrawRectangle(975 + gridXReplacement, 555 + gridYReplacement, 90, 90, createGridColor(accGrid[3].Double), colorWhite);

                System.Drawing.Color createGridColor(double? value)
                {
                    try
                    {
                        if (value == null) return System.Drawing.Color.Gray;

                        var perPoint = 0.01 * 256 / (biggestAcc - smallestAcc);
                        var x = (value - smallestAcc) * 100;
                        var f = perPoint * x + 128;

                        double r = 0;
                        double g = 0;
                        if (f <= 128)
                        {
                            r = 128;
                            g = 0;
                        }

                        if (f > 128 && f <= 256)
                        {
                            r = 128;
                            g = (double)f - 128;
                        }
                        if (f > 256)
                        {
                            r = 389 - (double)f;
                            g = 128;
                        }

                        return System.Drawing.Color.FromArgb(120, Convert.ToInt32(r), Convert.ToInt32(g), 0);
                    }
                    catch (Exception ex)
                    {
                        return System.Drawing.Color.Gray;
                    }
                }

                //Add Data to the grid
                cardCreator.AddTextCenter(accGrid[8].String == null ? Math.Round(float.Parse(accGrid[8].Double.ToString()), 1).ToString() : accGrid[8].String, System.Drawing.Color.White, 18, 750 + gridXReplacement, 405 + gridYReplacement);
                cardCreator.AddTextCenter(accGrid[9].String == null ? Math.Round(float.Parse(accGrid[9].Double.ToString()), 1).ToString() : accGrid[9].String, System.Drawing.Color.White, 18, 840 + gridXReplacement, 405 + gridYReplacement);
                cardCreator.AddTextCenter(accGrid[10].String == null ? Math.Round(float.Parse(accGrid[10].Double.ToString()), 1).ToString() : accGrid[10].String, System.Drawing.Color.White, 18, 930 + gridXReplacement, 405 + gridYReplacement);
                cardCreator.AddTextCenter(accGrid[11].String == null ? Math.Round(float.Parse(accGrid[11].Double.ToString()), 1).ToString() : accGrid[11].String, System.Drawing.Color.White, 18, 1020 + gridXReplacement, 405 + gridYReplacement);

                //cardCreator.DrawLineBetweenPoints(System.Drawing.Color.Gray, 3, 795, 380, 795, 650);

                cardCreator.AddTextCenter(accGrid[4].String == null ? Math.Round(float.Parse(accGrid[4].Double.ToString()), 2).ToString() : accGrid[4].String, System.Drawing.Color.White, 18, 750 + gridXReplacement, 495 + gridYReplacement);
                cardCreator.AddTextCenter(accGrid[5].String == null ? Math.Round(float.Parse(accGrid[5].Double.ToString()), 2).ToString() : accGrid[5].String, System.Drawing.Color.White, 18, 840 + gridXReplacement, 495 + gridYReplacement);
                cardCreator.AddTextCenter(accGrid[6].String == null ? Math.Round(float.Parse(accGrid[6].Double.ToString()), 2).ToString() : accGrid[6].String, System.Drawing.Color.White, 18, 930 + gridXReplacement, 495 + gridYReplacement);
                cardCreator.AddTextCenter(accGrid[7].String == null ? Math.Round(float.Parse(accGrid[7].Double.ToString()), 2).ToString() : accGrid[7].String, System.Drawing.Color.White, 18, 1020 + gridXReplacement, 495 + gridYReplacement);

                //cardCreator.DrawLineBetweenPoints(System.Drawing.Color.Gray, 3, 885, 380, 885, 650);

                cardCreator.AddTextCenter(accGrid[0].String == null ? Math.Round(float.Parse(accGrid[0].Double.ToString()), 2).ToString() : accGrid[0].String, System.Drawing.Color.White, 18, 750 + gridXReplacement, 585 + gridYReplacement);
                cardCreator.AddTextCenter(accGrid[1].String == null ? Math.Round(float.Parse(accGrid[1].Double.ToString()), 2).ToString() : accGrid[1].String, System.Drawing.Color.White, 18, 840 + gridXReplacement, 585 + gridYReplacement);
                cardCreator.AddTextCenter(accGrid[2].String == null ? Math.Round(float.Parse(accGrid[2].Double.ToString()), 2).ToString() : accGrid[2].String, System.Drawing.Color.White, 18, 930 + gridXReplacement, 585 + gridYReplacement);
                cardCreator.AddTextCenter(accGrid[3].String == null ? Math.Round(float.Parse(accGrid[3].Double.ToString()), 2).ToString() : accGrid[3].String, System.Drawing.Color.White, 18, 1020 + gridXReplacement, 585 + gridYReplacement);

                //cardCreator.DrawLineBetweenPoints(System.Drawing.Color.Gray, 3, 975, 380, 975, 650);

                //cardCreator.DrawLineBetweenPoints(System.Drawing.Color.Gray, 3, 705, 470, 1065, 470);
                //cardCreator.DrawLineBetweenPoints(System.Drawing.Color.Gray, 3, 705, 580, 1065, 580);

                //if (hitGrid != null)
                //{
                //    cardCreator.AddTextFloatRight(hitGrid[8].ToString(), System.Drawing.Color.White, 12, 305, 5);
                //    cardCreator.AddTextFloatRight(hitGrid[9].ToString(), System.Drawing.Color.White, 12, 205, 5);
                //    cardCreator.AddTextFloatRight(hitGrid[10].ToString(), System.Drawing.Color.White, 12, 105, 5);
                //    cardCreator.AddTextFloatRight(hitGrid[11].ToString(), System.Drawing.Color.White, 12, 5, 5);
                //    cardCreator.AddTextFloatRight(hitGrid[4].ToString(), System.Drawing.Color.White, 12, 305, 105);
                //    cardCreator.AddTextFloatRight(hitGrid[5].ToString(), System.Drawing.Color.White, 12, 205, 105);
                //    cardCreator.AddTextFloatRight(hitGrid[6].ToString(), System.Drawing.Color.White, 12, 105, 105);
                //    cardCreator.AddTextFloatRight(hitGrid[7].ToString(), System.Drawing.Color.White, 12, 5, 105);
                //    cardCreator.AddTextFloatRight(hitGrid[0].ToString(), System.Drawing.Color.White, 12, 305, 205);
                //    cardCreator.AddTextFloatRight(hitGrid[1].ToString(), System.Drawing.Color.White, 12, 205, 205);
                //    cardCreator.AddTextFloatRight(hitGrid[2].ToString(), System.Drawing.Color.White, 12, 105, 205);
                //    cardCreator.AddTextFloatRight(hitGrid[3].ToString(), System.Drawing.Color.White, 12, 5, 205);
                //}

                //add acc graph
                cardCreator.AddText($"Acc Graph & Grid", System.Drawing.Color.White, 25, 680, 30);

                Dictionary<float, float> graphPoints = playerMostRecentLiveData.Trackers.ScoreGraphTracker.Graph.ToDictionary(x => (float)Convert.ToDouble(x.Key), x => (float)Convert.ToDouble(x.Value));
                var lowestValue = graphPoints.Values.Min();
                var highestValue = graphPoints.Values.Max();
                var zoomFactor = 25 - (float)(((highestValue - lowestValue) * 100) * 2.8);
                if (zoomFactor < 0) zoomFactor = 2;
                cardCreator.AddAccGraph(720, 90, 310, 250, graphPoints, Convert.ToInt32(playerMostRecentLiveData.SongDuration), 1, System.Drawing.Color.White, zoomFactor);

            }
            _guid = Guid.NewGuid();
            await cardCreator.Create($"{GlobalConfiguration.BotImageStoragePath}EmbedBackground-{_guid}.png");

        }

        private async Task<EmbedBuilder> CreateEmbedBuilder(Score recentSong, ScoresaberPlayerFullModel.PlayerInfoModel playerInfo, BeatSaverMapModelNew beatSaverMapInfo)
        {
            var embedBuilder = new EmbedBuilder();
            embedBuilder = new EmbedBuilder
            {
                Title = $"**{recentSong.SongAuthorName} - {recentSong.Name} by {recentSong.LevelAuthorName}**",
                ImageUrl = $"{GlobalConfiguration.BotImageStorageLink}EmbedBackground-{_guid}.png",
                Url = $"https://scoresaber.com/leaderboard/{recentSong.LeaderboardId}",
                ThumbnailUrl = $"https://scoresaber.com/imports/images/songs/{recentSong.Id}.png",
                Color = Color.Blue,
                Footer = new EmbedFooterBuilder() { Text = $"Time Set: {recentSong.Timeset.DateTime.ToShortDateString() + " | " + recentSong.Timeset.DateTime.ToShortTimeString()} UTC" }
            };

            embedBuilder.Author = new EmbedAuthorBuilder() { IconUrl = $"https://new.scoresaber.com{playerInfo.Avatar}", Name = $"{ playerInfo.Name}", Url = $"https://scoresaber.com/u/{playerInfo.PlayerId}" };
            try
            {
                var spotify = await new Spotify().SearchItem(recentSong.Name, recentSong.SongAuthorName);
                var clickables =
              "\n" +
              $"- [Beatsaver](https://beatsaver.com/maps/{beatSaverMapInfo?.Id}) - " +
              $"[Preview Map](https://skystudioapps.com/bs-viewer/?id={beatSaverMapInfo?.Id}) - " +
              $"{(beatSaverMapInfo.Ranked ? $"[Replay](https://www.replay.beatleader.xyz/?id={beatSaverMapInfo.Id}&difficulty={recentSong.GetDifficulty()}&playerID={playerInfo.PlayerId}) - " : "")}" +
              $"{(spotify != null ? $"[Spotify]({spotify}) - " : "")}";
                embedBuilder.AddField(recentSong.GetDifficulty(), clickables);
            }
            catch (Exception ex)
            {

            }

            return embedBuilder;
        }

        private async Task PostEmbed(SocketSlashCommand command, string playerID, EmbedBuilder embedBuilder)
        {
            var componentBuilder = new ComponentBuilder();
            componentBuilder.WithButton(emote: Emote.Parse("<:leftarrow:923182957798244402>"), customId: "leftPlaythroughButton", style: ButtonStyle.Primary);
            componentBuilder.WithButton(emote: new Emoji("🌎"), customId: "globalLeaderboardPlaythroughButton", style: ButtonStyle.Secondary);
            componentBuilder.WithButton(emote: new Emoji("📍"), customId: "localLeaderboardPlaythroughButton", style: ButtonStyle.Secondary);
            //componentBuilder.WithButton("Website", style: ButtonStyle.Link, url: "http://beatsaberbot.com/");
            //componentBuilder.WithButton("Github", style: ButtonStyle.Link, url: "https://github.com/Daanniello/BeatsaberBot");
            //componentBuilder.WithButton("Discord Server", style: ButtonStyle.Link, url: "https://discord.gg/S3D3Yyu");
            componentBuilder.WithButton(emote: Emote.Parse("<:rightArrow:923182974638358528>"), customId: "rightPlaythroughButton", style: ButtonStyle.Primary);

            ////Temp Event image--------
            //var originalImg = embedBuilder.ImageUrl;
            //embedBuilder.ImageUrl = $"{GlobalConfiguration.BotImageStorageLink}newyear.gif";
            ////------------

            _msg = await command.Channel.SendMessageAsync("", false, embedBuilder.Build(), component: componentBuilder.Build());

            ////XMAS--------
            //embedBuilder.ImageUrl = originalImg;
            //await Task.Delay(4000);
            //await _msg.ModifyAsync(x => x.Embed = embedBuilder.Build());
            ////------------

            _discord.ButtonExecuted += _discord_ButtonExecuted;
        }

        private async Task _discord_ButtonExecuted(SocketMessageComponent button)
        {
            if (button.Message.Id == _msg.Id)
            {
                if (button.Data.CustomId == "leftPlaythroughButton")
                {
                    if (button.User.Id == _msgCreator)
                    {
                        if (_leaderboardToggle)
                        {
                            if (_leaderboardPage > 0)
                            {
                                _leaderboardPage--;
                                var embedBuilder = await new Leaderboard(_discord).GetPlayersAndCreateEmbed(_mapID, _leaderboardCountryToggle ? _countryCode : "", _leaderboardPage);
                                await _msg.ModifyAsync(x => x.Embed = embedBuilder.Build());
                            }
                        }
                        else
                        {
                            if (_recentsongNr > 1)
                            {
                                _recentsongNr--;
                                var embedBuilder = await CreateCardAndGetPlaythroughStatsEmbed(_playerID, _recentsongNr, _isTopSong);
                                await _msg.ModifyAsync(x => x.Embed = embedBuilder.Build());
                            }
                        }
                    }
                }
                if (button.Data.CustomId == "rightPlaythroughButton")
                {
                    if (button.User.Id == _msgCreator)
                    {
                        if (_leaderboardToggle)
                        {
                            _leaderboardPage++;
                            var embedBuilder = await new Leaderboard(_discord).GetPlayersAndCreateEmbed(_mapID, _leaderboardCountryToggle ? _countryCode : "", _leaderboardPage);
                            await _msg.ModifyAsync(x => x.Embed = embedBuilder.Build());
                        }
                        else
                        {
                            _recentsongNr++;
                            var embedBuilder = await CreateCardAndGetPlaythroughStatsEmbed(_playerID, _recentsongNr, _isTopSong);
                            await _msg.ModifyAsync(x => x.Embed = embedBuilder.Build());
                        }
                    }
                }
                if (button.Data.CustomId == "globalLeaderboardPlaythroughButton")
                {
                    if (_leaderboardToggle)
                    {
                        var embedBuilder = await CreateCardAndGetPlaythroughStatsEmbed(_playerID, _recentsongNr, _isTopSong);
                        await _msg.ModifyAsync(x => x.Embed = embedBuilder.Build());
                        _leaderboardToggle = false;
                        _leaderboardPage = 0;
                        _leaderboardCountryToggle = false;
                    }
                    else
                    {
                        var embedBuilder = await new Leaderboard(_discord).GetPlayersAndCreateEmbed(_mapID, "", 0);
                        await _msg.ModifyAsync(x => x.Embed = embedBuilder.Build());
                        _leaderboardToggle = true;
                        _leaderboardCountryToggle = false;
                    }
                }
                if (button.Data.CustomId == "localLeaderboardPlaythroughButton")
                {
                    if (_leaderboardToggle)
                    {
                        var embedBuilder = await CreateCardAndGetPlaythroughStatsEmbed(_playerID, _recentsongNr, _isTopSong);
                        await _msg.ModifyAsync(x => x.Embed = embedBuilder.Build());
                        _leaderboardToggle = false;
                        _leaderboardPage = 0;
                        _leaderboardCountryToggle = false;
                    }
                    else
                    {
                        var embedBuilder = await new Leaderboard(_discord).GetPlayersAndCreateEmbed(_mapID, _countryCode, 0);
                        await _msg.ModifyAsync(x => x.Embed = embedBuilder.Build());
                        _leaderboardToggle = true;
                        _leaderboardCountryToggle = true;
                    }
                }
                return;
            }
        }
    }
}
