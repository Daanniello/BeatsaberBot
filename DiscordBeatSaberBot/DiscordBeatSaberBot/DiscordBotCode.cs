using System;
using System.IO;

namespace DiscordBeatSaberBot
{
    internal static class DiscordBotCode
    {
        public static string discordBotCode = ReadFromDesktop("BotCode.txt");

        private static string ReadFromDesktop(string fileName)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fullName = Path.Combine(desktopPath, fileName);
            using (var steamReader = new StreamReader(fullName))
            {
                return steamReader.ReadToEnd();
            }
        }
    }
}