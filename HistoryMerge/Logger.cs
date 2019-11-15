using Microsoft.Extensions.Configuration;
using System;

namespace HistoryMerge
{
    public class Logger : ILogger
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public Logger(IConfiguration config)
        {
            _config = config;
            if (_config.GetSection("Logger").Exists())
            {
                switch(_config.GetSection("Logger").Value.ToUpper())
                {
                    case "CONSOLELOGGER":
                        if (_logger == null)
                            _logger = new ConsoleLogger();
                        break;
                }
            }
        }

        public void Log(String message)
        {
            _logger.Log(message);
        }
    }
}
