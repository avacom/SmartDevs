using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger;
using Core;
using Interfaces;
using Client;
using Interfaces.Common;
using Interfaces.Services;
using Interfaces.DataContracts;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceManager m = new ServiceManager();
            m.Initialize();
            Console.ReadKey();

            Device d = new Device();
            d.Host = "xtremelabs.org";
            d.Port = "9876";

            IAuditService s = new ClientBuilder<IAuditService>("AuditService").Proxy;

            string pubKey = s.ExchangeKeys(d, "1213445");

            Console.ReadKey();
            m.Deinitialize();
        }
    }
}
