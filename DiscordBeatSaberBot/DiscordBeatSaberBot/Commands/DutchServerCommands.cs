using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBeatSaberBot.Handlers;
using Color = Discord.Color;

namespace DiscordBeatSaberBot.Commands
{
    class DutchServerCommands : ICommand
    {
        [Help("RoleColor", "If you have the dutch 'verslaafd' role, you can chance the color of it.", "!bs rolecolor (#ffffff)",HelpAttribute.Catergories.DutchFunctions)]
        static public async Task RoleColor(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var moderationHelper = new GuildService(discordSocketClient, 505485680344956928);
            if (moderationHelper.UserHasRole(message.Author, "Staff") || moderationHelper.UserHasRole(message.Author, "Verslaafd"))
            {
                var roles = discordSocketClient.GetGuild(505485680344956928).Roles;
                var role = roles.FirstOrDefault(r => r.Name == "Verslaafd");
                try
                {
                    var hexcode = message.Content.Split(" ")[2].Trim();
                    var colorConverter = new ColorConverter();
                    dynamic color = colorConverter.ConvertFromString(hexcode);
                    await role.ModifyAsync(r => r.Color = new Color(color.R, color.G, color.B));
                    await message.Channel.SendMessageAsync("Color has been changed");
                }
                catch
                {
                    await message.Channel.SendMessageAsync("Error: Color is not correct");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync("You do not have permission");
            }
        }

        [Help("UpdateRoles", "Update roles from everyone in the dutch beat saber discord", "!bs updateroles", HelpAttribute.Catergories.AdminCommands)]
        static public async Task UpdateRoles(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (message.Author.IsDutchAdmin(discordSocketClient))
            {
                new Thread(async () => {
                    await UpdateDiscordBeatsaberRanksNL.UpdateNLAsync(discordSocketClient, message);
                    await message.Channel.SendMessageAsync("Done");
                }).Start();
            }
            else
            {
                await message.Channel.SendMessageAsync("You are not allowed to use this command.");
            }
        }

        [Help("LinkedNames", "Returns a list of all users linked in the dutch beat saber discord.", "!bs linkednames", HelpAttribute.Catergories.AdminCommands)]
        static public async Task LinkedNames(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (new GuildService(discordSocketClient, 505485680344956928).IsStaffInGuild(message.Author.Id, 505486321595187220))
            {
                var embed = new RoleAssignment(discordSocketClient).GetLinkedDiscordNamesEmbed();
                await message.Channel.SendMessageAsync(embed);
            }
        }

        [Help("NotLinkedNames", "Returns a list of all users that are not linked in the dutch beat saber discord.", "!bs notlinkednames", HelpAttribute.Catergories.AdminCommands)]
        static public async Task NotLinkedNames(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (new GuildService(discordSocketClient, 505485680344956928).IsStaffInGuild(message.Author.Id, 505486321595187220))
            {
                var embed = new RoleAssignment(discordSocketClient).GetNotLinkedDiscordNamesInGuildEmbed(505485680344956928);
                await message.Channel.SendMessageAsync(embed);
            }
        }

        [Help("ChangeColor", "If you have the dutch 'verslaafd' role, you can chance the color of it.", "!bs changecolor (#ffffff)", HelpAttribute.Catergories.DutchFunctions)]
        static public async Task ChangeColor(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (true)
                await message.Channel.SendMessageAsync("", false, new EmbedBuilder
                {
                    Title = "Je bent niet gemachtigt om je kleur aan te passen.",
                    Description = "Win het weekelijkse 'meeste uren event' om deze functie te krijgen"
                }.Build());
        }

        [Help("IRLevent", "Creates and IRL Event for the dutch discord.", "!bs irlevent", HelpAttribute.Catergories.AdminCommands)]
        static public async Task IRLevent(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (message.HasCertainRoleInNBSG(discordSocketClient, 711342955776049194))
            {
                var embedBuilder = EmbedBuilderExtension.NullEmbed("IRL Event handler", "Starting IRL Event handler...", null, null);
                var msg = await message.Channel.SendMessageAsync("", false, embedBuilder.Build());


                var irlEventHandler = new IRLeventHandler(message, discordSocketClient, msg);
            }
        }

        [Help("RandomEvent", "Creates and Random Event for the dutch discord.", "!bs randomevent", HelpAttribute.Catergories.AdminCommands)]
        static public async Task RandomEvent(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (message.HasCertainRoleInNBSG(discordSocketClient, 711342955776049194, 505486321595187220))
            {
                var embedBuilder = EmbedBuilderExtension.NullEmbed("Random Event Generator", "Starting random event handler...", null, null);
                var msg = await message.Channel.SendMessageAsync("", false, embedBuilder.Build());


                var randomEventHandler = new RandomEventHandler(message, discordSocketClient, msg);
            }
        }

        [Help("Unlink", "Will unlink your current Scoresaber from your Discord account ", "!bs unlink", HelpAttribute.Catergories.General)]
        static public async Task UnlinkScoresaberFromDiscord(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);

            if (r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()))
            {
                return;
            }
            else
            {
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Hmmmm?", "Your Discord does not seemed to be already linked. You can link a new scoresaber account with !bs link [ScoresaberID]").Build());
            }
        }

