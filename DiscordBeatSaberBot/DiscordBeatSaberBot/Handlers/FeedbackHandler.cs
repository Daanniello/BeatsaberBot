using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DiscordBeatSaberBot.Handlers.AutomaticCountryRankUpdateHandler;

namespace DiscordBeatSaberBot.Handlers
{
    public class FeedbackHandler
    {
        private DiscordSocketClient _discord;
        private List<CountryDiscordInfo> _countries;

        public FeedbackHandler(DiscordSocketClient discord, List<CountryDiscordInfo> countries)
        {
            _discord = discord;
            _countries = countries;

            InitFeedback();
        }

        public async void InitFeedback()
        {
            foreach (var country in _countries)
            {
                if (country.feedbackChannelID != 0 && country.feedbackStaffChannelID != 0)
                {
                    var feedbackChannel = _discord.GetGuild((ulong)country.discordServerID).GetTextChannel((ulong)country.feedbackChannelID);
                    var messages = await feedbackChannel.GetMessagesAsync(10).FlattenAsync();
                    bool shouldRepost = false;
                    if (messages.Count() == 1)
                    {

                    }
                    if (messages.Count() > 1)
                    {
                        foreach (var msg in messages)
                        {
                            await msg.DeleteAsync();
                        };
                        shouldRepost = true;
                    }
                    if (messages.Count() == 0 || shouldRepost)
                    {

                        var embedBuilder = EmbedBuilderExtension.NullEmbed("Feedback & Application Form", "Click on one of the two buttons bellow to start a 'Feedback form' or an 'Application form' \n" +
                            "Once a button has been pressed, You will get a notification from the bot in DM. There, the bot will lead you through a question process to fulfill your needs.\n\n" +
                            "- The Feedback form is meant for any type of feedback for this server and is **anonymous**!\n" +
                            "- The Application form is meant to apply for a 'job' within this server.\n\n" +
                            "Once your form in DM has been filled in, It will be directly send towards the Staff Channel.\n" +
                            "Staff members will need to react on the form and that reaction will directly be forwarded towards the user.");

                        var componentBuilder = new ComponentBuilder();
                        componentBuilder.WithButton("Start Feedback Form", customId: "feedbackFormButton", style: ButtonStyle.Primary);
                        componentBuilder.WithButton("Start Application Form", customId: "applicationFormButton", style: ButtonStyle.Primary);

                        await feedbackChannel.SendMessageAsync("", false, embedBuilder.Build(), component: componentBuilder.Build());
                    }

                    _discord.ButtonExecuted += Discord_ButtonExecuted;
                }
            }
        }

        private async System.Threading.Tasks.Task Discord_ButtonExecuted(SocketMessageComponent arg)
        {
            if (arg.Data.CustomId == "feedbackFormButton")
            {
                StartFormProcess(arg, "Feedback");                
            }
            if (arg.Data.CustomId == "applicationFormButton")
            {
                StartFormProcess(arg, "Application");
            }
            if (arg.Data.CustomId.Contains("acceptFormButton"))
            {
                StartStaffProcess(arg, "Approved");
            }
            if (arg.Data.CustomId.Contains("rejectFormButton"))
            {
                StartStaffProcess(arg, "Rejected");
            }
            if (arg.Data.CustomId.Contains("resendFormButton"))
            {
                StartStaffProcess(arg, "Resend");
            }

            return;
        }

