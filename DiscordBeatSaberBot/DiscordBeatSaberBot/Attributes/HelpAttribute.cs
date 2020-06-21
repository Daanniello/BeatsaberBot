using System;
using System.Collections.Generic;

namespace DiscordBeatSaberBot
{
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class HelpAttribute : Attribute
    {

        private string helpCommand = "";
        private string helpInfo = "";
        private Catergories helpCategory = Catergories.General;
        private string helpUsage = "";

        public HelpAttribute(string helpCommand, string helpInfo, string helpUsage, Catergories helpCategory)
        {
            this.helpCommand = helpCommand;
            this.helpInfo = helpInfo;
            this.helpCategory = helpCategory;
            this.helpUsage = helpUsage;
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
