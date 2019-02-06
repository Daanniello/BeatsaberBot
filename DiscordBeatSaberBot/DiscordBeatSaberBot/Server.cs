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
        public string ServerId;
        private DiscordSocketClient _discord;


        public Server(DiscordSocketClient discord, string serverId)
        {
            ServerId = serverId;
            _discord = discord;
        }


        public async Task AddVRroleMessage(SocketMessage user, bool vip = false)
        {
            ulong id = 0;
            if (user == null)
            {
                id = 138439306774577152;
            }
            if (vip == true || id == 138439306774577152)
            {


                var channelId = "510227606822584330";
                var embedBuilder = new EmbedBuilder
                {
                    Title = "**Role Toevoegen**", Description = "Kies een reactie om een role toe te voegen \n\n" +
                    "**Headsets** \n\n" +
                    "<:vive:537368500277084172> Vive \n\n" +
                    "<:oculus:537368385206616075> Oculus \n\n\n" +
                    "**Grips**\n\n" +
                    ":regional_indicator_x: X-Grip \n\n" +
                    ":regional_indicator_b: B-Grip \n\n" +
                    ":regional_indicator_v: V-Grip \n\n" +
                    ":regional_indicator_k: K-Grip \n\n" +
                    ":regional_indicator_r: R-Grip \n\n" +
                    ":regional_indicator_f: F-Grip \n\n" +
                    ":regional_indicator_c: Claw Grip \n\n" +
                    ":regional_indicator_p: Palm Grip \n\n" +
                    ":regional_indicator_t: Tracker Sabers \n\n" +
                    ":new: Andere grip die er niet tussen staat \n\n" +
                    "<:terebilo:508313942297280518> Regular Grip \n\n" +
                    "**Beat saber specials** \n\n" +
                    "🗺 Mapper \n\n" +
                    "💻 Modder \n\n\n" +
                    "**Games** \n\n" +
                    "<:vrchat:537413837100548115> Vrchat \n\n" +
                    "", Color = Color.Gold
                };
                var guild = _discord.GetGuild(505485680344956928);
                var channel = guild.GetTextChannel(510227606822584330);
                var message = await channel.SendMessageAsync("", false, embedBuilder.Build());
                await message.AddReactionAsync(new Emoji("<:vive:537368500277084172>"));
                await message.AddReactionAsync(new Emoji("<:oculus:537368385206616075>"));
            }
        }

        public async Task UserJoinedMessage(SocketGuildUser guildUser)
        {
            //Welkom message voor de nederlandse beat saber discord.
            if (guildUser.Guild.Id == 505485680344956928)
            {
                var user = _discord.GetUser(guildUser.Id);
                var welkomChannel = guildUser.Guild.Channels.FirstOrDefault(x => x.Name == "welkom") as ISocketMessageChannel;
                var dmChannel = await user.GetOrCreateDMChannelAsync();

                var embedBuilder = new EmbedBuilder
                {
                    Title = "***Welkom in de Nederlandse Beat saber Discord!*** \n",
                    Description = " **Info** \n We zijn een hechte community die samen de top van beat saber wilt bereiken. \n Om de 2 weken wordt er een beat saber multiplayer event gehouden.daarnaast wordt er om de maand een VRChat event gehouden.Verschillende andere events kunnen worden georganiseerd.\n"
                        + "**Beat saber bot** \n Als bot, Kun je verschillende functie bij mij aanroepen in de discord server :wink: \n Zo kun je met !bs help al mijn functies vinden. \n Doe dit het liefst in de #bot-commands channel. \n"
                        + "**Roles nodig?** \n Rollen geven in deze server verschillende functies en het zegt wat over jouw als beat saber gebruiker.In #role-toevoegen kun je reacties toevoegen om een specifieke role te krijgen. Neem een kijkje \n",
                    ThumbnailUrl = "https://cdn.discordapp.com/avatars/504633036902498314/8640cf47aeac6cf7fd071e111467cac5.png?size=256"
                };
                var embed = embedBuilder.Build();

                await dmChannel.SendMessageAsync("", false, embed);

                embedBuilder = new EmbedBuilder
                {
                    Title = "Welkom " + "***" + user.Username + "!***",
                    ThumbnailUrl = user.GetAvatarUrl()
                };

                await welkomChannel.SendMessageAsync("", false, embedBuilder);
            }
        }
    }
}
