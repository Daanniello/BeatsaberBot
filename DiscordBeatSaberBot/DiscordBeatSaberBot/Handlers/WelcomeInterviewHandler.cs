using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Handlers
{
    class WelcomeInterviewHandler
    {
        private DiscordSocketClient _discord;
        private ISocketMessageChannel _channel;
        private RestUserMessage _interviewEmbed;
        private DateTime _startDate;
        private ulong _discordId;
        public static List<string> interviewQuestions = new List<string>() {
        "Hoe wil je genoemd worden?",
        "Wat is je leeftijd?",
        "Hoe ben je met Beat Saber gestart?",
        "Wat zijn je hobby's?",
        "Welke games speel je buiten Beat Saber om?",
        "Nog iets wat je kwijt wilt?"
        };
        private List<string> _interviewAnswers = new List<string>();

        public WelcomeInterviewHandler(DiscordSocketClient discord, ISocketMessageChannel channel, ulong discordId)
        {
            _discord = discord;
            _channel = channel;
            _discordId = discordId;
        }

        public async Task AskForInterview()
        {
            _interviewEmbed = await _channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Zou je voor dat je de server komt een welkoms interview willen maken?", "Het interview is kort en voegt je reacties toe aan het welkomsbericht van de server. Hierdoor krijgen communityleden een beter beeld over wie je bent. Het is zo makkelijker om een gesprek te beginnen. \n\nAntwoord **Ja** of **Nee**").Build());
            await ReactionWatcher();
            return;
        }

        public async Task ReactionWatcher()
        {
            var dateTime = DateTime.Now;
            dateTime = dateTime.AddMinutes(2);
            bool hasReacted = false;
            do
            {
                var latestMessage = await _channel.GetMessagesAsync(1).Flatten().FirstAsync();

                if (latestMessage.Content.ToLower().Contains("ja") && latestMessage.Author.Id == _discordId)
                {
                    StartInterview();
                    await latestMessage.DeleteAsync();                    
                    hasReacted = true;
                }
                if (latestMessage.Content.ToLower().Contains("nee"))
                {
                    await _interviewEmbed.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Welkom in de server!", "Enjoy your stay", null, null).Build());
                    await Task.Delay(5000);
                    hasReacted = true;
                }
            } while (!hasReacted || dateTime < DateTime.Now);

            return;
        }

        public async void StartInterview()
        {
            var embed = EmbedBuilderExtension.NullEmbed("Info", "Je kunt vragen skippen door 'skip' te antwoorden. Na 20 minuten bij het niet antwoorden van een vraag stopt het interview automatisch. Het interview start in 3 seconden..", null, null);
            await _interviewEmbed.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Info", "Je kunt vragen skippen door 'skip' te antwoorden. Het interview start in 3 seconden..", null, null).Build());
            await Task.Delay(8000);
            await _interviewEmbed.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Info", "3 seconden...", null, null).Build());
            await Task.Delay(1000);
            await _interviewEmbed.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Info", "2 seconden...", null, null).Build());
            await Task.Delay(1000);
            await _interviewEmbed.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Info", "1 seconden...", null, null).Build());
            await Task.Delay(1000);
            _startDate = DateTime.Now;
            await _interviewEmbed.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Vraag 1 van de 6", interviewQuestions[0], null, null).Build());
            await InterviewAnswersWatcher();
            await _interviewEmbed.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Vraag 2 van de 6", interviewQuestions[1], null, null).Build());
            await InterviewAnswersWatcher();
            await _interviewEmbed.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Vraag 3 van de 6", interviewQuestions[2], null, null).Build());
            await InterviewAnswersWatcher();
            await _interviewEmbed.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Vraag 4 van de 6", interviewQuestions[3], null, null).Build());
            await InterviewAnswersWatcher();
            await _interviewEmbed.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Vraag 5 van de 6", interviewQuestions[4], null, null).Build());
            await InterviewAnswersWatcher();
            await _interviewEmbed.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Vraag 6 van de 6", interviewQuestions[5], null, null).Build());
            await InterviewAnswersWatcher();
            await _interviewEmbed.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Dit was het interview", "Welkom in de server! =D", null, null).Build());
            uploadAnswers();
        }

        public async Task InterviewAnswersWatcher()
        {
            var dateTimeLimit = DateTime.Now;
            dateTimeLimit = dateTimeLimit.AddMinutes(30);
            do
            {
                var latestMessage = await _channel.GetMessagesAsync(1).Flatten().FirstAsync();
                if(latestMessage.Content.ToLower() == "skip" && latestMessage.Author.Id == _discordId && latestMessage.CreatedAt > _startDate)
                {
                    _interviewAnswers.Add("skip");
                    await latestMessage.DeleteAsync();
                    return;
                } else if(latestMessage.Author.Id == _discordId && latestMessage.CreatedAt > _startDate)
                {
                    _interviewAnswers.Add(latestMessage.Content);
                    await latestMessage.DeleteAsync();
                    return;
                }

            } while (dateTimeLimit > DateTime.Now);            
            return;
        }

        public async void uploadAnswers()
        {
            await DatabaseContext.ExecuteInsertQuery($"insert into PlayerInterview(DiscordId,vraag1,vraag2,vraag3,vraag4,vraag5,vraag6) values({_discordId},'{_interviewAnswers[0]}','{_interviewAnswers[1]}','{_interviewAnswers[2]}','{_interviewAnswers[3]}','{_interviewAnswers[4]}','{_interviewAnswers[5]}')");
        }
    }
}
