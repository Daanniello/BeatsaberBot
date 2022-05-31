using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBeatSaberBot
{
    static class GlobalConfiguration
    {
        public static string inviteLink = "https://discord.com/api/oauth2/authorize?client_id=504633036902498314&permissions=277025508416&redirect_uri=http%3A%2F%2Fbeatsaberbot.com%2F&response_type=code&scope=bot%20applications.commands%20messages.read";

        public static string BotImageStoragePath = $"C:/Users/DaanS/source/repos/BeatSaberBotWeb/BeatSaberBotWeb/wwwroot/img/BeatSaberBot/";

        public static string BotImageStorageLink = "http://beatsaberbot.com/img/BeatSaberBot/";

        public static int TypingTimeOut = 10;

        public static string WebsiteRoot = @"C:/Users/DaanS/source/repos/BeatSaberBotWeb/BeatSaberBotWeb/wwwroot/";

    }
}
