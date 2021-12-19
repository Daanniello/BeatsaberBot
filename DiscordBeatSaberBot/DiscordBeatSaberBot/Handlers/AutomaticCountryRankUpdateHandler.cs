using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using ScoreSaberLib;
using ScoreSaberLib.Models;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace DiscordBeatSaberBot.Handlers
{
    public class AutomaticCountryRankUpdateHandler
    {
        private string _savePath = "../../../Resources/CountriesTop500Players/";
        public List<CountryDiscordInfo> CountriesToUpdate = new List<CountryDiscordInfo>();
        private DiscordSocketClient _discord;

        //To add a new country, Add it here as an enum and give info in the contructor
        public enum country
        {
            NL,
        }

        public AutomaticCountryRankUpdateHandler(DiscordSocketClient discord)
        {
            _discord = discord;

            //Add Netherlands----------------------------------------------------------------------------------------------------------------------------------------------------------
            var discordDutchRankRolesList = new Dictionary<int, long>();
            discordDutchRankRolesList.Add(1, 505488768011337738);
            discordDutchRankRolesList.Add(3, 505488871165788180);
            discordDutchRankRolesList.Add(10, 505488925129965571);
            discordDutchRankRolesList.Add(25, 505488963394600960);
            discordDutchRankRolesList.Add(50, 505500125792043008);
            discordDutchRankRolesList.Add(100, 505700269552697344);
            discordDutchRankRolesList.Add(250, 505700349177495563);
            discordDutchRankRolesList.Add(500, 505700397676101632);
            CountriesToUpdate.Add(new CountryDiscordInfo() { country = country.NL, discordServerID = 505485680344956928, rankRolesByRoleID = discordDutchRankRolesList, unrankedRoleID = 740567773918396467, lastTopRoleID = 505700472972115968, unverifiedRoleID = 549351808506658857, verifiedRoleID = 573459086293598209, serverOwnerID = 138439306774577152, foreignChannelID = 729279152712056902 });
            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        }

        //Updates all ranks from every country, only the players who ranked down or up
        public async Task UpdateRanks()
        {
            var scoresaberClient = new ScoreSaberClient();

            //Update ranks from each country
            foreach (var country in CountriesToUpdate)
            {
                var allPlayersFromScoresaber = new List<PlayerInfoModel.Player>();
                //Get top 500 players == 10 pages of 50 from scoresaber (new data)
                for (var x = 0; x < 10; x++)
                {
                    var playersPage = await scoresaberClient.Api.Players.GetPlayers(countryCodes: "NL", page: x + 1);
                    allPlayersFromScoresaber.AddRange(playersPage.Players);
                }

                //Get top 500 stores in the json file (old data)
                var allPlayersFromJson = OpenTop500fromJson(country.country);
                if (allPlayersFromJson == null) SaveTop500asJson(allPlayersFromScoresaber, country.country);

                //decide who to rank up/down
                if (allPlayersFromJson != null)
                {
                    foreach (var playerFromScoresaber in allPlayersFromScoresaber)
                    {
                        var playerFromJson = allPlayersFromJson.FirstOrDefault(x => x.Id == playerFromScoresaber.Id);
                        if (playerFromJson == null) continue;
                        if (playerFromScoresaber.CountryRank != playerFromJson.CountryRank)
                        {
                            var rankRoleFromJson = 0;
                            var rankRoleFromScoresaber = 0;
                            //Get the rankRoles from discord
                            foreach (var discordRankRole in country.rankRolesByRoleID.Keys)
                            {
                                if (playerFromJson.CountryRank <= discordRankRole)
                                {
                                    rankRoleFromJson = discordRankRole;
                                    break;
                                }
                            }
                            foreach (var discordRankRole in country.rankRolesByRoleID.Keys)
                            {
                                if (playerFromScoresaber.CountryRank <= discordRankRole)
                                {
                                    rankRoleFromScoresaber = discordRankRole;
                                    break;
                                }
                            }

                            //If these are not equal then this user must be updates.
                            if (rankRoleFromJson != rankRoleFromScoresaber && rankRoleFromScoresaber != 0)
                            {
                                var rankRoleIDtoUpdateTo = country.rankRolesByRoleID[rankRoleFromScoresaber];
                                var scoresaberID = playerFromScoresaber.Id;
                                var discordID = await new RoleAssignment(_discord).GetDiscordIdWithScoresaberId(scoresaberID);
                                if (discordID != 0)
                                {
                                    IGuild guild = _discord.GetGuild((ulong)country.discordServerID);
                                    IGuildUser user = await guild.GetUserAsync((ulong)discordID);
                                    if (user == null) await guild.DownloadUsersAsync();
                                    user = await guild.GetUserAsync((ulong)discordID);
                                    if (user == null) continue;

                                    //remove other rank roles 
                                    foreach (var roleID in user.RoleIds)
                                    {
                                        foreach (var roleid in country.rankRolesByRoleID.Values)
                                        {
                                            if (roleID == (ulong)roleid) await user.RemoveRoleAsync(guild.GetRole(roleID));
                                        }
                                    }

                                    //add new rank role
                                    var roleToAdd = guild.Roles.First(x => x.Id == (ulong)rankRoleIDtoUpdateTo);
                                    await user.AddRoleAsync(roleToAdd);
                                }
                            }
                        }
                    }

                    SaveTop500asJson(allPlayersFromScoresaber, country.country);
                }
            }
        }


        //Saves the top 500 players from a country into a json file.
        public bool SaveTop500asJson(List<PlayerInfoModel.Player> top500Players, country country)
        {
            try
            {
                var json = JsonConvert.SerializeObject(top500Players);
                if (File.Exists(_savePath + $"{country}.json")) File.WriteAllText(_savePath + $"{country}.json", json);
                else File.AppendAllText(_savePath + $"{country}.json", json);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<PlayerInfoModel.Player> OpenTop500fromJson(country country)
        {
            try
            {
                var json = File.ReadAllText(_savePath + $"{country}.json");
                return JsonConvert.DeserializeObject<List<PlayerInfoModel.Player>>(json);
            }
            catch
            {
                return null;
            }
        }

        //Updates everyones rank in all country discords, even if they didnt rank up or down
        public async Task ForceUpdateRanks(CountryDiscordInfo country, SocketMessage message)
        {
            var msg = await message.Channel.SendMessageAsync("Starting the rank role update. This might take a while...");

            var scoresaberClient = new ScoreSaberClient();
            //Once a week or something 

            IGuild guild = _discord.GetGuild((ulong)country.discordServerID);
            await guild.DownloadUsersAsync();
            var users = await guild.GetUsersAsync();

            var playersFromJson = OpenTop500fromJson(country.country);
            if (playersFromJson == null)
            {
                await msg.ModifyAsync(x => x.Content = $"Country doesnt have a json file yet, this will probably be created in some minutes");
                return;
            }

            var count = 0;
            foreach (var user in users)
            {
                if (user.IsBot) continue;
                count++;
                await msg.ModifyAsync(x => x.Content = $"User {count} / {users.Count}");
                //Remove all roles 
                var rolesToRemove = new List<IRole>();
                foreach (var roleID in country.rankRolesByRoleID)
                {
                    rolesToRemove.Add(guild.Roles.First(x => x.Id == (ulong)roleID.Value));
                }
                var verifiedRole = guild.Roles.First(x => x.Id == (ulong)country.verifiedRoleID);
                var unverifiedRole = guild.Roles.First(x => x.Id == (ulong)country.unrankedRoleID);
                var lastTopRole = guild.Roles.First(x => x.Id == (ulong)country.lastTopRoleID);
                var unrankedRole = guild.Roles.First(x => x.Id == (ulong)country.unrankedRoleID);
                rolesToRemove.Add(lastTopRole);
                rolesToRemove.Add(unverifiedRole);
                rolesToRemove.Add(verifiedRole);

                await user.RemoveRolesAsync(rolesToRemove);

                var scoresaberID = await RoleAssignment.GetScoresaberIdWithDiscordId(user.Id.ToString());
                //Is the user linked with the bot? 
                if (scoresaberID != "")
                {
                    var player = playersFromJson.FirstOrDefault(x => x.Id == scoresaberID);

                    //Is player found in the top 500? 
                    if (player != null)
                    {
                        await user.AddRoleAsync(verifiedRole);
                        var rankRoleFromScoresaber = 0;
                        foreach (var discordRankRole in country.rankRolesByRoleID.Keys)
                        {
                            if (player.CountryRank <= discordRankRole)
                            {
                                rankRoleFromScoresaber = discordRankRole;
                                break;
                            }
                        }

                        await user.AddRoleAsync(guild.Roles.First(x => x.Id == (ulong)country.rankRolesByRoleID[rankRoleFromScoresaber]));
                    }
                    else
                    {
                        var exactPlayer = await scoresaberClient.Api.Players.GetPlayer(Convert.ToInt64(scoresaberID));
                        //Can the player be found on scoresaber? 
                        if(exactPlayer != null)
                        {
                            //accept if the player doesnt have foreignChannel and has the correct country 
                            if (user.RoleIds.Where(x => x == (ulong)country.foreignChannelID).Count() == 0 && exactPlayer.Country == country.country.ToString())
                            {
                                await user.AddRoleAsync(verifiedRole);
                                //Is the player inactive? 
                                if (exactPlayer.CountryRank != 0)
                                {
                                    await user.AddRoleAsync(lastTopRole);
                                }
                                await user.AddRoleAsync(unrankedRole);
                            }
                            else
                            {
                                if(exactPlayer.Country == country.country.ToString())
                                {
                                    await user.AddRoleAsync(verifiedRole);
                                    await user.AddRoleAsync(unrankedRole);
                                }
                            }
                        }
                        else
                        {

                        }                                             
                    }
                }
                else
                {
                    if (user.RoleIds.Where(x => x == (ulong)country.foreignChannelID).Count() == 0)
                    {
                        await user.AddRoleAsync(unverifiedRole);
                        await user.AddRoleAsync(unrankedRole);
                    }
                }
            }
            await msg.ModifyAsync(x => x.Content = $"Done updating roles. Updated {users.Count} users.");
        }

        public class CountryDiscordInfo
        {
            public country country { get; set; }
            public long discordServerID { get; set; }

            public Dictionary<int, long> rankRolesByRoleID;
            public long unrankedRoleID;
            public long lastTopRoleID;
            public long verifiedRoleID;
            public long unverifiedRoleID;
            public long serverOwnerID;
            public long foreignChannelID;
        }
    }
}
