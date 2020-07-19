using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;

namespace DiscordBeatSaberBot
{
    static class DatabaseContext
    {

        public static List<List<object>> ExecuteSelectQuery(string query)
        {
            List<List<object>> items = new List<List<object>>();

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["BeatSaberDiscordBot"].ToString()))
            {
                SqlCommand oCmd = new SqlCommand(query, connection);
                connection.Open();
                try
                {
                    using (SqlDataReader reader = oCmd.ExecuteReader())
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

        public static bool ExecuteInsertQuery(string query)
        {

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["BeatSaberDiscordBot"].ToString()))
            {
                SqlCommand oCmd = new SqlCommand(query, connection);
                connection.Open();
                try
                {
                    oCmd.ExecuteNonQuery();
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

        public static bool ExecuteRemoveQuery(string query)
        {

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["BeatSaberDiscordBot"].ToString()))
            {
                SqlCommand oCmd = new SqlCommand(query, connection);
                connection.Open();
                try
                {
                    oCmd.ExecuteNonQuery();
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
