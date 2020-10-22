using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace DiscordBeatSaberBot.Api.Spotify
{
    class Spotify
    {
        private SpotifyWebAPI _spotify;

        public Spotify()
        {
            init().Wait();
        }

        public async Task init()
        {
            var clientId = await DatabaseContext.ExecuteSelectQuery("Select SpotifyClientId from Settings");
            var secretId = await DatabaseContext.ExecuteSelectQuery("Select SpotifySecretId from Settings");

            CredentialsAuth auth = new CredentialsAuth((string)clientId[0][0], (string)secretId[0][0]);
            Token token = await auth.GetToken();
            _spotify = new SpotifyWebAPI()
            {
                AccessToken = token.AccessToken,
                TokenType = token.TokenType
            };
        }

        public async Task<string> SearchItem(string searchname, string searchAuthorName)
        {            
            var results = _spotify.SearchItems(searchname, SearchType.Track);
            if (results.Tracks.Items.Count == 0) return null;

            var endResult = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
            foreach(var song in results.Tracks.Items)
            {
                foreach (var artist in song.Artists)
                {
                    if (artist.Name == searchAuthorName) endResult = "https://open.spotify.com/track/" + song.Id;
                }
                
            }

            return endResult;
        }
    }
}
