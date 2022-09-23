using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using ScoreSaberLib;
using System.Drawing;
using DiscordBeatSaberBot.Api.BeatSaverApi;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Discord;
using System.Threading.Tasks;
using Color = System.Drawing.Color;

namespace DiscordBeatSaberBot
{
    public class BeatSaberCardCollection
    {
        private static List<string> BanList = new List<string>()
        {
            "3433762889998282"
        };

        private DiscordSocketClient _discord;
        private bool hasNextBeenPressed = false;
        private bool tradeHasBeenAccepted = false;
        private SocketUser userToTradeWith;
        private ulong dmMessageID;

        public BeatSaberCardCollection(DiscordSocketClient discord)
        {
            _discord = discord;
        }

        public static async Task DrawAndSendRandomFifaCard(SocketSlashCommand command)
        {

            try
            {
                //Get player info
                var players = new List<ScoreSaberLib.Models.PlayerInfoModel.Player>();
                var amount = 20;
                for (var i = 1; i <= amount; i++)
                {
                    var playersData = await new ScoreSaberClient().Api.Players.GetPlayers(page: i);
                    players.AddRange(playersData.Players);
                }
                players = players.OrderBy(x => x.Rank).ToList();
                var top10 = 100;
                var top50 = 300;
                var top100 = 2000;
                var top250 = 20000;
                var top500 = 50000;
                var top1000 = 100000;
                var nr = new Random().Next(0, 100000);
                var rangeBegin = 0;
                var rangeEnd = 0;

                if (nr < top10)
                {
                    rangeBegin = 0;
                    rangeEnd = 10;
                }
                else if (nr < top50)
                {
                    rangeBegin = 10;
                    rangeEnd = 50;
                }
                else if (nr < top100)
                {
                    rangeBegin = 50;
                    rangeEnd = 100;
                }
                else if (nr < top250)
                {
                    rangeBegin = 100;
                    rangeEnd = 250;
                }
                else if (nr < top500)
                {
                    rangeBegin = 250;
                    rangeEnd = 500;
                }
                else if (nr <= top1000)
                {
                    rangeBegin = 500;
                    rangeEnd = 999;
                }

                var random = new Random();
                var player = players[random.Next(rangeBegin, rangeEnd)];

                //Prevent banned people from showing up. 
                if (BanList.Contains(player.Id))
                {
                    do
                    {
                        player = players[random.Next(rangeBegin, rangeEnd)];
                    } while (BanList.Contains(player.Id));
                }

                var playerFull = await new ScoreSaberClient().Api.Players.GetPlayer(Convert.ToInt64(player.Id));

                var hashList = new List<string>();
                var scores = new List<ScoreSaberLib.Models.PlayerScoresModel.PlayerScore>();
                var scores100 = await new ScoreSaberClient().Api.Players.GetPlayerScores(Convert.ToInt64(player.Id), 100, sort: Players.sort.top, page: 1);
                var scores200 = await new ScoreSaberClient().Api.Players.GetPlayerScores(Convert.ToInt64(player.Id), 100, sort: Players.sort.top, page: 2);
                var scores100recent = await new ScoreSaberClient().Api.Players.GetPlayerScores(Convert.ToInt64(player.Id), 100, sort: Players.sort.recent, page: 1);
                scores.AddRange(scores100);
                scores.AddRange(scores200);
                scores.AddRange(scores100recent);
                foreach (var score in scores100) hashList.Add(score.Leaderboard.SongHash);
                foreach (var score in scores200) hashList.Add(score.Leaderboard.SongHash);
                foreach (var score in scores100recent) hashList.Add(score.Leaderboard.SongHash);

                //Get all played maps with their tags
                var beatSaverMaps = new List<JToken>();
                JObject data = await new BeatSaverApi("").GetMapsByHash(hashList.GetRange(0, 49));
                var maps = data.Children().Where(x => x.ToString().Contains("\"tags\""));
                beatSaverMaps.AddRange(maps);
                JObject data2 = await new BeatSaverApi("").GetMapsByHash(hashList.GetRange(51, 49));
                var maps2 = data2.Children().Where(x => x.ToString().Contains("\"tags\""));
                beatSaverMaps.AddRange(maps2);
                JObject data3 = await new BeatSaverApi("").GetMapsByHash(hashList.GetRange(101, 49));
                var maps3 = data3.Children().Where(x => x.ToString().Contains("\"tags\""));
                beatSaverMaps.AddRange(maps3);
                JObject data4 = await new BeatSaverApi("").GetMapsByHash(hashList.GetRange(151, 49));
                var maps4 = data4.Children().Where(x => x.ToString().Contains("\"tags\""));
                beatSaverMaps.AddRange(maps4);
                JObject data5 = await new BeatSaverApi("").GetMapsByHash(hashList.GetRange(201, 49));
                var maps5 = data5.Children().Where(x => x.ToString().Contains("\"tags\""));
                beatSaverMaps.AddRange(maps5);
                JObject data6 = await new BeatSaverApi("").GetMapsByHash(hashList.GetRange(251, 49));
                var maps6 = data6.Children().Where(x => x.ToString().Contains("\"tags\""));
                beatSaverMaps.AddRange(maps6);

                var hashTags = new Dictionary<string, string>();
                foreach (var map in beatSaverMaps)
                {
                    var tags = map.First().Children().Where(x => x.ToString().Contains("\"tags\""));
                    dynamic m = map;
                    if (hashTags.Keys.FirstOrDefault(x => x.Contains(m.Name)) != null) continue;
                    hashTags.Add(m.Name, tags.First().ToString());
                }

                //calculate points
                var speed = 0;
                var tech = 0;
                var stamina = 0;
                var accuracy = 0;
                var total = 0;

                var speedHashes = hashTags.Where(x => x.Value.ToLower().Contains("speed") || x.Value.ToLower().Contains("challenge") && !x.Value.ToLower().Contains("speedcore"));
                var speedCount = 0;
                foreach (var speedhash in speedHashes)
                {
                    var score = scores.FirstOrDefault(x => x.Leaderboard.SongHash.ToLower() == speedhash.Key);
                    var totalPlays = score.Leaderboard.Plays;
                    var playerRank = score.Score.Rank;
                    var points = 100 - ((int)playerRank * 100 / ((int)totalPlays));
                    var pointloss = (double)player.Rank / 2000;
                    var newpoints = (double)points - pointloss * (double)points;
                    points = (int)newpoints;
                    speedCount++;
                    speed += points;
                }
                if (speedCount > 0) speed = speed / speedCount;

                var accHashes = hashTags.Where(x => x.Value.ToLower().Contains("accuracy") || x.Value.ToLower().Contains("balanced"));
                var accCount = 0;
                foreach (var acchash in accHashes)
                {
                    var score = scores.FirstOrDefault(x => x.Leaderboard.SongHash.ToLower() == acchash.Key);
                    var totalPlays = score.Leaderboard.Plays;
                    var playerRank = score.Score.Rank;
                    var points = 100 - ((int)playerRank * 100 / ((int)totalPlays));
                    var pointloss = (double)player.Rank / 2000;
                    var newpoints = (double)points - pointloss * (double)points;
                    points = (int)newpoints;
                    accCount++;
                    accuracy += points;
                }
                if (accCount > 0) accuracy = accuracy / accCount;

                var staminaHashes = hashTags.Where(x => x.Value.ToLower().Contains("fitness") || x.Value.ToLower().Contains("dance"));
                var staminaCount = 0;
                foreach (var staminahash in staminaHashes)
                {
                    var score = scores.FirstOrDefault(x => x.Leaderboard.SongHash.ToLower() == staminahash.Key);
                    var totalPlays = score.Leaderboard.Plays;
                    var playerRank = score.Score.Rank;
                    var points = 100 - ((int)playerRank * 100 / ((int)totalPlays));
                    var pointloss = (double)player.Rank / 2000;
                    var newpoints = (double)points - pointloss * (double)points;
                    points = (int)newpoints;
                    staminaCount++;
                    stamina += points;
                }
                if (staminaCount > 0) stamina = stamina / staminaCount;

                var techHashes = hashTags.Where(x => x.Value.ToLower().Contains("tech"));
                var techCount = 0;
                foreach (var techhash in techHashes)
                {
                    var score = scores.FirstOrDefault(x => x.Leaderboard.SongHash.ToLower() == techhash.Key);
                    var totalPlays = score.Leaderboard.Plays;
                    var playerRank = score.Score.Rank;
                    var points = 100 - ((int)playerRank * 100 / ((int)totalPlays));
                    var pointloss = (double)player.Rank / 2000;
                    var newpoints = (double)points - pointloss * (double)points;
                    points = (int)newpoints;
                    techCount++;
                    tech += points;
                }
                if (techCount > 0) tech = tech / techCount;

                var amountOfStats = 4;
                if (speed == 0) amountOfStats--;
                if (accuracy == 0) amountOfStats--;
                if (tech == 0) amountOfStats--;
                if (stamina == 0) amountOfStats--;
                total = amountOfStats == 0 ? 0 : (speed + accuracy + tech + stamina) / amountOfStats;

                //Create card 
                var cardCreator = new ImageCreator("../../../Resources/img/FIFA_Card_Template2.png");
                cardCreator.AddImageRounded(player.ProfilePicture.ToString(), 0, -0, 735 * 4, 1211, 0.90f, 8);
                cardCreator.AddImageRounded(player.ProfilePicture.ToString(), 278, 250, 400, 400);
                cardCreator.AddImage("../../../Resources/img/FIFA_Card_Template2.png", 0, 0, 735, 1211, isLocalFile: true);

                if (playerFull.Badges != null)
                {
                    var badges = (JArray)playerFull.Badges;
                    if (badges.Count > 0)
                    {
                        var url = badges.Children().Last().Children().Last().Children().First().ToString();
                        cardCreator.AddImage(url, 140, 520, 80, 30);
                    }
                }

                cardCreator.AddTextCenter(total.ToString(), Color.Black, 72, 185 + 5, 175 + 5);
                cardCreator.AddTextCenter(total.ToString(), Color.FromArgb(103, 90, 55), 72, 185, 175);

                cardCreator.AddText(player.Country, Color.FromArgb(103, 90, 55), 48, 130, 285);

                cardCreator.AddImageRounded($"https://flagpedia.net/data/flags/w580/{player.Country.ToLower()}.png", 140, 415, 80, 60);

                cardCreator.AddTextCenter(speed == 0 ? "?" : speed.ToString(), Color.Black, 36, 220 + 5, 840 + 5);
                cardCreator.AddTextCenter(stamina == 0 ? "?" : stamina.ToString(), Color.Black, 36, 220 + 5, 960 + 5);
                cardCreator.AddTextCenter(tech == 0 ? "?" : tech.ToString(), Color.Black, 36, 530 + 5, 840 + 5);
                cardCreator.AddTextCenter(accuracy == 0 ? "?" : accuracy.ToString(), Color.Black, 36, 530 + 5, 960 + 5);

                cardCreator.AddTextCenter(speed == 0 ? "?" : speed.ToString(), Color.FromArgb(103, 90, 55), 36, 220, 840);
                cardCreator.AddTextCenter(stamina == 0 ? "?" : stamina.ToString(), Color.FromArgb(103, 90, 55), 36, 220, 960);
                cardCreator.AddTextCenter(tech == 0 ? "?" : tech.ToString(), Color.FromArgb(103, 90, 55), 36, 530, 840);
                cardCreator.AddTextCenter(accuracy == 0 ? "?" : accuracy.ToString(), Color.FromArgb(103, 90, 55), 36, 530, 960);


                cardCreator.AddTextCenter(player.Name, Color.Black, player.Name.Count() > 12 ? 32 : 58, 380 + 5, player.Name.Count() > 12 ? 670 + 5 : 650 + 5);
                cardCreator.AddTextCenter(player.Name, Color.FromArgb(103, 90, 55), player.Name.Count() > 12 ? 32 : 58, 380, player.Name.Count() > 12 ? 670 : 650);

                cardCreator.AddMask("../../../Resources/img/FIFA_Card_Mask.png", isLocalFile: true);

                if (nr < top10) cardCreator.AddImage("../../../Resources/img/FIFA_Card_effect_10.png", 0, 0, 735, 1211, isLocalFile: true);
                else if (nr < top50) cardCreator.AddImage("../../../Resources/img/FIFA_Card_effect_50.png", 0, 0, 735, 1211, isLocalFile: true);
                else if (nr < top100) cardCreator.AddImage("../../../Resources/img/FIFA_Card_effect_100.png", 0, 0, 735, 1211, isLocalFile: true);

                cardCreator.AddTextCenter("Drawn by: " + command.User.Username, Color.Black, 22, 380 + 5, 1170 + 5);
                var size = cardCreator.AddTextCenter("Drawn by: " + command.User.Username, Color.FromArgb(103, 90, 55), 22, 380, 1170);
                //cardCreator.DrawRectangle(380 / 2 - 10, 1170, Convert.ToInt32(size.Width), Convert.ToInt32(size.Height), Color.FromArgb(80, 123, 90, 55));

                var today = DateTime.Now;
                var creationTime = DateTime.UtcNow;
                await cardCreator.Create($"F:\\BeatSaberTradingCards/BeatSaber_Card-{command.User.Id}-{command.User.Id}-{player.Id}-{total}-{player.Rank}-{nr}-{creationTime.ToShortDateString().Replace("-", "_") + "_" + creationTime.ToShortTimeString().Replace(":", "_")}.png");

                //send image in discord and delete it
                await command.Channel.SendFileAsync($"F:\\BeatSaberTradingCards/BeatSaber_Card-{command.User.Id}-{command.User.Id}-{player.Id}-{total}-{player.Rank}-{nr}-{creationTime.ToShortDateString().Replace("-", "_") + "_" + creationTime.ToShortTimeString().Replace(":", "_")}.png");
                //File.Delete($"../../../Resources/img/FIFA_Card-{player.Id}.png");


                //Updates user collection vote list username                 
                var json = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/CardCollectionVotes.json");
                var collectionVotes = JsonConvert.DeserializeObject<List<CardCollectionVotes>>(json);
                if(collectionVotes != null)
                {
                    if (collectionVotes.FirstOrDefault(x => x.DiscordID == command.User.Id.ToString()) == null)
                    {
                        var voted = new List<string>();
                        voted.Add(command.User.Id.ToString());
                        collectionVotes.Add(new CardCollectionVotes { DiscordID = command.User.Id.ToString(), Username = command.User.Username, AmountOfVotes = 1, DiscordIDsWhoVoted = voted });
                                              
                    }
                    else
                    {
                        var collection = collectionVotes.FirstOrDefault(x => x.DiscordID == command.User.Id.ToString());
                        collection.Username = command.User.Username;
                    }
                    var newJson = JsonConvert.SerializeObject(collectionVotes);
                    File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/CardCollectionVotes.json", newJson);
                }
            }
            catch (Exception ex)
            {
                var ohoh = ex;
                var jsonGlobal = System.IO.File.ReadAllText($"../../../Resources/DrawCardTimeOut.json");
                var playerTimeOuts = JsonConvert.DeserializeObject<List<PLayerTimeOut>>(jsonGlobal);
                var user = playerTimeOuts.First(x => x.DiscordID == command.User.Id.ToString());
                user.PacksLeft += 1;
                user.TimeOutTill = null;
                System.IO.File.WriteAllText($"../../../Resources/DrawCardTimeOut.json", JsonConvert.SerializeObject(playerTimeOuts));
                await command.Channel.SendMessageAsync("An unexpected error occurred so **you got 1 pack back in return**");
            }
        }

