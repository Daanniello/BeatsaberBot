using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Threading;
using System.IO;

namespace DiscordBeatSaberBot
{
    class EventManager
    {
        public ulong MessageId { get; set; }
        private DiscordSocketClient discord;
        private SocketMessage message;
        private string title;
        private string ondertitle;
        private string kern;
        private DateTime datum;
        private string imgUrl;
        private string bijlage;
        private string mention;


        public EventManager(SocketMessage message, DiscordSocketClient discord)
        {

            var tekst = File.ReadAllText("EventMessage.txt");


            try
            {
                this.discord = discord;
                this.message = message;
                QuestionRound().Wait();
                message.Channel.SendMessageAsync("Event wordt geplaatst...");
                SendEventMessage(505485680344956928, 510959349263499264).Wait();
                message.Channel.SendMessageAsync("Event is geplaatst!");
                //539815971183919120 test id
                //510959349263499264 announcement id 
            }
            catch
            {
                message.Channel.SendMessageAsync("Iets is verkeerd gegaan :c");
            }
          
        }

        private async Task QuestionRound()
        {



            await message.Channel.SendMessageAsync("Wat is de titel van het event?");
            title = await WaitForReaction();
            await message.Channel.SendMessageAsync("Wat is de ondertitel van het event?");
            ondertitle = await WaitForReaction();
            await message.Channel.SendMessageAsync("Upload de image voor het event?");
            imgUrl = await WaitForReaction();
            await message.Channel.SendMessageAsync("Voor welke roles moet er een mention komen?");
            mention = await WaitForReaction();
            await message.Channel.SendMessageAsync("Geef nu de kern weer van het event?");
            kern = await WaitForReaction();
            try
            {
                await message.Channel.SendMessageAsync("Geef nu de Datum weer van het event? \n formaat: yyyy-MM-dd HH:mm");
                var stringDatum = await WaitForReaction();
                //2009-05-08 14:40
                datum = DateTime.ParseExact(stringDatum, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {

                await message.Channel.SendMessageAsync("Geef nu de Datum weer van het event? \n formaat: yyyy-MM-dd HH:mm \n laaste poging");
                var stringDatum = await WaitForReaction();
                //2009-05-08 14:40
                datum = DateTime.ParseExact(stringDatum, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            }
            
            await message.Channel.SendMessageAsync("Nog extra bijlagen? urls, ps tekst, reminder");
            bijlage = await WaitForReaction();

          
        }

        private async Task<string> WaitForReaction()
        {
            var now = DateTime.Now;
            var newMessage = "";
            do
            {
                var iMessage = await message.Channel.GetMessagesAsync(1).Flatten().First();
                
                var id = iMessage.Author.Id;

                if (id == message.Author.Id)
                {
                    try
                    {
                        newMessage = iMessage.Attachments.First().Url;
                        break;
                    }
                    catch
                    {

                    }
                    newMessage = iMessage.Content;
                    break;
                }
                await Task.Delay(1000);

            } while (now < DateTime.Now.AddMinutes(10));
            return newMessage;
        }

        private async Task SendEventMessage(ulong guildId, ulong channelId)
        {

            var embedBuilder = new EmbedBuilder
            {
                Title = title,
                Description = "*" + ondertitle + "* \n\n" + kern + "\n\n" + "*" + bijlage + " \n \n Doe je mee met het event? voeg dan even een reactie toe in <#572721078359556097>" + "*",
                Footer = new EmbedFooterBuilder() { Text = "Dit event start op de volgende datum: " + datum.ToString() },
                Color = Color.Red

            };

            var channel = (ISocketMessageChannel)discord.GetGuild(guildId).GetChannel(channelId);

            using (var client = new WebClient())
            {

                client.DownloadFile(imgUrl, @"eventimg.jpg");
                await channel.SendFileAsync("eventimg.jpg");
            }

            var eventMessage = await channel.SendMessageAsync("", false, embedBuilder.Build());
            //await eventMessage.AddReactionAsync(new Emoji("✅"));

            File.WriteAllText("EventMessage.txt", eventMessage.Id.ToString());

            var eventDetailChannel = (ISocketMessageChannel)discord.GetChannel(572721078359556097);
            var embededMessage = (IUserMessage)await eventDetailChannel.GetMessageAsync(586248421715738629);

            var embedInfo = embededMessage.Embeds.First();
            var guild = discord.Guilds.FirstOrDefault(x => x.Id == (ulong)505485680344956928);


            var embedBuilderDetails = new EmbedBuilder
            {
                Title = "Volgende event *" + title +"*",
                Description = "*" + datum.ToString() + "* \n" + "https://discordapp.com/channels/" + guildId + "/" + channelId + "/" + eventMessage.Id.ToString() + "\n" + "Deelnemers: \n",
                Footer = new EmbedFooterBuilder { Text = "Start over: " + Math.Round(datum.Subtract(DateTime.Now).TotalHours) + " uur"},
                Color = Color.Blue

            };

            await embededMessage.ModifyAsync(msg => msg.Embed = embedBuilderDetails.Build());
            await embededMessage.RemoveAllReactionsAsync();
            await embededMessage.AddReactionAsync(new Emoji("✅"));

            var mentionmsg = await channel.SendMessageAsync(mention);
            await mentionmsg.DeleteAsync();
            
        }
    }
}
