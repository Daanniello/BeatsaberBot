using System;
using System.Collections.Generic;
using System.Linq;
using DiscordBeatSaberBot.Commands;

namespace DiscordBeatSaberBot
{
    public class CommandHelpProperty
    {
        public string CommandName = "";
        public string CommandInfo = "";
        public HelpAttribute.Catergories CommandCatergory = HelpAttribute.Catergories.General;
        public string CommandUsage = "";
    }

    public class ReflectionExtension
    {
        static public Dictionary<string, CommandHelpProperty> GetCustomHelpAttributeListFromClass<T>(T Class)
        {
            var memberInfo = Class.GetType();
            var methods = memberInfo.GetMethods();

            var HelpCommandList = new Dictionary<string, CommandHelpProperty>();

            foreach (var method in methods)
            {
                var attribute = method.CustomAttributes.Where(x => x.AttributeType == typeof(HelpAttribute)).FirstOrDefault();
                if (attribute != null)
                {
                    HelpCommandList.Add(attribute.ConstructorArguments[0].ToString().TrimStart('"').TrimEnd('"'), new CommandHelpProperty()
                    {
                        CommandName = attribute.ConstructorArguments[0].ToString().TrimStart('"').TrimEnd('"'),
                        CommandCatergory = (HelpAttribute.Catergories)Enum.GetValues(typeof(HelpAttribute.Catergories)).GetValue((int)attribute.ConstructorArguments[3].Value),
                        CommandInfo = attribute.ConstructorArguments[1].ToString().TrimStart('"').TrimEnd('"'),
                        CommandUsage = attribute.ConstructorArguments[2].ToString().Trim()
                    });
                }

                //HelpCommandList.Add(attribute.ConstructorArguments[0].ToString().TrimStart('"').TrimEnd('"') + " (" + Enum.GetValues(typeof(HelpAttribute.Catergories)).GetValue((int)attribute.ConstructorArguments[3].Value) + ")", attribute.ConstructorArguments[1].ToString().TrimStart('"').TrimEnd('"'));
            }



            return HelpCommandList;
        }

        static public Dictionary<string, string> GetCustomHelpAttributeListFromClass<T>(Type Class)
        {
            var memberInfo = Class;
            var methods = memberInfo.GetMethods();

            var HelpCommandList = new Dictionary<string, string>();

            foreach (var method in methods)
            {
                var attribute = method.CustomAttributes.Where(x => x.AttributeType == typeof(HelpAttribute)).FirstOrDefault();
                if (attribute != null) HelpCommandList.Add(attribute.ConstructorArguments[0].ToString() + " (" + Enum.GetValues(typeof(HelpAttribute.Catergories)).GetValue((int)attribute.ConstructorArguments[2].Value) + ")", attribute.ConstructorArguments[1].ToString());
            }

            return HelpCommandList;
        }

        static public Dictionary<string, CommandHelpProperty> GetAllCustomHelpAttributes()
        {
            var HelpList = new Dictionary<string, CommandHelpProperty>();

            var DSM = GetCustomHelpAttributeListFromClass(new DutchServerCommands());
            var GSM = GetCustomHelpAttributeListFromClass(new GlobalScoresaberCommands());
            var GM = GetCustomHelpAttributeListFromClass(new GenericCommands());

            DSM.ToList().ForEach(x => HelpList.Add(x.Key, x.Value));
            GSM.ToList().ForEach(x => HelpList.Add(x.Key, x.Value));
            GM.ToList().ForEach(x => HelpList.Add(x.Key, x.Value));

            return HelpList;
        }
    }
}