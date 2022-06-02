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
        static public async Task Help(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {
            if (!(command.Data.Options.Count > 0))
            {
                var componentBuilder = new ComponentBuilder();
                componentBuilder.WithButton(emote: Emote.Parse("<:leftarrow:923182957798244402>"), customId: "leftHelpButton", style: ButtonStyle.Primary);                
                componentBuilder.WithButton("Website", style: ButtonStyle.Link, url: "http://beatsaberbot.com/");
                componentBuilder.WithButton("Github", style: ButtonStyle.Link, url: "https://github.com/Daanniello/BeatsaberBot");
                componentBuilder.WithButton("Discord Server", style: ButtonStyle.Link, url: "https://discord.gg/S3D3Yyu");
                componentBuilder.WithButton(emote: Emote.Parse("<:rightArrow:923182974638358528>"), customId: "rightHelpButton", style: ButtonStyle.Primary);
                var currentMessage = await command.Channel.SendMessageAsync("", false, DiscordBeatSaberBot.Help.GetHelpList(discordSocketClient), components: componentBuilder.Build());

            }
            else
            {
                var extraInfoName = command.Data.Options.First().Value.ToString().Trim();
                var embed = DiscordBeatSaberBot.Help.GetSpecificHelp(extraInfoName);
                if (embed.Title == null)
                {
                    await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Wrong command name", "The command name does not exist").Build());
                }
                else
                {
                    await command.Channel.SendMessageAsync("", false, embed);
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
        static public async Task RemoveBG(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
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
            if (command.Data.Options.Count > 0)
            {
                imageUrl = command.Data.Options.First().Value.ToString();
            }
            else
            {
                try
                {
                    var file = await command.Channel.GetMessagesAsync(3).FlattenAsync();
                    dynamic attachments =  file.First().Attachments;
                    attachment = attachments[0];
                    imageUrl = attachment.Url;
                }
                catch
                {
                    await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Error", "Could not initialize the attachment").Build());
                    return;
                }
            }

            var saveUrl = $"../../../Resources/img/removebg_{command.User.Id}.gif";

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
                    await command.Channel.SendMessageAsync("Silverhaze needs to pay for this service... he broke, so no background removals for a month ORRRR donate your api key for 50 more removals a month ;)");
                    return;
                }
            }

            if (response.StatusCode != HttpStatusCode.OK) return;

            Byte[] bytes = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(saveUrl, bytes);



            await command.Channel.SendFileAsync(saveUrl);
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
                            if (extension.Contains("mp4")) multipartContent.Add(new StringContent("000000"), "background_color");
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
        static public async Task Statistics(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {
            var statistics = new Statistics(command);

            var type = await statistics.Start(command);
            //Create statistics image
            var categoryType = await statistics.CreateSelectedType(type);

            if (categoryType.Item1 == Functions.Statistics.category.Error || categoryType.Item2 == Functions.Statistics.type.Error)
            {
                await command.Channel.SendMessageAsync($"", false, EmbedBuilderExtension.NullEmbed("Error", "Could not collect data").Build());
                return;
            }

            var embed = new EmbedBuilder()
            {
                Title = $"Category: {categoryType.Item1}",
                Description = "*Contribute with \n`!bs settings create` \nif you want to help out this data collection.*",
                ImageUrl = $"attachment://piechart-{categoryType.Item1}-{categoryType.Item2}.png"
            }.Build();
            await command.Channel.SendFileAsync($"../../../Resources/img/piechart-{categoryType.Item1}-{categoryType.Item2}.png", embed: embed);

        }

        [Help("HelpListRaw", "Gives a raw list of help functions", "!bs helplistraw", HelpAttribute.Catergories.BotFunctions)]
        static public async Task HelpListRaw(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            await message.Channel.SendMessageAsync("", false, DiscordBeatSaberBot.Help.GetHelpListRaw());
        }

        [Help("Invite", "Gives an invite link for the bot to join in other discord servers.", "!bs invite", HelpAttribute.Catergories.BotFunctions)]
        static public async Task Invite(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {
            var embedTask = await BeatSaberInfoExtension.GetInviteLink();
            await command.Channel.SendMessageAsync("", false, embedTask.Build());
        }

        [Help("Leaderboard", "Gives a leaderboard with 25 players based on stats", "`!bs leaderboard [leaderboardID] [countrycode]`", HelpAttribute.Catergories.BotFunctions)]
        static public async Task Leaderboard(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var parameter = message.Content.Substring(15).Trim().Split(" ");
            string mapID = null;
            string countryCode = parameter.First();
            if (parameter.First().All(char.IsDigit) && parameter.First() != "")
            {
                mapID = parameter.First();
                if (parameter.Length > 1) countryCode = parameter[1];
                else countryCode = "";
            }
            await new Leaderboard(discordSocketClient).GetLeaderboardAndPost(message, countryCode, mapID);
        }

        [Help("Tools", "Gives a list of important community tools with description and link", "`!bs tools` | `!bs tools [ToolName]` | `!bs tools [ToolName] vote [1-5]`", HelpAttribute.Catergories.BotFunctions)]
        static public async Task Tools(DiscordSocketClient discordSocketClient, SocketSlashCommand command)
        {            
            if (command.Data.Options.Count > 0)
            {
                if (command.Data.Options.Count == 2)
                {
                    var toolName = command.Data.Options.ElementAt(0).Value.ToString();
                    var stars = Convert.ToInt32( command.Data.Options.ElementAt(1).Value);


                    await new ToolsList(discordSocketClient).VoteOnTool(toolName.Trim(), stars, command);
                }
                else if(command.Data.Options.Count == 1)
                {
                    await new ToolsList(discordSocketClient).SendMessageWithToolName(command, command.Data.Options.First().Value.ToString());
                }
            }
            else await new ToolsList(discordSocketClient).SendMessage(command);
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