        public static bool IsPlayerTimedOut(DateTime timeOfRequest, SocketSlashCommand command)
        {
            var jsonGlobal = System.IO.File.ReadAllText($"../../../Resources/DrawCardTimeOut.json");
            var playerTimeOuts = JsonConvert.DeserializeObject<List<PLayerTimeOut>>(jsonGlobal);

            if (playerTimeOuts == null)
            {
                playerTimeOuts = new List<PLayerTimeOut>();
                var player = new PLayerTimeOut() { DiscordID = command.User.Id.ToString(), PacksLeft = 3, TimeOutTill = null };
                player.PacksLeft--;
                playerTimeOuts.Add(player);
                command.Channel.SendMessageAsync($"Opening a pack... Remaining packs: {player.PacksLeft}");
                System.IO.File.WriteAllText($"../../../Resources/DrawCardTimeOut.json", JsonConvert.SerializeObject(playerTimeOuts));
                return false;
            }

            if (playerTimeOuts.FirstOrDefault(x => x.DiscordID == command.User.Id.ToString()) == null)
            {
                var player = new PLayerTimeOut() { DiscordID = command.User.Id.ToString(), PacksLeft = 3, TimeOutTill = null };
                player.PacksLeft--;
                playerTimeOuts.Add(player);
                command.Channel.SendMessageAsync($"Opening a pack... Remaining packs: {player.PacksLeft}");
                System.IO.File.WriteAllText($"../../../Resources/DrawCardTimeOut.json", JsonConvert.SerializeObject(playerTimeOuts));
                return false;
            }

            if (playerTimeOuts.FirstOrDefault(x => x.DiscordID == command.User.Id.ToString()) != null)
            {
                var player = playerTimeOuts.FirstOrDefault(x => x.DiscordID == command.User.Id.ToString());
                if (player.PacksLeft > 1)
                {
                    player.PacksLeft--;
                    command.Channel.SendMessageAsync($"Opening a pack... Remaining packs: {player.PacksLeft}");
                    System.IO.File.WriteAllText($"../../../Resources/DrawCardTimeOut.json", JsonConvert.SerializeObject(playerTimeOuts));
                    return false;
                }
                if (player.PacksLeft == 1)
                {
                    player.PacksLeft--;
                    var timeTillTimeOut = timeOfRequest.AddHours(23);
                    player.TimeOutTill = timeTillTimeOut;
                    command.Channel.SendMessageAsync($"Opening a pack... this is your last pack!");
                    System.IO.File.WriteAllText($"../../../Resources/DrawCardTimeOut.json", JsonConvert.SerializeObject(playerTimeOuts));
                    return false;
                }
                if (player.PacksLeft == 0)
                {
                    if (player.TimeOutTill == null)
                    {
                        player.PacksLeft += 3;
                        command.Channel.SendMessageAsync($"Opening a pack... Remaining packs: {player.PacksLeft}");
                        System.IO.File.WriteAllText($"../../../Resources/DrawCardTimeOut.json", JsonConvert.SerializeObject(playerTimeOuts));
                        return false;
                    }
                    TimeSpan timeToWait = (DateTime)player.TimeOutTill - timeOfRequest;
                    if (timeToWait.TotalSeconds < 0)
                    {
                        player.TimeOutTill = null;
                        player.PacksLeft += 3;
                        command.Channel.SendMessageAsync($"Opening a pack... Remaining packs: {player.PacksLeft}");
                        System.IO.File.WriteAllText($"../../../Resources/DrawCardTimeOut.json", JsonConvert.SerializeObject(playerTimeOuts));
                        return false;
                    }
                    else
                    {
                        command.Channel.SendMessageAsync($"No remaining packs. You will get new packs in {timeToWait.Hours} hours, {timeToWait.Minutes} minutes and {timeToWait.Seconds} seconds.");
                        return true;
                    }
                }
            }
            return true;
        }

