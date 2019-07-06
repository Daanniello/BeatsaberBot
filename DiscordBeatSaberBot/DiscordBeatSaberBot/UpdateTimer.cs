using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private Logger _logger;

        public UpdateTimer(DiscordSocketClient discord)
        {


            this.discord = discord;
            _logger = new Logger(discord);
        }

        public void Start(Func<Task> method, int hours, int minutes = 0, int seconds = 0)
        {
          
            var timespan = new TimeSpan(0, hours, minutes, seconds);

            var thread = new Thread(() => Update(timespan, method));
            thread.Start();
        }

        public async Task Update(TimeSpan timespan, Func<Task> method)
        {
            while (true)
            {
                try
                {
                    await method();
                }
                catch(Exception ex)
                {
                    _logger.Log(Logger.LogCode.error, ex.ToString());
                }

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

        public async Task EventNotification()
        {
       
                var eventDetailChannel = (ISocketMessageChannel)discord.GetChannel(572721078359556097);
                var embededMessage = (IUserMessage)await eventDetailChannel.GetMessageAsync(586248421715738629);

                var embedInfo = embededMessage.Embeds.First();
                var guild = discord.Guilds.FirstOrDefault(x => x.Id == (ulong)505485680344956928);

                if (embedInfo.Title.Contains("Geen events"))
                {
                var embedBuilder2 = new EmbedBuilder
                {
                    Title = "Geen events op het moment",
                    Description = "",
                    Footer = new EmbedFooterBuilder { Text = "" },
                    Color = embedInfo.Color

                };

                await embededMessage.ModifyAsync(msg => msg.Embed = embedBuilder2.Build());
                await embededMessage.RemoveAllReactionsAsync();
                return;
            }

                var time = embedInfo.Description.Split('\n');
                var date = time[0].Replace("*", "").Trim();
                var realDate = DateTime.ParseExact(date, "d-M-yyyy HH:mm:ss",
                                           System.Globalization.CultureInfo.InvariantCulture);

                var timeLeft = Math.Round(realDate.Subtract(DateTime.Now).TotalHours);
                if (timeLeft == 1)
                {
                    var msg = await eventDetailChannel.SendMessageAsync("<@&538905985050476545>");
                    await msg.DeleteAsync();
                }

                if (timeLeft < 0)
                {
                    var embedBuilder2 = new EmbedBuilder
                    {
                        Title = "Geen events op het moment",
                        Description = "",
                        Footer = new EmbedFooterBuilder { Text = "Start over: " + timeLeft + " uur" },
                        Color = embedInfo.Color

                    };

                    await embededMessage.ModifyAsync(msg => msg.Embed = embedBuilder2.Build());
                    return;
                }

                var embedBuilder = new EmbedBuilder
                {
                    Title = embedInfo.Title,
                    Description = embedInfo.Description,
                    Footer = new EmbedFooterBuilder { Text = "Start over: " + timeLeft + " uur" },
                    Color = embedInfo.Color

                };

                await embededMessage.ModifyAsync(msg => msg.Embed = embedBuilder.Build());
            
   
        }

        public async Task DutchWeeklyHoursCheck()
        {

        }
    }
}
