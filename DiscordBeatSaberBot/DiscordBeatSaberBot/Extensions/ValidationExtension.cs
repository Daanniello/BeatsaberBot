using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Discord.WebSocket;

namespace DiscordBeatSaberBot
{
    internal static class ValidationExtension
    {
        public static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        public static async Task<bool> IsDutch(string ID)
        {
            string url = "https://scoresaber.com/u/" + ID;
            using (var client = new HttpClient())
            {
                string html = await client.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                
                var htmlCollection = doc.DocumentNode.SelectSingleNode("//h5[@class='title is-5']");
                var titleCollection = htmlCollection.SelectNodes("//img");
                //Logo img
                var titleCollectionFiltered = titleCollection.Where(x => x.Attributes.Count == 1);
                var countryCode = titleCollectionFiltered.First().Attributes;
                

                if (countryCode["src"].Value.Contains("nl"))
                {
                    return true;
                }
            }
                return false;
        }

        public static bool IsOwner(ulong Id)
        {
            if (Id == 138439306774577152)
            {
                return true;
            }
            return false;
        }

        public static bool IsDutchAdmin(this SocketGuildUser user) // Staff ID : 505486321595187220
        {
            foreach (var role in user.Roles)
            {
                if (role.Id == 505486321595187220)
                {
                    return true;
                }
            }
            return false;
        }
    }
}