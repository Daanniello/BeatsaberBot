using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace DiscordBeatSaberBot
{
    internal static class MessageDelete
    {
        public static async Task<bool> DeleteMessageCheck(SocketMessage message, DiscordSocketClient discord)
        {
            var channels = new Dictionary<ulong, Func<Task<bool>>>();
            channels.Add(549350982081970176, async () => await DisallowMessages(message));
            channels.Add(537377323742265344, async () => await DisallowMessages(message));
            channels.Add(627174908484517888, async () => await AllowFile(message)); //Fan art channel

            if (!channels.Keys.Contains(message.Channel.Id)) return false;

            bool allowed = await channels[message.Channel.Id]();

            if (!allowed) await message.DeleteAsync();

            return !allowed;
        }

        private static async Task<bool> DisallowMessages(SocketMessage message)
        {
            var messageCached = await message.Channel.SendMessageAsync("Text messages are not allowed in this channel.");
            await Task.Delay(2000);
            await messageCached.DeleteAsync();
            return false;
        }

        private static async Task<bool> AllowFile(SocketMessage message)
        {
            if (((dynamic) message.Attachments).Length <= 0)
            {
                var messageCached = await message.Channel.SendMessageAsync("Text messages are not allowed in this channel. Only messages with files attached are allowed.");
                await Task.Delay(2000);
                await messageCached.DeleteAsync();
                return false;
            }

            return true;
        }
    }
}