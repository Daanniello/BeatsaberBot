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
                Console.WriteLine("Message Sent to " + user.Username);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task SendDMMessage(this SocketUser user, string message, DiscordSocketClient discord)
        {
            try
            {
                var userd = discord.GetUser(user.Id);
                var DMChannel = await userd.GetOrCreateDMChannelAsync();
                await DMChannel.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}