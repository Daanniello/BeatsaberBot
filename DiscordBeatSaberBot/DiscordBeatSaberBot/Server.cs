using System;
using System.Collections.Generic;
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
                    Title = "***Role Toevoegen***", Description = "Kies een reactie om een role toe te voegen \n \n" +
                    "***Headsets*** \n" +
                    "<:vive:537368500277084172> Vive \n" +
                    "<:oculus:537368385206616075> Oculus \n" +
                    "\n" +
                    "\n" +
                    "\n" +
                    "\n" +
                    "\n" +
                    "\n", Color = Color.Gold
                };
                var guild = _discord.GetGuild(505485680344956928);
                var channel = guild.GetTextChannel(510227606822584330);
                var message = await channel.SendMessageAsync("", false, embedBuilder.Build());
                await message.AddReactionAsync(new Emoji("<:vive:537368500277084172>"));
                await message.AddReactionAsync(new Emoji("<:oculus:537368385206616075>"));
            }
        }

        public async Task AddRole()
        {

        }
    }
}