        public class PLayerTimeOut
        {
            public string DiscordID { get; set; }
            public DateTime? TimeOutTill { get; set; }
            public int PacksLeft { get; set; }
        }

        public async void ShowInventory(SocketSlashCommand command)
        {

            string[] files = Directory.GetFiles(@$"F:\\BeatSaberTradingCards");
            var cards = new List<Card>();
            foreach (var file in files)
            {
                var parameters = file.Split("-");
                try
                {
                    var card = new Card()
                    {
                        Name = file.Replace("F:\\", "").Replace("\\", "/"),
                        DiscordID = parameters[1],
                        OwnerDiscordID = parameters[2],
                        ScoresaberID = parameters[3],
                        Score = Convert.ToInt32(parameters[4]),
                        rank = Convert.ToInt32(parameters[5]),
                        luckNumber = Convert.ToInt32(parameters[6].Replace(".png", ""))
                    };
                    cards.Add(card);
                }
                catch (Exception ex)
                {

                }
            }

            if (cards.Where(x => x.DiscordID == command.User.Id.ToString()).Count() > 0)
            {
                var json = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/CardCollectionVotes.json");
                var collectionVotes = JsonConvert.DeserializeObject<List<CardCollectionVotes>>(json);

                var hasVotes = false;
                if (collectionVotes != null) hasVotes = collectionVotes.Where(x => x.DiscordID == command.User.Id.ToString()).Count() > 0;

                var embed = EmbedBuilderExtension.NullEmbed($"{command.User.Username}'s Card Inventory",
                    $"Amount of cards: **{cards.Where(x => x.DiscordID == command.User.Id.ToString()).Count()}**\n" +
                    $"Amount of top 10 cards: **{cards.Where(x => x.DiscordID == command.User.Id.ToString() && x.rank <= 10).Count()}**\n" +
                    $"Amount of top 50 cards: **{cards.Where(x => x.DiscordID == command.User.Id.ToString() && x.rank <= 50).Count()}**\n" +
                    $"Amount of top 100 cards: **{cards.Where(x => x.DiscordID == command.User.Id.ToString() && x.rank <= 100).Count()}**\n" +
                    $"Best Luck Number: **{cards.OrderBy(x => x.luckNumber).Where(x => x.DiscordID == command.User.Id.ToString()).First().luckNumber}** (*Lower is better)\n" +
                    $"Highest Score Card: **{cards.OrderByDescending(x => x.Score).Where(x => x.DiscordID == command.User.Id.ToString()).First().Score}**\n " +
                    $"Total Collection value: **{cards.Where(x => x.DiscordID == command.User.Id.ToString()).Sum(x => (1000 - x.rank) * x.Score)} points**\n" +
                    $"Best Rank Card: **{cards.OrderBy(x => x.rank).Where(x => x.DiscordID == command.User.Id.ToString()).First().rank}**\n" +
                    $"Amount of collection votes: **{(hasVotes ? collectionVotes.FirstOrDefault(x => x.DiscordID == command.User.Id.ToString()).AmountOfVotes : 0)}**" +
                    $"\n\n\n" +
                    $"-Best card-");
                embed.Url = $"http://beatsaberbot.com/BeatSaberCards?rankingPageSelectRanking=OwnedDiscordID&searchinput={command.User.Id}#inventory";
                embed.ImageUrl = "http://beatsaberbot.com" + cards.Where(x => x.OwnerDiscordID == command.User.Id.ToString()).OrderByDescending(x => (1000 - x.rank) / 2 * x.Score).First().Name;
                embed.ThumbnailUrl = command.User.GetAvatarUrl();

                var componentBuilder = new ComponentBuilder();
                componentBuilder.WithButton("Vote 👍", customId: "CardCollectionVoteButton", style: ButtonStyle.Secondary);

                _discord.ButtonExecuted += VoteButtonExecute;

                var message = await command.Channel.SendMessageAsync("", false, embed.Build(), components: componentBuilder.Build());
                dmMessageID = message.Id;
            }
            else
            {
                await command.Channel.SendMessageAsync("You don't have any cards yet");
            }
        }

