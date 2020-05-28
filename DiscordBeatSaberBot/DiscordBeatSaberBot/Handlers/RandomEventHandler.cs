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

        public RandomEventHandler(SocketMessage message, DiscordSocketClient discord, RestUserMessage msg)
        {
            string tekst = File.ReadAllText("../../../Resources/irleventdata.txt");


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
            catch
            {
                ModifyEmbed(msg, "Er is iets fout gegaan :c");
                infoChannel.DeleteAsync();
                generalChannel.DeleteAsync();
            }
        }

        private async Task QuestionRound()
        {
            ModifyEmbed(msg, "Wat is de titel van het event? (Geen lange titel)");
            randomEventModel.title = await WaitForReaction();

            ModifyEmbed(msg, "Wie is/zijn de eventleiders? *geeft rechten om info toe te voegen of te wijzigen");
            randomEventModel.eventLeider = await WaitForReaction();

            ModifyEmbed(msg, "Wat is de datum? Bijvoorbeeld (05 02 2020 14:00)");
            var date = await WaitForReaction();
            try
            {
                randomEventModel.date = DateTime.Parse(date, new CultureInfo("de-DE"));
            }
            catch
            {
                ModifyEmbed(msg, "Datum formaat is verkeerd. Bijvoorbeeld (05 02 2020 14:00)");
                var date2 = await WaitForReaction();
                randomEventModel.date = DateTime.Parse(date2, new CultureInfo("de-DE"));
            }

            ModifyEmbed(msg, "Wat is de locatie waar het wordt gehouden?");
            randomEventModel.locatie = await WaitForReaction();

            ModifyEmbed(msg, "Voeg een image toe dat met het event te maken heeft");
            randomEventModel.imageUrl = await WaitForReaction();

            ModifyEmbed(msg, "Wat is de beschrijving van het event?");
            randomEventModel.description = await WaitForReaction();


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
                            randomEventModel.imageUrl = iMessage.Attachments.First().Url;

                            using (var client = new WebClient())
                            {
                                try
                                {
                                    client.DownloadFile(randomEventModel.imageUrl, "../../../Resources/Img/RandomEvent.jpg");
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
            msg.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Random Event generator", content, null, null).Build());
        }

        private async Task<RestTextChannel> CreateChannels()
        {
            infoChannel = await discord.GetGuild(505485680344956928).CreateTextChannelAsync(randomEventModel.title + "-randomevent-info", null, new RequestOptions());
            //await infoChannel.AddPermissionOverwriteAsync(discord.GetGuild(505485680344956928).Roles.FirstOrDefault(x => x.Id == 611102875241676811), new OverwritePermissions().Modify(readMessageHistory:Discord.PermValue.Allow, viewChannel: Discord.PermValue.Allow, sendMessages: Discord.PermValue.Deny));
            await infoChannel.AddPermissionOverwriteAsync(discord.GetGuild(505485680344956928).Roles.FirstOrDefault(x => x.Id == 505485680344956928), new OverwritePermissions().Modify(readMessageHistory: Discord.PermValue.Allow, viewChannel: Discord.PermValue.Deny, useExternalEmojis: Discord.PermValue.Deny, sendMessages: Discord.PermValue.Deny));

            generalChannel = await discord.GetGuild(505485680344956928).CreateTextChannelAsync(randomEventModel.title + "-randomevent-general", null, new RequestOptions());
            await generalChannel.AddPermissionOverwriteAsync(discord.GetGuild(505485680344956928).Roles.FirstOrDefault(x => x.Id == 505485680344956928), new OverwritePermissions().Modify(readMessageHistory: Discord.PermValue.Deny, viewChannel: Discord.PermValue.Deny));

            var eventleiders = randomEventModel.eventLeider.Split(" ");
            foreach (var eventleider in eventleiders)
            {
                var id = eventleider.Replace("<@!", "").Replace(">", "");
                var user = discord.GetUser(ulong.Parse(id));
                await infoChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions().Modify(sendMessages: Discord.PermValue.Allow));
            }

            var builder = EmbedBuilderExtension.NullEmbed(randomEventModel.title, randomEventModel.description, null, null);
            builder.AddField(new EmbedFieldBuilder { Name = "Datum", Value = randomEventModel.date.ToString() });
            builder.AddField(new EmbedFieldBuilder { Name = "Locatie", Value = randomEventModel.locatie });
            builder.AddField(new EmbedFieldBuilder { Name = "Eventleider(s)", Value = randomEventModel.eventLeider });

            builder.Footer = new EmbedFooterBuilder { Text = "Groen vinkje = Ik neem deel aan \nBlauw vinkje = Ik ben geinteresseerd \nRode kruis = verwijder deze channel" };

            await infoChannel.SendFileAsync("../../../Resources/Img/randomevent.jpg", "", false);

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
            deelnemers.Add(generalFakeId, new string[] { generalChannel.Id.ToString() });
            JsonExtension.InsertJsonData("../../../Resources/irleventdata.txt", messageInChannel.Id.ToString(), deelnemers);

            return infoChannel;
        }
    }
}
