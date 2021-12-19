using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Commands.Functions
{
    public class ToolsList
    {
        private List<ToolModel> _tools = new List<ToolModel>();
        private DiscordSocketClient _discord;
        private RestUserMessage _msg;
        private int _shownCategoryInt = 0;
        private string _starRatingPath = "../../../Resources/Tools/StarRating.json";
        public ToolsList(DiscordSocketClient discord)
        {
            _discord = discord;

            _tools.Add(new ToolModel()
            {
                Name = "BeatSaver",
                Description = "A website/host with the purpose to store all Beat Saber maps that are created. If you want to upload your map, this is the place to be.",
                Link = "https://beatsaver.com/",
                category = ToolCategory.Filters,
                logoUrl = "https://i.imgur.com/JMZT8fS.png",
                extraImageUrl = "https://i.imgur.com/2pif0YT.png"
            });
            _tools.Add(new ToolModel()
            {
                Name = "ModAssistant",
                Description = "A PC mod installer for Beat Saber. A secure place to install mods. ",
                Link = "https://github.com/Assistant/ModAssistant",
                DownloadLink = "https://github.com/Assistant/ModAssistant/releases",
                logoUrl = "https://i.imgur.com/TCsdaZW.jpg",
                extraImageUrl = "https://i.imgur.com/flAgoPP.png",
                category = ToolCategory.Modding
            });
            _tools.Add(new ToolModel()
            {
                Name = "BeatSaberMapFilter",
                Description = "A PC tool that can filter maps by lots of detailed values",
                Link = "https://github.com/Daanniello/Beat-Saber-Map-Filter",
                DownloadLink = "https://github.com/Daanniello/Beat-Saber-Map-Filter/releases",
                logoUrl = "https://i.imgur.com/3KnNDyo.png",
                ExtraDescription = "A tool to filter every possible element in a beat saber map. Instantly create playlists with these filters and displays helpfull charts.",
                extraImageUrl = "https://i.imgur.com/X7IlYRu.png",
                category = ToolCategory.Filters
            });
            _tools.Add(new ToolModel()
            {
                Name = "Yeehaw",
                Description = "A tool that shows how much PP you could get for certain scores.",
                Link = "https://scoresaber.balibalo.xyz/peepee",
                logoUrl = "https://scoresaber.balibalo.xyz/client/icon.png",
                ExtraDescription = "With this browser tool, you can gain info about how much PP you could get for getting a certain acc percentage on ranked maps. It also shows you as first what maps you could improve on to get the most pp. Its a really helpfull tool to see what you have to do to rank up fast.",
                extraImageUrl = "https://i.imgur.com/QTgohKF.png",
                category = ToolCategory.Competative
            });
            _tools.Add(new ToolModel()
            {
                Name = "ScoreSaber",
                Description = "ScoreSaber is Beat Saber's largest leaderboard system for custom songs.",
                Link = "https://scoresaber.com/",
                logoUrl = "https://i.imgur.com/OHF9PtC.png",
                ExtraDescription = "Scoresaber is the biggest global leaderboard with a big team behind it and a huge community. If you want to compare your own skills with others, then this is the best place to be. ",
                extraImageUrl = "https://i.imgur.com/rtqmzTP.png",
                category = ToolCategory.Competative
            });
            _tools.Add(new ToolModel()
            {
                Name = "BeatSaberMapCheck",
                Description = "A tool that automatically reviews your fresh made map for certain mapping errors.",
                Link = "https://kivalevan.me/BeatSaber-MapCheck/",
                logoUrl = "https://raw.githubusercontent.com/KivalEvan/BeatSaber-MapCheck/main/public/img/icon-large.png",
                ExtraDescription = "This tool looks into your map and calculates if there are any mapping errors like handclaps and way more.",
                extraImageUrl = "https://i.imgur.com/hf9BDya.png",
                category = ToolCategory.Mapping
            });
            _tools.Add(new ToolModel()
            {
                Name = "BSMG Wiki",
                Description = "A information page to start learning mapping.",
                Link = "https://bsmg.wiki/mapping/#mapping-quick-start",
                logoUrl = "https://i.imgur.com/4fn8X2i.png",
                ExtraDescription = "BSMG Wiki is the place to learn all about how to map beat saber maps. They provide a usefull structure for you to learn to know everything about how to quickly make an awesome map.",
                extraImageUrl = "https://i.imgur.com/i7Tq6ST.png",
                category = ToolCategory.Mapping
            });
            _tools.Add(new ToolModel()
            {
                Name = "PlaylisthubRanked",
                Description = "Playlisthub Ranked playlist is a tool that provides you with a playlist based on filters on ranked maps.",
                Link = "https://giannikoch.com/playlisthub/",
                logoUrl = "https://i.imgur.com/ycrJCjG.gif",
                ExtraDescription = "If you want to get a playlist with all maps above 10 stars, then this is a easy to use browser tool for you.",
                extraImageUrl = "https://i.imgur.com/pXbSnH5.png",
                category = ToolCategory.Competative
            });
            _tools.Add(new ToolModel()
            {
                Name = "bsviewer",
                Description = "A browser tool that fully previews a playthrough on a map.",
                Link = "https://skystudioapps.com/bs-viewer/",
                logoUrl = "https://skystudioapps.com/87d20fd14d909916f5c2da0d27fcdcbd.png",
                ExtraDescription = "A creat tool to get to know a map without playing it yourself. This tool shows you a beat saber environment and play the map.",
                extraImageUrl = "https://i.imgur.com/eVdNELG.png",
                category = ToolCategory.General
            });
            _tools.Add(new ToolModel()
            {
                Name = "BeatSavior",
                Description = "A browser tool that comes with a mod that tracks extra Beat Saber stats while playing.",
                Link = "https://beat-savior.herokuapp.com/#/",
                DownloadLink = "https://github.com/Mystogan98/BeatSaviorData/releases",
                logoUrl = "https://i.imgur.com/A2casIv.png",
                ExtraDescription = "This tool comes with a mod that you have to install. This mod trackers way more details when you are playing. Like ",
                extraImageUrl = "https://i.imgur.com/eSxbAVE.png",
                category = ToolCategory.Competative
            });
            _tools.Add(new ToolModel()
            {
                Name = "HitBloq",
                Description = "Hitbloq is a competitive Beat Saber service with endless map pools where anything is rankable.",
                Link = "https://hitbloq.com/",
                DownloadLink = "https://github.com/PauseChampions/Hitbloq/releases",
                logoUrl = "https://hitbloq.com/static/hitbloq.png",
                ExtraDescription = "This leaderboard is like Scoresaber but less strict in what maps are ranked, This is a great source for lots and really good playlists.",
                extraImageUrl = "https://i.imgur.com/7rooDyt.png",
                category = ToolCategory.Competative
            });
            _tools.Add(new ToolModel()
            {
                Name = "AccSaber",
                Description = "A competative leaderboard pure for acc players.",
                Link = "https://accsaber.com/leaderboard",
                logoUrl = "https://i.imgur.com/s4n7Mpt.png",
                ExtraDescription = "A leaderboard the focuses on acc. if you love to play acc then this is a must have. It shows 3 different types of acc. True acc, Standard acc and Tech acc.",
                extraImageUrl = "https://i.imgur.com/yYNUOPu.png",
                category = ToolCategory.Competative
            });
            //_tools.Add(new ToolModel()
            //{
            //    Name = "",
            //    Description = "",
            //    Link = "",
            //    DownloadLink = "",
            //    logoUrl = "",
            //    ExtraDescription = "",
            //    extraImageUrl = "",
            //    category = ToolCategory.Filters
            //});

            Task.Run(async () => {
                var ratings = await OpenStarRatingList();
                if (ratings == null) return;
                foreach (var tool in _tools)
                {
                    if (ratings.FirstOrDefault(x => x.toolName.ToLower() == tool.Name.ToLower()) == null)
                    {
                        tool.rate = 0;
                        continue;
                    }
                    tool.Rating = ratings.FirstOrDefault(x => x.toolName.ToLower() == tool.Name.ToLower());
                    tool.rate = ratings.FirstOrDefault(x => x.toolName.ToLower() == tool.Name.ToLower()).StarRatings.Average(x => x.Value);
                }

                var orderedListCopy = new List<ToolModel>();
                _tools.ForEach((item) => { orderedListCopy.Add(item.Clone()); });
                orderedListCopy = orderedListCopy.OrderByDescending(x => x.rate).ToList();
                var count = 0;
                foreach (var mustHaveTool in orderedListCopy)
                {
                    if (count >= 6) break;
                    mustHaveTool.category = ToolCategory.MustHaves;
                    _tools.Add(mustHaveTool);
                    count++;
                }
            }).Wait();
        }

        public async Task SendMessage(SocketMessage message)
        {
            _discord.ReactionAdded += _discord_ReactionAdded;

            var embed = await CreateToolsEmbed(ToolCategory.MustHaves);
            _msg = await message.Channel.SendMessageAsync("", false, embed.Build());
            _msg.AddReactionAsync(Emote.Parse("<:left:681842980134584355>"));
            _msg.AddReactionAsync(Emote.Parse("<:right:681843066104971287>"));
        }

        public async Task SendMessageWithToolName(SocketMessage message, string toolName)
        {
            toolName = toolName.Trim();
            var tool = _tools.FirstOrDefault(x => x.Name.ToLower() == toolName.ToLower());
            if (tool == null)
            {
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("No results", "There is no tool with that name").Build());
                return;
            }

            var ratingList = await OpenStarRatingList();
            bool hasStars = false;
            if (ratingList == null) hasStars = false;
            if (ratingList.FirstOrDefault(x => x.toolName.ToLower() == tool.Name.ToLower()) == null) hasStars = false;
            else hasStars = true;

            var stars = "";
            double avgStar = 0;
            var voteCount = 0;
            if (hasStars)
            {
                avgStar = ratingList.FirstOrDefault(x => x.toolName.ToLower() == tool.Name.ToLower()).StarRatings.Average(x => x.Value);
                voteCount = ratingList.FirstOrDefault(x => x.toolName.ToLower() == tool.Name.ToLower()).StarRatings.Count;
                for (var x = 0; x < 5; x++)
                {
                    if (x < Math.Round(avgStar)) stars += ":star:";
                    else stars += "<:graystar:918974210402025513>";
                }
            }

            var embedBuilder = new EmbedBuilder()
            {
                Title = $"Community Tool: ({toolName})",
                Color = Color.Blue,
                Description = $" {(avgStar != 0 ? $"{stars} {Math.Round(avgStar, 1)}" : "")}\n{voteCount} votes\n\n{tool.Description}\n\n{tool.ExtraDescription}\n\n {(tool.DownloadLink != null ? $"[Download Link]({tool.DownloadLink})" : "")}",
                ImageUrl = tool.extraImageUrl,
                ThumbnailUrl = tool.logoUrl,
                Url = tool.Link
            };
            embedBuilder.WithFooter(new EmbedFooterBuilder() { Text = $"Do you want to share how you feel about this tool? Give it a star with '!bs tools {toolName} vote 1-5'" });

            await message.Channel.SendMessageAsync("", false, embedBuilder.Build());
        }

        private async Task<EmbedBuilder> CreateToolsEmbed(ToolCategory category)
        {
            var embedBuilder = new EmbedBuilder()
            {
                Title = $"Community Tools (Category: {category.ToString()})",
                Color = Color.Blue,
                Description = $"*Type `!bs tools [toolName]` to get more information and a preview*\n\n {_tools.Where(x => x.category == category).Count()} tools"
            };

            embedBuilder.WithFooter(new EmbedFooterBuilder() { Text = $"Want to vote on your favorite tool? Type '!bs tools [toolname] vote [1-5]'" });

            var toolsToShow = _tools.Where(x => x.category == category);
            var orderedToolsToShow = toolsToShow.OrderByDescending(x => x.rate).ToList();
            foreach (var tool in orderedToolsToShow)
            {
                var ratingList = await OpenStarRatingList();
                bool hasStars = false;
                if (ratingList == null) hasStars = false;
                if (ratingList.FirstOrDefault(x => x.toolName.ToLower() == tool.Name.ToLower()) == null) hasStars = false;
                else hasStars = true;

                var stars = "";
                double avgStar = 0;
                var voteCount = 0;
                if (hasStars)
                {                    
                    avgStar = ratingList.FirstOrDefault(x => x.toolName.ToLower() == tool.Name.ToLower()).StarRatings.Average(x => x.Value);
                    voteCount = ratingList.FirstOrDefault(x => x.toolName.ToLower() == tool.Name.ToLower()).StarRatings.Count;
                    for (var x = 0; x < 5; x++)
                    {
                        if (x < Math.Round(avgStar)) stars += ":star:";
                        else stars += "<:graystar:918974210402025513>";
                    }
                }
                embedBuilder.AddField(tool.Name + $"\n{stars} {(avgStar != 0 ? $"{Math.Round(avgStar, 1)}" : "")}\n{voteCount} votes", $"{tool.Description}\n- [Link]({tool.Link}){(tool.DownloadLink != null ? $" - [DownloadLink]({tool.DownloadLink})" : "")}", true);
            }

            return embedBuilder;
        }

        public async Task VoteOnTool(string toolName, int stars, SocketMessage message)
        {
            var tool = _tools.FirstOrDefault(x => x.Name.ToLower() == toolName.ToLower());
            if (tool == null)
            {
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("No results", "There is no tool with that name").Build());
                return;
            }
            if(stars < 1 || stars > 5)
            {
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Wrong input", "You are only allowed to give a range between 1 and 5 stars").Build());
                return;
            }

            var starRatingList = await OpenStarRatingList();
            if(starRatingList == null)
            {
                var tempList = new List<ToolStarRatingModel>();
                var rating = new Dictionary<string, int>();
                rating.Add(message.Author.Id.ToString(), stars);
                tempList.Add(new ToolStarRatingModel() { toolName = toolName.ToLower(), StarRatings = rating });
                SaveStarRatingList(tempList);
                return;
            }
            var toolRatingList = starRatingList.FirstOrDefault(x => x.toolName.ToLower() == toolName.ToLower());     
            if(toolRatingList == null)
            {
                var rating = new Dictionary<string, int>();
                rating.Add(message.Author.Id.ToString(), stars);
                starRatingList.Add(new ToolStarRatingModel() { toolName = toolName.ToLower(), StarRatings = rating});
                SaveStarRatingList(starRatingList);
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Added your rating!", "Wow, you are the first one to give this tool a rating! now you are officially cool").Build());
                return;
            }
            bool hasAlreadyRated = false;
            if (toolRatingList.StarRatings.FirstOrDefault(x => x.Key == message.Author.Id.ToString()).Key != null) hasAlreadyRated = true;
            if (hasAlreadyRated)
            {
                toolRatingList.StarRatings.Remove(message.Author.Id.ToString());
                toolRatingList.StarRatings.Add(message.Author.Id.ToString(), stars);
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Added your rating!", "You already voted on this before, but its fine! I have edited your vote!").Build());
                await SaveStarRatingList(starRatingList);
                return;
            }
            else
            {
                toolRatingList.StarRatings.Add(message.Author.Id.ToString(), stars);
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Added your rating!", "Thanks for rating this tool! I think the creators will appreciate it! maybe").Build());
                await SaveStarRatingList(starRatingList);
                return;
            }            
        }

        private async Task<bool> SaveStarRatingList(List<ToolStarRatingModel> starRatingList)
        {
            try
            {
                var json = JsonConvert.SerializeObject(starRatingList);
                if (File.Exists(_starRatingPath)) File.WriteAllText(_starRatingPath, json);
                else File.AppendAllText(_starRatingPath, json);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<List<ToolStarRatingModel>> OpenStarRatingList()
        {
            try
            {
                var json = File.ReadAllText(_starRatingPath);
                return JsonConvert.DeserializeObject<List<ToolStarRatingModel>>(json);
            }
            catch
            {
                return null;
            }
        }

        private async System.Threading.Tasks.Task _discord_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg3.UserId != 504633036902498314 && arg3.MessageId == _msg.Id)
            {
                if (arg3.Emote.ToString() == "<:left:681842980134584355>")
                {
                    if (_shownCategoryInt > 0)
                    {
                        var embed = await CreateToolsEmbed((ToolCategory)_shownCategoryInt - 1);
                        _msg.ModifyAsync(x => x.Embed = embed.Build());
                        _shownCategoryInt--;
                    }
                    await _msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                }
                if (arg3.Emote.ToString() == "<:right:681843066104971287>")
                {
                    if (_shownCategoryInt + 1 < Enum.GetNames(typeof(ToolCategory)).Length)
                    {
                        var embed = await CreateToolsEmbed((ToolCategory)_shownCategoryInt + 1);
                        _msg.ModifyAsync(x => x.Embed = embed.Build());
                        _shownCategoryInt++;
                    }
                    await _msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                }
            }

            //_discord.ReactionAdded -= _discord_ReactionAdded;
            return;
        }

        public enum ToolCategory
        {
            MustHaves,
            Competative,
            Mapping,
            Modding,
            Filters,
            General

        }
        public class ToolModel
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Link { get; set; }
            public string DownloadLink { get; set; }

            public string ExtraDescription { get; set; }

            public string logoUrl { get; set; }
            public string extraImageUrl { get; set; }
            public ToolCategory category { get; set; }
            public ToolStarRatingModel Rating { get; set; }
            public double rate { get; set; }

            public ToolModel Clone()
            {
                return new ToolModel()
                {
                    Name = Name,
                    Description = Description,
                    Link = Link,
                    DownloadLink = DownloadLink,
                    ExtraDescription = ExtraDescription,
                    logoUrl = logoUrl,
                    extraImageUrl = extraImageUrl,
                    category = category,
                    Rating = Rating,
                    rate = rate
                };
            }
        }

        public class ToolStarRatingModel
        {
            public string toolName { get; set; }
            public Dictionary<string, int> StarRatings { get; set; }
        }
    }
}
