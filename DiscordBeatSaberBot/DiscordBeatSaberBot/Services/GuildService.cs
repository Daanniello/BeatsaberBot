using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBeatSaberBot.Handlers;

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

        public async Task UserJoinedMessage(RestGuildUser guildUser)
        {
            var guild = _discord.GetGuild(guildUser.GuildId);
            //Welkom message voor de nederlandse beat saber discord.
            if (guild.Id == 505485680344956928)
            {
                var user = _discord.GetUser(guildUser.Id);
                var welkomChannel = guild.Channels.FirstOrDefault(x => x.Name == "welkom") as ISocketMessageChannel;
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

                var userDescription = "Geen extra informatie";
                var interviewData = await DatabaseContext.ExecuteSelectQuery($"Select * from PlayerInterview where DiscordId={guildUser.Id}");
                var interviewQuestions = WelcomeInterviewHandler.interviewQuestions;
                if (interviewData.Count != 0)
                {
                    userDescription = $"" +
                    $"***{interviewQuestions[0]}***\n{interviewData[0][1]}\n\n" +
                    $"***{interviewQuestions[1]}***\n{interviewData[0][2]}\n\n" +
                    $"***{interviewQuestions[2]}***\n{interviewData[0][3]}\n\n" +
                    $"***{interviewQuestions[3]}***\n{interviewData[0][4]}\n\n" +
                    $"***{interviewQuestions[4]}***\n{interviewData[0][5]}\n\n" +
                    $"***{interviewQuestions[5]}***\n{interviewData[0][6]}";
                }

                embedBuilder = new EmbedBuilder
                {
                    Title = "***" + user.Username + " ***" + welkomMessages[r.Next(9)],
                    Color = Color.Red,
                    Description = userDescription,
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

        public async Task<bool> AddRole(string roleName, IUser user)
        {
            var guildUser = await ConvertUserToGuildUser(user);
            var userRoles = guildUser.RoleIds;
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
            var guildUser = await ConvertUserToGuildUser(user);
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

        public async Task<bool> DeleteRole(ulong roleId, IUser user)
        {
            var guildUser = await ConvertUserToGuildUser(user);

            foreach (var role in Guild.Roles)
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

        public async Task<bool> DeleteRole(string roleName, IUser user)
        {
            var guildUser = await ConvertUserToGuildUser(user);

            foreach (var role in Guild.Roles)
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

        public async Task<bool> UserHasRole(SocketUser user, string roleName)
        {
            var guildUser = await ConvertUserToGuildUser(user);
            if (guildUser == null) return false;
            var userRoles = _discord.GetGuild(guildUser.GuildId);
            foreach (var role in userRoles.Roles)
            {
                if (role.Name == roleName)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<RestGuildUser> ConvertUserToGuildUser(IUser user)
        {
            
            var guild = _discord.Guilds.FirstOrDefault(x => x.Id == ServerId);
            return await _discord.Rest.GetGuildUserAsync(guild.Id, user.Id);
        }

        //Checks if a user should be verified.
        //If verified -> add role 
        //If unverivied but is verified -> add role and delete unverified 
        public async void VerifiedDutchUsersCheck()
        {
            var guild = _discord.GetGuild(505485680344956928);
            var verifiedRole = guild.GetRole(573459086293598209);
            var unverifiedRole = guild.GetRole(549351808506658857);

            var verifiedUsers = await DatabaseContext.ExecuteSelectQuery("Select * from PlayerInCountry where GuildId = 505485680344956928");
            foreach (var verifiedUser in verifiedUsers)
            {


                var user = _discord.GetGuild(505485680344956928).GetUser(Convert.ToUInt64(verifiedUser[0]));

                if (user != null)
                {


                    await user.AddRoleAsync(verifiedRole);
                    await user.RemoveRoleAsync(unverifiedRole);

                    Console.WriteLine("Added verified to " + user.Username);
                    Console.WriteLine("Removed unverified to " + user.Username);
                }
            }
        }

        public async void VerifiedDutchUsersCheck2()
        {
            var guild = _discord.GetGuild(505485680344956928);
            var verifiedRole = guild.GetRole(573459086293598209);
            var unverifiedRole = guild.GetRole(549351808506658857);
            var verifiedUsers = await DatabaseContext.ExecuteSelectQuery("Select * from PlayerInCountry where GuildId = 505485680344956928");

            foreach (var user in guild.Users)
            {
                var a = false;
                foreach (var vuser in verifiedUsers)
                {
                    if (Convert.ToUInt64(vuser[0]) == user.Id) a = true;
                }
                if (!a)
                {
                    await user.RemoveRoleAsync(verifiedRole);
                    Console.WriteLine("Removed unverified to " + user.Username);
                }
            }
        }
    }
}