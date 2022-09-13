using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using ScoreSaberLib;
using System.Drawing;
using DiscordBeatSaberBot.Api.BeatSaverApi;

namespace DiscordBeatSaberBot
{
    public class BeatSaberCardCollection
    {
        public BeatSaberCardCollection()
        {

        }

        public static async void DrawAndSendRandomFifaCard(SocketSlashCommand command)
        {
            try
            {
                //Get player info
                var playersDataOne = await new ScoreSaberClient().Api.Players.GetPlayers(page: 1);
                var playersDataTwo = await new ScoreSaberClient().Api.Players.GetPlayers(page: 2);
                var players = playersDataOne.Players;
                players.AddRange(playersDataTwo.Players);
                var player = players[new Random().Next(0, 99)];

                var hashList = new List<string>();
                var scores100 = await new ScoreSaberClient().Api.Players.GetPlayerScores(Convert.ToInt64(player.Id), 100, sort: Players.sort.top, page: 1);
                var scores200 = await new ScoreSaberClient().Api.Players.GetPlayerScores(Convert.ToInt64(player.Id), 100, sort: Players.sort.top, page: 2);
                foreach (var score in scores100) hashList.Add(score.Leaderboard.SongHash);
                foreach (var score in scores200) hashList.Add(score.Leaderboard.SongHash);

                //dynamic beatSaverMaps;
                
                var f = await new BeatSaverApi("").GetMapsByHash(hashList.GetRange(0, 49));
                
                //beatSaverMaps.AddRange(await BeatSaverApi.GetMapsByHash(hashList.GetRange(0, 49).ToList()));
                //beatSaverMaps.AddRange(await BeatSaverApi.GetMapsByHash(hashList.GetRange(50, 99).ToList()));
                //beatSaverMaps.AddRange(await BeatSaverApi.GetMapsByHash(hashList.GetRange(100, 149).ToList()));
                //beatSaverMaps.AddRange(await BeatSaverApi.GetMapsByHash(hashList.GetRange(150, 199).ToList()));

                //Create card 
                var cardCreator = new ImageCreator("../../../Resources/img/FIFA_Card_Template.png");
                cardCreator.AddImageRounded(player.ProfilePicture.ToString(), 0, 0, 735 * 2, 1211, 0.85f, 8);
                cardCreator.AddImageRounded(player.ProfilePicture.ToString(), 278, 250, 400, 400);
                cardCreator.AddImage("../../../Resources/img/FIFA_Card_Template.png", 0, 0, 735, 1211, isLocalFile: true);

                cardCreator.AddText("99", Color.Black, 72, 115 + 5, 175 + 5);
                cardCreator.AddText("99", Color.FromArgb(103, 90, 55), 72, 115, 175);

                cardCreator.AddText(player.Country, Color.FromArgb(103, 90, 55), 48, 125, 285);

                cardCreator.AddImageRounded($"https://www.worldometers.info/img/flags/{player.Country.ToLower()}-flag.gif", 140, 415, 80, 60);

                cardCreator.AddTextCenter(player.Name, Color.Black, 58, 380 + 5, 650 + 5);
                cardCreator.AddTextCenter(player.Name, Color.FromArgb(103, 90, 55), 58, 380, 650);
                await cardCreator.Create($"../../../Resources/img/FIFA_Card-{player.Id}.png");

                //send image in discord and delete it
                await command.Channel.SendFileAsync($"../../../Resources/img/FIFA_Card-{player.Id}.png");
                File.Delete($"../../../Resources/img/FIFA_Card-{player.Id}.png");
            }
            catch(Exception ex)
            {
                var ohoh = ex;
            }
        }

        public static async void DrawAndSendRandomCard(SocketSlashCommand command)
        {
            try
            {
                var top50Global = await ScoresaberAPI.GetTop50Global();
                var player = top50Global.Players[new Random().Next(0, 49)];
                var scoresaberApi = new ScoresaberAPI(player.PlayerId);
                var topSong = await scoresaberApi.GetTopScores();
                var playerInfo = await scoresaberApi.GetPlayerFull();

                var cardCreator = new ImageCreator("../../../Resources/img/CardCollection-Template.png");
                cardCreator.AddImage($"https://new.scoresaber.com/api/static/covers/{topSong.Scores.First().Id}.png", 0, 0, 410, 580);
                cardCreator.AddImage($"https://new.scoresaber.com{player.Avatar}", 35, 55, 350, 240);
                cardCreator.AddImage($"https://new.scoresaber.com/api/static/flags/{player.Country.ToLower()}.png", 355, 25, 40, 25);
                cardCreator.AddText($"{player.PlayerName}", System.Drawing.Color.White, 20, 105, 20);
                cardCreator.AddImage("C:\\Users\\DaanS\\OneDrive\\Documenten\\GitHub\\BeatsaberBot\\DiscordBeatSaberBot\\DiscordBeatSaberBot\\Resources\\Img\\CardCollection-Template.png", 0, 0, 420, 590);

                var skillOne = "Plays Beat Saber";
                if (playerInfo.playerInfo.rank < 5) skillOne = "Reks every other top player!";
                if (playerInfo.playerInfo.CountryRank == 1) skillOne = "Is da best in da country!";
                if (playerInfo.playerInfo.Badges.Count() > 5) skillOne = "Collects badges!";
                cardCreator.AddText($"{skillOne}", System.Drawing.Color.White, 25, 30, 340);

                var skillTwo = "Grinds PP";
                if (playerInfo.scoreStats.TotalPlayCount > 2000) skillTwo = "Played every map \nthat exists (KNOWNLEDGE)";
                if (playerInfo.scoreStats.AvarageRankedAccuracy < 92) skillTwo = "Doesn't like acc";
                cardCreator.AddText($"{skillTwo}", System.Drawing.Color.White, 25, 30, 380);

                cardCreator.AddText($"#{playerInfo.playerInfo.rank}", System.Drawing.Color.White, 20, 310, 22);

                await cardCreator.Create($"../../../Resources/img/CardCollection-{player.PlayerId}.png");


                await command.Channel.SendFileAsync($"../../../Resources/img/CardCollection-{player.PlayerId}.png");
                File.Delete($"../../../Resources/img/CardCollection-{player.PlayerId}.png");
            }
            catch
            {

            }
        }
    }
}
