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
using Encryption;
using Common;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceManager m = new ServiceManager();
            m.Initialize();
            AuditHelper h = new AuditHelper();
            h.RegisterDevice("Sangreal", "6789");
            Console.ReadKey();
            m.Deinitialize();
        }
    }
}
