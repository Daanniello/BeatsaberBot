using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    static class DatabaseContext
    {

        public static async Task<List<List<object>>> ExecuteSelectQuery(string query)
        {
            List<List<object>> items = new List<List<object>>();

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["BeatSaberDiscordBot"].ToString()))
            {
                SqlCommand oCmd = new SqlCommand(query, connection);
                connection.Open();
                try
                {
                    using (SqlDataReader reader = await oCmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var item = new List<Object>();
                            items.Add(item);

                            for (int i = 0; i < reader.FieldCount; i++)
                                item.Add(reader[i]);
                        }

                    }
                }
                catch (Exception ex)
                {
                    connection.Close();
                    return null;
                }

                connection.Close();

                return items;
            }
        }

        public static async Task<bool> ExecuteInsertQuery(string query)
        {

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["BeatSaberDiscordBot"].ToString()))
            {
                SqlCommand oCmd = new SqlCommand(query, connection);
                connection.Open();
                try
                {
                    await oCmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    connection.Close();
                    return false;
                }

                connection.Close();
                return true;
            }
        }

        public static async Task<bool> ExecuteRemoveQuery(string query)
        {

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["BeatSaberDiscordBot"].ToString()))
            {
                SqlCommand oCmd = new SqlCommand(query, connection);
                connection.Open();
                try
                {
                    await oCmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    connection.Close();
                    return false;
                }

                connection.Close();
                return true;
            }
        }

    }
}
