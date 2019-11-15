using System;
using System.Collections.Generic;
using System.Text;

namespace HistoryMerge
{
    class ConsoleLogger : ILogger
    {
        public void Log(String message)
        {
            Console.WriteLine(message);
        }
    }
}
