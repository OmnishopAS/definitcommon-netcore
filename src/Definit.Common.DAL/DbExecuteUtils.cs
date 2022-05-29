using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Definit.Common.DAL
{
    public static class DbExecuteUtils
    {
        public static T ExecuteScalar<T>(string connectionString, string sql, T defaultValue = default(T))
        {
            using (var connection = new SqlConnection(connectionString))
            {
                return ExecuteScalar(connection, sql, defaultValue);
            }
        }

        public static T ExecuteScalar<T>(DbConnection connection, string sql, T defaultValue = default(T))
        {
            connection.Open();
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return defaultValue;

                        reader.Read();
                        if (reader.IsDBNull(0))
                            return defaultValue;

                        return (T)reader.GetValue(0);
                    }
                }

            }
            finally
            {
                connection.Close();
            }
        }

        public static DataTable GetDataTable(DbConnection connection, string sql)
        {
            var initialState = connection.State;
            if(initialState!=ConnectionState.Open)
            {
                connection.Open();
            }

            try
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Connection = connection;
                    var dt = new DataTable();
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                        return dt;
                    }
                }

            }
            finally
            {
                if(initialState==ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
        }
    }
}
