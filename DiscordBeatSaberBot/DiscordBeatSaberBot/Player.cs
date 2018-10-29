using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBeatSaberBot
{
    class Player
    {
        public string name { get; set; }
        public int rank { get; set; }
        public int countryRank { get; set; }

        public string countryName { get; set; }

        public string countryIcon { get; set; }

        public int playCount { get; set; }
        public string totalScore { get; set; }

        public string pp { get; set; }

        public string steamLink { get; set; }

        public string imgLink { get; set; }
    }
}
