using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DiscordBeatSaberBot
{
    static class DiscordBotCode
    {
        public static string discordBotCode = ReadFromDesktop("BotCode.txt");

        private static string ReadFromDesktop(string fileName)
        {
   
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fullName = System.IO.Path.Combine(desktopPath, fileName);
            using (StreamReader steamReader = new StreamReader(fullName))
            {
                return steamReader.ReadToEnd();
            }
        }
    }
}
