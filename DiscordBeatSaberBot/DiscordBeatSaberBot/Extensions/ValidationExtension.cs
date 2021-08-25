using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Discord.WebSocket;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using DiscordBeatSaberBot.Models.ScoreberAPI;

namespace DiscordBeatSaberBot
{
    internal static class ValidationExtension
    {
        public static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        public static async Task<bool> IsDutch(string ID)
        {
            string url = $"https://new.scoresaber.com/api/player/{ID}/full";
            using (var client = new HttpClient())
            {
                var playerInfoRaw = await client.GetAsync(url);
                if (playerInfoRaw.StatusCode != HttpStatusCode.OK) return false;
                var playerInfo = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(playerInfoRaw.Content.ReadAsStringAsync().Result);


                if (playerInfo.playerInfo.Country == "NL")
                {
                    return true;
                }
            }
                return false;
        }

        public static async Task<bool> IsDanish(string ID)
        {
            string url = $"https://new.scoresaber.com/api/player/{ID}/full";
            using (var client = new HttpClient())
            {
                var playerInfoRaw = await client.GetAsync(url);
                if (playerInfoRaw.StatusCode != HttpStatusCode.OK) return false;
                var playerInfo = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(playerInfoRaw.Content.ReadAsStringAsync().Result);


                if (playerInfo.playerInfo.Country == "DK")
                {
                    return true;
                }
            }
            return false;
        }

        public static async Task<bool> IsNotDutch(string ID)
        {
            string url = $"https://new.scoresaber.com/api/player/{ID}/full";
            using (var client = new HttpClient())
            {
                var playerInfoRaw = await client.GetAsync(url);
                if (playerInfoRaw.StatusCode != HttpStatusCode.OK) return false;
                var playerInfo = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(playerInfoRaw.Content.ReadAsStringAsync().Result);


                if (playerInfo.playerInfo.Country != "NL")
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsOwner(ulong Id)
        {
            if (Id == 138439306774577152)
            {
                return true;
            }
            return false;
        }

        public static bool IsDutchAdmin(this SocketGuildUser user) // Staff ID : 505486321595187220
        {
            foreach (var role in user.Roles)
            {
                if (role.Id == 505486321595187220)
                {
                    return true;
                }
            }
            return false;
        }

        public static async Task<bool> IsDutchAdmin(this SocketUser user, DiscordSocketClient discord) // Staff ID : 505486321595187220
        {
            var guildUser = await new GuildService(discord, 505485680344956928).ConvertUserToGuildUser(user);

            if (guildUser == null) return false;

            foreach (var roleId in guildUser.RoleIds)
            {
                if (roleId == 505486321595187220)
                {
                    return true;
                }
            }
            return false;
        }

        public static async Task<bool> IsDutchMod(this SocketUser user, DiscordSocketClient discord) // Staff ID : 505486321595187220
        {
            var guildUser = await new GuildService(discord, 505485680344956928).ConvertUserToGuildUser(user);

            if (guildUser == null) return false;

            foreach (var roleId in guildUser.RoleIds)
            {
                if (roleId == 711348102241583184)
                {
                    return true;
                }
            }
            return false;
        }
        public enum IdentityType
        {
            ScoresaberID,
            DiscordID,                                    
            Username,
            None
        }

        public static async Task<KeyValuePair<IdentityType, string>> GetIdentityFromData(string content)
        {
            if(content == "") return KeyValuePair.Create(IdentityType.None, content);
            // transfer a discordTag to a discordID
            var parameter = content.Replace("<@!", "").Replace(">", "").Trim();

            //Check if the parameter is a username or ID 
            var isUsername = false;
            if (!parameter.All(c => char.IsDigit(c))) isUsername = true;
            else isUsername = false;
            
            if (isUsername)
            {
                return KeyValuePair.Create(IdentityType.Username, parameter);
                
            }
            else
            {
                var scoresaberID = await RoleAssignment.GetScoresaberIdWithDiscordId(parameter);
                if (scoresaberID == "") return KeyValuePair.Create(IdentityType.ScoresaberID, parameter);
                else return KeyValuePair.Create(IdentityType.DiscordID, parameter);
            }            
        }

        public static async  Task<bool> HasCertainRoleInNBSG(this SocketMessage message, DiscordSocketClient discord, params ulong[] RoleId)
        {
            var guildUser = await new GuildService(discord, 505485680344956928).ConvertUserToGuildUser(message.Author);

            if (guildUser == null)
            {
                message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Not in the NBSG Discord", $"You need to be in the Dutch Beat Saber Discord to use this command").Build());
                return false;
            }

            foreach (var roleId in guildUser.RoleIds)
            {
                foreach(var id in RoleId)
                {
                    if (roleId == id)
                    {
                        return true;
                    }
                }                
            }

            var roleNames = "";
            foreach (var id in RoleId)
            {
                roleNames += discord.GetGuild(505485680344956928).GetRole(id).Name + ", ";
            }

            message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Validation Error", $"You do not have the rights to access this command. You would need to have the role: {roleNames}").Build());

            return false;
        }
    }
}