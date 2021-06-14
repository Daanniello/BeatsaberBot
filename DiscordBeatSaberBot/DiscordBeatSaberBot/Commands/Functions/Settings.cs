using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Commands.Functions
{
    public class Settings
    {
        private DiscordSocketClient _discord;
        public enum parameterType
        {
            None,
            DiscordTag,
            ScoresaberID,
            DiscordID
        }

        public Settings(DiscordSocketClient discord, SocketMessage message)
        {
            _discord = discord;
            //Check parameter type
            var messageParameters = message.Content.Substring(12);
            parameterType type = parameterType.None;
            if (messageParameters.Contains("@")) type = parameterType.DiscordTag;
            if (messageParameters.Trim().All(x => char.IsDigit(x)) && messageParameters != "") type = parameterType.ScoresaberID;

            //Clean up the parameter
            var parameter = messageParameters.Replace("@", "").Replace("<", "").Replace(">", "").Replace("!", "").Trim().ToLower();

            CheckWhatToDo(message, parameter, type);
        }

        public async void CheckWhatToDo(SocketMessage message, string parameter, parameterType type)
        {
            //Change a certain value
            //IF parameter == add 
            if (parameter == "edit")
            {
                new SettingsQuestionList(_discord, message).Edit();
                return;
            }

            //Start the questions list 
            //IF parameter == create
            if (parameter == "create")
            {
                new SettingsQuestionList(_discord, message).Create();
                return;
            }

            //Remove Data
            if (parameter == "remove")
            {
                new SettingsQuestionList(_discord, message).Remove(message.Author.Id);
                return;
            }

            //Check if the user has data
            //IF not, show message to create one with '!bs settings create'
            //var results = await DatabaseContext.ExecuteSelectQuery($"SELECT * FROM UserBeatSaberSettings WHERE {parameter}");
            if (type == parameterType.ScoresaberID)
            {
                new SettingsQuestionList(_discord, message).Get(parameter);
            }
            if (type == parameterType.DiscordTag)
            {
                var scoresaberID = await RoleAssignment.GetScoresaberIdWithDiscordId(parameter);
                new SettingsQuestionList(_discord, message).Get(scoresaberID);
            }
            if (type == parameterType.None)
            {
                var scoresaberID = await RoleAssignment.GetScoresaberIdWithDiscordId(message.Author.Id.ToString());
                new SettingsQuestionList(_discord, message).Get(scoresaberID);
            }
        }
    }

    public class SettingsQuestionList
    {
        private bool isEnded = false;
        private SocketMessage _message;
        private DiscordSocketClient _discord;
        public SettingsQuestionList(DiscordSocketClient discord, SocketMessage message)
        {
            _discord = discord;
            _message = message;


        }

        public async Task Get(string parameter)
        {
            var playerFull = await new ScoresaberAPI(parameter).GetPlayerFull();
            var results = await DatabaseContext.ExecuteSelectQuery($"select * from UserBeatSaberSettings where ScoreSaberID={playerFull.playerInfo.PlayerId}");

            if (results.Count <= 0)
            {
                await _message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Can't find this user", "This user does not have a settings page or does not exist.\n\n If you want your own page, type `!bs settings create`").Build());
                return;
            }

            var embed = new EmbedBuilder()
            {
                Title = $"Beat Saber Settings from {playerFull.playerInfo.Name} :flag_{playerFull.playerInfo.Country.ToLower()}:",
                ThumbnailUrl = "https://new.scoresaber.com" + playerFull.playerInfo.Avatar,
                Url = "https://scoresaber.com/u/" + playerFull.playerInfo.PlayerId,
                Footer = new EmbedFooterBuilder() { Text = "use (!bs settings create) to create your own settings page" }
            };

            var items = results.First();
            for (var x = 0; x < items.Count(); x++)
            {
                if (items[x].ToString().Trim() == "") results.First()[x] = "-";
            }

            embed.AddField("Personal info", $"" +
                $"Age: ***{results[0][13]}*** \n" +
                $"Height: ***{results[0][14]}*** \n" +
                $"Weight: ***{results[0][15]}*** \n" +
                $"Gender: ***{results[0][16]}*** \n");

            embed.AddField("Hardware", $"" +
                $"Headset: ***{results[0][11]}*** \n" +
                $"Controllers: ***{results[0][12]}*** \n");

            embed.AddField("Settings", $"" +
                $"Left Controller: ***{results[0][9]}*** \n" +
                $"Right Controller: ***{results[0][10]}*** \n" +
                $"Sabers: ***{results[0][1]}*** \n" +
                $"FAV Mods: ***{results[0][6]}*** \n" +
                $"Custom Notes: ***{results[0][5]}*** \n" +
                $"Avatar: ***{results[0][0]}*** \n" +
                $"Platform: ***{results[0][4]}*** \n" +
                $"In-Game height: ***{results[0][17]}*** \n" +
                $"Note Color RGB Left: ***{results[0][7]}*** \n" +
                $"Note Color RGB Right: ***{results[0][8]}*** \n");

            embed.AddField("Description", $"{results[0][18]}");

            await _message.Channel.SendMessageAsync("", false, embed.Build());
        }

        public static async Task<bool> HasSettingsPage(string scoresaberID)
        {
            var results = await DatabaseContext.ExecuteSelectQuery($"select * from UserBeatSaberSettings where ScoreSaberID={scoresaberID}");
            if (results.Count() > 0) return true;
            return false;
        }

        public static async Task<Embed> GetSettingsPageWithScoresaberID(string scoresaberID)
        {
            var playerFull = await new ScoresaberAPI(scoresaberID).GetPlayerFull();
            var results = await DatabaseContext.ExecuteSelectQuery($"select * from UserBeatSaberSettings where ScoreSaberID={playerFull.playerInfo.PlayerId}");
            if (results.Count() == 0) return null;

            var embed = new EmbedBuilder()
            {
                Title = $"Beat Saber Settings from {playerFull.playerInfo.Name} :flag_{playerFull.playerInfo.Country.ToLower()}:",
                ThumbnailUrl = "https://new.scoresaber.com" + playerFull.playerInfo.Avatar,
                Url = "https://scoresaber.com/u/" + playerFull.playerInfo.PlayerId,
                Footer = new EmbedFooterBuilder() { Text = "use (!bs settings create) to create your own settings page" }
            };

            var items = results.First();
            for (var x = 0; x < items.Count(); x++)
            {
                if (items[x].ToString().Trim() == "") results.First()[x] = "-";
            }

            embed.AddField("Personal info", $"" +
                $"Age: ***{results[0][13]}*** \n" +
                $"Height: ***{results[0][14]}*** \n" +
                $"Weight: ***{results[0][15]}*** \n" +
                $"Gender: ***{results[0][16]}*** \n");

            embed.AddField("Hardware", $"" +
                $"Headset: ***{results[0][11]}*** \n" +
                $"Controllers: ***{results[0][12]}*** \n");

            embed.AddField("Settings", $"" +
                $"Left Controller: ***{results[0][9]}*** \n" +
                $"Right Controller: ***{results[0][10]}*** \n" +
                $"Sabers: ***{results[0][1]}*** \n" +
                $"FAV Mods: ***{results[0][6]}*** \n" +
                $"Custom Notes: ***{results[0][5]}*** \n" +
                $"Avatar: ***{results[0][0]}*** \n" +
                $"Platform: ***{results[0][4]}*** \n" +
                $"In-Game height: ***{results[0][17]}*** \n" +
                $"Note Color RGB Left: ***{results[0][7]}*** \n" +
                $"Note Color RGB Right: ***{results[0][8]}*** \n");

            embed.AddField("Description", $"{results[0][18]}");

            return embed.Build();
        }

        public async Task Edit()
        {
            var scoreSaberID = await RoleAssignment.GetScoresaberIdWithDiscordId(_message.Author.Id.ToString());
            if (scoreSaberID == "")
            {
                await _message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Can't use this function if you are not linked!", "Type `!bs link (ScoreSaberID) to link`").Build());
                return;
            }

            var messageEmbed = await _message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Edit", "What do you want to change? \n\nType the exact word!").Build());

            var answer = await WaitForAnswer(30);
            var tableName = "";
            switch (answer.ToLower())
            {
                case "age":
                    tableName = "Age";
                    break;
                case "height":
                    tableName = "Height";
                    break;
                case "weight":
                    tableName = "Weight";
                    break;
                case "gender":
                    tableName = "Gender";
                    break;
                case "headset":
                    tableName = "Headset";
                    break;
                case "controllers":
                    tableName = "Controllers";
                    break;
                case "left controller":
                    tableName = "ControllerSettingsLeft";
                    break;
                case "right controller":
                    tableName = "ControllerSettingsRight";
                    break;
                case "sabers":
                    tableName = "Sabers";
                    break;
                case "fav mods":
                    tableName = "FavMods";
                    break;
                case "custom notes":
                    tableName = "Notes";
                    break;
                case "avatar":
                    tableName = "Avatar";
                    break;
                case "platform":
                    tableName = "Platform";
                    break;
                case "note color rgb left":
                    tableName = "NoteColorLeft";
                    break;
                case "note color rgb right":
                    tableName = "NoteColorRight";
                    break;
                case "in-game height":
                    tableName = "InGameHeight";
                    break;
                case "description":
                    tableName = "Extra";
                    break;
            }

            if (tableName == "")
            {
                await messageEmbed.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Edit", "This word does not exist as a setting").Build());

                return;
            }

            await messageEmbed.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Edit", "Now you can change the description of the setting. \nType what you want to change the setting to. \nDiscord markdown is enabled").Build());

            var description = await WaitForAnswer(30);
            await DatabaseContext.ExecuteInsertQuery($"UPDATE UserBeatSaberSettings SET {DatabaseContext.QueryCheck(tableName)} = '{DatabaseContext.QueryCheck(description)}' WHERE DiscordID={_message.Author.Id} AND ScoreSaberID={scoreSaberID}");
            await messageEmbed.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Edit", "Done!").Build());
        }

        public async Task Remove(ulong discordID)
        {
            var scoreSaberID = await RoleAssignment.GetScoresaberIdWithDiscordId(discordID.ToString());
            if (scoreSaberID == null)
            {
                await _message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("You are not linked to scoresaber", "Type `!bs link ScoresaberID` to link it.").Build());
                return;
            }
            var hasASettingsPage = await DatabaseContext.ExecuteSelectQuery($"SELECT * from UserBeatSaberSettings WHERE DiscordID={discordID.ToString()} AND ScoreSaberID={scoreSaberID}");
            if (hasASettingsPage.Count <= 0)
            {
                await _message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("You do not have a settings profile", "Type `!bs settings create` to create one.").Build());
                return;
            }
            else
            {
                await DatabaseContext.ExecuteSelectQuery($"DELETE FROM UserBeatSaberSettings WHERE DiscordID={discordID} AND ScoreSaberID={await RoleAssignment.GetScoresaberIdWithDiscordId(discordID.ToString())}");
                await _message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Your settings profile has been removed", "Type `!bs settings create` if you would like to create one again.").Build());
                return;
            }
        }

        public async Task Create()
        {
            var scoresaberID = await RoleAssignment.GetScoresaberIdWithDiscordId(_message.Author.Id.ToString());
            if (scoresaberID == "")
            {
                await _message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Can't use this function if you are not linked!", "Type `!bs link (ScoreSaberID) to link`").Build());
                return;
            }

            var hasASettingsPage = await DatabaseContext.ExecuteSelectQuery($"SELECT * from UserBeatSaberSettings WHERE DiscordID={_message.Author.Id} AND ScoreSaberID={scoresaberID}");
            if (hasASettingsPage.Count > 0)
            {
                await _message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("You already have a settings page", "Use `!bs settings` to see your own settings page. \nAnd use `!bs settings edit` to edit certain values").Build());
                return;
            }


            var embed = EmbedBuilderExtension.NullEmbed("Creating your settings page", "You are about to start answering questions that will be added to your settings page. \nThis is public and everyone can see these settings. \nAll questions are optional. \nDiscord Markdown is enabled. \n\nDo you want to start the questions? \nTYPE 'YES' OR 'NO'");
            var messageEmbed = await _message.Channel.SendMessageAsync("", false, embed.Build());


            var answer = await WaitForAnswer(30);
            if (isEnded)
            {
                Ended();
                return;
            }
            if (answer.ToLower() != "yes") return;

            //1
            var q1 = await AskQuestion("Question 1 (Sabers)", "What Sabers are you using? \nYou can put in a link, name or description", messageEmbed);
            if (isEnded)
            {
                Ended();
                return;
            }

            var q2 = await AskQuestion("Question 2 (Avatar)", "What custom avatar are you using in Beat Saber? \nYou can put in a link, name or description", messageEmbed);
            if (isEnded)
            {
                Ended();
                return;
            }

            var q3 = await AskQuestion("Question 3 (Favorite Mods)", "What are your favorite mods to use? \nYou can put in a link, name or description", messageEmbed);
            if (isEnded)
            {
                Ended();
                return;
            }

            var q4 = await AskQuestion("Question 4 (Headset)", "What VR headset are you using? \nYou can put in a link, name or description", messageEmbed);
            if (isEnded)
            {
                Ended();
                return;
            }

            var q5 = await AskQuestion("Question 5 (Controllers)", "What controllers are you using? \nYou can put in a link, name or description", messageEmbed);
            if (isEnded)
            {
                Ended();
                return;
            }

            var q6 = await AskQuestion("Question 6 (Controller Settings)", "What are the settings of your LEFT controller? \nYou can put in a link, name or description", messageEmbed);
            if (isEnded)
            {
                Ended();
                return;
            }

            var q7 = await AskQuestion("Question 7 (Controller Settings)", "What are the settings of your RIGHT controllers? \nYou can put in a link, name or description", messageEmbed);
            if (isEnded)
            {
                Ended();
                return;
            }

            var q8 = await AskQuestion("Question 8 (Note Colors)", "What are the RGB values of your LEFT note? \nYou can put in a link, name or description", messageEmbed);
            if (isEnded)
            {
                Ended();
                return;
            }

            var q9 = await AskQuestion("Question 9 (Note Colors)", "What are the RGB values of your RIGHT note? \nYou can put in a link, name or description", messageEmbed);
            if (isEnded)
            {
                Ended();
                return;
            }

            var q10 = await AskQuestion("Question 10 (Platform)", "What platform are you using in Beat Saber? \nYou can put in a link, name or description", messageEmbed);
            if (isEnded)
            {
                Ended();
                return;
            }

            var q11 = await AskQuestion("Question 11 (Custom Notes)", "What custom notes are you using? \nYou can put in a link, name or description", messageEmbed);
            if (isEnded)
            {
                Ended();
                return;
            }

            var q12 = await AskQuestion("Question 12 (Plaftform)", "What is your in-game height? \nYou can put in a link, name or description", messageEmbed);
            if (isEnded)
            {
                Ended();
                return;
            }

            var q13 = await AskQuestion("Question 13 (personal info)", "What is your age?", messageEmbed);
            if (isEnded)
            {
                Ended();
                return;
            }

            var q14 = await AskQuestion("Question 14 (personal info)", "What is your irl height?", messageEmbed);
            if (isEnded)
            {
                Ended();
                return;
            }

            var q15 = await AskQuestion("Question 15 (personal info)", "What is your weight?", messageEmbed);
            if (isEnded)
            {
                Ended();
                return;
            }

            var q16 = await AskQuestion("Question 16 (personal info)", "What is your gender?", messageEmbed);
            if (isEnded)
            {
                Ended();
                return;
            }

            var q17 = await AskQuestion("Question 17 (Extra)", "Add any text what you like? Self promotions? fun description? anything", messageEmbed);
            if (isEnded)
            {
                Ended();
                return;
            }

            await _message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("That was it!", "Use \n`!bs settings` to see your own settings. \n`!bs settings (scoresaberID / discordTag)` to see someone else his settings. \n`!bs settings remove` to remove your data. \n`!bs settings edit` to change a specific question.").Build());
            var toInsert = $"Insert into UserBeatSaberSettings (Sabers, Avatar, FavMods, Headset, Controllers, ControllerSettingsLeft, ControllerSettingsRight, NoteColorLeft, NoteColorRight, Platform, Notes, InGameHeight, Age, Height, Weight, Gender, DiscordID, ScoreSaberID, Extra) values (" +
                $"'{DatabaseContext.QueryCheck(q1)}', " +
                $"'{DatabaseContext.QueryCheck(q2)}', " +
                $"'{DatabaseContext.QueryCheck(q3)}', " +
                $"'{DatabaseContext.QueryCheck(q4)}', " +
                $"'{DatabaseContext.QueryCheck(q5)}', " +
                $"'{DatabaseContext.QueryCheck(q6)}', " +
                $"'{DatabaseContext.QueryCheck(q7)}', " +
                $"'{DatabaseContext.QueryCheck(q8)}', " +
                $"'{DatabaseContext.QueryCheck(q9)}', " +
                $"'{DatabaseContext.QueryCheck(q10)}', " +
                $"'{DatabaseContext.QueryCheck(q11)}', " +
                $"'{DatabaseContext.QueryCheck(q12)}', " +
                $"'{DatabaseContext.QueryCheck(q13)}', " +
                $"'{DatabaseContext.QueryCheck(q14)}', " +
                $"'{DatabaseContext.QueryCheck(q15)}', " +
                $"'{DatabaseContext.QueryCheck(q16)}', " +
                $"'{_message.Author.Id}', " +
                $"'{scoresaberID}', " +
                $"'{DatabaseContext.QueryCheck(q17)}')";


            var succes = await DatabaseContext.ExecuteInsertQuery(toInsert);
            if (!succes) await _message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Oh... oops something went wrong", "This function might bug out sometimes. You could DM Silverhaze#0001 for your answers to be uploaded. And to check out what the bug was.").Build());
            _discord.GetGuild(731936395223892028).GetTextChannel(775514268501934101).SendMessageAsync(toInsert + "-----------" + succes.ToString());
        }

        public async Task<string> AskQuestion(string title, string description, RestUserMessage messageEmbed = null)
        {
            var embed = EmbedBuilderExtension.NullEmbed(title, description);
            embed.Footer = new EmbedFooterBuilder() { Text = "Type SKIP if you want to skip the question" };

            if (messageEmbed != null)
            {
                await messageEmbed.ModifyAsync(x => x.Embed = embed.Build());
            }
            else
            {
                await _message.Channel.SendMessageAsync("", false, embed.Build());
            }


            var answer = await WaitForAnswer(300);
            if (answer.ToLower() == "skip") answer = "-";
            return answer;
        }

        public async Task<string> WaitForAnswer(int timeToWait)
        {
            var endTime = DateTime.Now.AddSeconds(timeToWait);
            var startTime = DateTime.Now;

            do
            {
                await Task.Delay(1000);
                var possibleReaction = await _message.Channel.GetMessagesAsync(1).Flatten().FirstAsync();
                if (possibleReaction.Author == _message.Author && possibleReaction.CreatedAt > startTime)
                {
                    var content = possibleReaction.Content;
                    possibleReaction.DeleteAsync();
                    return content;
                }

            } while (DateTime.Now < endTime);

            isEnded = true;
            return "";
        }

        public async void Ended()
        {
            await _message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Questions were canceled", "You waited too long to answer.").Build());
        }
    }
}