        private async Task VoteButtonExecute(SocketMessageComponent arg)
        {
            if (arg.Message.Id == dmMessageID)
            {
                if (arg.Data.CustomId == "CardCollectionVoteButton")
                {
                    var json = System.IO.File.ReadAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/CardCollectionVotes.json");
                    var collectionVotes = JsonConvert.DeserializeObject<List<CardCollectionVotes>>(json);

                    if (collectionVotes == null)
                    {
                        var cv = new List<CardCollectionVotes>();
                        var voted = new List<string>();
                        voted.Add(arg.User.Id.ToString());
                        cv.Add(new CardCollectionVotes() { DiscordID = arg.Message.Embeds.First().Url.Split("=").Last(), Username = _discord.GetUser(Convert.ToUInt64(arg.Message.Embeds.First().Url.Split("=").Last())).Username, AmountOfVotes = 1, DiscordIDsWhoVoted = voted });
                        collectionVotes = cv;
                    }
                    else
                    {
                        //Check if collection id is in the list already
                        if (collectionVotes.FirstOrDefault(x => x.DiscordID == arg.Message.Embeds.First().Url.Split("=").Last()) == null)
                        {
                            var voted = new List<string>();
                            voted.Add(arg.User.Id.ToString());
                            collectionVotes.Add(new CardCollectionVotes { DiscordID = arg.Message.Embeds.First().Url.Split("=").Last(), Username = _discord.GetUser(Convert.ToUInt64(arg.Message.Embeds.First().Url.Split("=").Last())).Username, AmountOfVotes = 1, DiscordIDsWhoVoted = voted });
                        }
                        else // user is in the list
                        {
                            var collection = collectionVotes.FirstOrDefault(x => x.DiscordID == arg.Message.Embeds.First().Url.Split("=").Last());
                            if (!collection.DiscordIDsWhoVoted.Contains(arg.User.Id.ToString()))
                            {
                                collection.AmountOfVotes += 1;
                                collection.DiscordIDsWhoVoted.Add(arg.User.Id.ToString());
                            }
                            collection.Username = _discord.GetUser(Convert.ToUInt64(arg.Message.Embeds.First().Url.Split("=").Last())).Username;
                        }
                    }

                    var newJson = JsonConvert.SerializeObject(collectionVotes);
                    File.WriteAllText(GlobalConfiguration.WebsiteRoot + "DataCollection/CardCollectionVotes.json", newJson);
                }
            }
            return;
        }

