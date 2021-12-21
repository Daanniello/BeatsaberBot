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


        public PlaythroughStats(DiscordSocketClient discord)
        {
            _discord = discord;
        }

        public async Task GetAndPostPlaythroughStatsWithScoresaberId(string playerId, SocketMessage message, int recentsongNr = 1, bool isTopSong = false)
        {
            _msgCreator = message.Author.Id;
            _playerID = playerId;
            _recentsongNr = recentsongNr;
            _isTopSong = isTopSong;

            var embedBuilder = await CreateCardAndGetPlaythroughStatsEmbed(playerId, recentsongNr, isTopSong);
            if (embedBuilder == null)
            {
                await message.Channel.SendMessageAsync($"This user could not be found id: {playerId}");
                return;
            }
            await PostEmbed(message, playerId, embedBuilder);
        }

        public async Task<EmbedBuilder> CreateCardAndGetPlaythroughStatsEmbed(string playerId, int recentsongNr = 1, bool isTopSong = false)
        {
            //Getting Data from api's
            var scoresaberApi = new ScoresaberAPI(playerId);
            var beatSaviourApi = new BeatSaviourApi(playerId);

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
            cardCreator.AddImage($"https://scoresaber.com/imports/images/songs/{recentSong.Id}.png", 0, 0, 1080, 720, 0.1f);

            var diff = recentSong.GetDifficulty();

            var maxScore = 0;
            dynamic noteCount = 0;
            if (beatSaverMapInfo != null)
            {

                cardCreator.AddText($"Bpm:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, 0);
                cardCreator.AddText($"{beatSaverMapInfo.Metadata.Bpm}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, 0);

                cardCreator.AddText($"Duration:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 30 : 40);
                cardCreator.AddText($"{beatSaverMapInfo.Metadata.Duration}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 30 : 40);

                cardCreator.AddText($"Notes:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 60 : 80);
                cardCreator.AddText($"{beatSaverMapInfo.Versions.First().Diffs.First(x => x.Difficulty == diff).Notes}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 60 : 80);

                cardCreator.AddText($"NJS:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 90 : 120);
                cardCreator.AddText($"{beatSaverMapInfo.Versions.First().Diffs.First(x => x.Difficulty == diff).Njs}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 90 : 120);

                cardCreator.AddText($"NJS offset:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 120 : 160);
                cardCreator.AddText($"{beatSaverMapInfo.Versions.First().Diffs.First(x => x.Difficulty == diff).Offset}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 120 : 160);

                cardCreator.AddText($"Bombs:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 150 : 200);
                cardCreator.AddText($"{beatSaverMapInfo.Versions.First().Diffs.First(x => x.Difficulty == diff).Bombs}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 150 : 200);

                cardCreator.AddText($"Obstacles:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 180 : 240);
                cardCreator.AddText($"{beatSaverMapInfo.Versions.First().Diffs.First(x => x.Difficulty == diff).Obstacles}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 180 : 240);

                cardCreator.AddText($"Max Score:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 210 : 280);
                cardCreator.AddText($"{recentSong.MaxScoreEx}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 210 : 280);

                cardCreator.AddText($"Mods:", System.Drawing.Color.Gray, hasBeatSaviour ? 15 : 30, 0, hasBeatSaviour ? 240 : 320);
                cardCreator.AddText($"{recentSong.Mods}", System.Drawing.Color.White, hasBeatSaviour ? 15 : 30, hasBeatSaviour ? 140 : 260, hasBeatSaviour ? 240 : 320);

                noteCount = beatSaverMapInfo.Versions.First().Diffs.First(x => x.Difficulty == diff).Notes;
                maxScore = (Convert.ToInt32(noteCount) - 13) * 920 + 4715;
                //maxScore = Convert.ToInt32(noteCount) * 920;
                if (maxScore < 0) maxScore = 0;

            }
            if (hasBeatSaviour)
            {
                cardCreator.AddTextFloatRight($"#{recentSong.Rank}", System.Drawing.Color.White, 50, 500, 0);
                cardCreator.AddTextFloatRight($"{recentSong.Pp}PP", System.Drawing.Color.White, 30, 600, 70);
                cardCreator.AddTextFloatRight($"({Math.Round(recentSong.Pp * recentSong.Weight, 2)}PP)", System.Drawing.Color.White, 15, 500, 85);
                cardCreator.AddTextFloatRight($"{Math.Round(Convert.ToDouble(recentSong.UScore) / Convert.ToDouble(recentSong.MaxScoreEx == 0 ? maxScore : recentSong.MaxScoreEx) * 100, 2)}%", System.Drawing.Color.White, 30, 500, 110);
                var swingloss = (100 - (playerMostRecentLiveData.Trackers.AccuracyTracker.AverageCut[0] + playerMostRecentLiveData.Trackers.AccuracyTracker.AverageCut[2]));
                var pointsWithoutUnderswing = (swingloss * noteCount) * 8;
                var AccWithoutUnderswingAndMisses = (playerMostRecentLiveData.Trackers.AccuracyTracker.AverageCut[1] + 100) * 100 / 115;
                cardCreator.AddTextFloatRight($"{Math.Round(Convert.ToDouble(recentSong.UScore + pointsWithoutUnderswing) / Convert.ToDouble(recentSong.MaxScoreEx == 0 ? maxScore : recentSong.MaxScoreEx) * 100, 2)}%", System.Drawing.Color.White, 20, 500, 160);
                cardCreator.AddTextFloatRight($"{Math.Round(AccWithoutUnderswingAndMisses, 2)}%", System.Drawing.Color.White, 20, 500, 190);
                cardCreator.AddTextFloatRight($"Data from the BeatSavior mod", System.Drawing.Color.Gray, 15, 20, 690);
            }
            else
            {
                cardCreator.AddTextFloatRight($"#{recentSong.Rank}", System.Drawing.Color.White, 50, 0, 0);
                cardCreator.AddTextFloatRight($"{recentSong.Pp}PP", System.Drawing.Color.White, 30, 100, 70);
                cardCreator.AddTextFloatRight($"({Math.Round(recentSong.Pp * recentSong.Weight, 2)}PP)", System.Drawing.Color.White, 15, 0, 85);
                cardCreator.AddTextFloatRight($"{Math.Round(Convert.ToDouble(recentSong.UScore) / Convert.ToDouble(recentSong.MaxScoreEx) * 100, 2)}%", System.Drawing.Color.White, 30, 0, 110);
                cardCreator.AddTextFloatRight($"Obtain the BeatSavior mod to get more stats", System.Drawing.Color.Gray, 15, 20, 690);
            }

            //Create AccGrid If needed
            if (hasBeatSaviour)
            {
                var PbIncrease = playerMostRecentLiveData.Trackers.ScoreTracker.PersonalBest == 0 ? "First Play" : $"{(playerMostRecentLiveData.Trackers.ScoreTracker.RawScore * 100) / playerMostRecentLiveData.Trackers.ScoreTracker.PersonalBest}% Increase";
                cardCreator.AddTextFloatRight($"{PbIncrease}", System.Drawing.Color.White, 20, 500, 225);
                cardCreator.AddTextCenter($"no swingloss", System.Drawing.Color.Gray, 9, 630, 171);
                cardCreator.AddTextCenter($"& no misses", System.Drawing.Color.Gray, 9, 630, 201);

                var misses = "";
                if (playerMostRecentLiveData.Trackers.HitTracker.Miss.ToString() == "0") misses = "FC";
                else misses = playerMostRecentLiveData.Trackers.HitTracker.Miss.ToString();
                cardCreator.AddText($"Misses:", System.Drawing.Color.Gray, 25, 0, 280);
                cardCreator.AddText($"{misses}", System.Drawing.Color.White, 25, 240, 280);

                cardCreator.AddText($"Pauses:", System.Drawing.Color.Gray, 25, 0, 320);
                cardCreator.AddText($"{playerMostRecentLiveData.Trackers.WinTracker.NbOfPause}", System.Drawing.Color.White, 25, 240, 320);

                cardCreator.AddText($"Max Combo:", System.Drawing.Color.Gray, 25, 0, 360);
                cardCreator.AddText($"{playerMostRecentLiveData.Trackers.HitTracker.MaxCombo}", System.Drawing.Color.White, 25, 240, 360);

                cardCreator.AddImage($"https://i.imgur.com/qYAKHbO.png", 235, 420, 80, 80); //Left Hand
                cardCreator.AddImage($"https://i.imgur.com/djd63gV.png", 360, 420, 80, 80); //Right Hand
                cardCreator.AddImage($"https://i.imgur.com/fBQueGL.png", 480, 410, 100, 100); //Both Hands

                cardCreator.AddText($"Acc Total:", System.Drawing.Color.Gray, 25, 0, 510);
                cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AccLeft, 2)}", System.Drawing.Color.White, 25, 275, 510);
                cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AccRight, 2)}", System.Drawing.Color.White, 25, 410, 510);
                cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AverageAcc, 2)}", System.Drawing.Color.White, 25, 530, 510);
                cardCreator.AddTextCenter($"points", System.Drawing.Color.Gray, 15, 630, 520);

                cardCreator.AddText($"Acc Pure:", System.Drawing.Color.Gray, 25, 0, 550);
                cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.LeftAverageCut[1], 2)}", System.Drawing.Color.White, 25, 275, 550);
                cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.RightAverageCut[1], 2)}", System.Drawing.Color.White, 25, 410, 550);
                cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AverageCut[1], 2)}", System.Drawing.Color.White, 25, 530, 550);
                cardCreator.AddTextCenter($"points", System.Drawing.Color.Gray, 15, 630, 560);

                cardCreator.AddText($"Time Dependence:", System.Drawing.Color.Gray, 16, 0, 598);
                cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.LeftTimeDependence, 3)}", System.Drawing.Color.White, 25, 275, 590);
                cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.RightTimeDependence, 3)}", System.Drawing.Color.White, 25, 410, 590);
                cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AverageTimeDependence, 3)}", System.Drawing.Color.White, 25, 530, 590);
                cardCreator.AddTextCenter($"", System.Drawing.Color.Gray, 15, 630, 600);

                cardCreator.AddText($"PreSwing:", System.Drawing.Color.Gray, 25, 0, 630);
                cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.LeftAverageCut[0], 2)}", System.Drawing.Color.White, 25, 275, 630);
                cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.RightAverageCut[0], 2)}", System.Drawing.Color.White, 25, 410, 630);
                cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AverageCut[0], 2)}", System.Drawing.Color.White, 25, 530, 630);
                cardCreator.AddTextCenter($"points", System.Drawing.Color.Gray, 15, 630, 640);

                cardCreator.AddText($"PostSwing:   ", System.Drawing.Color.Gray, 25, 0, 670);
                cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.LeftAverageCut[2], 2)}", System.Drawing.Color.White, 25, 275, 670);
                cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.RightAverageCut[2], 2)}", System.Drawing.Color.White, 25, 410, 670);
                cardCreator.AddTextCenter($"{Math.Round(playerMostRecentLiveData.Trackers.AccuracyTracker.AverageCut[2], 2)}", System.Drawing.Color.White, 25, 530, 670);
                cardCreator.AddTextCenter($"points", System.Drawing.Color.Gray, 15, 630, 680);



                var accGrid = playerMostRecentLiveData.Trackers.AccuracyTracker.GridAcc;
                var hitGrid = playerMostRecentLiveData.Trackers.AccuracyTracker.GridCut;

                var smallestAcc = accGrid.Min(x => x.Double);
                var biggestAcc = accGrid.Max(x => x.Double);


                var gridXReplacement = -20;
                var gridYReplacement = 40;
                cardCreator.AddText($"Acc Grid", System.Drawing.Color.White, 25, 800 + gridXReplacement, 320 + gridYReplacement);

                //Create colors on the grid acc to see what could be improved.
                cardCreator.DrawRectangle(705 + gridXReplacement, 375 + gridYReplacement, 90, 90, createGridColor(accGrid[8].Double));
                cardCreator.DrawRectangle(795 + gridXReplacement, 375 + gridYReplacement, 90, 90, createGridColor(accGrid[9].Double));
                cardCreator.DrawRectangle(885 + gridXReplacement, 375 + gridYReplacement, 90, 90, createGridColor(accGrid[10].Double));
                cardCreator.DrawRectangle(975 + gridXReplacement, 375 + gridYReplacement, 90, 90, createGridColor(accGrid[11].Double));

                cardCreator.DrawRectangle(705 + gridXReplacement, 465 + gridYReplacement, 90, 90, createGridColor(accGrid[4].Double));
                cardCreator.DrawRectangle(795 + gridXReplacement, 465 + gridYReplacement, 90, 90, createGridColor(accGrid[5].Double));
                cardCreator.DrawRectangle(885 + gridXReplacement, 465 + gridYReplacement, 90, 90, createGridColor(accGrid[6].Double));
                cardCreator.DrawRectangle(975 + gridXReplacement, 465 + gridYReplacement, 90, 90, createGridColor(accGrid[7].Double));

                cardCreator.DrawRectangle(705 + gridXReplacement, 555 + gridYReplacement, 90, 90, createGridColor(accGrid[0].Double));
                cardCreator.DrawRectangle(795 + gridXReplacement, 555 + gridYReplacement, 90, 90, createGridColor(accGrid[1].Double));
                cardCreator.DrawRectangle(885 + gridXReplacement, 555 + gridYReplacement, 90, 90, createGridColor(accGrid[2].Double));
                cardCreator.DrawRectangle(975 + gridXReplacement, 555 + gridYReplacement, 90, 90, createGridColor(accGrid[3].Double));

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
                cardCreator.AddText($"Acc Graph", System.Drawing.Color.White, 25, 760, 30);

                Dictionary<float, float> graphPoints = playerMostRecentLiveData.Trackers.ScoreGraphTracker.Graph.ToDictionary(x => (float)Convert.ToDouble(x.Key), x => (float)Convert.ToDouble(x.Value));
                var lowestValue = graphPoints.Values.Min();
                var highestValue = graphPoints.Values.Max();
                var zoomFactor = 25 - (float)(((highestValue - lowestValue) * 100) * 2.8);
                if (zoomFactor < 0) zoomFactor = 2;
                cardCreator.AddAccGraph(740, 90, 300, 250, graphPoints, Convert.ToInt32(playerMostRecentLiveData.SongDuration), 1, System.Drawing.Color.White, zoomFactor);

            }

            await cardCreator.Create($"C:/Users/DaanS/source/repos/BeatSaberBotWeb/BeatSaberBotWeb/wwwroot/img/BeatSaberBot/EmbedBackground-{playerInfo.PlayerId}-{_recentsongNr}-{_isTopSong}-{recentSong.LeaderboardId}.png");

        }

        private async Task<EmbedBuilder> CreateEmbedBuilder(Score recentSong, ScoresaberPlayerFullModel.PlayerInfoModel playerInfo, BeatSaverMapModelNew beatSaverMapInfo)
        {
            var embedBuilder = new EmbedBuilder();
            embedBuilder = new EmbedBuilder
            {
                Title = $"**{recentSong.SongAuthorName} - {recentSong.Name} by {recentSong.LevelAuthorName}**",
                ImageUrl = $"http://beatsaberbot.com/img/BeatSaberBot/EmbedBackground-{playerInfo.PlayerId}-{_recentsongNr}-{_isTopSong}-{recentSong.LeaderboardId}.png",
                Url = $"https://scoresaber.com/leaderboard/{recentSong.LeaderboardId}",
                ThumbnailUrl = $"https://scoresaber.com/imports/images/songs/{recentSong.Id}.png",
                Color = Color.Blue,
                Footer = new EmbedFooterBuilder() { Text = $"Time Set: {recentSong.Timeset.DateTime.ToShortDateString() + " | " + recentSong.Timeset.DateTime.ToShortTimeString()} UTC" }
            };

            embedBuilder.Author = new EmbedAuthorBuilder() { IconUrl = $"https://new.scoresaber.com{playerInfo.Avatar}", Name = $"{ playerInfo.Name}", Url = $"https://scoresaber.com/u/{playerInfo.PlayerId}" };
            try
            {
                var clickables =
              "\n" +
              $"[Download Map]({beatSaverMapInfo?.Versions.First().DownloadUrl}) - " +
              $"[Preview Map](https://skystudioapps.com/bs-viewer/?id={beatSaverMapInfo?.Id}) - " +
              $"[Spotify]({await new Spotify().SearchItem(recentSong.Name, recentSong.SongAuthorName)}) - " +
              $"[Beatsaver](https://beatsaver.com/maps/{beatSaverMapInfo?.Id})";
                embedBuilder.AddField(recentSong.GetDifficulty(), clickables);
            }
            catch (Exception ex)
            {

            }

            return embedBuilder;
        }

        private async Task PostEmbed(SocketMessage message, string playerID, EmbedBuilder embedBuilder)
        {
            _msg = await message.Channel.SendMessageAsync("", false, embedBuilder.Build());
            await _msg.AddReactionAsync(Emote.Parse("<:left:681842980134584355>"));
            await _msg.AddReactionAsync(Emote.Parse("<:right:681843066104971287>"));
            await _msg.AddReactionAsync(new Emoji("🌎"));
            await _msg.AddReactionAsync(new Emoji("📍"));
            _discord.ReactionAdded += _discord_ReactionAdded;

        }

        private async Task _discord_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg3.UserId != 504633036902498314 && arg3.MessageId == _msg.Id)
            {
                if (arg3.Emote.ToString() == "<:left:681842980134584355>")
                {
                    if (arg3.UserId == _msgCreator)
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
                    await _msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                }
                if (arg3.Emote.ToString() == "<:right:681843066104971287>")
                {
                    if (arg3.UserId == _msgCreator)
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
                    await _msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                }
                if (arg3.Emote.ToString() == "🌎")
                {
                    if (_leaderboardToggle)
                    {
                        var embedBuilder = await CreateCardAndGetPlaythroughStatsEmbed(_playerID, _recentsongNr, _isTopSong);
                        await _msg.ModifyAsync(x => x.Embed = embedBuilder.Build());
                        await _msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                        _leaderboardToggle = false;
                        _leaderboardPage = 0;
                        _leaderboardCountryToggle = false;
                    }
                    else
                    {
                        var embedBuilder = await new Leaderboard(_discord).GetPlayersAndCreateEmbed(_mapID, "", 0);
                        await _msg.ModifyAsync(x => x.Embed = embedBuilder.Build());
                        await _msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                        _leaderboardToggle = true;
                        _leaderboardCountryToggle = false;
                    }
                }
                if (arg3.Emote.ToString() == "📍")
                {
                    if (_leaderboardToggle)
                    {
                        var embedBuilder = await CreateCardAndGetPlaythroughStatsEmbed(_playerID, _recentsongNr, _isTopSong);
                        await _msg.ModifyAsync(x => x.Embed = embedBuilder.Build());
                        await _msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                        _leaderboardToggle = false;
                        _leaderboardPage = 0;
                        _leaderboardCountryToggle = false;
                    }
                    else
                    {
                        var embedBuilder = await new Leaderboard(_discord).GetPlayersAndCreateEmbed(_mapID, _countryCode, 0);
                        await _msg.ModifyAsync(x => x.Embed = embedBuilder.Build());
                        await _msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                        _leaderboardToggle = true;
                        _leaderboardCountryToggle = true;
                    }
                }
            }
            return;
        }
    }
}
