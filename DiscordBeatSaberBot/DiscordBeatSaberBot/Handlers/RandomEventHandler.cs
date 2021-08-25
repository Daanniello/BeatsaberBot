using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBeatSaberBot.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Handlers
{
    public class RandomEventHandler
    {
        private SocketMessage message;
        private RestUserMessage msg;
        private DiscordSocketClient discord;
        private IRLeventModel randomEventModel = new IRLeventModel();

        private RestTextChannel infoChannel;
        private RestTextChannel generalChannel;

        private RestUserMessage validationMessage;

        public RandomEventHandler(SocketMessage message, DiscordSocketClient discord, RestUserMessage msg)
        {
            string tekst = File.ReadAllText("../../../Resources/irleventdata.txt");


            try
            {
                this.discord = discord;
                this.message = message;
                this.msg = msg;
                QuestionRound().Wait();
                ModifyEmbed(msg, "Creating the event...");
                CreateChannels().Wait();
                ModifyEmbed(msg, "Event has been created! waiting for staff to make it official");
                ValidateEvent();
            }
            catch
            {
                ModifyEmbed(msg, "Something went wrong :c");
                if (infoChannel != null) infoChannel.DeleteAsync();
                if (generalChannel != null) generalChannel.DeleteAsync();
            }
        }

        private async Task QuestionRound()
        {
            ModifyEmbed(msg, "**You are about to start the progress of making a new event** \nThink well about how you expect the event to go from begin till end, plan it carefully.\nIf this will be an IRL event, than please provide as much info as possible.\nYou have 10 minutes for each answer before the manager will quit itself. \n\nAnswer with `yes` to continue");
            if (await WaitForReaction() != "yes")
            {
                msg.DeleteAsync();
                return;
            }

            var previewEmbed = EmbedBuilderExtension.NullEmbed("Preview", $"");
            previewEmbed.Footer = new EmbedFooterBuilder { Text = "Green check = I will be there \nBlue check = I am interested \nRed cross = I will not be there, Delete this channel" };
            var previewMessage = await message.Channel.SendMessageAsync("", false, previewEmbed.Build());

            ModifyEmbed(msg, "What will be the title of the event? (Use a short title. Should be compact but recognizable)");
            randomEventModel.title = await WaitForReaction();

            previewEmbed.Title = randomEventModel.title;
            await previewMessage.ModifyAsync(x => x.Embed = previewEmbed.Build());

            ModifyEmbed(msg, "What will be the description of the event?");
            randomEventModel.description = await WaitForReaction();

            previewEmbed.Description = randomEventModel.description;
            await previewMessage.ModifyAsync(x => x.Embed = previewEmbed.Build());


            ModifyEmbed(msg, "Who are the eventleaders? *Gives rights in the announcement channel to add info* **Use discord tags**");
            randomEventModel.eventLeider = await WaitForReaction();

            previewEmbed.AddField(new EmbedFieldBuilder { Name = "Eventleader(s)", Value = randomEventModel.eventLeider });
            await previewMessage.ModifyAsync(x => x.Embed = previewEmbed.Build());

            ModifyEmbed(msg, "What will be the date of the event? **Use this format** Format: (05 02 2020 14:00) as in (day month year hour)");
            var date = await WaitForReaction();
            try
            {
                randomEventModel.date = DateTime.Parse(date, new CultureInfo("de-DE"));
                previewEmbed.AddField(new EmbedFieldBuilder { Name = "Date", Value = randomEventModel.date.ToString() });
                await previewMessage.ModifyAsync(x => x.Embed = previewEmbed.Build());
            }
            catch
            {
                ModifyEmbed(msg, "The date format is wrong. Format: (05 02 2020 14:00) as in (day month year hour)");
                var date2 = await WaitForReaction();
                randomEventModel.date = DateTime.Parse(date2, new CultureInfo("de-DE"));
                previewEmbed.AddField(new EmbedFieldBuilder { Name = "Date", Value = randomEventModel.date.ToString() });
                await previewMessage.ModifyAsync(x => x.Embed = previewEmbed.Build());
            }

            ModifyEmbed(msg, "What will be the location? *If its an online event or irl event, describe where to gather*");
            randomEventModel.locatie = await WaitForReaction();

            previewEmbed.AddField(new EmbedFieldBuilder { Name = "Location", Value = randomEventModel.locatie });
            await previewMessage.ModifyAsync(x => x.Embed = previewEmbed.Build());

            ModifyEmbed(msg, "Does the event have a website? **answer with yes or no**");
            if (await WaitForReaction() == "yes")
            {
                ModifyEmbed(msg, "Input the website url");
                randomEventModel.websiteUrl = await WaitForReaction();
                previewEmbed.AddField(new EmbedFieldBuilder { Name = "Website URL", Value = randomEventModel.websiteUrl });
                await previewMessage.ModifyAsync(x => x.Embed = previewEmbed.Build());
            }

            ModifyEmbed(msg, "Does the event have a minimum or/and maximum amount of participants? **answer with yes or no**");
            if (await WaitForReaction() == "yes")
            {
                ModifyEmbed(msg, "Input the amount in the format you like. *Example: 2-10*");
                randomEventModel.minmaxParticipants = await WaitForReaction();
                previewEmbed.AddField(new EmbedFieldBuilder { Name = "Minimum & Maximum amount of participants", Value = randomEventModel.minmaxParticipants });
                await previewMessage.ModifyAsync(x => x.Embed = previewEmbed.Build());
            }

            ModifyEmbed(msg, "Does the event require a payment to participate? **answer with yes or no**");
            if (await WaitForReaction() == "yes")
            {
                ModifyEmbed(msg, "Input the amount");
                randomEventModel.payment = await WaitForReaction();
                previewEmbed.AddField(new EmbedFieldBuilder { Name = "Required payment", Value = randomEventModel.payment });
                await previewMessage.ModifyAsync(x => x.Embed = previewEmbed.Build());
            }

            ModifyEmbed(msg, "Is there an age requirement? **answer with yes or no**");
            if (await WaitForReaction() == "yes")
            {
                ModifyEmbed(msg, "Input the requirement. format example: 13 - 99");
                randomEventModel.ageRequirement = await WaitForReaction();
                previewEmbed.AddField(new EmbedFieldBuilder { Name = "Age Requirement", Value = randomEventModel.ageRequirement });
                await previewMessage.ModifyAsync(x => x.Embed = previewEmbed.Build());
            }

            ModifyEmbed(msg, "Add a media as banner for the event. *Should be fitting one* **Can be a link or attachment**");
            randomEventModel.imageUrl = await WaitForReaction();

            previewEmbed.WithImageUrl(randomEventModel.imageUrl);
            await previewMessage.ModifyAsync(x => x.Embed = previewEmbed.Build());
        }

        private async Task<string> WaitForReaction()
        {
            var now = DateTime.Now;
            string newMessage = "";
            do
            {
                var iMessage = await message.Channel.GetMessagesAsync(1).Flatten().FirstAsync();

                ulong id = iMessage.Author.Id;

                if (id == message.Author.Id)
                {
                    //If the answer is a image
                    if (iMessage.Attachments.Count > 0)
                    {
                        try
                        {
                            newMessage = iMessage.Attachments.First().Url;

                            //using (var client = new WebClient())
                            //{
                            //    try
                            //    {
                            //        client.DownloadFile(randomEventModel.imageUrl, "../../../Resources/Img/RandomEvent.jpg");
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        throw ex;
                            //    }
                            //}

                            await iMessage.DeleteAsync();
                            break;
                        }
                        catch { }
                    }

                    newMessage = iMessage.Content;
                    await iMessage.DeleteAsync();
                    break;
                }

                await Task.Delay(1000);
            } while (now < DateTime.Now.AddMinutes(10));

            return newMessage;
        }

        private void ModifyEmbed(RestUserMessage msg, String content)
        {
            msg.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Event manager", content, null, null).Build());
        }

        private async Task<RestTextChannel> CreateChannels()
        {
            infoChannel = await discord.GetGuild(505485680344956928).CreateTextChannelAsync(randomEventModel.title + "-event-info", null, new RequestOptions());
            //await infoChannel.AddPermissionOverwriteAsync(discord.GetGuild(505485680344956928).Roles.FirstOrDefault(x => x.Id == 611102875241676811), new OverwritePermissions().Modify(readMessageHistory:Discord.PermValue.Allow, viewChannel: Discord.PermValue.Allow, sendMessages: Discord.PermValue.Deny));
            await infoChannel.AddPermissionOverwriteAsync(discord.GetGuild(505485680344956928).Roles.FirstOrDefault(x => x.Id == 505485680344956928), new OverwritePermissions().Modify(readMessageHistory: Discord.PermValue.Allow, viewChannel: Discord.PermValue.Deny, useExternalEmojis: Discord.PermValue.Deny, sendMessages: Discord.PermValue.Deny));

            generalChannel = await discord.GetGuild(505485680344956928).CreateTextChannelAsync(randomEventModel.title + "-event-general", null, new RequestOptions());
            await generalChannel.AddPermissionOverwriteAsync(discord.GetGuild(505485680344956928).Roles.FirstOrDefault(x => x.Id == 505485680344956928), new OverwritePermissions().Modify(readMessageHistory: Discord.PermValue.Deny, viewChannel: Discord.PermValue.Deny));

            var eventleiders = randomEventModel.eventLeider.Split(" ");
            foreach (var eventleider in eventleiders)
            {
                var id = eventleider.Replace("<@!", "").Replace(">", "");
                var user = discord.GetUser(ulong.Parse(id));
                await infoChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions().Modify(sendMessages: Discord.PermValue.Allow));
            }

            var builder = EmbedBuilderExtension.NullEmbed(randomEventModel.title, randomEventModel.description, null, null);
            builder.AddField(new EmbedFieldBuilder { Name = "Date", Value = randomEventModel.date.ToString() });
            builder.AddField(new EmbedFieldBuilder { Name = "Location", Value = randomEventModel.locatie });
            builder.AddField(new EmbedFieldBuilder { Name = "Eventleader(s)", Value = randomEventModel.eventLeider });
            if (randomEventModel.websiteUrl != "") builder.AddField(new EmbedFieldBuilder { Name = "Website URL", Value = randomEventModel.websiteUrl });
            if (randomEventModel.payment != "") builder.AddField(new EmbedFieldBuilder { Name = "Required payment", Value = randomEventModel.payment });
            if (randomEventModel.minmaxParticipants != "") builder.AddField(new EmbedFieldBuilder { Name = "Minimum & Maximum amount of participants", Value = randomEventModel.minmaxParticipants });
            if(randomEventModel.ageRequirement != "") builder.AddField(new EmbedFieldBuilder { Name = "Age Requirement", Value = randomEventModel.ageRequirement });

            builder.Footer = new EmbedFooterBuilder { Text = "Green check = I will be there \nBlue check = I am interested \nRed cross = I will not be there, Delete this channel" };

            //await infoChannel.SendFileAsync("../../../Resources/Img/randomevent.jpg", "", false);
            await infoChannel.SendMessageAsync(randomEventModel.imageUrl);

            var messageInChannel = await infoChannel.SendMessageAsync(null, false, builder.Build());
            var deelnemersMessage = await infoChannel.SendMessageAsync(null, false, EmbedBuilderExtension.NullEmbed("participants", "", null, null).Build());

            await messageInChannel.AddReactionAsync(Emote.Parse("<:green_check:671412276594475018>"));
            await messageInChannel.AddReactionAsync(Emote.Parse("<:blue_check:671413239992549387>"));
            await messageInChannel.AddReactionAsync(Emote.Parse("<:red_check:671413258468720650>"));

            var deelnemers = new Dictionary<ulong, string[]>();
            //deelnemers.Add(deelnemersMessage.Id, new string[]{ "Testname", "Testname2" });
            var f = messageInChannel.Id.ToString() + "0";
            var generalFakeId = ulong.Parse(f);
            deelnemers.Add(deelnemersMessage.Id, new string[] { "Yeet" });
            deelnemers.Add(generalFakeId, new string[] { generalChannel.Id.ToString() });
            JsonExtension.InsertJsonData("../../../Resources/irleventdata.txt", messageInChannel.Id.ToString(), deelnemers);

            return infoChannel;
        }

        public async void ValidateEvent()
        {
            validationMessage = await discord.GetGuild(505485680344956928).GetTextChannel(505732796245868564).SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("A new event has been created!", $"Created by: {message.Author.Username} - {message.Author.Id}\n\nIs this event ready to be published? \n\n **approving this event will make it public for everyone. Denying it will delete the channels**").Build());
            await validationMessage.AddReactionAsync(Emote.Parse("<:green_check:671412276594475018>"));
            await validationMessage.AddReactionAsync(Emote.Parse("<:red_check:671413258468720650>"));
            discord.ReactionAdded += Discord_ReactionAdded;
        }

        private async Task Discord_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction reaction)
        {
            if (reaction.Emote.Name == "green_check" && reaction.UserId != 504633036902498314)
            {
                // move channel to event category
            
                await infoChannel.ModifyAsync(x => x.CategoryId = 671662624718585887);
                await infoChannel.SyncPermissionsAsync();
                await generalChannel.ModifyAsync(x => x.CategoryId = 671662624718585887);
                await validationMessage.RemoveAllReactionsAsync();
                await validationMessage.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Event Published", "cool").Build());
                discord.ReactionAdded -= Discord_ReactionAdded;
            }

            if (reaction.Emote.Name == "red_check" && reaction.UserId != 504633036902498314)
            {
                await generalChannel.DeleteAsync();
                await infoChannel.DeleteAsync();
                await validationMessage.RemoveAllReactionsAsync();
                await validationMessage.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Event Deleted", "begone").Build());
                discord.ReactionAdded -= Discord_ReactionAdded;
            }
            
            return;
        }
    }
}
