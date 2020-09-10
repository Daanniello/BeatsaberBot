using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace DiscordBeatSaberBot
{
    internal static class MessageDelete
    {

        private static List<ulong> allowedUsers = new List<ulong>() {
            612726222601846793
        };

        public static async Task<bool> DeleteMessageCheck(SocketMessage message, DiscordSocketClient discord)
        {
            var channels = new Dictionary<ulong, Func<Task<bool>>>
            {
                //{ 549350982081970176, async () => await DisallowMessagesIfNotBotCommand(message) }, // Verbind je discord
                { 537377323742265344, async () => await DisallowMessages(message) },
                { 627174908484517888, async () => await AllowFile(message) } //Fan art channel
            };

            if (!channels.Keys.Contains(message.Channel.Id)) return false;
            if (allowedUsers.Contains(message.Author.Id)) return false;

            var allowed = await channels[message.Channel.Id]();

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

        private static async Task<bool> DisallowMessagesIfNotBotCommand(SocketMessage message)
        {
            if (message.Content.Contains("!bs"))
            {
                return false;
            }            
            else
            {
                var messageCached = await message.Channel.SendMessageAsync("Text messages are not allowed in this channel.");
                await Task.Delay(2000);
                await messageCached.DeleteAsync();
                return false;
            }          
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