        [Help("Mute", "Will unlink your current Scoresaber from your Discord account ", "!mute (DiscordTag or ID)", HelpAttribute.Catergories.AdminCommands)]
        static public async Task Mute(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (message.Author.IsDutchAdmin(discordSocketClient) || message.Author.IsDutchMod(discordSocketClient))
            {
                var discordId = message.Content.Substring(9).Replace("<@!", "").Replace(">", "");
                new RoleAssignment(discordSocketClient).MutePerson(ulong.Parse(discordId));
                message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("User has been muted", $"{discordId} has been muted").Build());
            }
            else
            {
                message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Validation error", "You have not the rights to use this command.").Build());
            }
        }

        [Help("UnMute", "Will unlink your current Scoresaber from your Discord account ", "!unmute (DiscordTag or ID)", HelpAttribute.Catergories.AdminCommands)]
        static public async Task UnMute(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (message.Author.IsDutchAdmin(discordSocketClient) || message.Author.IsDutchMod(discordSocketClient))
            {
                var discordId = message.Content.Substring(11).Replace("<@!", "").Replace(">", "");
                new RoleAssignment(discordSocketClient).UnMutePerson(ulong.Parse(discordId));
                message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("User has been unmuted", $"{discordId} has been unmuted").Build());
            }
            else
            {
                message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Validation error", "You have not the rights to use this command.").Build());
            }            
        }

        [Help("Link", "Will link your Scoresaber profile to your Discord account ", "!link (ScoresaberID)", HelpAttribute.Catergories.General)]
        static public async Task LinkScoresaberWithDiscord(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);

            if (r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()))
            {
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Pog", $"Your Discord ID is already linked with your scoresaber, No worries {message.Author.Username}. If you want to unlink, type !bs unlink.").Build());

                await message.Channel.SendMessageAsync("Your Discord ID is already linked with your scoresaber, No worries " + message.Author.Username);
                return;
            }
            else
            {
                var ScoresaberId = message.Content.Substring(9);
                ScoresaberId = Regex.Replace(ScoresaberId, "[^0-9]", "");

                if (!ValidationExtension.IsDigitsOnly(ScoresaberId))
                {
                    await message.Channel.SendMessageAsync("Scoresaber ID is wrong");
                    return;
                }

                if (await ValidationExtension.IsDutch(ScoresaberId))
                {
                    var guildChannel = message.Channel as SocketGuildChannel;
                    if (guildChannel.Guild.Id != 505485680344956928)
                    {
                        message.Channel.SendMessageAsync("It seems that you are Dutch and trying to link your account outside the Dutch Discord. A Dutch request needs to be validated. Consider joining the Dutch Beat Saber Discord. (https://discord.gg/cH7mTyq)");
                    }
                    r.MakeRequest(message);
                }
                else
                {
                    JsonExtension.InsertJsonData("../../../GlobalAccounts.txt", message.Author.Id.ToString(), ScoresaberId);
                    await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Added user to the list", "Added " + message.Author.Id.ToString() + " with scoresaberID " + ScoresaberId + " to the global list", null, null).Build());
                    return;
                }
            }

            var moderationHelper = new GuildService(discordSocketClient, 505485680344956928);

            var user = message.Author;
            if (moderationHelper.UserHasRole(user, "Nieuwkomer"))
            {
                await moderationHelper.AddRole("Link my discord please", user);
                await moderationHelper.DeleteRole("Nieuwkomer", user);
            }
        }
    }
}
