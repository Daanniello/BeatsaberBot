using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    class Danskbog
    {
        private List<TimeSpan> schedule;
        private List<string> scheduleMessage;
        private TimeSpan timeNow;
        private DiscordSocketClient discord;

        public Danskbog(DiscordSocketClient discord)
        {
            this.discord = discord;
            timeNow = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Minute);
            var ontbijt = new TimeSpan(11, 00, 00);
            var middag = new TimeSpan(13, 00, 00);
            var voorAvond = new TimeSpan(15, 00, 00);
            var avond = new TimeSpan(19, 00, 00);
            var nacht = new TimeSpan(22, 00, 00);
            var slapen = new TimeSpan(01, 00, 00);

            var mention = "<@221373638979485696> <@138439306774577152>";
            var ontbijtMessage = "**11:00** - Gudmweningn <3, vergeet niet iets lichts te eten. Is goed voor je energie voor het begin van de dag =3 " + mention;
            var middagMessage = "**13:00** - Hii dansk. hope you are doing ok ^^, vergeet je drankje niet en maybe een kleine snack" + mention;
            var voorAvondMessage = "**15:00** - -w- heheh middagsnack tijd!! neem een tosti en wat fruit :3 vergeet je drankje niet! " + mention;
            var avondMessage = "**19:00** - nomnomnom avond eten =3 maak er wat lekkers van eetsmakelijk. en je drankje niet vergeten" + mention;
            var nachtMessage = "**22:00** - Je laatste drankje van de dag ^^ " + mention;
            var slapenMessage = "**01:00** - nacht voedsel hehehe ;3 en noten woahh... slaaplekker alvast <3 " + mention;


            schedule = new List<TimeSpan>();
            scheduleMessage = new List<string>();

            schedule.Add(ontbijt);
            schedule.Add(middag);
            //schedule.Add(voorAvond);
            schedule.Add(avond);
            schedule.Add(nacht);
            schedule.Add(slapen);

            scheduleMessage.Add(ontbijtMessage);
            scheduleMessage.Add(middagMessage);
            //scheduleMessage.Add(voorAvondMessage);
            scheduleMessage.Add(avondMessage);
            scheduleMessage.Add(nachtMessage);
            scheduleMessage.Add(slapenMessage);



            TimerRunning(new CancellationToken());


        }

        private async void TimerRunning(CancellationToken token)
        {
            try
            {
                var watch = Stopwatch.StartNew();
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var sleep = GetNextMeal();
                        Console.WriteLine("Waiting for next food time for danskbog <3..." + sleep);
                        try
                        {
                            await Task.Delay((int)(sleep.Item1.Negate().TotalMilliseconds) - (int)(watch.ElapsedMilliseconds % 1000), token);
                        }
                        catch
                        {
                            await Task.Delay((int)(sleep.Item1.TotalMilliseconds) - (int)(watch.ElapsedMilliseconds % 1000), token);
                            //await Task.Delay((int)(sleep.Item1.TotalMilliseconds) - (int)(watch.ElapsedMilliseconds % 1000), token);
                        }
                        await Task.Delay(60000);
                        ISocketMessageChannel channel = (ISocketMessageChannel)discord.GetGuild(439514151040057344).GetChannel(552874394255360008);
                        var guild = discord.GetGuild(439514151040057344);
                        var channelid = guild.GetTextChannel(552874394255360008);
                        try
                        {
                            await channelid.SendMessageAsync(scheduleMessage[sleep.Item2]);
                        }
                        catch
                        {
                            await channelid.SendMessageAsync(scheduleMessage[0]);
                        }
                        

                        Console.WriteLine("Food Time!");

                    }
                    catch (TaskCanceledException)
                    {

                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            
        }

        private (TimeSpan, int) GetNextMeal()
        {
            var timeNow = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Minute);
            TimeSpan temp = new TimeSpan();
            TimeSpan temp2 = new TimeSpan(22,0,0);
            var count = 0;
            Console.WriteLine(timeNow);
            foreach (var mealtime in schedule)
            {
                var value = timeNow.Subtract(mealtime);
                if (temp > value)
                {
                    if (timeNow > new TimeSpan(22,0,0))
                    {

                    }
                    else
                    {
                        Console.WriteLine(value);
                        return (value, count);
                    }
                    
                }
                else
                {
                    if (temp2 < timeNow)
                    {
                        return (timeNow.Subtract(new TimeSpan(22,0,0)).Add(new TimeSpan(3,0,0)), 0);
                    }
                    
                  
                }
                
                count++;
            }
            Console.WriteLine(temp2);
            return (temp2, 0);

        }
    }
}
