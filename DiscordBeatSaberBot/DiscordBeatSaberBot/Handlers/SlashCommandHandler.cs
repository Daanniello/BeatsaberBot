using Discord;
using Discord.WebSocket;
using DiscordBeatSaberBot.Commands;
using DiscordBeatSaberBot.Commands.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Handlers
{
    public class SlashCommandHandler
    {
        private DiscordSocketClient _discord;

        public int TotalCommandsUsed = 0;

        public SlashCommandHandler(DiscordSocketClient discord)
        {
            _discord = discord;
            _discord.SlashCommandExecuted += _discord_SlashCommandExecuted;
        }

        private async Task _discord_SlashCommandExecuted(SocketSlashCommand command)
        {

            switch (command.CommandName)
            {
                case "randomgif":
                    HandleTaskException(GlobalScoresaberCommands.RandomGif(_discord, command), command);
                    break;
                case "improve":
                    HandleTaskException(GlobalScoresaberCommands.Improve(_discord, command), command);
                    break;
                case "ranktracker":
                    HandleTaskException(GlobalScoresaberCommands.RankTracker(_discord, command), command);
                    break;
                case "profile":
                    HandleTaskException(GlobalScoresaberCommands.Profile(_discord, command), command);
                    break;
                case "link":
                    HandleTaskException(DutchServerCommands.LinkScoresaberWithDiscord(_discord, command), command);
                    break;
                case "unlink":
                    HandleTaskException(DutchServerCommands.UnLinkScoresaberFromDiscord(_discord, command), command);
                    break;
                case "map":
                    HandleTaskException(GlobalScoresaberCommands.Map(_discord, command), command);
                    break;
                case "settings":
                    HandleTaskException(GlobalScoresaberCommands.Settings(_discord, command), command);
                    break;
                case "help":
                    HandleTaskException(GenericCommands.Help(_discord, command), command);
                    break;
                case "eventmanager":
                    HandleTaskException(DutchServerCommands.RandomEvent(_discord, command), command);
                    break;
                case "compare":
                    HandleTaskException(GlobalScoresaberCommands.Compare(_discord, command), command);
                    break;
                case "statistics":
                    HandleTaskException(GenericCommands.Statistics(_discord, command), command);
                    break;
                case "tools":
                    HandleTaskException(GenericCommands.Tools(_discord, command), command);
                    break;
                case "invite":
                    HandleTaskException(GenericCommands.Invite(_discord, command), command);
                    break;
                case "draw":
                    HandleTaskException(GlobalScoresaberCommands.Draw(command), command);
                    break;
                case "removebg":
                    HandleTaskException(GenericCommands.RemoveBG(_discord, command), command);
                    break;
                case "search":
                    HandleTaskException(GlobalScoresaberCommands.SearchUserCommand(_discord, command), command);
                    break;
                case "topsongs":
                    HandleTaskException(GlobalScoresaberCommands.TopSongs(_discord, command), command);
                    break;
                case "recentsongs":
                    HandleTaskException(GlobalScoresaberCommands.Recentsongs(_discord, command), command);
                    break;
                case "recentsong":
                    HandleTaskException(GlobalScoresaberCommands.NewRecentSong(_discord, command), command);
                    break;
                case "topsong":
                    HandleTaskException(GlobalScoresaberCommands.NewTopSong(_discord, command), command);
                    break;
                case "updateroles":
                    HandleTaskException(DutchServerCommands.UpdateRoles(_discord, command), command);
                    break;
                case "playerbase":
                    HandleTaskException(DutchServerCommands.Playerbase(_discord, command), command);
                    break;
                case "patterncatalog":
                    HandleTaskException(GlobalScoresaberCommands.PatternCatalog(_discord, command), command);
                    break;
                default:
                    break;
            }

            return;
        }

        public async Task HandleTaskException(Task task, SocketSlashCommand command, bool keepAuthor = false)
        {
            try
            {
                Console.WriteLine($"Command {command.CommandName} executed in {command.Channel.Name} by {command.User.Username}");
                //await command.DeferAsync();
                await command.RespondAsync("Results:");
                //task.Wait();
                //var msg = await command.FollowupAsync("Results:");
             
                if (!keepAuthor)
                {
                    await Task.Delay(500);
                    //msg.DeleteAsync();
                }
                TotalCommandsUsed++;
            }
            catch (Exception ex)
            {
                var exception = ex;
                throw new ArgumentException("Error", "failed");
            }
        }

        public async void CreateSlashCommands(bool pushGlobalCommands = false)
        {
            if (pushGlobalCommands) CreateGlobalSlashCommands();

            var guild = _discord.GetGuild(731936395223892028);
            var dutchGuild = _discord.GetGuild(505485680344956928);
            var irelandGuild = _discord.GetGuild(676524581271371814);
            try
            {
                //Delete Global Command if needed
                //var commands = await dutchGuild.GetApplicationCommandsAsync();
                //await commands.First(x => x.Name == "patterncatalog").DeleteAsync();

                //eventmanager
                await dutchGuild.CreateApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("eventmanager")
                    .WithDescription("Starts a process to create an event for the Dutch Discord")
                    .Build());
                //updateroles
                var countryDiscords = new List<ApplicationCommandOptionChoiceProperties>();
                var countries = new AutomaticCountryRankUpdateHandler(_discord).CountryList;
                foreach (var country in countries)
                {
                    countryDiscords.Add(new ApplicationCommandOptionChoiceProperties() { Name = country.country.ToString(), Value = country.discordServerID.ToString() });
                }
                foreach (var counrty in countries)
                {
                    var countryGuild = _discord.GetGuild((ulong)counrty.discordServerID);
                    await countryGuild.CreateApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("updateroles")
                    .WithDescription("Force updates all roles in the whole server")
                    .AddOption("country", ApplicationCommandOptionType.String, "Example: NL, IE", true, choices: countryDiscords.ToArray())
                    .Build());
                }                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


        }

        private async void CreateGlobalSlashCommands()
        {
            try
            {
                //Global Command ------------------------------------------------------------------------------------
                //RandomGif
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                .WithName("randomgif")
                .WithDescription("Posts a random Gif")
                .AddOption("type", ApplicationCommandOptionType.String, "Example: Funny, Horror, Dogs", false)
                .Build());
                //Improve
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("improve")
                    .WithDescription("Gives a list of ranked maps that relatively gives a lot of PP")
                    .AddOption("target_acc", ApplicationCommandOptionType.Number, "Example: 94, 95.5, 92.2", true)
                    .Build());
                //RankTracker
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("ranktracker")
                    .WithDescription("Enables or Disables rank updates from the bot in DM")
                    .Build());
                //profile
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("profile")
                    .WithDescription("Shows a card with a summary of interesting statistics from scoresaber")
                    .Build());
                //link
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("link")
                    .WithDescription("Links your discord account with your scoresaber account")
                    .AddOption("scoresaber_id", ApplicationCommandOptionType.String, "Example: https://scoresaber.com/u/76561198333869741 or 76561198333869741", true)
                    .Build());
                //unlink
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("unlink")
                    .WithDescription("unlinks your discord account from your scoresaber account")
                    .Build());
                //map
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("map")
                    .WithDescription("Shows details from a beat saber map")
                    .AddOption("search", ApplicationCommandOptionType.String, "Example: shrek")
                    .AddOption("bsr_key", ApplicationCommandOptionType.String, "Example: 666")
                    .Build());
                //settings
                var settingChoices = new List<ApplicationCommandOptionChoiceProperties>();
                settingChoices.Add(new ApplicationCommandOptionChoiceProperties() { Name = "Create", Value = "Create" });
                settingChoices.Add(new ApplicationCommandOptionChoiceProperties() { Name = "Edit", Value = "Edit" });
                settingChoices.Add(new ApplicationCommandOptionChoiceProperties() { Name = "Remove", Value = "Remove" });
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("settings")
                    .WithDescription("Shows details from a beat saber player")
                    .AddOption("scoresaber_id", ApplicationCommandOptionType.String, "Example: 76561198333869741", false)
                    .AddOption("mention", ApplicationCommandOptionType.Mentionable, "Example: @Silverhaze", false)
                    .AddOption("action", ApplicationCommandOptionType.String, "Example: Edit, Create,  Remove", false, choices: settingChoices.ToArray())
                    .Build());
                //help
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("help")
                    .WithDescription("Shows a list with all available command with details on how to use them")
                    .AddOption("command_name", ApplicationCommandOptionType.String, "Example: recentsong")
                    .Build());
                //compare
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("compare")
                    .WithDescription("Shows a comparison of 2 player's scoresaber stats")
                    .AddOption("player_one", ApplicationCommandOptionType.String, "Example: scoresaberid -> 76561198033166451 or @Silverhaze", true)
                    .AddOption("player_two", ApplicationCommandOptionType.String, "Example: scoresaberid -> 2538637699496776 or @Garsh", true)
                    .Build());
                //statistics
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("statistics")
                    .WithDescription("Shows statistics by graphs from types of data from the `/settings` command")
                    .Build());
                //tools
                var choices = new List<ApplicationCommandOptionChoiceProperties>();
                foreach (var tool in new ToolsList(_discord)._tools)
                {
                    if (tool.category != ToolsList.ToolCategory.MustHaves) choices.Add(new ApplicationCommandOptionChoiceProperties() { Name = tool.Name, Value = tool.Name });
                }
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("tools")
                    .WithDescription("Shows usefull community created tools for the beat saber community")
                    .AddOption("toolname", ApplicationCommandOptionType.String, "Example: scoresaber, hitbloq", false, choices: choices.OrderBy(x => x.Name).ToArray())
                    .AddOption("stars", ApplicationCommandOptionType.Integer, "Example: 0, 2, 5 ", false, maxValue: 5, minValue: 0)
                    .Build());
                //invite
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("invite")
                    .WithDescription("Gives an Invite link to use to share this bot")
                    .Build());
                //draw
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("draw")
                    .WithDescription("Gives a pokemon card of a random top 50 player")
                    .Build());
                //removebg
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("removebg")
                    .WithDescription("removes a background from an image")
                    .AddOption("url", ApplicationCommandOptionType.String, "Example: https://cdn.scoresaber.com/covers/image.png", true)
                    .Build());
                //search
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("search")
                    .WithDescription("Searches a player and displays commands that can be used on that player")
                    .AddOption("username", ApplicationCommandOptionType.String, "Example: silverhaze, cerret", false)
                    .AddOption("scoresaber_id", ApplicationCommandOptionType.String, "Example: 76561198187936410", false)
                    .AddOption("discord_tag", ApplicationCommandOptionType.Mentionable, "Example: @silverhaze", false)
                    .AddOption("discord_id", ApplicationCommandOptionType.String, "Example: 5345345234324", false)
                    .Build());
                //topsongs
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("topsongs")
                    .WithDescription("Shows the top 8 maps of a player")
                    .AddOption("page", ApplicationCommandOptionType.Integer, "Example: 2,5,7", false)
                    .Build());
                //recentsongs
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("recentsongs")
                    .WithDescription("Shows the top 8 recent played maps")
                    .AddOption("page", ApplicationCommandOptionType.Integer, "Example: 2,5,7", false)
                    .Build());
                //recentsong
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("recentsong")
                    .WithDescription("Shows the most recent played map with lots of details")
                    .AddOption("page", ApplicationCommandOptionType.Integer, "Example: 2,5,7", false)
                    .AddOption("scoresaber_id", ApplicationCommandOptionType.String, "Example: 76561198187936410", false)
                    .AddOption("discord_id", ApplicationCommandOptionType.String, "Example: 76561198187936410", false)
                    .AddOption("mention", ApplicationCommandOptionType.Mentionable, "Example: @silverhaze", false)
                    .Build());
                //topsong
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("topsong")
                    .WithDescription("Shows the top played map with lots of details")
                    .AddOption("page", ApplicationCommandOptionType.Integer, "Example: 2,5,7", false)
                    .AddOption("scoresaber_id", ApplicationCommandOptionType.String, "Example: 76561198187936410", false)
                    .AddOption("discord_id", ApplicationCommandOptionType.String, "Example: 76561198187936410", false)
                    .AddOption("mention", ApplicationCommandOptionType.Mentionable, "Example: @silverhaze", false)
                    .Build());
                //playerbase
                await _discord.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
                    .WithName("playerbase")
                    .WithDescription("Shows the total amount of players")
                    .AddOption("country", ApplicationCommandOptionType.String, "Example: NL, US, UK, FR", false)
                    .Build());
                //Pattern Catalog (Global)
                var slashCommandBuilder = new SlashCommandBuilder()
                    .WithName("patterncatalog")
                    .WithDescription("A catalog for all kind of patterns used in Beat Saber");

                var patternCatalogActionChoices = new List<ApplicationCommandOptionChoiceProperties>();
                patternCatalogActionChoices.Add(new ApplicationCommandOptionChoiceProperties() { Name = "Add New Pattern to the catalog", Value = "Add New Pattern to the catalog" });

                var patternCatalogPatternChoicesList = new List<List<ApplicationCommandOptionChoiceProperties>>();
                var patternCatalogPatternChoices = new List<ApplicationCommandOptionChoiceProperties>();
                foreach (var pattern in await new PatternCatalog().OpenPatternList())
                {
                    if (patternCatalogPatternChoices.Count < 25)
                    {
                        patternCatalogPatternChoices.Add(new ApplicationCommandOptionChoiceProperties() { Name = $"{pattern.Name}", Value = $"{pattern.Name}" });
                    }
                    else
                    {
                        var copyList = new List<ApplicationCommandOptionChoiceProperties>();
                        copyList.AddRange(patternCatalogPatternChoices.ToArray());
                        patternCatalogPatternChoicesList.Add(copyList);
                        patternCatalogPatternChoices.Clear();
                        patternCatalogPatternChoices.Add(new ApplicationCommandOptionChoiceProperties() { Name = $"{pattern.Name}", Value = $"{pattern.Name}" });
                    }
                }
                patternCatalogPatternChoicesList.Add(patternCatalogPatternChoices);

                slashCommandBuilder.AddOption("action", ApplicationCommandOptionType.String, "Add a pattern to the pattern catalog", false, choices: patternCatalogActionChoices.ToArray());

                var count = 0;
                foreach (var patternList in patternCatalogPatternChoicesList)
                {
                    count++;
                    slashCommandBuilder.AddOption($"patterns_{count}", ApplicationCommandOptionType.String, "Choose a pattern to display", false, choices: patternList.ToArray());
                }

                await _discord.CreateGlobalApplicationCommandAsync(slashCommandBuilder.Build());


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
