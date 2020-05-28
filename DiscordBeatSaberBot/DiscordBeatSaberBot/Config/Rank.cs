using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;

namespace DiscordBeatSaberBot
{
    internal static class Rank
    {
        public static Color Master = Color.Red;
        public static Color Challenger = Color.Magenta;
        public static Color Diamond = Color.Blue;
        public static Color Platinum = Color.Teal;
        public static Color Gold = Color.Gold;
        public static Color Silver = Color.LighterGrey;
        public static Color bronze = Color.DarkOrange;

        public static int rankMaster = 10;
        public static int rankChallenger = 500;
        public static int rankDiamond = 5000;
        public static int rankPlatinum = 10000;
        public static int rankGold = 40000;
        public static int rankSilver = 80000;

        public static async Task<Color> GetRankColor(int rank)
        {
            if (rank <= 10)
                return Master;
            if (rank <= 500)
                return Challenger;
            if (rank <= 5000)
                return Diamond;
            if (rank <= 10000)
                return Platinum;
            if (rank <= 40000)
                return Gold;
            if (rank <= 80000)
                return Silver;

            return bronze;
        }
    }
}