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

        public async Task MakeRequest(SocketMessage message)
        {
            //command: requestrole
            //command: requestverification
            //Request comes in with DiscordId + ScoresaberID

            ulong DiscordId = message.Author.Id;
            string ScoresaberId = message.Content.Substring(9);
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
                              "***(Reminder) Type !bs link [Scoresaber ID]***",
                Color = Color.Orange
            };

            var sendMessage = await channel.SendMessageAsync("", false, embedBuilder.Build());
            await sendMessage.AddReactionAsync(new Emoji("✅"));
            await sendMessage.AddReactionAsync(new Emoji("⛔"));

            //await message.DeleteAsync();
        }

        public async Task<bool> LinkAccount(string discordId, string scoresaberId)
        {
            var GlobalAccountsPath = "../../../DutchAccounts.txt";

            //command: linkaccount
            string DiscordId = discordId;
            string ScoresaberId = scoresaberId;

            var account = new Dictionary<string, object>();

            account = JsonExtension.GetJsonData(GlobalAccountsPath);

            if (account == null || account.Count == 0)
                account = new Dictionary<string, object>();


            foreach (var acc in account)
            {
                if (acc.Key == DiscordId && acc.Value.ToString() == ScoresaberId)
                    return false;
            }

            account.Add(DiscordId, ScoresaberId);             

            using (var file = File.CreateText(GlobalAccountsPath))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, account);
            }

            //await message.DeleteAsync();
            return true;
        }

        public ulong GetDiscordIdWithScoresaberId(string scoresaberId)
        {
            string GlobalAccountsPath = "../../../DutchAccounts.txt";
            var account = new Dictionary<string, object>();
            account = JsonExtension.GetJsonData(GlobalAccountsPath);

            if (account == null || account.Count == 0)
                return 0;
            foreach (var player in account)
            {
                if (player.Value.ToString() == scoresaberId)
                    return ulong.Parse(player.Key);
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

            if (DutchAccounts == null) return false;

            foreach (var user in DutchAccounts)
            {
                if (user.Key == DiscordId)
                    condition = true;
            }

            if (GlobalAccounts == null) return false;

            foreach (var user in GlobalAccounts)
            {
                if (user.Key == DiscordId)
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

        public async void MutePerson(ulong discordId)
        {
            //Mute role: 550291116667437056
            var dutchDiscord = _discordSocketClient.GetGuild(505485680344956928);
            var muteRole = dutchDiscord.GetRole(550291116667437056);
            var user = dutchDiscord.GetUser(discordId);
            user.AddRoleAsync(muteRole);

            //Make sure all channels have the mute role setup right.
            foreach (var channel in dutchDiscord.Channels)
            {
                if (channel.Id == 716676959068749874) continue;
                channel.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions().Modify(
                    readMessageHistory: PermValue.Inherit, 
                    viewChannel: PermValue.Inherit, 
                    sendMessages: PermValue.Deny,
                    attachFiles: PermValue.Deny,
                    useExternalEmojis: PermValue.Deny,
                    connect: PermValue.Deny,
                    mentionEveryone: PermValue.Deny,
                    addReactions: PermValue.Deny
                    ));
            }
        }

        public void UnMutePerson(ulong discordId)
        {
            var dutchDiscord = _discordSocketClient.GetGuild(505485680344956928);
            var muteRole = dutchDiscord.GetRole(550291116667437056);
            var user = dutchDiscord.GetUser(discordId);
            user.RemoveRoleAsync(muteRole);
        }
    }
}