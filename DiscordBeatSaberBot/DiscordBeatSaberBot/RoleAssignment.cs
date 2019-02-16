using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DiscordBeatSaberBot
{
    class RoleAssignment
    {
        private DiscordSocketClient _discordSocketClient;
        
        public RoleAssignment(DiscordSocketClient discordSocketClient)
        {
            _discordSocketClient = discordSocketClient;
        }

        public async void MakeRequest(SocketMessage message)
        {
            //command: requestrole
            //command: requestverification
            //Request comes in with DiscordId + ScoresaberID
    
            var DiscordId = message.Author.Id;
            var ScoresaberId = message.Content.Substring(24);

            //GuildID AND ChannelID
            var guild_id = (ulong)505485680344956928;
            var guild_channel_id = (ulong)546386157965934592;

            var guild = _discordSocketClient.Guilds.FirstOrDefault(x => x.Id == guild_id);
            var channel = guild.Channels.First(x => x.Id == guild_channel_id) as IMessageChannel;

            EmbedBuilder embedBuilder = new EmbedBuilder()
            {
                Title = message.Author.Username,
                ThumbnailUrl = message.Author.GetAvatarUrl(),
                Description = "" +
                "**Scoresaber ID:** " + ScoresaberId + "\n" +
                "**Discord ID:** " + DiscordId + "\n" +
                "**Scoresaber link:** https://scoresaber.com/u/" + ScoresaberId + " \n" +
                "*Waiting for approval by a Staff*" + " \n\n" +
                "***(Reminder) Type !bs requestverification [Scoresaber ID]***",


            };

            await channel.SendMessageAsync("", false, embedBuilder.Build());
            await message.DeleteAsync();


        }

        public async void LinkAccount(SocketMessage message)
        {
            bool authenticationCheck()
            {
                var guild = _discordSocketClient.Guilds.FirstOrDefault(x => x.Id == (ulong)505485680344956928);
                var userRoles = guild.GetUser(message.Author.Id).Roles;
                foreach (var role in userRoles)
                {
                    if (role.Name == "Staff")
                    {
                        return true;
                    }
                }
                return false;
            }
            if (!authenticationCheck())
            {
                return;
            }

            var filePath = "../../../account.txt";

            //command: linkaccount
            var parameters = message.Content.Substring(30).Split(" ");
            var DiscordId = parameters[0];
            var ScoresaberId = parameters[1];

            var account = new List<string[]>();

            using (var r = new StreamReader(filePath))
            {
                var json = r.ReadToEnd();
                account = JsonConvert.DeserializeObject<List<string[]>>(json);
            }

            if (account == null || account.Count == 0)
            {
                account = new List<string[]>();
           
            }

            if (account.Contains(new string[] { DiscordId, ScoresaberId}))
            {
                await message.Channel.SendMessageAsync("You already linked your discord");
                return; 
            }
            account.Add(new string[] { DiscordId, ScoresaberId });

            using (var file = File.CreateText(filePath))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, account);
            }
            await message.DeleteAsync();
            await message.Channel.SendMessageAsync("Done! Linked " + DiscordId + " with " + ScoresaberId);
        }

        public ulong GetDiscordIdWithScoresaberId(string scoresaberId)
        {
            var filePath = "../../../account.txt";
            var account = new List<string[]>();
            using (var r = new StreamReader(filePath))
            {
                var json = r.ReadToEnd();
                account = JsonConvert.DeserializeObject<List<string[]>>(json);
            }

            if (account == null || account.Count == 0)
            {
                return 0;
            }
            foreach (var player in account)
            {
                if (player[1] == scoresaberId)
                {
                    return ulong.Parse(player[0]);
                }
            }
            return 0;
        }

    }
}
