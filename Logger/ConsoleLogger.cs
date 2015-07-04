using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public class ConsoleLogger: ILogger
    {
        public void TraceMessage(string message)
        {
            DateTime dt = DateTime.Now;
            Console.WriteLine(string.Format("{0:dd/MM/yyyy HH:mm:ss}{1}{2}", dt, Environment.NewLine, message));
        }

        public void TraceException(Exception ex)
        {
            DateTime dt = DateTime.Now;
            Console.WriteLine(string.Format("{0:dd/MM/yyyy HH:mm:ss}{1}Exception: {2}{3}Stack Trace: {4}", dt, Environment.NewLine, ex.Message, Environment.NewLine, ex.StackTrace));
        }
    }
}
