using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            if (!Validation.IsDigitsOnly(ScoresaberId))
            {
                await message.Channel.SendMessageAsync("Scoresaber ID is verkeerd");
                return;
            }

            //GuildID AND ChannelID
            var guild_id = (ulong)505485680344956928;
            var guild_channel_id = (ulong)549350982081970176;

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
                Color = Color.Orange

            };

            var sendMessage = await channel.SendMessageAsync("", false, embedBuilder.Build());
            await sendMessage.AddReactionAsync(new Emoji("✅"));
            await sendMessage.AddReactionAsync(new Emoji("⛔"));
            
            //await message.DeleteAsync();


        }

        public async Task<bool> LinkAccount(string discordId, string scoresaberId)
        {
            

            var filePath = "../../../account.txt";

            //command: linkaccount
            var DiscordId = discordId;
            var ScoresaberId = scoresaberId;

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
            
           
            foreach (var acc in account)
            {
                if (acc[0] == DiscordId && acc[1] == ScoresaberId)
                {
                    
                    return false;
                }
            }
            account.Add(new string[] { DiscordId, ScoresaberId });

            using (var file = File.CreateText(filePath))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, account);
            }
            //await message.DeleteAsync();
            return true;

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

        public bool CheckIfDiscordIdIsLinked(string DiscordId)
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
                return false;
            }

            foreach (var player in account)
            {
                if (player[0] == DiscordId)
                {
                    return true;
                }
            }
            return false;
        }

        public string GetScoresaberIdWithDiscordId(string DiscordId)
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
                return "0";
            }

            foreach (var player in account)
            {
                if (player[0] == DiscordId)
                {
                    return player[1];
                }
            }
            return "0";
        }

    }
}
