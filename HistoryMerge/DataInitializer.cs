using System;
using System.Data.SQLite;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace HistoryMerge
{
    class DataInitializer
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public DataInitializer(IConfiguration config, ILogger logger)
        {
            this._config = config;
            this._logger = logger;
        }

        public void Init()
        {
            SQLiteConnection.CreateFile(_config.GetSection("ConnectionStrings").GetSection("DatabaseName").Value);
            SqlDataService dataService = new SqlDataService(_config, _logger);

            try
            {
                _logger.Log("Creating data tables...");
                dataService.ExecuteScalar(File.ReadAllText("DBInit.sql"));
                _logger.Log("Data tables created successfully");
                _logger.Log("Populating data...");
                dataService.ExecuteScalar(File.ReadAllText("DBPopulate.sql"));
                _logger.Log("Data populated successfully");
            } catch (Exception ex)
            {
                _logger.Log("Error initializing data: " + ex.Message);
            }
        }
    }
}
