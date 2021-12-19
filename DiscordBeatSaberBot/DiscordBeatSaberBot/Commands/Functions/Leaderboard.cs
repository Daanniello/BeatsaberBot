using Discord;
using Discord.Rest;
using Discord.WebSocket;
using ScoreSaberLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Commands.Functions
{
    public class Leaderboard
    {
        private ScoreSaberClient _scoresaberClient;
        private DiscordSocketClient _discord;
        private RestUserMessage _msg;
        private int _embedPage = 0;
        private List<ScoreSaberLib.Models.PlayerInfoModel.Player> _players;
        private string _countryCode;
        private List<ScoreSaberLib.Models.LeaderboardScoresModel.Score> _scores;
        private string _mapID;

        public Leaderboard(DiscordSocketClient discord)
        {
            _discord = discord;
            _scoresaberClient = new ScoreSaberClient();
        }

        public async Task GetLeaderboardAndPost(SocketMessage message, string countryCode = "", string mapID = null)
        {
            _mapID = mapID;
            _countryCode = countryCode;
            //Get 1000 players from leaderboard of choice
            var players = new List<ScoreSaberLib.Models.PlayerInfoModel.Player>();
            _scores = new List<ScoreSaberLib.Models.LeaderboardScoresModel.Score>();

            var taskList = new List<Task>();
            for (var i = 0; i < 10; i++)
            {
                GetPlayers(i);
            }

            async void GetPlayers(int i)
            {
                taskList.Add(Task.Run(async () =>
                {
                    if (mapID != null)
                    {
                        if (countryCode != "")
                        {
                            try
                            {
                                var result = await _scoresaberClient.Api.Leaderboards.GetLeaderboardScoresByID(Convert.ToInt32(mapID), countryCode, page: i + 1);
                                if (result != null) _scores.AddRange(result);
                            }
                            catch
                            {
                                return;
                            }                           
                        }
                        else
                        {
                            var result = await _scoresaberClient.Api.Leaderboards.GetLeaderboardScoresByID(Convert.ToInt32(mapID), page: i + 1);
                            if (result != null) _scores.AddRange(result);
                        }                       
                    }
                    else
                    {
                        if (countryCode != "")
                        {
                            var result = await _scoresaberClient.Api.Players.GetPlayers(countryCodes: countryCode, page: i + 1);
                            if (result != null) players.AddRange(result.Players);
                        }
                        else
                        {
                            var result = await _scoresaberClient.Api.Players.GetPlayers(page: i + 1);
                            if (result != null) players.AddRange(result.Players);
                        }
                    }

                }));
            }

            Task.WaitAll(taskList.ToArray());
            _players = players;
            //Create Embed 
            var embedBuilder = await CreateEmbed(players, countryCode, 0, _mapID);

            //Send message
            _msg = await message.Channel.SendMessageAsync("", false, embedBuilder.Build());

            //Add reactions
            await _msg.AddReactionAsync(Emote.Parse("<:left:681842980134584355>"));
            await _msg.AddReactionAsync(Emote.Parse("<:right:681843066104971287>"));

            _discord.ReactionAdded += _discord_ReactionAdded;
        }

        private async Task<EmbedBuilder> CreateEmbed(List<ScoreSaberLib.Models.PlayerInfoModel.Player> players, string countryCode, int page, string mapID = null)
        {
            dynamic commandType = null;
            var mapName = "";
            var mapImage = "";
            if (mapID != null)
            {
                commandType = (CommandTypeMap)page;
                var result = await _scoresaberClient.Api.Leaderboards.GetLeaderboardInfoByID(Convert.ToInt32(mapID));
                mapName = result.SongName;
                mapImage = result.CoverImage.ToString();
            }
            else commandType = (CommandType)page;
            

            var embedBuilder = new EmbedBuilder();
            embedBuilder.Title = mapID != null ? $"Leaderboard {mapName} {countryCode.ToUpper()} ({commandType})" : $"Leaderboard {(countryCode != "" ? countryCode.ToUpper() : "Global")} ({commandType})";
            embedBuilder.Color = Color.Red;
            if (mapID != null) embedBuilder.Url = $"https://scoresaber.com/leaderboard/{mapID}";
            embedBuilder.ThumbnailUrl = mapID != null ? mapImage : "https://i.imgur.com/KQgAVOB.png";
            embedBuilder.Timestamp = DateTime.Now;
            var description = $"***Top 25 | Data by the top {(mapID != null ? $"{_scores.Count}" : $"{_players.Count}")} of {(countryCode != "" ? countryCode : "Global")}***\n\n";
            //Order by commandType

            if(mapID != null)
            {
                if (commandType == CommandTypeMap.Rank)
                {
                    _scores = _scores.OrderBy(x => x.Rank).ToList();
                    for (var i = 0; i < (_scores.Count < 25 ? _scores.Count : 25); i++)
                    {
                        description += $"**{_scores[i].Rank}** - [{_scores[i].LeaderboardPlayerInfo.Name}](https://scoresaber.com/u/{_scores[i].LeaderboardPlayerInfo.Id}) \n";
                    }
                }
                if (commandType == CommandTypeMap.MaxCombo)
                {
                    _scores = _scores.OrderByDescending(x => x.MaxCombo).ToList();
                    for (var i = 0; i < (_scores.Count < 25 ? _scores.Count : 25); i++)
                    {
                        description += $"**{_scores[i].MaxCombo}** - [{_scores[i].LeaderboardPlayerInfo.Name}](https://scoresaber.com/u/{_scores[i].LeaderboardPlayerInfo.Id}) \n";
                    }
                }
            }
            else
            {
                if (commandType == CommandType.Rank)
                {
                    players = players.OrderBy(x => x.Rank).ToList();
                    for (var i = 0; i < 25; i++)
                    {
                        description += $"**{players[i].Rank}** - [{players[i].Name}](https://scoresaber.com/u/{players[i].Id}) \n";
                    }
                }
                if (commandType == CommandType.RanksUp)
                {
                    players = players.OrderByDescending(x => x.Histories.Last() - x.Histories.First()).ToList();
                    for (var i = 0; i < 25; i++)
                    {
                        description += $"**{players[i].Histories.Last() - players[i].Histories.First()}** - [{players[i].Name}](https://scoresaber.com/u/{players[i].Id}) \n";
                    }
                }
                if (commandType == CommandType.TotalScore)
                {
                    players = players.OrderByDescending(x => x.ScoreStats.TotalScore).ToList();
                    for (var i = 0; i < 25; i++)
                    {
                        description += $"**{players[i].ScoreStats.TotalScore}** - [{players[i].Name}](https://scoresaber.com/u/{players[i].Id}) \n";
                    }
                }
                if (commandType == CommandType.Replays)
                {
                    players = players.OrderByDescending(x => x.ScoreStats.ReplaysWatched).ToList();
                    for (var i = 0; i < 25; i++)
                    {
                        description += $"**{players[i].ScoreStats.ReplaysWatched}** - [{players[i].Name}](https://scoresaber.com/u/{players[i].Id}) \n";
                    }
                }
                if (commandType == CommandType.RankedPlayCount)
                {
                    players = players.OrderByDescending(x => x.ScoreStats.RankedPlayCount).ToList();
                    for (var i = 0; i < 25; i++)
                    {
                        description += $"**{players[i].ScoreStats.RankedPlayCount}** - [{players[i].Name}](https://scoresaber.com/u/{players[i].Id}) \n";
                    }
                }
                if (commandType == CommandType.Accuracy)
                {
                    players = players.OrderByDescending(x => x.ScoreStats.AverageRankedAccuracy).ToList();
                    for (var i = 0; i < 25; i++)
                    {
                        description += $"**{Math.Round(players[i].ScoreStats.AverageRankedAccuracy, 2)}** - [{players[i].Name}](https://scoresaber.com/u/{players[i].Id}) \n";
                    }
                }
                if (commandType == CommandType.TotalPlayCount)
                {
                    players = players.OrderByDescending(x => x.ScoreStats.TotalPlayCount).ToList();
                    for (var i = 0; i < 25; i++)
                    {
                        description += $"**{players[i].ScoreStats.TotalPlayCount}** - [{players[i].Name}](https://scoresaber.com/u/{players[i].Id}) \n";
                    }
                }
            }          

            embedBuilder.Description = $"{description}";

            return embedBuilder;
        }

        private async Task _discord_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg3.UserId != 504633036902498314 && arg3.MessageId == _msg.Id)
            {
                if (arg3.Emote.ToString() == "<:left:681842980134584355>")
                {
                    if (_embedPage <= 0) return;
                    var embed = await CreateEmbed(_players, _countryCode, _embedPage - 1, _mapID);
                    _msg.ModifyAsync(x => x.Embed = embed.Build());
                    _embedPage--;
                    await _msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                }
                if (arg3.Emote.ToString() == "<:right:681843066104971287>")
                {
                    if (_embedPage >= (_mapID != null ? Enum.GetValues(typeof(CommandTypeMap)).Length : Enum.GetValues(typeof(CommandType)).Length)) return;
                    var embed = await CreateEmbed(_players, _countryCode, _embedPage + 1, _mapID);
                    _msg.ModifyAsync(x => x.Embed = embed.Build());
                    _embedPage++;
                    await _msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                }
            }
            return;
        }

        private enum CommandType
        {
            Rank = 0,
            RanksUp = 1,
            TotalScore = 2,
            Replays = 3,
            RankedPlayCount = 4,
            Accuracy = 5,
            TotalPlayCount = 6
        }

        private enum CommandTypeMap
        {
            Rank = 0,
            MaxCombo = 1,
            AvgAcc = 2
        }
    }
}
