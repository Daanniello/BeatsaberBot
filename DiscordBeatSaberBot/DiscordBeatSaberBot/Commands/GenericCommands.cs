using System.Text.RegularExpressions;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using DiscordBeatSaberBot.Extensions;

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
            if (message.Author.IsDutchAdmin(discordSocketClient))
            {
                var msg = message.Content.Substring(12);
                //C:\Users\Daan\Documents\GitHub\BeatsaberBot\DiscordBeatSaberBot\DiscordBeatSaberBot\BeatSaberSettings.txt
                JsonExtension.InsertJsonData(@"../../../BeatSaberSettings.txt", "gamePlaying", msg);
                await discordSocketClient.SetGameAsync(msg);
                await message.Channel.SendMessageAsync("Game now set to " + msg);
            }
            else
            {
                await message.Channel.SendMessageAsync("Please don't touch this command you normie");
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

        [Help("SendDM", "Sends a DM to somone if he is in the server", "!bs senddm (DiscordTag or ID) (Message)", HelpAttribute.Catergories.BotFunctions)]
        static public async Task SendDM(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var guildUserSender = message.Author as SocketGuildUser;
            if (guildUserSender.IsDutchAdmin() || ValidationExtension.IsOwner(message.Author.Id))
            {
                //!bs senddm <@!32432424> hello
                var content = message.Content.Substring(11);
                var splitContent = Regex.Split(content, @"<@!([0-9]+)>");
                var user = discordSocketClient.GetUser(ulong.Parse(splitContent[1]));

                await user.SendDMMessage(splitContent[2], discordSocketClient);
                await message.Channel.SendMessageAsync($"DM send to {splitContent[1]}");

            }
        }

        [Help("Poll", "Creates a poll with reactions so people can vote on a subject.", "!bs poll (Question)", HelpAttribute.Catergories.BotFunctions)]
        static public async Task Poll(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var poll = new QuickPoll(message);
            await poll.CreatePoll();
        }
    }
}
