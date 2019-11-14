using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace DiscordBeatSaberBot
{
    internal class RoleAssignment
    {
        private readonly DiscordSocketClient _discordSocketClient;

        public RoleAssignment(DiscordSocketClient discordSocketClient)
        {
            _discordSocketClient = discordSocketClient;
        }

        public async void MakeRequest(SocketMessage message)
        {
            //command: requestrole
            //command: requestverification
            //Request comes in with DiscordId + ScoresaberID

            ulong DiscordId = message.Author.Id;
            string ScoresaberId = message.Content.Substring(24);
            ScoresaberId = Regex.Replace(ScoresaberId, "[^0-9]", "");

            if (!ValidationExtension.IsDigitsOnly(ScoresaberId))
            {
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Wrong scoresaber ID", "Get you link from the url from your scoresaber page.", null, null).Build());

                return;
            }

            //GuildID AND ChannelID
            ulong guild_id = 505485680344956928;
            ulong guild_channel_id = 549350982081970176;

            var guild = _discordSocketClient.Guilds.FirstOrDefault(x => x.Id == guild_id);
            var channel = guild.Channels.First(x => x.Id == guild_channel_id) as IMessageChannel;

            var embedBuilder = new EmbedBuilder
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
            string filePath = "../../../DutchAccounts.txt";

            //command: linkaccount
            string DiscordId = discordId;
            string ScoresaberId = scoresaberId;

            var account = new List<string[]>();

            using (var r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                account = JsonConvert.DeserializeObject<List<string[]>>(json);
            }

            if (account == null || account.Count == 0)
                account = new List<string[]>();


            foreach (var acc in account)
            {
                if (acc[0] == DiscordId && acc[1] == ScoresaberId)
                    return false;
            }

            account.Add(new[] { DiscordId, ScoresaberId });

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
            string filePath = "../../../DutchAccounts.txt";
            var account = new List<string[]>();
            using (var r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                account = JsonConvert.DeserializeObject<List<string[]>>(json);
            }

            if (account == null || account.Count == 0)
                return 0;
            foreach (var player in account)
            {
                if (player[1] == scoresaberId)
                    return ulong.Parse(player[0]);
            }

            return 0;
        }

        public bool CheckIfDiscordIdIsLinked(string DiscordId)
        {
            var DutchAccountsPath = "../../../DutchAccounts.txt";
            var GlobalAccountsPath = "../../../GlobalAccounts.txt";

            var DutchAccounts = JsonExtension.GetJsonData(DutchAccountsPath);
            var GlobalAccounts = JsonExtension.GetJsonData(GlobalAccountsPath);

            if (DutchAccounts == null && GlobalAccounts == null)
                return false;

            if (DutchAccounts.Count == 0 && GlobalAccounts.Count == 0)
                return false;

            var condition = false;

            foreach (var player in DutchAccounts)
            {
                if (player.Key == DiscordId)
                    condition = true;
            }

            foreach (var player in GlobalAccounts)
            {
                if (player.Key == DiscordId)
                    condition = true;
            }

            return condition;
        }

        public string GetScoresaberIdWithDiscordId(string DiscordId)
        {
            var DutchAccountsPath = "../../../DutchAccounts.txt";
            var GlobalAccountsPath = "../../../GlobalAccounts.txt";

            var DutchAccounts = JsonExtension.GetJsonData(DutchAccountsPath);
            var GlobalAccounts = JsonExtension.GetJsonData(GlobalAccountsPath);

            if (DutchAccounts == null && GlobalAccounts == null)
                return "0";

            if (DutchAccounts.Count == 0 && GlobalAccounts.Count == 0)
                return "0";

            var condition = "0";

            foreach (var player in DutchAccounts)
            {
                if (player.Key == DiscordId)
                    return player.Value.ToString();
            }

            foreach (var player in GlobalAccounts)
            {
                if (player.Key == DiscordId)
                    return player.Value.ToString();
            }

            return "0";
        }

        public List<string> GetLinkedDiscordNames()
        {
            var discordNames = new List<string>();

            string filePath = "../../../DutchAccounts.txt";
            var account = new List<string[]>();
            using (var r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                account = JsonConvert.DeserializeObject<List<string[]>>(json);
            }

            if (account == null || account.Count == 0)
                return new List<string>();
            foreach (var player in account)
            {
                string discordId = player[0];
                var user = _discordSocketClient.GetUser(ulong.Parse(discordId));
                discordNames.Add(user.Username);
            }

            discordNames.Sort();
            return discordNames;
        }

        public string GetLinkedDiscordNamesEmbed()
        {
            var discordNames = GetLinkedDiscordNames();
            string namesAsString = "";
            namesAsString += "``` Discord users linked with scoresaber List \n\n";
            foreach (string name in discordNames)
            {
                namesAsString += name + "\n";
            }

            namesAsString += "Count: " + discordNames.Count;
            namesAsString += "```";


            return namesAsString;
        }

        public List<string> GetNotLinkedDiscordNamesInGuild(ulong guildId)
        {
            var nameList = GetLinkedDiscordNames();
            var guild = _discordSocketClient.GetGuild(guildId);
            var nameListNotLinked = new List<string>();

            foreach (var user in guild.Users)
            {
                if (!nameList.Contains(user.Username))
                    nameListNotLinked.Add(user.Username);
            }

            nameListNotLinked.Sort();
            return nameListNotLinked;
        }

        public string GetNotLinkedDiscordNamesInGuildEmbed(ulong guildId)
        {
            var discordNames = GetNotLinkedDiscordNamesInGuild(guildId);
            string namesAsString = "";
            namesAsString += "``` Discord users Not linked with scoresaber List \n\n";
            foreach (string name in discordNames)
            {
                namesAsString += name + "\n";
            }

            namesAsString += "Count: " + discordNames.Count;
            namesAsString += "```";

            return namesAsString;
        }
    }
}