        public class CardCollectionVotes
        {
            public string DiscordID { get; set; }
            public string Username { get; set; }
            public int AmountOfVotes { get; set; }
            public List<string> DiscordIDsWhoVoted { get; set; }
        }

        public async Task StartTradeProcess(SocketSlashCommand command, SocketUser userToTradeWith)
        {
            try
            {
                this.userToTradeWith = userToTradeWith;
                var dmChannel = await command.User.CreateDMChannelAsync();
                var embed = EmbedBuilderExtension.NullEmbed($"Trading offer towards {userToTradeWith.Username} ({userToTradeWith.Id})", "You are about to create a trading offer. Do you want to proceed?");
                var componentBuilder = new ComponentBuilder();
                componentBuilder.WithButton("YES", customId: "tradingYesButton", style: ButtonStyle.Success);
                componentBuilder.WithButton("NO", customId: "tradingNoButton", style: ButtonStyle.Danger);

                var message = await dmChannel.SendMessageAsync("", false, embed.Build(), components: componentBuilder.Build());
                dmMessageID = message.Id;
                _discord.ButtonExecuted += Discord_ButtonExecuted;
            }
            catch (Exception ex)
            {

            }
        }

        private async System.Threading.Tasks.Task Discord_ButtonExecuted(SocketMessageComponent arg)
        {
            if (arg.Message.Id == dmMessageID)
            {
                if (arg.Data.CustomId == "tradingYesButton")
                {
                    StartTradeOfferProcess(arg);
                }
                if (arg.Data.CustomId == "tradingNoButton" || arg.Data.CustomId == "tradingAbortButton")
                {
                    arg.Message.DeleteAsync();
                }
                if (arg.Data.CustomId == "tradingNextStepOneButton" || arg.Data.CustomId == "tradingSendOfferButton")
                {
                    hasNextBeenPressed = true;
                }
                if (arg.Data.CustomId == "tradingAcceptOfferButton")
                {
                    tradeHasBeenAccepted = true;
                }
            }
            return;
        }

