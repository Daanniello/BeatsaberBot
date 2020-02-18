using System;
using System.Collections.Generic;

namespace DiscordBeatSaberBot
{
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    class HelpAttribute : Attribute
    {

        private string helpCommand = "";
        private string helpInfo = "";

        public HelpAttribute(string helpCommand, string helpInfo)
        {
            this.helpCommand = helpCommand;
            this.helpInfo = helpInfo;
        }
    }
}
