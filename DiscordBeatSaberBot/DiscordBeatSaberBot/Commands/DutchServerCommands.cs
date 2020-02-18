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
        [Help("RoleColor", "If you have the dutch 'verslaafd' role, you can chance the color of it.")]
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

        [Help("UpdateRoles", "Update roles from everyone in the dutch beat saber discord")]
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

        [Help("LinkedNames", "Returns a list of all users linked in the dutch beat saber discord.")]
        static public async Task LinkedNames(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (new GuildService(discordSocketClient, 505485680344956928).IsStaffInGuild(message.Author.Id, 505486321595187220))
            {
                var embed = new RoleAssignment(discordSocketClient).GetLinkedDiscordNamesEmbed();
                await message.Channel.SendMessageAsync(embed);
            }
        }

        [Help("NotLinkedNames", "Returns a list of all users that are not linked in the dutch beat saber discord.")]
        static public async Task NotLinkedNames(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (new GuildService(discordSocketClient, 505485680344956928).IsStaffInGuild(message.Author.Id, 505486321595187220))
            {
                var embed = new RoleAssignment(discordSocketClient).GetNotLinkedDiscordNamesInGuildEmbed(505485680344956928);
                await message.Channel.SendMessageAsync(embed);
            }
        }

        [Help("ChangeColor", "If you have the dutch 'verslaafd' role, you can chance the color of it.")]
        static public async Task ChangeColor(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (true)
                await message.Channel.SendMessageAsync("", false, new EmbedBuilder
                {
                    Title = "Je bent niet gemachtigt om je kleur aan te passen.",
                    Description = "Win het weekelijkse 'meeste uren event' om deze functie te krijgen"
                }.Build());
        }

        [Help("CreateEvent", "Creates a handler that guides you for creating an event in the dutch beat saber group.")]
        static public async Task CreatEvent(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var thread = new Thread(() => new EventManager(message, discordSocketClient));
            thread.Start();
        }

        [Help("ResetDutchHours", "Reset the hours from the weekly event in the dutch beat saber discord. ")]
        static public async Task ResetDutchHours(DiscordSocketClient discordSocketClient, SocketMessage message, BeatSaberHourCounter DutchHourCounter)
        {
            DutchHourCounter.InsertAndResetAllDutchMembers(discordSocketClient);
            await message.Channel.SendMessageAsync("Reset completed");
        }

        [Help("TopDutchHours", "Reset the hours from the weekly event in the dutch beat saber discord. ")]
        static public async Task TopDutchHours(DiscordSocketClient discordSocketClient, SocketMessage message, BeatSaberHourCounter DutchHourCounter)
        {
            await message.Channel.SendMessageAsync("", false, DutchHourCounter.GetTop25BeatSaberHours());
        }

        [Help("IRLevent", "Creates and IRL Event for the dutch discord.")]
        static public async Task IRLevent(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            if (ValidationExtension.IsOwner(message.Author.Id))
            {
                var embedBuilder = EmbedBuilderExtension.NullEmbed("IRL Event handler", "Starting IRL Event handler...", null, null);
                var msg = await message.Channel.SendMessageAsync("", false, embedBuilder.Build());


                var irlEventHandler = new IRLeventHandler(message, discordSocketClient, msg);
            }
        }



        [Help("RequestVerification", "Reset the hours from the weekly event in the dutch beat saber discord. ")]
        static public async Task RequestVerification(DiscordSocketClient discordSocketClient, SocketMessage message)
        {
            var r = new RoleAssignment(discordSocketClient);

            if (r.CheckIfDiscordIdIsLinked(message.Author.Id.ToString()))
            {
                await message.Channel.SendMessageAsync("Your Discord ID is already linked with your scoresaber, No worries " + message.Author.Username);
                return;
            }
            else
            {
                var ScoresaberId = message.Content.Substring(24);
                ScoresaberId = Regex.Replace(ScoresaberId, "[^0-9]", "");

                if (!ValidationExtension.IsDigitsOnly(ScoresaberId))
                {
                    await message.Channel.SendMessageAsync("Scoresaber ID is wrong");
                    return;
                }

                if (await ValidationExtension.IsDutch(ScoresaberId))
                {
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
