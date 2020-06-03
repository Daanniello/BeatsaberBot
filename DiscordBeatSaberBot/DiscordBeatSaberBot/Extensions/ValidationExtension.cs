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
            string url = "https://scoresaber.com/u/" + ID;
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

        public static bool IsDutchAdmin(this SocketUser user, DiscordSocketClient discord) // Staff ID : 505486321595187220
        {
            var guildUser = new GuildService(discord, 505485680344956928).ConvertUserToGuildUser(user);

            if (guildUser == null) return false;

            foreach (var role in guildUser.Roles)
            {
                if (role.Id == 505486321595187220)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsDutchMod(this SocketUser user, DiscordSocketClient discord) // Staff ID : 505486321595187220
        {
            var guildUser = new GuildService(discord, 505485680344956928).ConvertUserToGuildUser(user);

            if (guildUser == null) return false;

            foreach (var role in guildUser.Roles)
            {
                if (role.Id == 711348102241583184)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasCertainRoleInNBSG(this SocketMessage message, DiscordSocketClient discord, params ulong[] RoleId)
        {
            var guildUser = new GuildService(discord, 505485680344956928).ConvertUserToGuildUser(message.Author);

            if (guildUser == null)
            {
                message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Not in the NBSG Discord", $"You need to be in the Dutch Beat Saber Discord to use this command").Build());
                return false;
            }

            foreach (var role in guildUser.Roles)
            {
                foreach(var id in RoleId)
                {
                    if (role.Id == id)
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