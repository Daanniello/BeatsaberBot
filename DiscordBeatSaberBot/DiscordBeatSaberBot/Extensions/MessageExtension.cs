using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace DiscordBeatSaberBot
{
    internal static class MessageExtension
    {
        public static async Task SendDMMessageWithTime(DiscordSocketClient discord, ulong userID, TimeSpan timeTillSending, string message)
        {
            try
            {

                var user = discord.GetUser(userID);
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                Console.WriteLine("Sending Message to " + user.Username + " in " + timeTillSending + " Sec");
                await Task.Delay(timeTillSending);
                await dmChannel.SendMessageAsync(message);
                Console.WriteLine("Message Send to " + user.Username);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}