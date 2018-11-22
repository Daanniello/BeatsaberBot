using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Discord;
using Discord.WebSocket;

namespace DiscordBeatSaberBot
{
    class SavedMessages
    {
        public List<EmbedBuilder> Embedbuilders;
        public SocketUserMessage message;
        public ulong messageId;
        public string discordId;

        public SavedMessages(SocketUserMessage message)
        {
            Embedbuilders = new List<EmbedBuilder>();
            discordId = message.Author.Username;
            messageId = message.Id;
            this.message = message;


        }

        public void AddEmbed(EmbedBuilder embed)
        {
            Embedbuilders.Add(embed);
        }
    }
}
