using Interfaces.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public static class LoggerManager
    {
        //public static ConsoleLogger ConsoleLog = new ConsoleLogger();
        public static ILogger Log = new ConsoleLogger();
    }
}
