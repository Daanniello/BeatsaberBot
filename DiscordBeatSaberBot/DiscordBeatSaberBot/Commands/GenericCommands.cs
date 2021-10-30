using System.Text.RegularExpressions;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using DiscordBeatSaberBot.Extensions;
using System.Net.Http;
using System.Net;
using DiscordBeatSaberBot.Commands.Functions;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBeatSaberBot.Commands
{
    class GenericCommands : ICommand
    {
        [Help("Help", "The help command helps you with getting to know the bot. Use the Help with command name behind it to get more info.", "!bs help", HelpAttribute.Catergories.BotFunctions)]
        static public async Task Help(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (message.Content.Length == 8)
            {
                var currentMessage = await message.Channel.SendMessageAsync("", false, DiscordBeatSaberBot.Help.GetHelpList(discordSocketClient));
                await currentMessage.AddReactionAsync(Emote.Parse("<:left:681842980134584355>"));
                currentMessage.AddReactionAsync(Emote.Parse("<:right:681843066104971287>"));
            }
            else
            {
                var extraInfoName = message.Content.Substring(9).Trim();
                var embed = DiscordBeatSaberBot.Help.GetSpecificHelp(extraInfoName);
                if(embed.Title == null)
                {
                    await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Wrong command name", "The command name does not exist").Build());
                }
                else
                {
                    await message.Channel.SendMessageAsync("", false, embed);
                }
            }
        }

        [Help("Playing", "Sets the game of the discord bot.", "!bs playing (gameName)", HelpAttribute.Catergories.BotFunctions)]
        static public async Task Playing(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (await message.Author.IsDutchAdmin(discordSocketClient))
            {
                var msg = message.Content.Substring(12);

                await DatabaseContext.ExecuteInsertQuery($"Insert into Settings (DiscordPlayingGame) values ('{msg}')");

                await discordSocketClient.SetGameAsync(msg);
                await message.Channel.SendMessageAsync("Game now set to " + msg);
            }
            else
            {
                await message.Channel.SendMessageAsync("Please don't touch this command you normie");
            }            
        }

        [Help("RemoveBG", "Removes the background of an image", "!bs removebg [add the image png, jpg]", HelpAttribute.Catergories.BotFunctions)]
        static public async Task RemoveBG(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var removebgKeyList = new List<string>();
            removebgKeyList.Add("4D9tdgmUy6go4Uj1mPqHmfUc");
            removebgKeyList.Add("PkAgEVZ331oZNihazQEVGUuS");
            removebgKeyList.Add("9UE6rGbDxqHRpHQCqek66Fuy");

            var removebgAPI = "https://api.remove.bg/v1.0/removebg";


            var unscreenKey = "k2k9KCBmSwwGgdHkDw5BPpZM";
            var unscreenAPI = "https://api.unscreen.com/v1.0/videos";

            Attachment attachment;
            string imageUrl;
            if (message.Content.Split(" ").Length > 2)
            {
                imageUrl = message.Content.Substring(13);
            }
            else
            {
                try
                {
                    dynamic attachments = message.Attachments;
                    attachment = attachments[0];
                    imageUrl = attachment.Url;
                }
                catch
                {
                    await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Error", "Could not initialize the attachment").Build());
                    return;
                }
            }                                       

            var saveUrl = $"../../../Resources/img/removebg_{message.Author}.gif";

            var isVideo = false;
            var extension = imageUrl.Split(".")[imageUrl.Split(".").Length - 1].ToLower();
            if (extension.Contains("gif") || extension.Contains("mp4")) isVideo = true;

            
            var response = await SendApiCall(0);
            //await GetApiCall();

            if (response.StatusCode == HttpStatusCode.PaymentRequired)
            {
                var retryCount = 0;
                var wasBreaked = false;
                HttpResponseMessage retryResponse;
                do
                {
                    retryCount++;
                    response = await SendApiCall(retryCount);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        wasBreaked = true;
                        break;
                    }

                } while (response.StatusCode == HttpStatusCode.PaymentRequired || retryCount >= removebgKeyList.Count());

                if (!wasBreaked)
                {
                    await message.Channel.SendMessageAsync("Silverhaze needs to pay for this service... he broke, so no background removals for a month ORRRR donate your api key for 50 more removals a month ;)");
                    return;
                }                
            }

            if (response.StatusCode != HttpStatusCode.OK) return;

            Byte[] bytes = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(saveUrl, bytes);



            await message.Channel.SendFileAsync(saveUrl);
            File.Delete(saveUrl);




            //Api Call method POST
            async Task<HttpResponseMessage> SendApiCall(int tryCount)
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), $"{(isVideo ? unscreenAPI : removebgAPI)}"))
                    {
                        request.Headers.TryAddWithoutValidation("X-API-Key", $"{(isVideo ? unscreenKey : removebgKeyList[tryCount])}");

                        var multipartContent = new MultipartFormDataContent();
                        multipartContent.Add(new StringContent(imageUrl), $"{(isVideo ? "video_url" : "image_url")}");
                        if (isVideo)
                        {
                            multipartContent.Add(new StringContent(extension), "format");
                            if(extension.Contains("mp4")) multipartContent.Add(new StringContent("000000"), "background_color");
                        }
                        request.Content = multipartContent;

                        var response = await httpClient.SendAsync(request);

                        return response;
                    }
                }
            }

            //Api Call method Get
            async Task<HttpResponseMessage> GetApiCall()
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"{unscreenAPI}"))
                    {
                        request.Headers.TryAddWithoutValidation("X-API-Key", $"{unscreenKey}");

                        var response = await httpClient.SendAsync(request);
                        var content = await response.Content.ReadAsStringAsync();
                        return response;
                    }
                }
            }
        }

        [Help("statistics", "Shows statistics from beat saber players", "`!bs statistics`", HelpAttribute.Catergories.BotFunctions)]
        static public async Task Statistics(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var statistics = new Statistics(message);

            var type = await statistics.Start(message);
            //Create statistics image
            var categoryType = await statistics.CreateSelectedType(type);

            if(categoryType.Item1 == Functions.Statistics.category.Error || categoryType.Item2 == Functions.Statistics.type.Error)
            {
                await message.Channel.SendMessageAsync($"", false, EmbedBuilderExtension.NullEmbed("Error", "Could not collect data").Build());
                return;
            }

            var embed = new EmbedBuilder()
            {
                Title = $"Category: {categoryType.Item1}",
                Description = "*Contribute with \n`!bs settings create` \nif you want to help out this data collection.*",
                ImageUrl = $"attachment://piechart-{categoryType.Item1}-{categoryType.Item2}.png"
            }.Build();
            await message.Channel.SendFileAsync($"../../../Resources/img/piechart-{categoryType.Item1}-{categoryType.Item2}.png", embed: embed);

        }

        [Help("Number", "Gives random info about a number", "!bs number (x)", HelpAttribute.Catergories.BotFunctions)]
        static public async Task Number(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var nr = message.Content.Substring(11);
            
            using (var client = new HttpClient())
            {
                var numberdata = await client.GetAsync("http://numbersapi.com/" + nr);
                if (numberdata.StatusCode != HttpStatusCode.OK) return;
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Number Data from " + nr, await numberdata.Content.ReadAsStringAsync()).Build());
            }
        }

        [Help("HelpListRaw", "Gives a raw list of help functions", "!bs playing (gameName)", HelpAttribute.Catergories.BotFunctions)]
        static public async Task HelpListRaw(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            await message.Channel.SendMessageAsync("", false, DiscordBeatSaberBot.Help.GetHelpListRaw());
        }

        [Help("Invite", "Gives an invite link for the bot to join in other discord servers.", "!bs invite", HelpAttribute.Catergories.BotFunctions)]
        static public async Task Invite(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var embedTask = await BeatSaberInfoExtension.GetInviteLink();
            await message.Channel.SendMessageAsync("", false, embedTask.Build());
        }

        [Help("Poll", "Creates a poll with reactions so people can vote on a subject.", "!bs poll (Question)", HelpAttribute.Catergories.BotFunctions)]
        static public async Task Poll(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var poll = new QuickPoll(message);
            await poll.CreatePoll();
            await message.DeleteAsync();
        }
    }
}
