using System;
using System.Collections.Generic;
using Discord;
using System.Text;

namespace DiscordBeatSaberBot
{
    static class Rank
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

        static public Color GetRankColor(int rank)
        {
            if (rank <= 10)
            {
                return Master;
            }
            else if (rank <= 500)
            {
                return Challenger;
            }else if (rank <= 5000)
            {
                return Diamond;
            }
            if (rank <= 10000)
            {
                return Platinum;
            }
            else if (rank <= 40000)
            {
                return Gold;
            }
            if (rank <= 80000)
            {
                return Silver;
            }

            return bronze;
        }
    }
}
