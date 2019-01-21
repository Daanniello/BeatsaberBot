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


        public async Task AddVRroleMessage(SocketMessage user)
        {

            if (user.Author.Id == 138439306774577152)
            {


                var channelId = "510227606822584330";
                var embedBuilder = new EmbedBuilder
                {
                    Title = "Vive Or Oculus?", Description = "Kies een reactie om je role toe te voegen", Color = Color.Gold
                };

                var message = await _discord.GetGuild(505485680344956928).GetTextChannel(510227606822584330).SendMessageAsync("", false, embedBuilder.Build());
                await message.AddReactionAsync(new Emoji("⬅"));
                await message.AddReactionAsync(new Emoji("➡"));
                await message.AddReactionAsync(new Emoji("🚫"));
            }
        }

        public async Task AddRole()
        {

        }
    }
}
