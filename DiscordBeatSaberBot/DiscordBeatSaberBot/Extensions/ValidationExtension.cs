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

        public static async Task<bool> IsLanguage(string ID, string language)
        {
            string url = $"https://new.ScoreSaber.com/api/player/{ID}/full";
            using (var client = new HttpClient())
            {
                var playerInfoRaw = await client.GetAsync(url);
                if (playerInfoRaw.StatusCode != HttpStatusCode.OK) return false;
                var playerInfo = JsonConvert.DeserializeObject<ScoreSaberPlayerFullModel>(playerInfoRaw.Content.ReadAsStringAsync().Result);

                return playerInfo.playerInfo.Country == language;
            }
        }

        public static async Task<bool> IsDutch(string ID)
        {
            return await IsLanguage(ID, "NL");
        }

        public static async Task<bool> IsDanish(string ID)
        {
            return await IsLanguage(ID, "DK");
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