        public async Task StartTradeOfferProcess(SocketMessageComponent arguments)
        {
            try
            {
                var embed = EmbedBuilderExtension.NullEmbed(arguments.Message.Embeds.First().Title, $"Please insert the url / url's of **the cards you want to give** \n\n you can find them here [{arguments.User.Username} Inventory](http://beatsaberbot.com/BeatSaberCards?rankingPageSelectRanking=OwnedDiscordID&searchinput={arguments.User.Id}#inventory)\n\nYou can click on a card and the url will be copied on your clipboard. \nPlease insert the url's one by one.\n\nYou have 10 minutes. Please press Next when finished.\n\n ----------CARDS----------");
                embed.WithImageUrl("http://beatsaberbot.com/img/howtotrade.gif");
                var componentBuilder = new ComponentBuilder();
                componentBuilder.WithButton("NEXT", customId: "tradingNextStepOneButton", style: ButtonStyle.Success, disabled: true);
                componentBuilder.WithButton("ABORT", customId: "tradingAbortButton", style: ButtonStyle.Danger, disabled: false);




                await arguments.Message.ModifyAsync(msg => msg.Embed = embed.Build());
                await arguments.Message.ModifyAsync(msg => msg.Components = componentBuilder.Build());


                var cardListUserOne = new List<string>();
                var cardListUserTwo = new List<string>();

                string[] files = Directory.GetFiles(@$"F:\\BeatSaberTradingCards");
                var cards = new List<Card>();
                foreach (var file in files)
                {
                    var parameters = file.Split("-");
                    try
                    {
                        var card = new Card()
                        {
                            Name = file.Replace("F:\\", "").Replace("\\", "/"),
                            DiscordID = parameters[1],
                            OwnerDiscordID = parameters[2],
                            ScoresaberID = parameters[3],
                            Score = Convert.ToInt32(parameters[4]),
                            rank = Convert.ToInt32(parameters[5]),
                            luckNumber = Convert.ToInt32(parameters[6].Replace(".png", ""))
                        };
                        cards.Add(card);
                    }
                    catch (Exception ex)
                    {

                    }
                }

                var startTime = DateTime.Now;
                var endTime = startTime.AddMinutes(10);
                var content = "";
                do
                {
                    await Task.Delay(1000);
                    var message = await arguments.Channel.GetMessagesAsync(1).FlattenAsync();
                    if (message.First().CreatedAt > startTime && message.First().Author.IsBot == false)
                    {
                        content = message.First().Content;
                        content = content.Replace("http://beatsaberbot.com/BeatSaberTradingCards/", "/BeatSaberTradingCards/");
                        if (cards.Where(x => x.OwnerDiscordID == arguments.User.Id.ToString()).FirstOrDefault(x => x.Name == content) != null && !cardListUserOne.Contains(content))
                        {
                            embed.Description = embed.Description + $"\n\n [Card](http://beatsaberbot.com{content})";
                            var newEmbed = EmbedBuilderExtension.NullEmbed(embed.Title, embed.Description);

                            var componentBuilderAfterCard = new ComponentBuilder();
                            componentBuilderAfterCard.WithButton("NEXT", customId: "tradingNextStepOneButton", style: ButtonStyle.Success, disabled: false);
                            componentBuilderAfterCard.WithButton("ABORT", customId: "tradingAbortButton", style: ButtonStyle.Danger, disabled: false);

                            await arguments.Message.ModifyAsync(msg => msg.Embed = newEmbed.Build());
                            await arguments.Message.ModifyAsync(msg => msg.Components = componentBuilderAfterCard.Build());
                            cardListUserOne.Add(content);
                        }
                    }

                } while (endTime > startTime && hasNextBeenPressed == false);

                hasNextBeenPressed = false;

                if (content == "")
                {
                    await arguments.Message.DeleteAsync();
                    return;
                }

                var embedTwo = EmbedBuilderExtension.NullEmbed(arguments.Message.Embeds.First().Title, $"Please insert the url / url's of **the card you want to have** \n\n you can find them here [{userToTradeWith.Username} Inventory](http://beatsaberbot.com/BeatSaberCards?rankingPageSelectRanking=OwnedDiscordID&searchinput={userToTradeWith.Id}#inventory)\n\nYou can click on a card and the url will be copied on your clipboard. \nPlease insert the url's one by one.\n\nYou have 10 minutes. Please press Next when finished.\n\n ----------CARDS----------");
                embedTwo.WithImageUrl("http://beatsaberbot.com/img/howtotrade.gif");
                await arguments.Message.ModifyAsync(x => x.Embed = embedTwo.Build());

                startTime = DateTime.Now;
                endTime = startTime.AddMinutes(10);
                content = "";
                do
                {
                    await Task.Delay(1000);
                    var message = await arguments.Channel.GetMessagesAsync(1).FlattenAsync();
                    if (message.First().CreatedAt > startTime && message.First().Author.IsBot == false)
                    {
                        content = message.First().Content;
                        content = content.Replace("http://beatsaberbot.com/BeatSaberTradingCards/", "/BeatSaberTradingCards/");
                        if (cards.Where(x => x.OwnerDiscordID == userToTradeWith.Id.ToString()).FirstOrDefault(x => x.Name == content) != null && !cardListUserTwo.Contains(content))
                        {
                            embedTwo.Description = embedTwo.Description + $"\n\n [Card](http://beatsaberbot.com{content})";
                            var newEmbed = EmbedBuilderExtension.NullEmbed(embedTwo.Title, embedTwo.Description);

                            var componentBuilderAfterCard = new ComponentBuilder();
                            componentBuilderAfterCard.WithButton("NEXT", customId: "tradingNextStepOneButton", style: ButtonStyle.Success, disabled: false);
                            componentBuilderAfterCard.WithButton("ABORT", customId: "tradingAbortButton", style: ButtonStyle.Danger, disabled: false);

                            await arguments.Message.ModifyAsync(msg => msg.Embed = newEmbed.Build());
                            await arguments.Message.ModifyAsync(msg => msg.Components = componentBuilderAfterCard.Build());
                            cardListUserTwo.Add(content);
                        }
                    }

                } while (endTime > startTime && hasNextBeenPressed == false);

                hasNextBeenPressed = false;

                if (content == "")
                {
                    await arguments.Message.DeleteAsync();
                    return;
                }

                if (cardListUserOne.Count == 0 || cardListUserTwo.Count == 0)
                {
                    await arguments.Channel.SendMessageAsync("One of the sides didn't have any cards, trade offer has been aborted");
                    await arguments.Message.DeleteAsync();
                    return;
                }


                await arguments.Message.DeleteAsync();

                var cardListOneString = "";
                cardListUserOne.ForEach(card => cardListOneString += $"[Card](http://beatsaberbot.com{card}) \n");

                var cardListTwoString = "";
                cardListUserTwo.ForEach(card => cardListTwoString += $"[Card](http://beatsaberbot.com{card}) \n");

                var finalEmbed = EmbedBuilderExtension.NullEmbed($"Do you want to sent the following trade offer towards {userToTradeWith.Username}?", $"**You will give:**\n{cardListOneString} \n\n**You will get:**\n{cardListTwoString}");
                var componentBuilderSendOffer = new ComponentBuilder();
                componentBuilderSendOffer.WithButton("SEND OFFER", customId: "tradingSendOfferButton", style: ButtonStyle.Success);
                componentBuilderSendOffer.WithButton("ABORT OFFER", customId: "tradingAbortButton", style: ButtonStyle.Danger);
                var finalMessage = await arguments.Channel.SendMessageAsync("", false, finalEmbed.Build(), components: componentBuilderSendOffer.Build());
                dmMessageID = finalMessage.Id;

                do
                {
                    await Task.Delay(2000);

                } while (hasNextBeenPressed == false);
                await finalMessage.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Offer has been sent!", "You will get reaction here soon!").Build());
                await finalMessage.ModifyAsync(x => x.Components = new ComponentBuilder().Build());

                //Send message to the user to trade with and ask if the offer will ge accepted or denied
                var userToTradeWithDM = await userToTradeWith.CreateDMChannelAsync();
                var offerEmbed = EmbedBuilderExtension.NullEmbed($"You got a trade offer from {arguments.User.Username}!", $"**You will get:**\n{cardListOneString} \n\n**You will give away:**\n{cardListTwoString} \n\n***Make sure the trade is as expected!\nYou have 20 minutes to decide if you want to accept or deny the trade.***");
                var componentBuilderAcceptOffer = new ComponentBuilder();
                componentBuilderAcceptOffer.WithButton("ACCEPT OFFER", customId: "tradingAcceptOfferButton", style: ButtonStyle.Success);
                componentBuilderAcceptOffer.WithButton("DENY OFFER", customId: "tradingAbortButton", style: ButtonStyle.Danger);

                var toTradeWithMsg = await userToTradeWithDM.SendMessageAsync("", false, offerEmbed.Build(), components: componentBuilderAcceptOffer.Build());
                dmMessageID = toTradeWithMsg.Id;

                startTime = DateTime.Now;
                endTime = startTime.AddMinutes(20);
                do
                {
                    await Task.Delay(2000);

                } while (endTime > startTime && tradeHasBeenAccepted == false);

                if (tradeHasBeenAccepted)
                {
                    //Make the trade happen
                    foreach (var tradingCardString in cardListUserOne)
                    {
                        var cardFile = files.FirstOrDefault(x => x == "F:\\" + tradingCardString.Replace("/", "\\"));
                        var ownerDiscordID = cardFile.Split("-")[2];

                        var cardName = cardFile.Split("-");
                        cardName[2] = userToTradeWith.Id.ToString();
                        var newName = "";
                        foreach (var part in cardName) newName += part + "-";
                        newName = newName.Remove(newName.Length - 1);

                        File.Move(cardFile, newName);
                    }

                    foreach (var tradingCardString in cardListUserTwo)
                    {
                        var cardFile = files.FirstOrDefault(x => x == "F:\\" + tradingCardString.Replace("/", "\\"));
                        var ownerDiscordID = cardFile.Split("-")[2];

                        var cardName = cardFile.Split("-");
                        cardName[2] = arguments.User.Id.ToString();
                        var newName = "";
                        foreach (var part in cardName) newName += part + "-";
                        newName = newName.Remove(newName.Length - 1);

                        File.Move(cardFile, newName);
                    }

                    await userToTradeWithDM.SendMessageAsync("You have accepted the trade offer and the trade has been completed!");
                    await arguments.Channel.SendMessageAsync($"Your trade offer for {userToTradeWith.Username} has been accepted! Your cards have been traded!");
                }
                else
                {
                    await userToTradeWithDM.SendMessageAsync("You have denied the trade offer and the trade has been aborted!");
                    await arguments.Channel.SendMessageAsync($"Your trade offer for {userToTradeWith.Username} has been denied :c Your cards have not been traded.");
                }

                await toTradeWithMsg.DeleteAsync();
            }
            catch (Exception ex)
            {

            }
        }

