using System;
using System.Collections.Generic;

namespace DiscordBeatSaberBot
{
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    class HelpAttribute : Attribute
    {

        private string helpCommand = "";
        private string helpInfo = "";
        private Catergories helpCategory = Catergories.General;

        public HelpAttribute(string helpCommand, string helpInfo, Catergories helpCategory)
        {
            this.helpCommand = helpCommand;
            this.helpInfo = helpInfo;
            this.helpCategory = helpCategory;
        }

        public enum Catergories
        {
            General,
            BotFunctions,
            AdminCommands,
            DutchFunctions
        }
    }
}
