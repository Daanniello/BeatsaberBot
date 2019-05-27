using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    static class MessageDelete
    {
        

        public async static Task<bool> DeleteMessageCheck(SocketMessage message, DiscordSocketClient discord)
        {
            
            List<ulong> channels = new List<ulong> {
                549350982081970176,
                537377323742265344
            };
   
            if (channels.Contains(message.Channel.Id))
            {
                var user = discord.GetGuild(505485680344956928).GetUser(message.Author.Id) as IGuildUser;

                foreach(var id in user.RoleIds)
                {
                    if (id == 505486321595187220)
                    {
                        return false;
                    }
                }

                await Task.Delay(2000);
                await message.DeleteAsync();
                return true;
            }
            return false;
        }
    }
}
