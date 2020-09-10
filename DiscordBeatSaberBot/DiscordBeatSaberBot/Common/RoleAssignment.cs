using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore.Storage;
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

        /// <summary>
        /// Creates a request to validate the scoresaber account to a specific discord account. 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="countryGuildId"></param>
        /// <param name="countryChannelToPostId"></param>
        /// <returns></returns>
        public async Task MakeRequest(SocketMessage message, ulong countryGuildId, ulong countryChannelToPostId)
        {
            ulong DiscordId = message.Author.Id;
            string ScoresaberId = message.Content.Substring(9);
            ScoresaberId = Regex.Replace(ScoresaberId, "[^0-9]", "");

            if (!ValidationExtension.IsDigitsOnly(ScoresaberId))
            {
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Wrong scoresaber ID", "Get you link from the url from your scoresaber page.", null, null).Build());

                return;
            }

            //GuildID AND ChannelID
            ulong guild_id = countryGuildId;
            ulong guild_channel_id = countryChannelToPostId;

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

        public async Task PersoonlijkeVragenLijst(SocketMessage message)
        {
            if (message.Channel.Id != 549350982081970176) return;
            var vragenLijstEmbed = await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Wil je een paar kennismakingsvragen invullen om mensen je te laten leren kennen?", "De antwoorden worden toegevoegd aan je welkoms message. Je hoeft de vragenlijst niet in te vullen. Bepaalde vragen kun je skippen door middel van de reacties", null, null).Build());
            await vragenLijstEmbed.AddReactionAsync(Emote.Parse("<:green_check:671412276594475018>"));
            await vragenLijstEmbed.AddReactionAsync(Emote.Parse("<:red_check:671413258468720650>"));
        }

        public async Task<bool> LinkAccount(string discordId, string scoresaberId, ulong guildId)
        {
            try
            {
                await DatabaseContext.ExecuteInsertQuery($"Insert into Player (ScoresaberId, DiscordId, CountryCode) values ({scoresaberId}, {discordId}, 'NL')");
                await DatabaseContext.ExecuteInsertQuery($"Insert into PlayerInCountry (DiscordId, GuildId) values ({discordId}, {guildId})");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UnlinkAccount(string discordId)
        {
            try
            {
                await DatabaseContext.ExecuteRemoveQuery($"Delete from Player where DiscordId={discordId}");
                await DatabaseContext.ExecuteRemoveQuery($"Delete from PlayerInCountry where DiscordId={discordId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unlink error: " + ex);
                return false;
            }
        }

        public async Task<ulong> GetDiscordIdWithScoresaberId(string scoresaberId)
        {
            try
            {
                var result = await DatabaseContext.ExecuteSelectQuery($"Select * from Player where ScoresaberId={scoresaberId}");
                return Convert.ToUInt64(result[0][1]);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<bool> CheckIfDiscordIdIsLinked(string DiscordId)
        {
            try
            {
                var result = await DatabaseContext.ExecuteSelectQuery($"Select * from Player where DiscordId={DiscordId}");
                if (result.Count() > 0) return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }

        public async Task<string> GetScoresaberIdWithDiscordId(string DiscordId)
        {
            try
            {
                var result = await DatabaseContext.ExecuteSelectQuery($"Select * from Player where DiscordId={DiscordId}");
                return result[0][0].ToString();
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public async void MutePerson(ulong discordId, ulong guildId, string timerange = null)
        {
            DateTime dateTillUnmute = DateTime.Now;

            //2y1M5w3d6h10m43s
            if (timerange != null)
            {

                var timesRaw = Regex.Split(timerange, @"([0-9]+[a-zA-Z])");
                var times = timesRaw.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                
                foreach (var time in times)
                {
                    var values = Regex.Replace(time, @"(?<=\d)(?=\p{L})", " ").Split(" ");
                    if (values[1] == "s") dateTillUnmute = dateTillUnmute.AddSeconds(Convert.ToInt32(values[0]));
                    if (values[1] == "m") dateTillUnmute = dateTillUnmute.AddMinutes(Convert.ToInt32(values[0]));
                    if (values[1] == "h") dateTillUnmute = dateTillUnmute.AddHours(Convert.ToInt32(values[0]));
                    if (values[1] == "d") dateTillUnmute = dateTillUnmute.AddDays(Convert.ToInt32(values[0]));
                    if (values[1] == "w") dateTillUnmute = dateTillUnmute.AddDays(Convert.ToInt32(values[0]) * 7);
                    if (values[1] == "M") dateTillUnmute = dateTillUnmute.AddMonths(Convert.ToInt32(values[0]));
                    if (values[1] == "y") dateTillUnmute = dateTillUnmute.AddYears(Convert.ToInt32(values[0]));
                }
            }           

            var guild = _discordSocketClient.GetGuild(Convert.ToUInt64(guildId));
            var countryInfo = await DatabaseContext.ExecuteSelectQuery($"Select * from Country where GuildId={guildId}");
            var muteRoleId = countryInfo[0][4];
            var muteRole = guild.GetRole(Convert.ToUInt64(muteRoleId));
            var mutedPeopleChannelId = Convert.ToUInt64(countryInfo[0][5]);


            //Mute role: 550291116667437056
            var user = guild.GetUser(discordId);
            user.AddRoleAsync(muteRole);

            //Add unmute date in database
            if (dateTillUnmute > DateTime.Now)
            {
                await DatabaseContext.ExecuteInsertQuery($"update PlayerInCountry set Muted = ('{dateTillUnmute.ToString()}') where DiscordId={discordId}");
            }
            else
            {
                await DatabaseContext.ExecuteInsertQuery($"update PlayerInCountry set Muted = NULL where DiscordId={discordId}");
            }

            //Send message in dutch staff channel 
            string s = dateTillUnmute < DateTime.Now ? "unmute" : dateTillUnmute.ToString(); 
            await guild.GetTextChannel(711347912680013825).SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed($"{user.Username} (id:{user.Id}) is now muted", $"{user.Username} is muted until {s}").Build());

            //Make sure all channels have the mute role setup right.
            foreach (var channel in guild.Channels)
            {
                //TODO announcement channel contorleren
                if (channel.Id == 510959349263499264) continue;
                if (channel.Id == mutedPeopleChannelId) continue;
                await channel.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions().Modify(
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

        public async void UnMutePerson(ulong discordId, ulong guildId)
        {
            var guild = _discordSocketClient.GetGuild(Convert.ToUInt64(guildId));
            var countryInfo = await DatabaseContext.ExecuteSelectQuery($"Select * from Country where GuildId={guildId}");
            var muteRoleId = countryInfo[0][4];
            var muteRole = guild.GetRole(Convert.ToUInt64(muteRoleId));
            var user = guild.GetUser(discordId);
            user.RemoveRoleAsync(muteRole);
            await DatabaseContext.ExecuteInsertQuery($"update PlayerInCountry set Muted = NULL where DiscordId={discordId}");
            await guild.GetTextChannel(711347912680013825).SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed($"{user.Username} (id:{user.Id}) is now unmuted", $":)").Build());

        }
    }
}