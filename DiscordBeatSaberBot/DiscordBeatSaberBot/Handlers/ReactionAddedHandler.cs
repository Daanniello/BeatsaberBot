using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBeatSaberBot.Extensions;

namespace DiscordBeatSaberBot.Handlers
{
    public class ReactionAddedHandler
    {
        public async Task HandleReaction(DiscordSocketClient discordSocketClient, SocketReaction reaction, ISocketMessageChannel channel, Dictionary<string, string> _reactionWatcher, Program program)
        {

            if (reaction.MessageId.ToString() == "586248421715738629")
            {
                var eventDetailChannel = (ISocketMessageChannel)discordSocketClient.GetChannel(572721078359556097);
                var embededMessage = (IUserMessage)await eventDetailChannel.GetMessageAsync(586248421715738629);

                var embedInfo = embededMessage.Embeds.First();
                var guild = discordSocketClient.Guilds.FirstOrDefault(x => x.Id == (ulong)505485680344956928);
                var user = guild.GetUser(reaction.User.Value.Id);

                var embedBuilder = new EmbedBuilder
                {
                    Title = embedInfo.Title,
                    Description = embedInfo.Description + "\n" + user.Username,
                    Footer = new EmbedFooterBuilder { Text = embedInfo.Footer.ToString() },
                    Color = embedInfo.Color
                };

                await embededMessage.ModifyAsync(msg => msg.Embed = embedBuilder.Build());
            }

            var data = JsonExtension.GetJsonData("../../../Resources/irleventdata.txt");

            if (reaction.UserId != 504633036902498314 && data.Keys.Contains(reaction.MessageId.ToString()))
            {
                if (reaction.Emote.ToString() == "<:green_check:671412276594475018>")
                {
                    var deelnemersMsgData =
                        JsonExtension.ToDictionary<string[]>(data[reaction.MessageId.ToString()]);
                    var eventDetailChannel = reaction.Channel;
                    var msgId = deelnemersMsgData.First().Key;
                    var embededMessage =
                        (IUserMessage)await eventDetailChannel.GetMessageAsync(ulong.Parse(msgId));

                    var embedInfo = embededMessage.Embeds.First();
                    var user = discordSocketClient.GetUser(reaction.User.Value.Id);

                    var embedBuilder = new EmbedBuilder
                    {
                        Title = embedInfo.Title,
                        Description = embedInfo.Description + "\n" + "<@!" + user.Id + ">",
                        Footer = new EmbedFooterBuilder { Text = embedInfo.Footer.ToString() },
                        Color = embedInfo.Color
                    };
                    //Add permissions to see the general event channel
                    var d = deelnemersMsgData[reaction.MessageId + "0"];
                    var generalChannel = discordSocketClient.GetGuild(505485680344956928)
                        .GetChannel(ulong.Parse(d.First()));
                    await generalChannel.AddPermissionOverwriteAsync(user,
                        new OverwritePermissions().Modify(
                            sendMessages: PermValue.Inherit,
                            viewChannel: PermValue.Allow, 
                            readMessageHistory: PermValue.Allow));

                    await embededMessage.ModifyAsync(msg => msg.Embed = embedBuilder.Build());
                }
                else if (reaction.Emote.ToString() == "<:blue_check:671413239992549387>")
                {
                    var user = discordSocketClient.GetUser(reaction.User.Value.Id);
                    var deelnemersMsgData =
                        JsonExtension.ToDictionary<string[]>(data[reaction.MessageId.ToString()]);
                    var d = deelnemersMsgData[reaction.MessageId + "0"];
                    var generalChannel = discordSocketClient.GetGuild(505485680344956928)
                        .GetChannel(ulong.Parse(d.First()));
                    await generalChannel.AddPermissionOverwriteAsync(user,
                        new OverwritePermissions().Modify(
                            sendMessages: PermValue.Inherit,
                            viewChannel: PermValue.Allow, 
                            readMessageHistory: PermValue.Allow));
                }
                else if (reaction.Emote.ToString() == "<:red_check:671413258468720650>")
                {
                    var user = discordSocketClient.GetUser(reaction.User.Value.Id);
                    var generalChannel = discordSocketClient.GetGuild(505485680344956928)
                        .GetChannel(reaction.Channel.Id);
                    await generalChannel.AddPermissionOverwriteAsync(user,
                        new OverwritePermissions().Modify(
                            sendMessages: PermValue.Deny, 
                            viewChannel: PermValue.Deny,
                            readMessageHistory: PermValue.Deny));
                }
            }

            if (reaction.MessageId.ToString() == "586248421715738629")
            {
                var messageID = reaction.MessageId;
                var userID = reaction.UserId;
            }

            if (channel.Id == 549343990957211658)
            {
                var guild = discordSocketClient.Guilds.FirstOrDefault(x => x.Id == (ulong)505485680344956928);
                var user = guild.GetUser(reaction.User.Value.Id);
                var userRoles = user.Roles;
                foreach (var role in userRoles)
                    if (role.Name == "Nieuwkomer")
                    {
                        var addRole = guild.Roles.FirstOrDefault(x => x.Name == "Unverified");
                        var deleteRole = guild.Roles.FirstOrDefault(x => x.Name == "Nieuwkomer");
                        await user.AddRoleAsync(addRole);
                        await user.RemoveRoleAsync(deleteRole);
                    }
            }

            if (channel.Id == 549350982081970176)
            {
                async Task<bool> authenticationCheck()
                {
                    var guild = discordSocketClient.Guilds.FirstOrDefault(x => x.Id == (ulong)505485680344956928);
                    var user = await discordSocketClient.Rest.GetGuildUserAsync(505485680344956928, reaction.UserId);
                    
                    foreach (var roleId in user.RoleIds)
                        if (roleId == 505486321595187220)//Staff Role ID
                            return true;

                    return false;
                }

                if (await authenticationCheck())
                {
                    //{✅}
                    //{⛔}
                    // vinkje = status veranderen en actie uitvoeren om er in te zetten
                    // denied is status veranderen en user mention gebruiken
                    var messageFromReaction = await reaction.Channel.GetMessageAsync(reaction.MessageId);
                    var casted = (IUserMessage)messageFromReaction;
                    var usedEmbed = casted.Embeds.First();
                    if (reaction.Emote.Name == "⛔")
                    {
                        var deniedEmbed = new EmbedBuilder();
                        //need staff check
                        try
                        {
                            deniedEmbed = new EmbedBuilder
                            {
                                Title = usedEmbed.Title,
                                Description = usedEmbed.Description.Replace("Waiting for approval", "Denied"),
                                ThumbnailUrl = usedEmbed.Thumbnail.Value.Url,
                                Color = Color.Red
                            };
                        }
                        catch
                        {
                            deniedEmbed = new EmbedBuilder
                            {
                                Title = usedEmbed.Title,
                                Description = usedEmbed.Description.Replace("Waiting for approval", "Denied"),
                                Color = Color.Red
                            };
                        }

                        await casted.ModifyAsync(msg => msg.Embed = deniedEmbed.Build());
                        await casted.RemoveAllReactionsAsync();
                    }

                    if (reaction.Emote.Name == "✅")
                    {
                        var digitsOnly = new Regex(@"[^\d]");
                        var oneSpace = new Regex("[ ]{2,}");
                        var IDS = oneSpace.Replace(digitsOnly.Replace(usedEmbed.Description, " "), "-").Split("-");
                        var discordId = IDS[2];
                        var scoresaberId = IDS[1];
                        var approvedEmbed = new EmbedBuilder();
                        try
                        {
                            approvedEmbed = new EmbedBuilder
                            {
                                Title = usedEmbed.Title,
                                Description = usedEmbed.Description.Replace("Waiting for approval", "Approved"),
                                ThumbnailUrl = usedEmbed.Thumbnail.Value.Url,
                                Color = Color.Green
                            };
                        }
                        catch
                        {
                            approvedEmbed = new EmbedBuilder
                            {
                                Title = usedEmbed.Title,
                                Description = usedEmbed.Description.Replace("Waiting for approval", "Approved"),
                                Color = Color.Green
                            };
                        }

                        var guildChannel = channel as SocketGuildChannel;
                        //Adds user in the players table and in the players in country table
                        var check = await new RoleAssignment(discordSocketClient).LinkAccount(discordId, scoresaberId, guildChannel.Guild.Id);
                        if (check)
                        {
                            await casted.ModifyAsync(msg => msg.Embed = approvedEmbed.Build());
                            await casted.RemoveAllReactionsAsync();
                        }

                        var player = await new ScoresaberAPI(scoresaberId).GetPlayerFull();
                        

                        DutchRankFeed.GiveRoleWithRank(player.playerInfo.CountryRank, scoresaberId, discordSocketClient);
                        var dutchGuild = new GuildService(discordSocketClient, 505485680344956928);
                        IUser linkingUser = dutchGuild.Guild.GetUser(await new RoleAssignment(discordSocketClient).GetDiscordIdWithScoresaberId(scoresaberId));
                        await dutchGuild.AddRole("Verified", linkingUser);


                        await dutchGuild.DeleteRole("Unverified", linkingUser);

                        await program.UserJoinedMessage(linkingUser);
                    }
                }
            }

            //Add Roles from reactions added to specific channels 
            if (channel.Id == 510227606822584330 || channel.Id == 627292184143724544)
            {
                var guild = discordSocketClient.GetGuild(505485680344956928);
                if (channel.Id == 510227606822584330)
                    guild = discordSocketClient.GetGuild(505485680344956928);
                else if (channel.Id == 627292184143724544)
                    guild = discordSocketClient.GetGuild(627156958880858113);
                var user = await discordSocketClient.Rest.GetGuildUserAsync(guild.Id, reaction.UserId);

                var t = reaction.Emote.ToString().Replace("<a:", "<:");

                foreach (var reactionDic in _reactionWatcher)
                    if (reactionDic.Key == t)
                    {
                        var role = guild.Roles.FirstOrDefault(x => x.Name == reactionDic.Value);
                        if (role == null) role = guild.Roles.FirstOrDefault(x => x.Id.ToString() == reactionDic.Value);
                        try
                        {
                            await user.AddRoleAsync(role);
                        }
                        catch (Exception ex)
                        {

                        }
                    }

            }

            if (reaction.UserId != 504633036902498314)
            {
                //Turn page from help command

                //right (681843066104971287)
                //left (681842980134584355)
                if (reaction.Emote.ToString() == "<:right:681843066104971287>")
                {
                    var t = reaction.Message.ToString();
                    var message = await channel.GetMessageAsync(reaction.MessageId);
                    var casted = (IUserMessage)message;
                    var usedEmbed = casted.Embeds.First();
                    var pagenr = usedEmbed.Title.Split("[")[1].Split("]")[0];

                    var currentNr = int.Parse(pagenr.Split("/")[0]);
                    var maxNr = int.Parse(pagenr.Split("/")[1]);

                    if (currentNr >= maxNr) return;

                    casted.ModifyAsync(msg =>
                        msg.Embed = Help.GetHelpList(discordSocketClient, int.Parse(pagenr.Split("/").First())));
                }

                if (reaction.Emote.ToString() == "<:left:681842980134584355>")
                {
                    var t = reaction.Message.ToString();
                    var message = await channel.GetMessageAsync(reaction.MessageId);
                    var casted = (IUserMessage)message;
                    var usedEmbed = casted.Embeds.First();
                    var pagenr = usedEmbed.Title.Split("[")[1].Split("]")[0];

                    var currentNr = int.Parse(pagenr.Split("/")[0]);

                    if (currentNr <= 0) return;

                    casted.ModifyAsync(msg =>
                            msg.Embed = Help.GetHelpList(discordSocketClient,
                                int.Parse(pagenr.Split("/").First()) - 2));
                }
            }

        }
    }
}