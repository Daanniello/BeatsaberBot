using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    class ReactionEvent
    {
        private DiscordSocketClient discordSocketClient;
        private ulong NLroleAddChannelId = 510227606822584330;
        private ulong NLguildId = 505485680344956928;

        public ReactionEvent(DiscordSocketClient discordSocketClient)
        {
            this.discordSocketClient = discordSocketClient;
        }

        private async Task<Task> ReactionAdded(Cacheable<IUserMessage, ulong> cache,
            ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            //var t = new Server(discordSocketClient, "");
            //await t.AddVRroleMessage();
            //510227606822584330
            //{<:vive:537368500277084172>}
            //{<:oculus:537368385206616075>}

            if (channel.Id == NLroleAddChannelId)
            {
                var guild = discordSocketClient.GetGuild(NLguildId);
                var user = guild.GetUser(reaction.UserId);
                //await (user as IGuildUser).AddRoleAsync(new role);
                if (reaction.Emote.ToString() == "⬅")
                {
                    var role = guild.Roles.FirstOrDefault(x => x.Name == "Vive");
                    await (user as IGuildUser).AddRoleAsync(role);
                }

                if (reaction.Emote.ToString() == "➡")
                {
                    var role = guild.Roles.FirstOrDefault(x => x.Name == "Oculus");
                    await (user as IGuildUser).AddRoleAsync(role);
                }

                if (reaction.Emote.ToString() == "🚫")
                {
                    await (user as IGuildUser).RemoveRolesAsync(guild.Roles.Where(x => x.Name == "Oculus" || x.Name == "Vive"));
                }
                //{🚫}
            }
            return Task.CompletedTask;
        }
    }
}
