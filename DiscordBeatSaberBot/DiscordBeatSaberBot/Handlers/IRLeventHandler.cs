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
    public class IRLeventHandler
    {
        private SocketMessage message;
        private RestUserMessage msg;
        private DiscordSocketClient discord;
        private IRLeventModel irlEventModel = new IRLeventModel();

        private RestTextChannel infoChannel;
        private RestTextChannel generalChannel;

        public IRLeventHandler(SocketMessage message, DiscordSocketClient discord, RestUserMessage msg)
        {
            try
            {
                this.discord = discord;
                this.message = message;
                this.msg = msg;
                QuestionRound().Wait();
                ModifyEmbed(msg, "Event word aangemaakt...");
                CreateChannels().Wait();
                ModifyEmbed(msg, "Event is succesvol aangemaakt");
            }
            catch(Exception ex)
            {
                ModifyEmbed(msg, $"Er is iets fout gegaan :c \n {ex.Message}");
                infoChannel.DeleteAsync();
                generalChannel.DeleteAsync();
            }
        }

        private async Task QuestionRound()
        {
            ModifyEmbed(msg, "Wat is de titel van het IRL event?");
            irlEventModel.title = await WaitForReaction();

            ModifyEmbed(msg, "Wie is/zijn de eventleiders? *geeft rechten om info toe te voegen of te wijzigen");
            irlEventModel.eventLeider = await WaitForReaction();

            ModifyEmbed(msg, "Wat is de website url?");
            irlEventModel.url = await WaitForReaction();

            ModifyEmbed(msg, "Wat is de datum? Bijvoorbeeld (05 02 2020 14:00)");
            var date = await WaitForReaction();
            try
            {
                irlEventModel.date = DateTime.Parse(date, new CultureInfo("de-DE"));
            }
            catch
            {
                ModifyEmbed(msg, "Datum formaat is verkeerd. Bijvoorbeeld (05 02 2020 14:00)");
                var date2 = await WaitForReaction();
                irlEventModel.date = DateTime.Parse(date2, new CultureInfo("de-DE"));
            }

            ModifyEmbed(msg, "Voeg een image toe dat het IRL event beschrijft");
            irlEventModel.imageUrl = await WaitForReaction();

            ModifyEmbed(msg, "Wat is de prijs om deel te nemen aan het event? Bijvoorbeeld (5.00)");
            irlEventModel.price = await WaitForReaction();

            ModifyEmbed(msg, "Wat is de beschrijving van het event?");
            irlEventModel.description = await WaitForReaction();


        }

        private async Task<string> WaitForReaction()
        {
            var now = DateTime.Now;
            string newMessage = "";
            do
            {
                var iMessage = await message.Channel.GetMessagesAsync(1).Flatten().First();

                ulong id = iMessage.Author.Id;

                if (id == message.Author.Id)
                {
                    if (iMessage.Attachments.Count > 0)
                    {
                        try
                        {
                            irlEventModel.imageUrl = iMessage.Attachments.First().Url;

                            using (var client = new WebClient())
                            {
                                try
                                {                      
                                    client.DownloadFile(irlEventModel.imageUrl, "../../../Resources/Img/irlevent.jpg");                                   
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }

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
            msg.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("IRL Event handler", content, null, null).Build());
        }

        private async Task<RestTextChannel> CreateChannels()
        {
            infoChannel = await discord.GetGuild(505485680344956928).CreateTextChannelAsync(irlEventModel.title + "-irlevent-info", null, new RequestOptions());
            //await infoChannel.AddPermissionOverwriteAsync(discord.GetGuild(505485680344956928).Roles.FirstOrDefault(x => x.Id == 611102875241676811), new OverwritePermissions().Modify(readMessageHistory:Discord.PermValue.Allow, viewChannel: Discord.PermValue.Allow, sendMessages: Discord.PermValue.Deny));
            await infoChannel.AddPermissionOverwriteAsync(discord.GetGuild(505485680344956928).Roles.FirstOrDefault(x => x.Id == 505485680344956928), new OverwritePermissions().Modify(
                readMessageHistory: Discord.PermValue.Allow, 
                viewChannel: Discord.PermValue.Allow, 
                useExternalEmojis: Discord.PermValue.Deny, 
                sendMessages: Discord.PermValue.Deny));

            infoChannel.ModifyAsync(x => x.CategoryId = 671662624718585887);

            generalChannel = await discord.GetGuild(505485680344956928).CreateTextChannelAsync(irlEventModel.title + "-irlevent-general", null, new RequestOptions());
            await generalChannel.AddPermissionOverwriteAsync(discord.GetGuild(505485680344956928).Roles.FirstOrDefault(x => x.Id == 505485680344956928), new OverwritePermissions().Modify(
                readMessageHistory: Discord.PermValue.Deny, 
                viewChannel: Discord.PermValue.Deny));

            generalChannel.ModifyAsync(x => x.CategoryId = 671662624718585887);


            try
            {
                if (double.Parse(irlEventModel.price) > 0)
                {
                    irlEventModel.price = irlEventModel.price + " Euro";
                }
                else
                {
                    irlEventModel.price = "Gratis";
                }
            }
            catch
            {
                irlEventModel.price = "Gratis";
            }

            var eventleiders = irlEventModel.eventLeider.Split(" ");
            foreach (var eventleider in eventleiders) {
                var id = eventleider.Replace("<@!", "").Replace(">", "");
                var user = discord.GetUser(ulong.Parse(id));
                await infoChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions().Modify(sendMessages: Discord.PermValue.Allow));
            }

            var builder = EmbedBuilderExtension.NullEmbed(irlEventModel.title, irlEventModel.description, null, null);
            builder.AddField(new EmbedFieldBuilder { Name = "Datum", Value = irlEventModel.date.ToString() });
            builder.AddField(new EmbedFieldBuilder { Name = "Prijs", Value = irlEventModel.price });
            builder.AddField(new EmbedFieldBuilder { Name = "Link", Value = irlEventModel.url });
            builder.AddField(new EmbedFieldBuilder { Name = "Eventleider(s)", Value = irlEventModel.eventLeider });

            builder.Footer = new EmbedFooterBuilder { Text = "Groen vinkje = Ik neem deel aan \nBlauw vinkje = Ik ben geinteresseerd \nRode kruis = verwijder deze channel"};

            await infoChannel.SendFileAsync("../../../Resources/Img/irlevent.jpg", "", false);

            var messageInChannel = await infoChannel.SendMessageAsync(null, false, builder.Build());
            var deelnemersMessage = await infoChannel.SendMessageAsync(null, false, EmbedBuilderExtension.NullEmbed("Deelnemers", "", null, null).Build());
         
            await messageInChannel.AddReactionAsync(Emote.Parse("<:green_check:671412276594475018>"));
            await messageInChannel.AddReactionAsync(Emote.Parse("<:blue_check:671413239992549387>"));
            await messageInChannel.AddReactionAsync(Emote.Parse("<:red_check:671413258468720650>"));

            var deelnemers = new Dictionary<ulong, string[]>();
            //deelnemers.Add(deelnemersMessage.Id, new string[]{ "Testname", "Testname2" });
            var f = messageInChannel.Id.ToString() + "0";
            var generalFakeId = ulong.Parse(f);
            deelnemers.Add(deelnemersMessage.Id, new string[] { "Yeet" });
            deelnemers.Add(generalFakeId, new string[] { generalChannel.Id.ToString()});            
            JsonExtension.InsertJsonData("../../../Resources/irleventdata.txt", messageInChannel.Id.ToString(), deelnemers);

            return infoChannel;
        }
    }
}
