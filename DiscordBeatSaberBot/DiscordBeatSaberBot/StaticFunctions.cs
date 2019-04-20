using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    class StaticFunctions
    {
        static public async Task SendDMMessageWithTime(DiscordSocketClient discord, ulong userID, TimeSpan timeTillSending, string message)
        {
            
            var user = discord.GetUser(userID);
            var dmChannel = await user.GetOrCreateDMChannelAsync();
            Console.WriteLine("Sending Message to " + user.Username + " in " + timeTillSending.ToString() + " Sec");
            await Task.Delay(timeTillSending);
            await dmChannel.SendMessageAsync(message);
            Console.WriteLine("Message Send to " + user.Username);

        }
    }
}
