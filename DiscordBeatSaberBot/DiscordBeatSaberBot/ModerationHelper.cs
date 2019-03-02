using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    class ModerationHelper
    {
        private DiscordSocketClient _discord;
        private ulong _guildId;
        public SocketGuild _guild;

        public ModerationHelper(DiscordSocketClient discord, ulong GuildId)
        {
            _discord = discord;
            _guildId = GuildId;
            _guild = _discord.Guilds.FirstOrDefault(x => x.Id == _guildId);
        }

        public async Task<bool> AddRole(string roleName, SocketUser user)
        {
            var guildUser = ConvertUserToGuildUser(user);       
            var userRoles = guildUser.Roles;
            foreach (var role in userRoles)
            {
                if (role.Name == roleName)
                {
                    var addedRole = _guild.Roles.FirstOrDefault(x => x.Name == roleName);
                    await guildUser.AddRoleAsync(addedRole);
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> AddRole(ulong roleId, SocketUser user)
        {
            var guildUser = ConvertUserToGuildUser(user);       
            var userRoles = guildUser.Roles;
            foreach (var role in userRoles)
            {
                if (role.Id == roleId)
                {
                    var addedRole = _guild.Roles.FirstOrDefault(x => x.Id == roleId);
                    await guildUser.AddRoleAsync(addedRole);
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> DeleteRole(ulong roleId, SocketUser user)
        {
            var guildUser = ConvertUserToGuildUser(user);
            var userRoles = guildUser.Roles;
            foreach (var role in userRoles)
            {
                if (role.Id == roleId)
                {
                    var addedRole = _guild.Roles.FirstOrDefault(x => x.Id == roleId);
                    await guildUser.RemoveRoleAsync(addedRole);
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> DeleteRole(string roleName, SocketUser user)
        {
            var guildUser = ConvertUserToGuildUser(user);
            var userRoles = guildUser.Roles;
            foreach (var role in userRoles)
            {
                if (role.Name == roleName)
                {
                    var addedRole = _guild.Roles.FirstOrDefault(x => x.Name == roleName);
                    await guildUser.RemoveRoleAsync(addedRole);
                    return true;
                }
            }
            return false;
        }

        public bool UserHasRole(SocketUser user, string roleName)
        {
            var guildUser = ConvertUserToGuildUser(user);
            var userRoles = guildUser.Roles;
            foreach (var role in userRoles)
            {
                if (role.Name == roleName)
                {
                    return true;
                }
            }
            return false;
        }

        private SocketGuildUser ConvertUserToGuildUser(SocketUser user)
        {
            var guild = _discord.Guilds.FirstOrDefault(x => x.Id == _guildId);
            return guild.GetUser(user.Id);
        }
    }
}
