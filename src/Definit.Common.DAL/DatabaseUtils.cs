using System;
using System.Data.SqlClient;

namespace Definit.Common.DAL
{
    public class DatabaseUtils
    {
        public static string GetLatestDatabaseMigrationSafe(string connectionString)
        {
            if (!CheckDatabaseExists(connectionString))
                return null;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT OBJECT_ID('dbo.__EFMigrationsHistory', 'U')", connection))
                {
                    if (cmd.ExecuteScalar() == DBNull.Value)
                        return null;
                }              

                using (SqlCommand cmd = new SqlCommand(@"SELECT TOP 1 MigrationId FROM [__EFMigrationsHistory] ORDER BY MigrationId DESC", connection))
                {
                    var lastMigrationValue = cmd.ExecuteScalar();
                    if (lastMigrationValue == DBNull.Value)
                        return null;
                    return (string)lastMigrationValue;
                }
            }
        }

        public static bool CheckDatabaseExists(string connectionString)
        {
            var connBuilder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = connBuilder.InitialCatalog;
            connBuilder.InitialCatalog = "master";

            using (var connection = new SqlConnection(connBuilder.ToString()))
            {
                connection.Open();
                using (var command = new SqlCommand($"SELECT db_id('{databaseName}')", connection))
                {
                    return (command.ExecuteScalar() != DBNull.Value);
                }
            }
        }
    }
}
