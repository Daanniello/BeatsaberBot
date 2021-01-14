using System.Text.RegularExpressions;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using DiscordBeatSaberBot.Extensions;
using System.Net.Http;
using System.Net;

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
        
        [Discord.Commands.RequireOwner]
        [Help("CreateAchievementFeed", "Creates the current channel to an achievementfeed channel", "!bs createachievementfeed", HelpAttribute.Catergories.AdminCommands)]
        static public async Task CreateAchievementFeed(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var chnl = message.Channel as SocketGuildChannel; 
            await DatabaseContext.ExecuteInsertQuery($"Insert into AchievementFeedSettings (GuildId, ChannelId) values ({chnl.Guild.Id}, {chnl.Id})");
            await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed($"This channel is now an achievement-feed channel", "Use [!bs achievmentfeed] to be shown in the achievement-feed").Build());
        }

        [Help("AchievementFeedtest", "Adds you to the achievementsfeed in the current discord server", "!bs achievementfeedtest", HelpAttribute.Catergories.BotFunctions)]
        static public async Task AchievementFeedTest(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var discordId = message.Author.Id;
            var r = new RoleAssignment(discordSocketClient);
            var scoresaberId = await r.GetScoresaberIdWithDiscordId(discordId.ToString());
            if (scoresaberId != "")
            {
               //todo
            }
            else
            {
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Not linked yet", "You are not linked with the bot yet. Type !bs link [ScoresaberID] to link your scoresaber with discord.").Build());
            }
        }

        [Help("AchievementFeed", "Adds you to the achievementsfeed in Silverhazes discord", "!bs achievement", HelpAttribute.Catergories.BotFunctions)]
        static public async Task AchievementFeed(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var discordId = message.Author.Id;
            var r = new RoleAssignment(discordSocketClient);
            var scoresaberId = await r.GetScoresaberIdWithDiscordId(discordId.ToString());
            if(scoresaberId != "")
            {
                try
                {
                    await DatabaseContext.ExecuteInsertQuery($"Insert into ServerSilverhazeAchievementFeed (ScoreSaberId) values ('{scoresaberId}')");
                }
                catch
                {
                    await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed($"Errorrrrr", "are you already in the feed?").Build());
                }
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed($"Successfully added with {scoresaberId}", "Your achievements will now be shown in the achievement channel").Build());

            }
            else
            {
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Not linked yet", "You are not linked with the bot yet. Type !bs link [ScoresaberID] to link your scoresaber with discord.").Build());
            }            
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
