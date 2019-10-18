using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DiscordBeatSaberBot
{
    class BeatSaberHourCounter
    {
        private DiscordSocketClient discord;
        private Logger _logger;

        public BeatSaberHourCounter(DiscordSocketClient discord, bool dontStart = false)
        {
            this.discord = discord;
            _logger = new Logger(discord);
            if (!dontStart)
            {
                UpdateCounterInServer();
            }
            
        }

        //ToDo: Might have bugs
        public void TurnOnCounterForPlayer(SocketGuildUser userOld, SocketGuildUser userNew)
        {
            if (userOld.Activity == null && userNew.Activity == null) return;
            //if (userOld.Activity.Type == ActivityType.Listening && userNew.Activity.Type == ActivityType.Listening) return;
            
            var userOldName = "";
            var userNewName = "";

            if (userOld.Activity == null)
            {
                userOldName = "null";
            }
            else
            {
                userOldName = userOld.Activity.Name;
            }

            if (userNew.Activity == null)
            {
                userNewName = "null";
            }
            else
            {
                userNewName = userNew.Activity.Name;
            }

            if (/*IsStreamingWithBeatsaber(userNew) == false && IsStreamingWithBeatsaber(userOld) || */ userOldName == "Beat Saber" && userNewName != "Beat Saber" ) //Finished Gaming
            {
                var data = getData("../../../DiscordIDBeatsaberHourCounter.txt");
                if (data.Count == 0)
                { // first input
                    var t = new string[] { "0", DateTime.Now.ToString() };
                    data = new Dictionary<ulong, string[]>();
                    data.Add(123456, t);
                }

                var newData = new Dictionary<ulong, string[]>();
                foreach (var discordId in data)
                {
                    if (discordId.Key == userOld.Id)
                    {
                        var dateTime = discordId.Value[1];
                        var totalHoursnew = new TimeSpan();

                        if (dateTime == "")
                        {
                            _logger.Log(Logger.LogCode.error, "DateTime is empty! | " + userNew.Username);
                            return;
                        }
                        else
                        {
                            totalHoursnew = DateTime.Now - DateTime.Parse(dateTime);
                        }

                        if (totalHoursnew.TotalMinutes > 240)
                        {
                            _logger.Log(Logger.LogCode.warning, discord.GetUser(Convert.ToUInt64(discordId.Key)).Username + " heeft 4 uur+ beat saber gespeeld" + "\n " + "Minutes before: " + int.Parse(discordId.Value[0]));
                        }

                        var currentHours = int.Parse(discordId.Value[0]);
                        var totalHours = (int)totalHoursnew.TotalMinutes + currentHours;

                        //discord.GetGuild(505485680344956928).GetTextChannel(550288060919709706).SendMessageAsync("User: " + userNew.Username + " | current minutes:" + currentHours + " | Minutes to add: " + (int)totalHoursnew.TotalMinutes + " | Total now: " + totalHours + " | Start time: " + dateTime);
                    

                        var value = new string[] { totalHours.ToString(), "" };

                        newData.Add(discordId.Key, value);
                    }
                    else
                    {
                        newData.Add(discordId.Key, discordId.Value);
                    }


                }
                setData("../../../DiscordIDBeatsaberHourCounter.txt", newData);
                UpdateCounterInServer();
            }

            if (userOldName != "Beat Saber" && userNewName == "Beat Saber" /* || IsStreamingWithBeatsaber(userNew) */) //Starting Gaming
            {
                var data = getData("../../../DiscordIDBeatsaberHourCounter.txt");
                if (data.Count == 0)
                { // first input
                    var t = new string[] { "0", DateTime.Now.ToString() };
                    data = new Dictionary<ulong, string[]>();
                    data.Add(123456, t);
                }
                var newData = new Dictionary<ulong, string[]>();
                foreach (var discordId in data)
                {
                    if (discordId.Key == userOld.Id)
                    {
                        var hourCount = discordId.Value[0];
                        var dateTime = DateTime.Now;

                        var value = new string[] { hourCount, dateTime.ToString() };

                        newData.Add(discordId.Key, value);

                    }
                    else
                    {
                        newData.Add(discordId.Key, discordId.Value);
                    }

                }

                setData("../../../DiscordIDBeatsaberHourCounter.txt", newData);



            }

            Dictionary<ulong, string[]> getData(string filePath)
            {
                using (var r = new StreamReader(filePath))
                {
                    var json = r.ReadToEnd();
                    var data = JsonConvert.DeserializeObject<Dictionary<ulong, string[]>>(json);
                    if (data == null) return new Dictionary<ulong, string[]>();
                    return data;
                }
            }

            void setData(string filePath, Dictionary<ulong, string[]> newData)
            {
                using (var file = File.CreateText(filePath))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(file, newData);
                }
            }


        }

        public void InsertAndResetAllDutchMembers(DiscordSocketClient discord)
        {
            var guild = discord.GetGuild(505485680344956928);

            var newList = new Dictionary<ulong, string[]>();

            foreach (var user in guild.Users)
            {
                var bo = true;
                foreach (var role in user.Roles)
                {
                    if (role.Id == 572731208107294732) bo = false;
                }
                if(bo) newList.Add(user.Id, new string[] { "0", "" });

            }

            newList.Add(0000, new string[] { "Start date", DateTime.Now.ToString() });

            using (var file = File.CreateText("../../../DiscordIDBeatsaberHourCounter.txt"))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, newList);
            }

            UpdateCounterInServer();
        }

        public Embed GetTop25BeatSaberHours()
        {

            var topdata = getData("../../../DiscordIDBeatsaberHourCounter.txt");

            var embedBuilder = new EmbedBuilder
            {
                Title = "Top 25 Dutch Beat saber hours",
                Description = "***Dit zijn de top25 makkers met de meeste uren in beat saber sinds\n " + topdata.GetValueOrDefault((ulong)0000)[1] + "\n De gene die na een week de meeste uren heeft krijgt de VERSLAAFD role*** \n\n **Naam--------------------Minuten** \n",
                Footer = new EmbedFooterBuilder { Text = "Laatste update: " + DateTime.Now },
                Color = Color.Gold
            };
            var counter = 0;
            topdata.Remove(0000);
            var sortedList = topdata.OrderByDescending(key => int.Parse(key.Value[0]));

            foreach (var top in sortedList)
            {
                try
                {
                    if (counter >= 25 || top.Value[0] == "Start date" || top.Value[0] == "0") continue;
                    var name = discord.GetUser(top.Key).Username;
                    embedBuilder.Description += name + "    : " + top.Value[0] + "\n";
                    counter++;
                }
                catch
                {
                    Console.WriteLine("Error User" + top.Key + " not found!");
                }

            }

            return embedBuilder.Build();


            Dictionary<ulong, string[]> getData(string filePath)
            {
                using (var r = new StreamReader(filePath))
                {
                    var json = r.ReadToEnd();
                    try
                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<ulong, string[]>>(json);
                        if (data == null) return new Dictionary<ulong, string[]>();
                        return data;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    return null; 
                    
                }
            }



        }

        //ToDo: Broken cause of game.
        public bool IsStreamingWithBeatsaber(SocketGuildUser userNew)
        {
            if (userNew.Id == 138439306774577152)
            {

            }
            var TwitchUrl = "";
            try
            {
                dynamic user = userNew;
                var split = user.Game.StreamUrl.Split('/');
                TwitchUrl = split[split.Length - 1];
            }
            catch
            {
                return false;
            }
                
            //Beat saber id: 503116

            try
            {
                var client = new RestClient();
                client.BaseUrl = new Uri(" https://api.twitch.tv/helix/streams?user_login=" + TwitchUrl);
                var request = new RestRequest();
                request.Method = Method.GET;
                //request.Resource = "SilverHaze420";
                request.AddHeader("Client-ID", "iv0p2rfrqqqs6e23d12e1mr07a7h7h");
                request.AddHeader("accept", "application/vnd.twitchtv.v3+json");
                request.RequestFormat = DataFormat.Json;
                var response = client.Execute(request);
                var results = JsonConvert.DeserializeObject<dynamic>(response.Content);
                string stringResult = results.ToString();

                if (stringResult.Contains("503116"))
                {
                    return true;
                }


            }
            catch
            {
                return false;
            }
            return false;
        }

        private async void UpdateCounterInServer()
        {
            var dutchGuild = discord.GetGuild(505485680344956928);
            var txtChannel = dutchGuild.GetTextChannel(572721078359556097);
            var message = await txtChannel.GetMessageAsync(572721530262257675);
            IUserMessage msg = message as IUserMessage;
            await msg.ModifyAsync(x => { x.Embed = GetTop25BeatSaberHours();});
        }

        public void ResetHours()
        {
            var topUser = GetTopDutchHoursUser();
            Console.WriteLine(topUser);
            discord.GetGuild(505485680344956928).GetTextChannel(510959349263499264).SendMessageAsync(
                topUser.Mention + " heeft de meeste uren van de week in beat saber... Congrats! \nJe krijgt de VERSLAAFD role. \nJe kunt nu de command \n\n**!bs rolecolor [Hexcode]**\n\n gebruiken om de kleur van de role aan te passen\n\nDe uren worden nu weer gereset."
                );
            InsertAndResetAllDutchMembers(discord);
        }

        public SocketUser GetTopDutchHoursUser()
        {
            var topdata = getData("../../../DiscordIDBeatsaberHourCounter.txt");

            var counter = 0;
            topdata.Remove(0000);
            var sortedList = topdata.OrderByDescending(key => int.Parse(key.Value[0]));

            var user = discord.GetUser(sortedList.First().Key);
            return user;

            Dictionary<ulong, string[]> getData(string filePath)
            {
                using (var r = new StreamReader(filePath))
                {
                    var json = r.ReadToEnd();
                    try
                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<ulong, string[]>>(json);
                        if (data == null) return new Dictionary<ulong, string[]>();
                        return data;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    return null;

                }
            }

        }


    }
}
