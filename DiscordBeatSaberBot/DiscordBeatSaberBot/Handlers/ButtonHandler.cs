using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot.Handlers
{
    public class ButtonHandler
    {
        public async Task HandleButton(DiscordSocketClient discord, SocketMessageComponent component)
        {
            //Help Command Pages--------------------------------------------------------------------------------
            if (component.Data.CustomId == "rightHelpButton")
            {
                var message = component.Message;                
               
                var usedEmbed = message.Embeds.First();
                var pagenr = usedEmbed.Title.Split("[")[1].Split("]")[0];

                var currentNr = int.Parse(pagenr.Split("/")[0]);
                var maxNr = int.Parse(pagenr.Split("/")[1]);

                if (currentNr >= maxNr) return;

                await message.ModifyAsync(msg => msg.Embed = Help.GetHelpList(discord, int.Parse(pagenr.Split("/").First())));
            }

            if (component.Data.CustomId == "leftHelpButton")
            {

                var message = component.Message;
                var usedEmbed = message.Embeds.First();
                var pagenr = usedEmbed.Title.Split("[")[1].Split("]")[0];

                var currentNr = int.Parse(pagenr.Split("/")[0]);

                if (currentNr <= 0) return;

                await message.ModifyAsync(msg => msg.Embed = Help.GetHelpList(discord, int.Parse(pagenr.Split("/").First()) - 2));
            }
            await component.RespondAsync();
        }
    }
}
