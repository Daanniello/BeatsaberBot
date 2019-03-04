using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DiscordBeatSaberBot
{
    class Server
    {
        private List<string> welkomMessages;

        public ulong ServerId;
        private DiscordSocketClient _discord;


        public Server(DiscordSocketClient discord, ulong serverId = 0)
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
        }

        public async Task UserJoinedMessage(SocketGuildUser guildUser)
        {
            //Welkom message voor de nederlandse beat saber discord.
            if (guildUser.Guild.Id == 505485680344956928)
            {
                var user = _discord.GetUser(guildUser.Id);
                var welkomChannel = guildUser.Guild.Channels.FirstOrDefault(x => x.Name == "welkom") as ISocketMessageChannel;
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                Random r = new Random();

                var embedBuilder = new EmbedBuilder
                {
                    Title = "***Welkom in de Nederlandse Beat saber Discord!*** \n",
                    Description = " \n**Info** \n We zijn een hechte community die samen de top van beat saber wilt bereiken. \n Om de 2 weken wordt er een beat saber multiplayer event gehouden. \n daarnaast wordt er om de maand een VRChat event gehouden.Verschillende andere events kunnen worden georganiseerd.\n"
                        + "\n**Beat saber bot** \n Als bot, Kun je verschillende functie bij mij aanroepen in de discord server :wink: \n Zo kun je met ***!bs help*** al mijn functies vinden. \n Doe dit het liefst in de #bot-commands channel. \n"
                        + "\n**Roles nodig?** \n Rollen geven in deze server verschillende functies en het zegt wat over jouw als beat saber gebruiker. \n In #role-toevoegen kun je reacties toevoegen om een specifieke role te krijgen. Neem een kijkje \n",
                    ThumbnailUrl = "https://cdn.discordapp.com/avatars/504633036902498314/8640cf47aeac6cf7fd071e111467cac5.png?size=256",
                    
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

                await welkomChannel.SendMessageAsync("", false, embedBuilder);
            }
        }

        public bool IsStaffInGuild(ulong discordId, ulong staffID)
        {
            var guild = _discord.GetGuild(ServerId);
            var roles = guild.GetUser(discordId).Roles;
            foreach (var role in roles)
            {
                if (role.Id == staffID)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
