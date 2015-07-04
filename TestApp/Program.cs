using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger;
using Core;
using Interfaces;
using Client;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ILogger log = new ConsoleLogger();
            ServiceManager m = new ServiceManager(log);
            m.Initialize();
            Console.ReadKey();
            ITestService s = new ClientBuilder<ITestService>(log, "TestService").Proxy;
            Console.WriteLine(s.GetString());
            Console.ReadKey();
            m.Deinitialize();
        }
    }
}
