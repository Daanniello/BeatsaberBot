using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Commands.Functions
{
    public class Playlists
    {
        public SocketSlashCommand _command;

        public async void CreateNewPlaylist(DiscordSocketClient discord, SocketSlashCommand command)
        {
            _command = command;
            var msg = await command.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("-", "-").Build());
            var title = await AskQuestion($"What is the title of the playlist?", $"Give a title, make sure it is short but powerful", msg);
            var description = await AskQuestion($"What is the description of the playlist?", $"Give a description, make sure it describes the content of the playlist well", msg);
            dynamic attachment = command.Data.Options.First().Value;

            var playlist = new PlaylistsModel { Title = title, Description = description, JsonPath = attachment.Url};
            AddPlaylist(playlist, command.User);

            await msg.ModifyAsync(x => x.Embed = EmbedBuilderExtension.NullEmbed("Done!", "Your playlist is now public! thank you for your contribution").Build());
        }

        public void AddPlaylist(PlaylistsModel playlist, SocketUser user)
        {
            var pathJson = @"../../../Resources/playlists.json";
            var pathAttachments = @"../../../Resources/Playlists/";

            //var l = new List<PlaylistsModel>();
            //l.Add(new PlaylistsModel());
            //var ss = JsonConvert.SerializeObject(l);
            //File.WriteAllText(pathJson, ss);

            using (var client = new WebClient())
            {
                client.DownloadFile(playlist.JsonPath, $"{pathAttachments}{user.Id}_{playlist.Title}.json");
            }
            var pl = File.ReadAllText($"{pathAttachments}{user.Id}_{playlist.Title}.json");

            var playlistsJson = File.ReadAllText(pathJson);
            var playlistsObject = JsonConvert.DeserializeObject<List<PlaylistsModel>>(playlistsJson);
            var plObject = JsonConvert.DeserializeObject<PlaylistsJson>(pl);
            playlist.ImageBaseString = plObject.Image;
            playlistsObject.Add(playlist);
            var json = JsonConvert.SerializeObject(playlistsObject);
            File.WriteAllText(pathJson, json);
        }

        public async Task<string> AskQuestion(string title, string description, RestUserMessage messageEmbed = null)
        {
            var embed = EmbedBuilderExtension.NullEmbed(title, description);

            if (messageEmbed != null)
            {
                await messageEmbed.ModifyAsync(x => x.Embed = embed.Build());
            }
            else
            {
                await _command.Channel.SendMessageAsync("", false, embed.Build());
            }


            var answer = await WaitForAnswer(300);
            return answer;
        }

        public async Task<string> WaitForAnswer(int timeToWait)
        {
            var endTime = DateTime.Now.AddSeconds(timeToWait);
            var startTime = DateTime.Now;

            do
            {
                await Task.Delay(1000);
                var possibleReaction = await _command.Channel.GetMessagesAsync(1).Flatten().FirstAsync();
                if (possibleReaction.Author.Id == _command.User.Id && possibleReaction.CreatedAt > startTime)
                {
                    var content = possibleReaction.Content;
                    possibleReaction.DeleteAsync();
                    return content;
                }

            } while (DateTime.Now < endTime);
            return "";
        }

        public class PlaylistsModel
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string ImageBaseString { get; set; }
            public string JsonPath { get; set; }
            public int Upvotes { get; set; }
            public int Downloads { get; set; }
        }

        public partial class PlaylistsJson
        {
            [JsonProperty("image")]
            public string Image { get; set; }
        }
    }
}
