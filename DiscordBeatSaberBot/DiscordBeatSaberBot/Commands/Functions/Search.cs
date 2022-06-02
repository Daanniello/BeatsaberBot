using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBeatSaberBot.Extensions;
using DiscordBeatSaberBot.Models.ScoreberAPI;
using Newtonsoft.Json;
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
            var playerModel = await new ScoresaberAPI(scoresaberID).GetPlayerFull();
            if (playerModel == null)
            {
                await command.Channel.SendMessageAsync("oh oh... Scoresaber/Discord ID is incorrect or the Scoresaber api crashed. Try again.");
                return;
            }

            var embedBuilder = new EmbedBuilder();
            embedBuilder.Title = playerModel.playerInfo.Name;
            embedBuilder.Url = $"https://scoresaber.com/u/{playerModel.playerInfo.PlayerId}";
            embedBuilder.ThumbnailUrl = $"https://new.scoresaber.com{playerModel.playerInfo.Avatar}";
            embedBuilder.Description = (hasSettingsPage ? $"**📰 About {playerModel.playerInfo.Name}**" +
                $"\n{await new SettingsQuestionList(_discord, command).GetSettingInfo(_scoresaberID, 18)}\n\n" +
                $"Uses a **{await new SettingsQuestionList(_discord, command).GetSettingInfo(_scoresaberID, 11)}\n\n**" +
                $"Has played over a total of **{playerModel.scoreStats.TotalPlayCount} maps**\n" +
                $"With an average accuracy of **{playerModel.scoreStats.AvarageRankedAccuracy}%**" : $"Has played over a total of **{playerModel.scoreStats.TotalPlayCount} maps**\n" +
                $"With an average accuracy of **{playerModel.scoreStats.AvarageRankedAccuracy}%**");            

            _originalEmbedBuilder = embedBuilder;

            var componentBuilder = new ComponentBuilder();            
            componentBuilder.WithButton(label: "Profile", customId: "profileSearchButton", style: ButtonStyle.Success);
            if (hasSettingsPage) componentBuilder.WithButton(label: "Settings", customId: "settingsSearchButton", style: ButtonStyle.Success);
            componentBuilder.WithButton(label: "Compare", customId: "compareSearchButton", style: ButtonStyle.Success);
            componentBuilder.WithButton(label: "Tops", customId: "topsongsSearchButton", style: ButtonStyle.Success);
            componentBuilder.WithButton(label: "Recent", customId: "recentsongSearchButton", style: ButtonStyle.Success);
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
                            var cardId =await BeatSaberInfoExtension.GetAndCreateProfileImage(_scoresaberID);
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
                                var scoresaberID2FullModel = await new ScoresaberAPI(scoresaberID2).GetPlayerFull();
                                
                                using (HttpClient hc = new HttpClient())
                                {
                                    var urlPlayerInfo2 = $"https://new.scoresaber.com/api/player/{_scoresaberID}/full";
                                    var player2InfoRaw = await hc.GetStringAsync(urlPlayerInfo2);
                                    var playerModel = JsonConvert.DeserializeObject<ScoresaberPlayerFullModel>(player2InfoRaw);
                                    var cardID = await BeatSaberInfoExtension.GetAndCreateCompareImage(scoresaberID2FullModel, playerModel);
                                    var embedBuilderCompare = new EmbedBuilder() { ThumbnailUrl = _originalEmbedBuilder.ThumbnailUrl, ImageUrl = $"{GlobalConfiguration.BotImageStorageLink}CompareCard_{scoresaberID2}_{_scoresaberID}_{cardID}.png" };
                                    embedBuilderCompare.AddField($"{button.User.Username}", "-", true);
                                    embedBuilderCompare.AddField($"VS", "-", true);
                                    embedBuilderCompare.AddField($"{playerModel.playerInfo.Name}", "-", true);
                                    await button.Message.ModifyAsync(x => x.Embed = embedBuilderCompare.Build());
                                }                                                              
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

            //if (arg1.Id == msg.Id && arg3.UserId != 504633036902498314)
            //{
            //    if (socketReacitonsList.Contains(arg3)) return;
            //    socketReacitonsList.Add(arg3);

            //    if (arg3.Emote.Name == "👤")
            //    {
            //        await GetAndCreateProfileImage(scoresaberID);
            //        await command.Channel.SendFileAsync($"../../../Resources/img/RankingCard_{scoresaberID}.png", $"<@!{arg3.UserId}> here you go. What an amazing profile!");
            //        File.Delete($"../../../Resources/img/RankingCard_{scoresaberID}.png");
            //    }
            //    if (arg3.Emote.Name == "🔧")
            //    {
            //        await command.Channel.SendMessageAsync($"<@!{arg3.UserId}> here you go. Did you know you can use `!bs statistics` to see community stats?", false, await SettingsQuestionList.GetSettingsPageWithScoresaberID(scoresaberID));
            //    }
            //    if (arg3.Emote.Name == "🔁")
            //    {
            //        var discordID = arg3.UserId.ToString();
            //        if (!await new RoleAssignment(discord).CheckIfDiscordIdIsLinked(discordID))
            //        {
            //            await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed($"You are not linked {arg3.User}", "Link your scoresaber by using the command `!bs link [scoresaberID]`").Build());
            //            return;
            //        }
            //        var scoresaberID2 = await RoleAssignment.GetScoresaberIdWithDiscordId(discordID);
            //        var scoresaberID2FullModel = await new ScoresaberAPI(scoresaberID2).GetPlayerFull();
            //        await GetAndCreateUserCompareImage(scoresaberID2, scoresaberID);
            //        await GetAndCreateCompareImage(scoresaberID2FullModel, playerModel);
            //        await command.Channel.SendFileAsync($"../../../Resources/img/UserCompareCard_{scoresaberID2}_{scoresaberID}.png", $"<@!{arg3.UserId}> here you go. I wonder who is better");
            //        await command.Channel.SendFileAsync($"../../../Resources/img/CompareCard_{scoresaberID2}_{scoresaberID}.png");

            //        File.Delete($"../../../Resources/img/CompareCard_{scoresaberID2}_{scoresaberID}.png");
            //        File.Delete($"../../../Resources/img/UserCompareCard_{scoresaberID2}_{scoresaberID}.png");
            //    }
            //    if (arg3.Emote.Name == "🆕")
            //    {
            //        await GetAndCreateRecentsongsCardImage(scoresaberID);
            //        await command.Channel.SendFileAsync($"../../../Resources/img/RecentsongsCard_{scoresaberID}.png", $"<@!{arg3.UserId}> here you go.");
            //        File.Delete($"../../../Resources/img/RecentsongsCard_{scoresaberID}.png");
            //    }
            //    if (arg3.Emote.Name == "📰")
            //    {
            //        await command.Channel.SendMessageAsync($"<@!{arg3.UserId}> here you go. This data is actually really helpfull!");
            //        //await new PlaythroughStats(discord).GetAndPostPlaythroughStatsWithScoresaberId(scoresaberID, message);
            //    }
            //    if (arg3.Emote.Name == "👑")
            //    {
            //        await GetAndCreateTopsongsCardImage(scoresaberID);
            //        await command.Channel.SendFileAsync($"../../../Resources/img/TopsongsCard_{scoresaberID}.png", $"<@!{arg3.UserId}> here you go. ");
            //        File.Delete($"../../../Resources/img/TopsongsCard_{scoresaberID}.png");
            //    }
            //}
        }
    }
}
