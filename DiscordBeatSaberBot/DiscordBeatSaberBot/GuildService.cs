using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DiscordBeatSaberBot
{
    internal class GuildService
    {
        private readonly DiscordSocketClient _discord;

        public ulong ServerId;
        public SocketGuild Guild;

        private readonly List<string> welkomMessages;


        public GuildService(DiscordSocketClient discord, ulong serverId = 0)
        {
            welkomMessages = new List<string>();
            welkomMessages.Add("just joined the server - glhf!");
            welkomMessages.Add("just joined. Everyone, look busy!");
            welkomMessages.Add("just joined. Can I get a heal?");
            welkomMessages.Add("joined your party.");
            welkomMessages.Add("just joined. Hide your bananas.");
            welkomMessages.Add("just arrived. Seems OP - please nerf.");
            welkomMessages.Add("just slid into the server.");
            welkomMessages.Add("hopped into the server. Kangaroo!!");
            welkomMessages.Add("just showed up. Hold my beer.");

            ServerId = serverId;
            _discord = discord;
            Guild = _discord.Guilds.FirstOrDefault(x => x.Id == serverId);
        }

        public async Task UserJoinedMessage(SocketGuildUser guildUser)
        {
            //Welkom message voor de nederlandse beat saber discord.
            if (guildUser.Guild.Id == 505485680344956928)
            {
                var user = _discord.GetUser(guildUser.Id);
                var welkomChannel = guildUser.Guild.Channels.FirstOrDefault(x => x.Name == "welkom") as ISocketMessageChannel;
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                var r = new Random();

                var embedBuilder = new EmbedBuilder
                {
                    Title = "***Welkom in de Nederlandse Beat saber Discord!*** \n",
                    Description = " \n**Info** \n We zijn een hechte community die samen de top van beat saber wilt bereiken. \n"
                                  + "\n**Beat saber bot** \n Als bot, Kun je verschillende functies bij mij aanroepen. \n Zo kun je met ***!bs help*** al mijn functies bekijken. \n Doe dit het liefst in de #bot-commands channel. \n"
                                  + "\n**Roles nodig?** \n Rollen geven in deze server verschillende functies en het zegt wat over jouw als beat saber gebruiker. \n In #role-toevoegen kun je reacties toevoegen om een specifieke role te krijgen. Neem een kijkje \n",
                    ThumbnailUrl = "https://cdn.discordapp.com/avatars/504633036902498314/8640cf47aeac6cf7fd071e111467cac5.png?size=256"
                };
                var embed = embedBuilder.Build();
                await dmChannel.SendFileAsync(AppDomain.CurrentDomain.BaseDirectory + "\\NLBS.png");
                await dmChannel.SendMessageAsync("", false, embed);

                embedBuilder = new EmbedBuilder
                {
                    Title = "***" + user.Username + " ***" + welkomMessages[r.Next(9)],
                    Color = Color.Red,
                    ThumbnailUrl = user.GetAvatarUrl()
                };

                await welkomChannel.SendMessageAsync("", false, embedBuilder.Build());
            }
        }

        public bool IsStaffInGuild(ulong discordId, ulong staffID)
        {
            var guild = _discord.GetGuild(ServerId);
            var roles = guild.GetUser(discordId).Roles;
            foreach (var role in roles)
            {
                if (role.Id == staffID)
                    return true;
            }

            return false;
        }

        public async Task<bool> AddRole(string roleName, SocketUser user)
        {
            var guildUser = ConvertUserToGuildUser(user);
            var userRoles = guildUser.Roles;
            foreach (var role in Guild.Roles)
            {
                if (role.Name == roleName)
                {
                    var addedRole = Guild.Roles.FirstOrDefault(x => x.Name == roleName);
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
            foreach (var role in Guild.Roles)
            {
                if (role.Id == roleId)
                {
                    var addedRole = Guild.Roles.FirstOrDefault(x => x.Id == roleId);
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
                    var addedRole = Guild.Roles.FirstOrDefault(x => x.Id == roleId);
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
                    var addedRole = Guild.Roles.FirstOrDefault(x => x.Name == roleName);
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
            var guild = _discord.Guilds.FirstOrDefault(x => x.Id == ServerId);
            return guild.GetUser(user.Id);
        }
    }
}