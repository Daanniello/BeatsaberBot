using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DiscordBeatSaberBot
{
    internal class UpdateTimer
    {
        private readonly Logger _logger;
        private readonly DiscordSocketClient discord;

        public UpdateTimer(DiscordSocketClient discord)
        {
            this.discord = discord;
            _logger = new Logger(discord);
        }

        public void Start(Func<Task> method, string methodName, int hours, int minutes = 0, int seconds = 0)
        {
            //The time that the function needs to be called
            var timespan = new TimeSpan(0, hours, minutes, seconds);

            try
            {
                var task = new Task(() => Update(timespan, method, methodName));
                task.Start();
                //var thread = new Thread(() => Update(timespan, method, methodName));
                //thread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateTimer Exception");
                Console.WriteLine(ex);
            }
            
        }

        public async Task Update(TimeSpan timespan, Func<Task> method, string methodName)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine($"Updating.. {methodName}");
                    await method();
                }
                catch (Exception ex)
                {
                    _logger.Log(Logger.LogCode.error, ex.ToString());
                }
                await Task.Delay(timespan);
            }
        }

        public async Task DutchDiscordUserCount(DateTime startTime)
        {
            //discord.GetGuild(505485680344956928).GetTextChannel(572721078359556097).SendMessageAsync("Test");
            var guild = discord.GetGuild(505485680344956928);
            var message = await guild.GetTextChannel(572721078359556097).GetMessageAsync(574215648373375025);
            var msg = (IUserMessage)message;

            var embedbuilder = new EmbedBuilder
            {
                Title = guild.Name + " info",
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder {Name = "Aantal gebruikers", Value = guild.Users.Count},
                    new EmbedFieldBuilder {Name = "Server gemaakt op", Value = guild.CreatedAt},
                    new EmbedFieldBuilder {Name = "Aantal emotes", Value = guild.Emotes.Count},
                    new EmbedFieldBuilder {Name = "Beat Saber bot uptime", Value = "Days: " + (DateTime.Now - startTime).Days + " Hours: " + (DateTime.Now - startTime).Hours}
                },
                Footer = new EmbedFooterBuilder { Text = "Laatste update: " + DateTime.Now },
                Color = Color.Red
            };

            await msg.ModifyAsync(text => text.Embed = embedbuilder.Build());
        }

        public async Task AutomaticUnMute()
        {
            var mutedPeople = await DatabaseContext.ExecuteSelectQuery("Select * from PlayerInCountry where Muted IS NOT NULL");
            foreach(var muted in mutedPeople)
            {
                var dateUntillUnmute = Convert.ToDateTime(muted[2]);
                if (DateTime.Now > dateUntillUnmute)
                {
                    new RoleAssignment(discord).UnMutePerson(Convert.ToUInt64(muted[0]), Convert.ToUInt64(muted[1]));
                }
            }
            
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
            string date = time[0].Replace("*", "").Trim();
            var realDate = DateTime.ParseExact(date, "d-M-yyyy HH:mm:ss",
                CultureInfo.InvariantCulture);

            double timeLeft = Math.Round(realDate.Subtract(DateTime.Now).TotalHours);
            if (timeLeft == 1)
            {
                //var msg = await eventDetailChannel.SendMessageAsync("<@&538905985050476545>");
                //await msg.DeleteAsync();
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
    }
}