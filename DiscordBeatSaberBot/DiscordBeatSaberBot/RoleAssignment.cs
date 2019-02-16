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
            //Request comes in with DiscordId + ScoresaberID
            var parameters = message.Content.Substring(22).Split(" ");
            var DiscordId = parameters[0];
            var ScoresaberId = parameters[1];

            //GuildID AND ChannelID
            var guild_id = (ulong)7887;
            var guild_channel_id = (ulong)34234;

            var guild = _discordSocketClient.Guilds.FirstOrDefault(x => x.Id == guild_id);
            var channel = guild.Channels.First(x => x.Id == guild_channel_id) as IMessageChannel;

            EmbedBuilder embedBuilder = new EmbedBuilder()
            {
                Title = message.Author.Username,
                ThumbnailUrl = message.Author.GetAvatarUrl(),
                Description = "" +
                "**Scoresaber ID:** " + ScoresaberId + "\n" +
                "**Discord ID:** " + DiscordId + "\n" +
                "**Scoresaber link:** https://scoresaber.com/u/" + ScoresaberId,


            };

            await channel.SendMessageAsync("", false, embedBuilder.Build());
            await message.DeleteAsync();


        }

        public void LinkAccount(SocketMessage message)
        {
            bool authenticationCheck()
            {
                var guild = _discordSocketClient.Guilds.FirstOrDefault(x => x.Id == (ulong)34324);
                var userRoles = guild.GetUser(message.Author.Id).Roles;
                foreach (var role in userRoles)
                {
                    if (role.Name == "staff")
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
            var parameters = message.Content.Substring(22).Split(" ");
            var DiscordId = parameters[0];
            var ScoresaberId = parameters[1];

            var account = new List<string[]>();

            using (var r = new StreamReader(filePath))
            {
                var json = r.ReadToEnd();
                account = JsonConvert.DeserializeObject<List<string[]>>(json);
            }

            if (account.Contains(new string[] { DiscordId, ScoresaberId}))
            {
                return; 
            }
            account.Add(new string[] { DiscordId, ScoresaberId });

            using (var file = File.CreateText(filePath))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, account);
            }
        }

    }
}
