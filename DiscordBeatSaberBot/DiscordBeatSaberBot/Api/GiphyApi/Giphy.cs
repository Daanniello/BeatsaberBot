using Discord.WebSocket;
using GiphyDotNet;
using GiphyDotNet.Model.Parameters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Api.GiphyApi
{
    class Giphy
    {
        private GiphyDotNet.Manager.Giphy _giphy;

        public Giphy()
        {
            _giphy = new GiphyDotNet.Manager.Giphy("ewKEoFkgHEn6TXJyi1J8t2rcZANDIaC6");
        }

        public async Task<string> SearchParameter(string parameter, Rating rating = Rating.G)
        {
            var offset = new Random().Next(0, 4000);

            var searchParameter = new SearchParameter()
            {
                Query = parameter,
                Rating = rating,
                Offset = offset,
                Limit = 5
            };
            var gifResult = await _giphy.GifSearch(searchParameter);

            if (gifResult.Pagination.Offset > gifResult.Pagination.TotalCount) {
                offset = gifResult.Pagination.TotalCount - 50;
                if (offset < 0) offset = 0;
                gifResult = await _giphy.GifSearch(new SearchParameter()
                {
                    Query = parameter,
                    Rating = rating,
                    Offset = offset,
                    Limit = 50
                });
            }

            if (gifResult == null || gifResult.Data.Length <= 0)
            {
                return null;
            }
            var randomNumber = new Random().Next(0, gifResult.Data.Length);
            return gifResult.Data[randomNumber].BitlyGifUrl;

        }

        public async Task PostTrendingGif(DiscordSocketClient discord, ulong guildId, ulong channelId)
        {
            var trendingGif = await _giphy.TrendingGifs(new TrendingParameter() { Rating = Rating.G});

            var channel = discord.GetGuild(guildId).GetTextChannel(channelId);

            await channel.SendMessageAsync("**---Latest trending post---**");
            await channel.SendMessageAsync(trendingGif.Data[0].BitlyGifUrl);
        }
    }
}
