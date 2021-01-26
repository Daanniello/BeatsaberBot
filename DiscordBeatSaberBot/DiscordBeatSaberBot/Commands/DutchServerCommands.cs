using System;
using System.Collections.Generic;
using System.Configuration;
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
        [Help("RoleColor", "If you have the Dutch 'verslaafd' role, you can chance the color of it.", "!bs rolecolor (#ffffff)", HelpAttribute.Catergories.DutchFunctions)]
        static public async Task RoleColor(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var moderationHelper = new GuildService(discordSocketClient, 505485680344956928);
            if (await moderationHelper.UserHasRole(message.Author, "Staff") || await moderationHelper.UserHasRole(message.Author, "Verslaafd"))
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

        [Help("UpdateRoles", "Update roles from everyone in the Dutch beat saber discord", "!bs updateroles", HelpAttribute.Catergories.AdminCommands)]
        static public async Task UpdateRoles(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (await message.Author.IsDutchAdmin(discordSocketClient))
            {
                new Thread(async () =>
                {
                    await UpdateDiscordBeatsaberRanksNL.UpdateNLAsync(discordSocketClient, message);
                    await message.Channel.SendMessageAsync("Done");
                }).Start();
            }
            else
            {
                await message.Channel.SendMessageAsync("You are not allowed to use this command.");
            }
        }

        [Help("ChangeColor", "If you have the Dutch 'verslaafd' role, you can change the color of it.", "!bs changecolor (#ffffff)", HelpAttribute.Catergories.DutchFunctions)]
        static public async Task ChangeColor(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (true)
                await message.Channel.SendMessageAsync("", false, new EmbedBuilder
                {
                    Title = "Je bent niet gemachtigd om je kleur aan te passen.",
                    Description = "Win het weekelijkse 'meeste uren event' om deze functie te krijgen"
                }.Build());
        }

        [Help("IRLevent", "Creates and IRL Event for the Dutch discord.", "!bs irlevent", HelpAttribute.Catergories.AdminCommands)]
        static public async Task IRLevent(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (await message.HasCertainRoleInNBSG(discordSocketClient, 711342955776049194))
            {
                var embedBuilder = EmbedBuilderExtension.NullEmbed("IRL Event handler", "Starting IRL Event handler...", null, null);
                var msg = await message.Channel.SendMessageAsync("", false, embedBuilder.Build());


                var irlEventHandler = new IRLeventHandler(message, discordSocketClient, msg);
            }
        }

        [Help("RandomEvent", "Creates and Random Event for the Dutch discord.", "!bs randomevent", HelpAttribute.Catergories.AdminCommands)]
        static public async Task RandomEvent(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (await message.HasCertainRoleInNBSG(discordSocketClient, 711342955776049194, 505486321595187220))
            {
                var embedBuilder = EmbedBuilderExtension.NullEmbed("Random Event Generator", "Starting random event handler...", null, null);
                var msg = await message.Channel.SendMessageAsync("", false, embedBuilder.Build());


                var randomEventHandler = new RandomEventHandler(message, discordSocketClient, msg);
            }
        }

        [Help("Link", "Will link your ScoreSaber profile to your Discord account ", "!link (ScoreSaberID)", HelpAttribute.Catergories.General)]
        static public async Task LinkScoreSaberWithDiscord(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            var moderationHelper = new GuildService(discordSocketClient, 505485680344956928);


            if (await r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()))
            {
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Pog", $"Your Discord ID is already linked with your ScoreSaber, No worries {message.Author.Username}. If you want to unlink, type !bs unlink.").Build());

                await message.Channel.SendMessageAsync("Your Discord ID is already linked with your ScoreSaber, No worries " + message.Author.Username);
                return;
            }
            else
            {
                var ScoreSaberId = message.Content.Substring(9);
                ScoreSaberId = Regex.Replace(ScoreSaberId, "[^0-9]", "");

                if (!ValidationExtension.IsDigitsOnly(ScoreSaberId))
                {
                    await message.Channel.SendMessageAsync("ScoreSaber ID is wrong");
                    return;
                }

                if (await ValidationExtension.IsDutch(ScoreSaberId))
                {
                    var guildChannel = message.Channel as SocketGuildChannel;
                    if (guildChannel.Guild.Id != 505485680344956928)
                    {
                        await ProcessNonDutch(ScoreSaberId);
                        return;
                    }
                    await ProcessDutch(ScoreSaberId);
                    return;
                }
                else if (!await ValidationExtension.IsDutch(ScoreSaberId))
                {
                    await ProcessNonDutch(ScoreSaberId);
                    return;
                }
                else
                {
                    await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Error", "Your account does not exist on the ScoreSaber API. It might be because it is new. Try again later.", null, null).Build());
                }
            }

            async Task ProcessDutch(string ScoreSaberId)
            {
                var guildChannel = message.Channel as SocketGuildChannel;
                if (guildChannel == null) message.Channel.SendMessageAsync("Looks like you use this command in DM. This command does not work in DM. Consider joining the Dutch Beat Saber Discord. (https://discord.gg/cH7mTyq)");
                if (guildChannel.Guild.Id != 505485680344956928)
                {
                    message.Channel.SendMessageAsync("It seems that you are Dutch and trying to link your account outside the Dutch Discord. A Dutch request needs to be validated. Consider joining the Dutch Beat Saber Discord. (<https://discord.gg/cH7mTyq>)");
                    return;
                }

                await new WelcomeInterviewHandler(discordSocketClient, message.Channel, message.Author.Id).AskForInterview();

                r.MakeRequest(message, 505485680344956928, 549350982081970176);

                var user = message.Author;
                if (await moderationHelper.UserHasRole(user, "Nieuwkomer"))
                {
                    await moderationHelper.AddRole("Unverified", user);
                    await moderationHelper.DeleteRole("Nieuwkomer", user);
                }
            }

            async Task ProcessNonDutch(string ScoreSaberId)
            {
                DatabaseContext.ExecuteInsertQuery($"Insert into Player (ScoreSaberId, DiscordId) values ({ScoreSaberId}, {message.Author.Id.ToString()})");

                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Added user to the list", "Added " + message.Author.Id.ToString() + " with ScoreSaberID " + ScoreSaberId + " to the global list", null, null).Build());

                var guildChannel = message.Channel as SocketGuildChannel;
                if (guildChannel == null) await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Error", "This command can not be done in DM", null, null).Build());
                if (guildChannel.Guild.Id == 505485680344956928)
                {
                    await moderationHelper.AddRole("Foreign channel", message.Author);
                    await moderationHelper.DeleteRole("Nieuwkomer", message.Author);
                }
            }
        }

        [Help("Unlink", "Will unlink your ScoreSaber profile from your Discord account ", "!unlink", HelpAttribute.Catergories.General)]
        static public async Task UnLinkScoreSaberFromDiscord(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);
            var discordId = message.Author.Id.ToString();
            if (await r.CheckIfDiscordIdIsLinked(discordId))
            {
                await r.UnlinkAccount(discordId);
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Succesfully Unlinked", $"Your discordId {discordId} is now unlinked from your ScoreSaber ID").Build());
                return;
            }
            else
            {
                await message.Channel.SendMessageAsync("", false, EmbedBuilderExtension.NullEmbed("Hmmmm?", "Your Discord does not seemed to be linked. You can link a new ScoreSaber account with !bs link [ScoreSaberID]").Build());
            }
        }
    }
}
