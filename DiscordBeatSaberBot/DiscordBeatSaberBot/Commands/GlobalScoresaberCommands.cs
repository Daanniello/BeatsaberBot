using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace DiscordBeatSaberBot.Commands
{
    internal class GlobalScoresaberCommands : ICommand
    {
        [Help("Top10", "Show the global Top 10 players.")]
        public static async Task Top10(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var embedTask = await BeatSaberInfoExtension.GetTop10Players();
            await message.Channel.SendMessageAsync("", false, embedTask.Build());
        }

        [Help("Playerbase", "Returns the playerbase from scoresaber")]
        public static async Task Playerbase(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            new Thread(async () =>
            {
                var count = await Rank.GetPlayerbaseCount(message);
                message.Channel.SendMessageAsync("", false, count);
            }).Start();
        }

        [Help("Compare", "Compares two player's stats with each other.")]
        public static async Task Comapre(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var embedTask = await BeatSaberInfoExtension.PlayerComparer(message.Content.Substring(12));
            await message.Channel.SendMessageAsync("", false, embedTask.Build());
        }

        [Help("Country", "Searches up the top x from a country")]
        public static async Task Country(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var embedTask = await BeatSaberInfoExtension.GetTopxCountry(message.Content.Substring(12));
            await message.Channel.SendMessageAsync("", false, embedTask.Build());
        }

        [Help("Songs", "Searches up the song with the name")]
        public static async Task Songs(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var builderList = await BeatSaberInfoExtension.GetSongs(message.Content.Substring(10));
            if (builderList.Count > 4)
            {
                await message.Channel.SendMessageAsync("", false,
                    EmbedBuilderExtension.NullEmbed("Search term to wide",
                        "I can not post more as 6 songs. " + "\n Try searching with a more specific word please. \n" +
                        ":rage:", null, null).Build());
            }
            else
            {
                foreach (var builder in builderList)
                {
                    await message.Channel.SendMessageAsync("", false, builder.Build());
                }
            }
        }

        [Help("RecentSong", "Get info from the latest song played")]
        public static async Task RecentSong(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()) && message.Content.Count() == 14)
            {
                var scoresaberId = r.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
                var embedTask = await BeatSaberInfoExtension.GetRecentSongWithId(scoresaberId);
                await message.Channel.SendMessageAsync("", false, embedTask.Build());
            }
            else
            {
                if (message.Content.Length <= 18)
                {
                    await message.Channel.SendMessageAsync("", false,
                        EmbedBuilderExtension.NullEmbed("Search failed",
                                "Search value is not long enough. it should be larger than 3 characters.", null, null)
                            .Build());
                    return;
                }

                var username = message.Content.Substring(15);
                var id = await BeatSaberInfoExtension.GetPlayerId(username);
                var embedTask = await BeatSaberInfoExtension.GetRecentSongWithId(id[0]);
                await message.Channel.SendMessageAsync("", false, embedTask.Build());
            }
        }

        [Help("TopSong", "Show the best played ranked map from a person")]
        public static async Task TopSong(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()) && message.Content.Trim().Count() == 11)
            {
                var scoresaberId = r.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
                var embedTask = await BeatSaberInfoExtension.GetBestSongWithId(scoresaberId);
                await message.Channel.SendMessageAsync("", false, embedTask.Build());
            }
            else
            {
                var embedTask = await BeatSaberInfoExtension.GetTopSongList(message.Content.Substring(12));
                await message.Channel.SendMessageAsync("", false, embedTask.Build());
            }
        }

        [Help("Search", "Shows scoresaber information about a player")]
        public static async Task Search(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            if (message.Content.Contains("@"))
            {
                var discordId = message.Content.Substring(11).Replace("<", "").Replace(">", "").Replace("@", "")
                    .Replace("!", "");
                if (r.CheckIfDiscordIdIsLinked(discordId))
                {
                    var scoresaberId = r.GetScoresaberIdWithDiscordId(discordId);
                    var embedTask = await BeatSaberInfoExtension.SearchLinkedPlayer(scoresaberId);
                    await message.Channel.SendMessageAsync("", false, embedTask.Build());
                }
                else
                {
                    await message.Channel.SendMessageAsync("", false,
                        EmbedBuilderExtension.NullEmbed("User's discord not linked",
                            "Your discord is not linked yet. Type !bs verification [Scoresaberlink] to link it.", null,
                            null).Build());
                }
            }
            else if (r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()) && message.Content.Count() == 10)
            {
                var scoresaberId = r.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
                var embedTask = await BeatSaberInfoExtension.SearchLinkedPlayer(scoresaberId);
                await message.Channel.SendMessageAsync("", false, embedTask.Build());
            }
            else
            {
                if (message.Content.Substring(10).Count() == 0)
                {
                    await message.Channel.SendMessageAsync("", false,
                        EmbedBuilderExtension.NullEmbed("User's discord not linked",
                            "Your discord is not linked yet. Type !bs verification [Scoresaberlink] to link it.", null,
                            null).Build());
                    return;
                }

                foreach (var embed in await BeatSaberInfoExtension.GetPlayer(message.Content.Substring(11)))
                {
                    var completedMessage = await message.Channel.SendMessageAsync("", false, embed.Build());

                    //await completedMessage.AddReactionAsync(new Emoji("⬅"));
                    //await completedMessage.AddReactionAsync(new Emoji("➡"));
                }
            }
        }
    }
}