        public static void ToggleCardDrawDMNotification(SocketSlashCommand command)
        {
            var jsonGlobal = System.IO.File.ReadAllText($"../../../Resources/NewCardPacksNotification.json");
            var CardPackNotificationList = JsonConvert.DeserializeObject<List<string>>(jsonGlobal);

            if (CardPackNotificationList == null)
            {
                var list = new List<string>();
                list.Add(command.User.Id.ToString());
                CardPackNotificationList = list;
                command.Channel.SendMessageAsync("You will now receive DM notfications when new packs are available");
            }
            else
            {

                if (CardPackNotificationList.Contains(command.User.Id.ToString()))
                {
                    CardPackNotificationList.Remove(command.User.Id.ToString());
                    command.Channel.SendMessageAsync("You will now stop receiving DM notfications when new packs are available");
                }
                else
                {
                    CardPackNotificationList.Add(command.User.Id.ToString());
                    command.Channel.SendMessageAsync("You will now receive DM notfications when new packs are available");
                }
            }

            System.IO.File.WriteAllText($"../../../Resources/NewCardPacksNotification.json", JsonConvert.SerializeObject(CardPackNotificationList));
        }

        public async Task NotifyUsersOnNewCardPacks()
        {
            do
            {
                var json = System.IO.File.ReadAllText($"../../../Resources/NewCardPacksNotification.json");
                var CardPackNotificationList = JsonConvert.DeserializeObject<List<string>>(json);

                var json2 = System.IO.File.ReadAllText($"../../../Resources/DrawCardTimeOut.json");
                var playerTimeOuts = JsonConvert.DeserializeObject<List<PLayerTimeOut>>(json2);

                var playerTimeOutsWhoWantNotficiations = playerTimeOuts.Where(x => CardPackNotificationList.Contains(x.DiscordID) && x.TimeOutTill != null && x.TimeOutTill > DateTime.Now).OrderBy(x => x.TimeOutTill).ToList();


                var player = playerTimeOutsWhoWantNotficiations.First();
                var timeLeft = (DateTime)player.TimeOutTill - DateTime.Now;
                if (timeLeft.TotalSeconds > 60 * 60)
                {
                    await Task.Delay(1000 * 60 * 60);

                }
                else
                {
                    await Task.Delay(Convert.ToInt32(timeLeft.TotalSeconds) * 1000);
                    try
                    {
                        var dm = await _discord.GetUser(Convert.ToUInt64(player.DiscordID)).CreateDMChannelAsync();
                        await dm.SendMessageAsync("You have received new Beat Saber card packs!");
                    }
                    catch (Exception x)
                    {

                    }
                }
            } while (true);

            return;
        }

        //public static void TempAddHistory()
        //{
        //    string[] files = Directory.GetFiles(@$"F:\\BeatSaberTradingCards");
        //    var cards = new List<Card>();
        //    foreach (var file in files)
        //    {
        //        var creationTime = File.GetCreationTimeUtc(file);
        //        var timeToAdd = "-" + creationTime.ToShortDateString().Replace("-", "_") + "_" + creationTime.ToShortTimeString().Replace(":","_");
        //        var parts = file.Split(".");
        //        var newName = parts[0] + timeToAdd + "." + parts[1];
        //        File.Move(file, newName);
        //    }
        //    var f = 2;

        //}

        public class Card
        {
            public string Name { get; set; }
            public string DiscordID { get; set; }

            public string OwnerDiscordID { get; set; }
            public string ScoresaberID { get; set; }
            public int Score { get; set; }
            public int rank { get; set; }
            public int luckNumber { get; set; }

            public DateTime CreationDate { get; set; }
        }
    }
}