        public async void StartStaffProcess(SocketMessageComponent arg, string type)
        {
            await arg.Channel.SendMessageAsync($"{arg.User.Mention} Give a statement about why this form has been {type}. (This will be send to the user) You have 30 minutes");

            var startTime = DateTime.Now;
            var endTime = startTime.AddMinutes(30);
            var content = "";
            do
            {
                await Task.Delay(1000);
                var message = await arg.Channel.GetMessagesAsync(1).FlattenAsync();
                if (message.First().CreatedAt > startTime && arg.User.Id == message.First().Author.Id)
                {
                    content = message.First().Content;
                    break;
                }

            } while (endTime > startTime);
            if (content == "")
            {
                await arg.Channel.SendMessageAsync("Giving a reaction has failed. Please redo the reaction process.");
            }

            await arg.Channel.SendMessageAsync("Reaction has been send to the form author!");

            var id = Convert.ToUInt64(arg.Message.Components.First().Components.First().CustomId.Split("_")[1]);

            var componentBuilder = new ComponentBuilder();
            componentBuilder.WithButton("Send another reaction", customId: $"resendFormButton_{arg.User.Id}", style: ButtonStyle.Danger);
            await arg.Message.ModifyAsync(x => x.Components = componentBuilder.Build());    

            var user = await _discord.GetUserAsync(id);
            var dm = await user.CreateDMChannelAsync();
            await dm.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed($"{(type == "Resend" ? "Extra reaction from staff": $"Your form has been {type} by staff!")}", $"**Reaction from staff:** \n{content}").Build());
        }

        public async void StartFormProcess(SocketMessageComponent arg, string type)
        {
            var dm = await arg.User.CreateDMChannelAsync();

            var formContent = "";
            if(type == "Application")
            {
                formContent = "" +
                    "Age: \n\n" +
                    "I want to apply for: Staff, Mod, Event leader, Programmer, Visual Design, Innovation, Youtube, Recruiter, etc \n\n" +
                    "I want to apply for this function because...\n\n" +
                    "I think I am suitable for this function because...";
            }
            if (type == "Feedback")
            {
                formContent = "My feedback is...";
            }

            var embedBuilder = EmbedBuilderExtension.NullEmbed($"{type} Form", "Please copy the following content, fill it in and send it when it is finished. You have 30 minutes\n" +
                "```" +
                $"{type} Form:\n" +
                $"{formContent}" +
                "```");

            await dm.SendMessageAsync("", false, embedBuilder.Build());

            var startTime = DateTime.Now;
            var endTime = startTime.AddMinutes(30);
            var content = "";
            do
            {
                await Task.Delay(1000);
                var message = await dm.GetMessagesAsync(1).FlattenAsync();
                if(message.First().CreatedAt > startTime && message.First().Author.IsBot == false)
                {
                    content = message.First().Content;
                    break;
                }

            } while (endTime > startTime);

            if (content == "")
            {
                await dm.SendMessageAsync("Form has not been sent. No content or reaction was too late");
                return;
            }

            await dm.SendMessageAsync("Form has been sent, You should get a reaction soon.");

            var country = _countries.FirstOrDefault(x => (ulong)x.feedbackChannelID == arg.Channel.Id);
            if(country != null)
            {
                var channel = _discord.GetGuild((ulong)country.discordServerID).GetTextChannel((ulong)country.feedbackStaffChannelID);
                var embedBuilderStaff = EmbedBuilderExtension.NullEmbed($"{type} Form by {(type == "Feedback" ? "(anonymous)" : $"{arg.User.Username}")}", $"```{content}```\n\n" +
                    $"**Accept or Reject the form. With both processes, a reply has to be given. This reply will be given to the author of the form.** \n\n" +
                    $" - *Accepting means this feedback or application has been approved and following actions or statements should be given.*\n" +
                    $" - *Rejecting means this feedback or application has been rejected and following actions or statements should be given.*");
                var componentBuilder = new ComponentBuilder();
                componentBuilder.WithButton("Accept and start reply process", customId: $"acceptFormButton_{arg.User.Id}", style: ButtonStyle.Success);
                componentBuilder.WithButton("Reject and start reply process", customId: $"rejectFormButton_{arg.User.Id}", style: ButtonStyle.Danger);

                await channel.SendMessageAsync("", false, embedBuilderStaff.Build(), component: componentBuilder.Build());
            }   
        }
    }
}
