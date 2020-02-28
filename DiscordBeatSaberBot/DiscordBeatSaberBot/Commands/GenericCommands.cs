using System.Text.RegularExpressions;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using DiscordBeatSaberBot.Extensions;

namespace DiscordBeatSaberBot.Commands
{
    class GenericCommands : ICommand
    {
        [Help("Help", "The help command helps you with getting to know the bot. Use the Help with command name behind it to get more info.", HelpAttribute.Catergories.BotFunctions)]
        static public async Task Help(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (message.Content.Length == 8)
            {
                var currentMessage = await message.Channel.SendMessageAsync("", false, DiscordBeatSaberBot.Help.GetHelpList(discordSocketClient));
                await currentMessage.AddReactionAsync(Emote.Parse("<:left:681842980134584355>"));
                currentMessage.AddReactionAsync(Emote.Parse("<:right:681843066104971287>"));
            }
        }

        [Help("Playing", "Sets the game of the discord bot.", HelpAttribute.Catergories.BotFunctions)]
        static public async Task Playing(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var msg = message.Content.Substring(12);
            //C:\Users\Daan\Documents\GitHub\BeatsaberBot\DiscordBeatSaberBot\DiscordBeatSaberBot\BeatSaberSettings.txt
            JsonExtension.InsertJsonData(@"../../../BeatSaberSettings.txt", "gamePlaying", msg);
            await discordSocketClient.SetGameAsync(msg);
            await message.Channel.SendMessageAsync("Game now set to " + msg);
        }

        [Help("Invite", "Gives an invite link for the bot to join in other discord servers.", HelpAttribute.Catergories.BotFunctions)]
        static public async Task Invite(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var embedTask = await BeatSaberInfoExtension.GetInviteLink();
            await message.Channel.SendMessageAsync("", false, embedTask.Build());
        }

        [Help("SendDM", "Sends a DM to somone if he is in the server", HelpAttribute.Catergories.BotFunctions)]
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

        [Help("Poll", "Creates a poll with reactions so people can vote on a subject.", HelpAttribute.Catergories.BotFunctions)]
        static public async Task Poll(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var poll = new QuickPoll(message);
            await poll.CreatePoll();
        }
    }
}
