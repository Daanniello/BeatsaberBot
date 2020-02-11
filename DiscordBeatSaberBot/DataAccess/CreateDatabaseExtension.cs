using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace DiscordBeatSaberBot.DataAccess
{
    public class CreateDatabaseExtension
    {
        string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\daan.smits\Documents\GitHub\BeatsaberBot\DiscordBeatSaberBot\DataAccess\BeatsaberBotDatabase.mdf;Integrated Security=True";

        public void CreateDatabase()
        {
            CreateTableUsers();
        }
        
        public void CreateTableUsers()
        {
            Execute("create table Users (UserId int, UserDiscordId varchar(50), UserScoresaberId varchar(50))");
        }

        public void Execute(string sql)
        {
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
