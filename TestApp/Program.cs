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
            Console.ReadKey();

            Device d = Configuration.ConfigManager.Configuration.CurrentDevice;

            IAuditService s = new ClientBuilder<IAuditService>("AuditService").Proxy;

            string pubKey = s.ExchangeKeys(d, "1213445");

            Token t = new Token();
            t.DeviceID = d.ID;
            t.Password = "ololololshto111";

            string coded = AsymmetricEncryption.EncryptCredentials(t, Constants.KEY_SIZE, Configuration.ConfigManager.Configuration.PublicKey);

            Token decoded = AsymmetricEncryption.DecryptCredentials(coded, Constants.KEY_SIZE, Configuration.ConfigManager.Configuration.PrivateKey);

            Console.ReadKey();
            m.Deinitialize();
        }
    }
}
