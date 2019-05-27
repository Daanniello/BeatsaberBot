using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DiscordBeatSaberBot
{
    class UpdateTimer
    {
        private DiscordSocketClient discord;

        public UpdateTimer(DiscordSocketClient discord, int hours, int minutes = 0, int seconds = 0)
        {
            this.discord = discord;
            var timespan = new TimeSpan(0, hours, minutes, seconds);

            var thread = new Thread(() => Update(timespan));
            thread.Start();


        }

        public async Task Update(TimeSpan timespan)
        {
            while (true)
            {
                await DutchDiscordUserCount();

                await Task.Delay(timespan);
            }
        }

        public async Task DutchDiscordUserCount()
        {
            //discord.GetGuild(505485680344956928).GetTextChannel(572721078359556097).SendMessageAsync("Test");
            var guild = discord.GetGuild(505485680344956928);
            var message = await guild.GetTextChannel(572721078359556097).GetMessageAsync(574215648373375025);
            var msg = (IUserMessage) message;

            var embedbuilder = new EmbedBuilder
            {
                Title = guild.Name + " info",
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder{Name = "Aantal gebruikers", Value = guild.Users.Count},
                    new EmbedFieldBuilder{Name = "Server gemaakt op", Value = guild.CreatedAt},
                    new EmbedFieldBuilder{Name = "Aantal emotes", Value = guild.Emotes.Count},
                },
                Footer = new EmbedFooterBuilder { Text = "Laatste update: " + DateTime.Now },
                Color = Color.Red,
            };

            await msg.ModifyAsync(text => text.Embed = embedbuilder.Build());
        }
    }
}
