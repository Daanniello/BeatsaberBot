﻿using System;
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
        /// Creates a request to validate the ScoreSaber account to a specific discord account. 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="countryGuildId"></param>
        /// <param name="countryChannelToPostId"></param>
        /// <returns></returns>
        public async Task MakeRequest(SocketMessage message, ulong countryGuildId, ulong countryChannelToPostId)
        {
            ulong DiscordId = message.Author.Id;
            string ScoreSaberId = message.Content.Substring(9);
            ScoreSaberId = Regex.Replace(ScoreSaberId, "[^0-9]", "");

            if (!ValidationExtension.IsDigitsOnly(ScoreSaberId))
            {
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Wrong ScoreSaber ID", "Get you link from the url from your ScoreSaber page.", null, null).Build());

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
                              "**ScoreSaber ID:** " + ScoreSaberId + "\n" +
                              "**Discord ID:** " + DiscordId + "\n" +
                              "**ScoreSaber link:** https://scoresaber.com/u/" + ScoreSaberId + " \n" +
                              "*Waiting for approval by a Staff*" + " \n\n" +
                              "***(Reminder) Type !bs link [ScoreSaber ID]***",
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
            var vragenLijstEmbed = await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Wil je een paar kennismakingsvragen invullen om mensen je te laten leren kennen?", "De antwoorden worden toegevoegd aan je welkomstbericht. Je hoeft de vragenlijst niet in te vullen. Bepaalde vragen kun je overslaan door middel van de reacties", null, null).Build());
            await vragenLijstEmbed.AddReactionAsync(Emote.Parse("<:green_check:671412276594475018>"));
            await vragenLijstEmbed.AddReactionAsync(Emote.Parse("<:red_check:671413258468720650>"));
        }

        public async Task<bool> LinkAccount(string discordId, string ScoreSaberId, ulong guildId)
        {
            try
            {
                await DatabaseContext.ExecuteInsertQuery($"Insert into Player (ScoreSaberId, DiscordId, CountryCode) values ({ScoreSaberId}, {discordId}, 'NL')");
                await DatabaseContext.ExecuteInsertQuery($"Insert into PlayerInCountry (DiscordId, GuildId) values ({discordId}, {guildId})");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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
                Console.WriteLine("Unlink error: " + ex.ToString());
                return false;
            }
        }

        public async Task<ulong> GetDiscordIdWithScoreSaberId(string ScoreSaberId)
        {
            try
            {
                var result = await DatabaseContext.ExecuteSelectQuery($"Select * from Player where ScoreSaberId={ScoreSaberId}");
                return Convert.ToUInt64(result[0][1]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString())
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

        public async Task<string> GetScoreSaberIdWithDiscordId(string DiscordId)
        {
            try
            {
                var result = await DatabaseContext.ExecuteSelectQuery($"Select * from Player where DiscordId={DiscordId}");
                return result[0][0].ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString())
                return "";
            }
        }
    }
}