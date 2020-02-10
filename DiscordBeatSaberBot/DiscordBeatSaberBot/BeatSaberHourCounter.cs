using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using RestSharp;

namespace DiscordBeatSaberBot
{
    internal class BeatSaberHourCounter
    {
        private readonly Logger _logger;
        private readonly DiscordSocketClient discord;

        public BeatSaberHourCounter(DiscordSocketClient discord, bool dontStart = false)
        {
            this.discord = discord;
            _logger = new Logger(discord);
            if (!dontStart)
                UpdateCounterInServer();
        }

        //ToDo: Might have bugs
        public void TurnOnCounterForPlayer(SocketGuildUser userOld, SocketGuildUser userNew)
        {

            if (userOld.Activity == null && userNew.Activity == null) return;
            //if (userOld.Activity.Type == ActivityType.Listening && userNew.Activity.Type == ActivityType.Listening) return;

            string userOldName = "";
            string userNewName = "";

            if (userOld.Activity == null)
                userOldName = "null";
            else
                userOldName = userOld.Activity.Name;

            if (userNew.Activity == null)
                userNewName = "null";
            else
                userNewName = userNew.Activity.Name;

            Game userOldGameStatus = (Game) userOld.Activity;
            Game userOldNewStatus = (Game)userNew.Activity;


            if ( /*IsStreamingWithBeatsaber(userNew) == false && IsStreamingWithBeatsaber(userOld) || */ userOldName.Contains("Beat Saber") && userNewName.Contains("Beat Saber")) //Finished Gaming
            {


                var data = getData("../../../DiscordIDBeatsaberHourCounter.txt");
                if (data.Count == 0)
                {
                    // first input
                    var t = new[] {"0", ""};
                    data = new Dictionary<ulong, string[]>();
                    data.Add(280054293955084288, t);
                }

                var newData = new Dictionary<ulong, string[]>();
                foreach (var discordId in data)
                {
                    if (discordId.Key == userOld.Id)
                    {
                        string dateTime = discordId.Value[1];
                        var totalHoursnew = new TimeSpan();

                        if (dateTime == "")
                        {
                            _logger.Log(Logger.LogCode.error, "DateTime is empty! | " + userNew.Username);
                            return;
                        }

                        totalHoursnew = DateTime.Now - DateTime.Parse(dateTime);
                        _logger.Log(Logger.LogCode.debug, userNew.Username + " | Adding hours -> | " + totalHoursnew);


                        if (totalHoursnew.TotalMinutes > 240)
                            _logger.Log(Logger.LogCode.warning, discord.GetUser(Convert.ToUInt64(discordId.Key)).Username + " heeft 4 uur+ beat saber gespeeld" + "\n " + "Minutes before: " + int.Parse(discordId.Value[0]));

                        int currentHours = int.Parse(discordId.Value[0]);
                        int totalHours = (int) totalHoursnew.TotalMinutes + currentHours;

                        //discord.GetGuild(505485680344956928).GetTextChannel(550288060919709706).SendMessageAsync("User: " + userNew.Username + " | current minutes:" + currentHours + " | Minutes to add: " + (int)totalHoursnew.TotalMinutes + " | Total now: " + totalHours + " | Start time: " + dateTime);


                        var value = new[] {totalHours.ToString(), ""};

                        newData.Add(discordId.Key, value);
                    }
                    else
                    {
                        newData.Add(discordId.Key, discordId.Value);
                    }
                }

                if (newData == null)
                {
                    _logger.Log(Logger.LogCode.fatal_error, "Inputted Data for Beat saber hour counter END is null!! | " + newData.ToString());
                    return;
                }

                setData("../../../DiscordIDBeatsaberHourCounter.txt", newData);
                UpdateCounterInServer();
            }

            if(userOld.Username == "The Cutest Ahri")
            {
                var t = userNew;
                //activity = custom status 
            }

            if (!userOldName.Contains("Beat Saber") && userNewName.Contains("Beat Saber") /* || IsStreamingWithBeatsaber(userNew) */) //Starting Gaming
            {

                var data = getData("../../../DiscordIDBeatsaberHourCounter.txt");
                if (data.Count == 0)
                {
                    // first input
                    var t = new[] { "0", "" };
                    data = new Dictionary<ulong, string[]>();
                    data.Add(280054293955084288, t);
                }

                var newData = new Dictionary<ulong, string[]>();
                foreach (var discordId in data)
                {
                    if (discordId.Key == userOld.Id)
                    {
                        string hourCount = discordId.Value[0];
                        var dateTime = DateTime.Now;

                        var value = new[] {hourCount, dateTime.ToString()};

                        newData.Add(discordId.Key, value);
                    }
                    else
                    {
                        newData.Add(discordId.Key, discordId.Value);
                    }
                }

                if(newData == null)
                {
                    _logger.Log(Logger.LogCode.fatal_error, "Inputted Data for Beat saber hour counter START is null!! | " + newData.ToString());
                    return;
                }

                setData("../../../DiscordIDBeatsaberHourCounter.txt", newData);
            }

            Dictionary<ulong, string[]> getData(string filePath)
            {
                using (var r = new StreamReader(filePath))
                {
                    string json = r.ReadToEnd();
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
                bool bo = true;
                foreach (var role in user.Roles)
                {
                    if (role.Id == 572731208107294732) bo = false;
                }

                if (bo) newList.Add(user.Id, new[] {"0", ""});
            }

            newList.Add(0, new[] {"Start date", DateTime.Now.ToString()});

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
                Description = "***Dit zijn de top25 makkers met de meeste uren in beat saber sinds\n " + topdata.GetValueOrDefault((ulong) 0)[1] + "\n De gene die na een week de meeste uren heeft krijgt de VERSLAAFD role*** \n\n **Naam--------------------Minuten** \n",
                Footer = new EmbedFooterBuilder {Text = "Laatste update: " + DateTime.Now},
                Color = Color.Gold
            };
            int counter = 0;
            topdata.Remove(0);
            var sortedList = topdata.OrderByDescending(key => int.Parse(key.Value[0]));

            foreach (var top in sortedList)
            {
                try
                {
                    if (counter >= 25 || top.Value[0] == "Start date" || top.Value[0] == "0") continue;
                    string name = discord.GetUser(top.Key).Username;
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
                    string json = r.ReadToEnd();
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
            if (userNew.Id == 138439306774577152) { }

            string TwitchUrl = "";
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
                    return true;
            }
            catch
            {
                return false;
            }

            return false;
        }

        private async Task UpdateCounterInServer()
        {
            var dutchGuild = discord.GetGuild(505485680344956928);
            var txtChannel = dutchGuild.GetTextChannel(572721078359556097);
            IMessage message = null;
            try
            {
                message = await txtChannel.GetMessageAsync(572721530262257675);
                var msg = message as IUserMessage;
                await msg.ModifyAsync(x => { x.Embed = GetTop25BeatSaberHours(); });
            }
            catch (Exception ex)
            {
                _logger.Log(Logger.LogCode.fatal_error, ex.ToString());
                Console.WriteLine(ex);
            }
        }

        public void ResetHours()
        {
            var topUser = GetTopDutchHoursUser();
            Console.WriteLine(topUser);

            var embedBuilder = new EmbedBuilder()
            {
                Title = "User met de meeste uren van de week",
                Description = topUser.Mention + " heeft de meeste uren van de week in beat saber... Congrats! \nJe krijgt de VERSLAAFD role. \nJe kunt nu de command \n\n**!bs rolecolor [Hexcode]**\n\n gebruiken om de kleur van de role aan te passen\n\nDe uren worden nu weer gereset."
            };

            discord.GetGuild(505485680344956928).GetTextChannel(510959349263499264).SendMessageAsync("", false, embedBuilder.Build());

            InsertAndResetAllDutchMembers(discord);
        }

        public SocketUser GetTopDutchHoursUser()
        {
            var topdata = getData("../../../DiscordIDBeatsaberHourCounter.txt");

            int counter = 0;
            topdata.Remove(0000);
            var sortedList = topdata.OrderByDescending(key => int.Parse(key.Value[0]));

            var user = discord.GetUser(sortedList.First().Key);
            return user;

            Dictionary<ulong, string[]> getData(string filePath)
            {
                using (var r = new StreamReader(filePath))
                {
                    string json = r.ReadToEnd();
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