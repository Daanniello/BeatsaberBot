using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBeatSaberBot
{
    static class CommandHelper
    {
        private static List<string> helpCommands = new List<string>();

        private static string prefix = "Prefix = !bs";
        private static string top10 = "top10 => (Gets the top 10 players)";
        private static string searchPlayer = "Search [Username] => (Gets information about [Username])";
        private static string searchSongs = "Songs [Songname] => (gives all available songs with information)";
        private static string topSong = "topsong => (gets the current most played song and shows the top 10 players)";
        private static string ranks = "ranks => (gets all ranks with colors and range)";
        private static string invitationLink = "invite => (gets the invitationLink for the Beatsaber bot)";

        static CommandHelper()
        {
            helpCommands.Add(prefix);
            helpCommands.Add(top10);
            helpCommands.Add(searchPlayer);
            helpCommands.Add(searchSongs);
            helpCommands.Add(topSong);
            helpCommands.Add(ranks);
            helpCommands.Add(invitationLink);
        }

        public static List<string> Help()
        {
            return helpCommands;
        }
    }
}
