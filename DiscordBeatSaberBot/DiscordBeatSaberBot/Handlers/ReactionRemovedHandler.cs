using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DiscordBeatSaberBot.Handlers
{
    public class ReactionRemovedHandler
    {
        public async Task HandleReaction(DiscordSocketClient discordSocketClient, SocketReaction reaction,
            ISocketMessageChannel channel, Dictionary<string, string> _reactionWatcher, Program program)
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
                    Description = embedInfo.Description.Replace("\n" + user.Username, "").Trim(),
                    Footer = new EmbedFooterBuilder { Text = embedInfo.Footer.ToString() },
                    Color = embedInfo.Color
                };

                await embededMessage.ModifyAsync(msg => msg.Embed = embedBuilder.Build());
                return;
            }

            //IRL Event section
            var data = JsonExtension.GetJsonData("../../../Resources/irleventdata.txt");

            if (reaction.UserId != 504633036902498314 && data.Keys.Contains(reaction.MessageId.ToString()))
            {
                //green_check (671412276594475018)
                //blue_check (671413239992549387)
                //red_check (671413258468720650)
                //<:peepoLimburg:600782036919123968>

                if (reaction.Emote.ToString() == "<:green_check:671412276594475018>")
                {
                    var deelnemersMsgData = JsonExtension.ToDictionary<string[]>(data[reaction.MessageId.ToString()]);
                    var eventDetailChannel = reaction.Channel;
                    var msgId = deelnemersMsgData.First().Key;
                    var embededMessage = (IUserMessage)await eventDetailChannel.GetMessageAsync(ulong.Parse(msgId));

                    var embedInfo = embededMessage.Embeds.First();
                    var user = discordSocketClient.GetUser(reaction.User.Value.Id);

                    var des = embedInfo.Description.Split("\n");
                    var description = "";
                    foreach (var deelnemer in des)
                        if (!deelnemer.Contains(reaction.UserId.ToString()))
                            description += "\n" + deelnemer;

                    var embedBuilder = new EmbedBuilder
                    {
                        Title = embedInfo.Title,
                        Description = description,
                        Footer = new EmbedFooterBuilder { Text = embedInfo.Footer.ToString() },
                        Color = embedInfo.Color
                    };

                    await embededMessage.ModifyAsync(msg => msg.Embed = embedBuilder.Build());
                    return;
                }

                if (reaction.Emote.ToString() == "<:red_check:671413258468720650>")
                {
                    var user = discordSocketClient.GetUser(reaction.User.Value.Id);
                    var deelnemersMsgData = JsonExtension.ToDictionary<string[]>(data[reaction.MessageId.ToString()]);
                    var d = deelnemersMsgData[reaction.MessageId + "0"];
                    var generalChannel = discordSocketClient.GetGuild(505485680344956928)
                        .GetChannel(ulong.Parse(d.First()));
                    await generalChannel.AddPermissionOverwriteAsync(user,
                        new OverwritePermissions().Modify(sendMessages: PermValue.Deny, viewChannel: PermValue.Deny,
                            readMessageHistory: PermValue.Deny));


                    var user2 = discordSocketClient.GetUser(reaction.User.Value.Id);
                    var infoChannel = discordSocketClient.GetGuild(505485680344956928).GetChannel(reaction.Channel.Id);
                    await generalChannel.AddPermissionOverwriteAsync(user2,
                        new OverwritePermissions().Modify(sendMessages: PermValue.Deny, viewChannel: PermValue.Deny,
                            readMessageHistory: PermValue.Deny));
                    return;
                }
            }

            //Remove Roles from reactions added to specific channels 

                if (channel.Id == 510227606822584330 || channel.Id == 627292184143724544)
                {
                    var guild = discordSocketClient.GetGuild(505485680344956928);
                    if (channel.Id == 510227606822584330)
                        guild = discordSocketClient.GetGuild(505485680344956928);
                    else if (channel.Id == 627292184143724544)
                        guild = discordSocketClient.GetGuild(627156958880858113);

                    var user = guild.GetUser(reaction.UserId);

                    var t = reaction.Emote.ToString();

                    foreach (var reactionDic in _reactionWatcher)
                        if (reactionDic.Key == t)
                        {
                            var role = guild.Roles.FirstOrDefault(x => x.Name == reactionDic.Value);
                            await (user as IGuildUser).RemoveRoleAsync(role);
                        }
                }

                //Turn page from help command

                //right (681843066104971287)
                //left (681842980134584355)
                if (reaction.UserId != 504633036902498314)
                {
                    if (reaction.Emote.ToString() == "<:right:681843066104971287>")
                    {
                        var t = reaction.Message.ToString();
                        var message = await channel.GetMessageAsync(reaction.MessageId);
                        var casted = (IUserMessage) message;
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
                        var casted = (IUserMessage) message;
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