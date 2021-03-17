using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace DiscordBeatSaberBot
{
    public class BeatSaberCardCollection
    {
        public BeatSaberCardCollection()
        {

        }

        public static async void DrawAndSendRandomCard(SocketMessage message)
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
            

            await message.Channel.SendFileAsync($"../../../Resources/img/CardCollection-{player.PlayerId}.png");
            File.Delete($"../../../Resources/img/CardCollection-{player.PlayerId}.png");
        }
    }
}
