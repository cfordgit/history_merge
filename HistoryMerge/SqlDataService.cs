using System;
using System.Data.SQLite;
using Microsoft.Extensions.Configuration;

namespace HistoryMerge
{
    class SqlDataService
    {
        public SQLiteConnection connection;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public SqlDataService(IConfiguration config, ILogger logger)
        {
            this._config = config;
            this._logger = logger;
        }

        public SQLiteConnection CreateConnection()
        {
            try
            {
                connection = new SQLiteConnection(_config.GetConnectionString("SqlConnectionString"));
                connection.Open();
            } catch (Exception ex)
            {
                _logger.Log("Unable to connect to database: " + ex.Message);
            }

            return connection;
        }

        public object ExecuteScalar(string commandText)
        {
            Object returnValue = null;
            SQLiteConnection conn = null;
            try
            {
                using (conn = CreateConnection())
                {
                    SQLiteCommand cmd = new SQLiteCommand(commandText, conn);
                    returnValue = cmd.ExecuteScalar();
                }
            } catch (Exception ex)
            {
                _logger.Log("Error executing command: " + ex.Message);
                return null;
            }

            return returnValue;
        }

        public SQLiteDataReader ExecuteReader(SQLiteConnection conn, string commandText)
        {
            SQLiteDataReader dataReader = null;
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(commandText, conn);
                dataReader = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                _logger.Log("Error executing reader: " + ex.Message);
            }

            return dataReader;
        }
    }
}
