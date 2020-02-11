using System;
using System.Data.SqlClient;
using System.Linq;
using DiscordBeatSaberBot.DataAccess.Models;
using DiscordBeatSaberBot.DataAccess.Tables;
using Microsoft.EntityFrameworkCore;

namespace DiscordBeatSaberBot.DataAccess
{
    public static class DatabaseExtension 
    {
        //Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\daan.smits\Documents\GitHub\BeatsaberBot\DiscordBeatSaberBot\DataAccess\BeatsaberBotDatabase.mdf;Integrated Security=True
        public static void ConnectToDatabase()
        {
            using (var db = new UserTable())
            {
                //var scoresaberUser = new ScoresaberUserModel
                //{
                //    Username = "SilverhazeTest1",
                //    ScoresaberId = 1234567
                //};
                //var discordUser = new DiscordUserModel();

                //var User = new UserModel();
                //User.ScoresaberUser = scoresaberUser;
                //User.DiscordUser = discordUser;

                //db.Users.Add(User);
                var createTableUsers = new CreateDatabaseExtension();
                createTableUsers.CreateDatabase();
            }
        }
    }
}
