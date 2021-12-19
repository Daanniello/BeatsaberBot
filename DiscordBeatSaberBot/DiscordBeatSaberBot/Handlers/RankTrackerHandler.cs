using Discord.WebSocket;
using Newtonsoft.Json;
using ScoreSaberLib;
using ScoreSaberLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Handlers.RankTrackerHandler
{
    public class RankTrackerHandler
    {
        private string _savePath = "../../../Resources/RankTracker/ranktrackerlist.json";
        private DiscordSocketClient _discord;

        public RankTrackerHandler(DiscordSocketClient discord)
        {
            _discord = discord;
        }

        public async Task CheckForAllRankChanges()
        {
            var scoresaberClient = new ScoreSaberClient();
            var players = OpenRankTrackerListFromJson();
            if (players == null) return;
            var updatedPlayers = new List<PlayerInfoModel.Player>();
            var roleAssignment = new RoleAssignment(_discord);
            foreach (var player in players)
            {
                var oldPlayerData = player;
                var newPlayerData = await scoresaberClient.Api.Players.GetPlayer(Convert.ToInt64(player.Id));
                if (newPlayerData == null) continue;
                updatedPlayers.Add(newPlayerData);

                if (oldPlayerData.Rank != newPlayerData.Rank)
                {
                    var discordID = await roleAssignment.GetDiscordIdWithScoresaberId(player.Id);
                    if (discordID == 0) continue;
                    NotifyPlayerOfRankChanges(discordID, oldPlayerData, newPlayerData);
                }
            }

            SaveRankTrackerListToJson(updatedPlayers);
        }

        private async void NotifyPlayerOfRankChanges(ulong discordID, PlayerInfoModel.Player oldPlayerData, PlayerInfoModel.Player newPlayerData)
        {

            try
            {
                var user = await _discord.Rest.GetUserAsync(discordID);
                var dm = await user.GetOrCreateDMChannelAsync();

                var embedBuilder = EmbedBuilderExtension.EmbedBuilder();
                embedBuilder.Title = "Rank Tracker";
                embedBuilder.Color = oldPlayerData.Rank > newPlayerData.Rank ? Discord.Color.Green : Discord.Color.Red;
                embedBuilder.ThumbnailUrl = newPlayerData.ProfilePicture.ToString();
                embedBuilder.Url = $"https://scoresaber.com/u/{newPlayerData.Id}";

                var description = "";
                if (oldPlayerData.Rank > newPlayerData.Rank) description = $"Your rank has improved from **{oldPlayerData.Rank}** to **{newPlayerData.Rank}**";
                if (oldPlayerData.Rank < newPlayerData.Rank) description = $"Your rank has lowered from **{oldPlayerData.Rank}** to **{newPlayerData.Rank}**";

                embedBuilder.Description = description;


                await dm.SendMessageAsync("", false, embedBuilder.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
            }
        }


        public async Task<bool> DeletePlayerFromRankTracker(long discordID)
        {
            var scoresaberID = await RoleAssignment.GetScoresaberIdWithDiscordId(discordID.ToString());
            if (scoresaberID == "") return false;
            var players = OpenRankTrackerListFromJson();
            if (players == null) return false;
            var player = players.FirstOrDefault(x => x.Id == scoresaberID.ToString());
            if (player == null) return false;
            players.Remove(player);
            return SaveRankTrackerListToJson(players);
        }

        public async Task<bool> IsBeingTracked(long discordID)
        {
            var scoresaberID = await RoleAssignment.GetScoresaberIdWithDiscordId(discordID.ToString());
            if (scoresaberID == "") return false;
            var players = OpenRankTrackerListFromJson();
            if (players == null) return false;
            if (players.FirstOrDefault(x => x.Id.ToString() == scoresaberID) != null) return true;
            return false;
        }
        public async Task<bool> AddPlayerToRankTracker(long discordID)
        {
            var scoresaberID = await RoleAssignment.GetScoresaberIdWithDiscordId(discordID.ToString());
            if (scoresaberID == "") return false;
            var scoresaberClient = new ScoreSaberClient();
            var players = OpenRankTrackerListFromJson();
            var player = await scoresaberClient.Api.Players.GetPlayer(Convert.ToInt64(scoresaberID));
            if (players == null) players = new List<PlayerInfoModel.Player>();
            players.Add(player);
            return SaveRankTrackerListToJson(players);
        }

        private bool SaveRankTrackerListToJson(List<PlayerInfoModel.Player> rankTrackerPlayers)
        {
            try
            {
                var json = JsonConvert.SerializeObject(rankTrackerPlayers);
                if (File.Exists(_savePath)) File.WriteAllText(_savePath, json);
                else File.AppendAllText(_savePath, json);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private List<PlayerInfoModel.Player> OpenRankTrackerListFromJson()
        {
            try
            {
                var json = File.ReadAllText(_savePath);
                return JsonConvert.DeserializeObject<List<PlayerInfoModel.Player>>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
