using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBeatSaberBot.Extensions;
using DiscordBeatSaberBot.Models.ScoreberAPI;
using Newtonsoft.Json;
using ScoreSaberLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Commands.Functions
{
    public class Search
    {
        private RestUserMessage _msg;
        private string _scoresaberID;
        private DiscordSocketClient _discord;
        private EmbedBuilder _originalEmbedBuilder;
        private ComponentBuilder _originalComponentBuilder;

        public Search(DiscordSocketClient discord)
        {
            _discord = discord;
        }

        public async Task CreateUserSearchEmbedWithScoresaberIDAndSend(string scoresaberID, SocketSlashCommand command, DiscordSocketClient discord)
        {
            _scoresaberID = scoresaberID;
            var isInDatabase = await new RoleAssignment(discord).GetDiscordIdWithScoresaberId(scoresaberID) != 0;
            var hasSettingsPage = await SettingsQuestionList.HasSettingsPage(scoresaberID);
            var player = await new ScoreSaberClient().Api.Players.GetPlayer(Convert.ToInt64(scoresaberID));
            if (player == null)
            {
                await command.Channel.SendMessageAsync("oh oh... Scoresaber/Discord ID is incorrect or the Scoresaber api crashed. Try again.");
                return;
            }

            var embedBuilder = new EmbedBuilder();
            embedBuilder.Title = player.Name;
            embedBuilder.Url = $"https://scoresaber.com/u/{player.Id}";
            embedBuilder.ThumbnailUrl = player.ProfilePicture.AbsoluteUri;
            embedBuilder.Description = (hasSettingsPage ? $"**📰 About {player.Name}**" +
                $"\n{await new SettingsQuestionList(_discord, command).GetSettingInfo(_scoresaberID, 18)}\n\n" +
                $"Uses a **{await new SettingsQuestionList(_discord, command).GetSettingInfo(_scoresaberID, 11)}\n\n**" +
                $"Has played over a total of **{player.ScoreStats.TotalPlayCount} maps**\n" +
                $"With an average accuracy of **{player.ScoreStats.AverageRankedAccuracy}%**" : $"Has played over a total of **{player.ScoreStats.TotalPlayCount} maps**\n" +
                $"With an average accuracy of **{player.ScoreStats.AverageRankedAccuracy}%**");

            _originalEmbedBuilder = embedBuilder;

            var componentBuilder = new ComponentBuilder();
            componentBuilder.WithButton(label: "Profile", customId: "profileSearchButton", style: ButtonStyle.Primary);
            if (hasSettingsPage) componentBuilder.WithButton(label: "Settings", customId: "settingsSearchButton", style: ButtonStyle.Primary);
            componentBuilder.WithButton(label: "Compare", customId: "compareSearchButton", style: ButtonStyle.Primary);
            componentBuilder.WithButton(label: "Tops", customId: "topsongsSearchButton", style: ButtonStyle.Primary);
            componentBuilder.WithButton(label: "Recent", customId: "recentsongSearchButton", style: ButtonStyle.Primary);
            componentBuilder.WithButton(emote: Emote.Parse("<:leftarrow:923182957798244402>"), customId: "backSearchButton", style: ButtonStyle.Danger);

            _originalComponentBuilder = componentBuilder;

            discord.ButtonExecuted += Discord_ButtonExecuted;
            _msg = await command.Channel.SendMessageAsync("", false, embedBuilder.Build(), components: componentBuilder.Build());
        }

        private async Task Discord_ButtonExecuted(SocketMessageComponent button)
        {
            if (button.Message.Id == _msg.Id)
            {
                try
                {
                    switch (button.Data.CustomId)
                    {
                        case "backSearchButton":
                            await button.Message.ModifyAsync(x => x.Embed = _originalEmbedBuilder.Build());
                            break;
                        case "profileSearchButton":
                            var cardId = await BeatSaberInfoExtension.GetAndCreateProfileImage(_scoresaberID);
                            var currentEmbed = button.Message.Embeds.First();
                            var embedBuilder = new EmbedBuilder() { ImageUrl = $"{GlobalConfiguration.BotImageStorageLink}RankingCard_{_scoresaberID}_{cardId}.png" };
                            await button.Message.ModifyAsync(x => x.Embed = embedBuilder.Build());
                            break;
                        case "settingsSearchButton":
                            var embed = await SettingsQuestionList.GetSettingsPageWithScoresaberID(_scoresaberID);
                            await button.Message.ModifyAsync(x => x.Embed = embed);
                            break;
                        case "compareSearchButton":
                            var discordId = button.User.Id.ToString();
                            if (await new RoleAssignment(_discord).CheckIfDiscordIdIsLinked(discordId))
                            {
                                var scoresaberID2 = await RoleAssignment.GetScoresaberIdWithDiscordId(discordId);

                                var scoresaberClient = new ScoreSaberClient();
                                var playerOne = await scoresaberClient.Api.Players.GetPlayer(Convert.ToInt64(_scoresaberID));
                                var playerTwo = await scoresaberClient.Api.Players.GetPlayer(Convert.ToInt64(_scoresaberID));

                                var cardID = await BeatSaberInfoExtension.GetAndCreateCompareImage(playerTwo, playerOne);
                                var embedBuilderCompare = new EmbedBuilder() { ThumbnailUrl = _originalEmbedBuilder.ThumbnailUrl, ImageUrl = $"{GlobalConfiguration.BotImageStorageLink}CompareCard_{scoresaberID2}_{_scoresaberID}_{cardID}.png" };
                                embedBuilderCompare.AddField($"{button.User.Username}", "-", true);
                                embedBuilderCompare.AddField($"VS", "-", true);
                                embedBuilderCompare.AddField($"{playerOne.Name}", "-", true);
                                await button.Message.ModifyAsync(x => x.Embed = embedBuilderCompare.Build());

                            }
                            break;
                        case "recentsongSearchButton":
                            var recentsongEmbed = await new PlaythroughStats(_discord).CreateCardAndGetPlaythroughStatsEmbed(_scoresaberID);
                            await button.Message.ModifyAsync(x => x.Embed = recentsongEmbed.Build());
                            break;
                        case "topsongsSearchButton":
                            var cardIDtopsongs = await BeatSaberInfoExtension.GetAndCreateRecentsongsCardImage(_scoresaberID, isTopsong: true);
                            var embedBuilderTopsongs = new EmbedBuilder() { Title = _originalEmbedBuilder.Title + "'s Topsongs", ThumbnailUrl = _originalEmbedBuilder.ThumbnailUrl, ImageUrl = $"{GlobalConfiguration.BotImageStorageLink}TopsongsCard_{_scoresaberID}_{cardIDtopsongs}.png" };
                            await button.Message.ModifyAsync(x => x.Embed = embedBuilderTopsongs.Build());
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public void GetEmbed()
        {

        }


    }
}
