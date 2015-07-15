using Client;
using Common;
using Configuration;
using Encryption;
using Interfaces.DataContracts;
using Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AuditService: IAuditService
    {
        string publicKey;
        string privateKey;
        List<DeviceCredentials> PairedDevices;
        Device CurrentDevice;

        public AuditService()
        {
            AsymmetricEncryption.GenerateKeys(Constants.KEY_SIZE, out publicKey, out privateKey);
            ConfigManager.Configuration.PublicKey = publicKey;
            ConfigManager.Configuration.PrivateKey = privateKey;
            PairedDevices = ConfigManager.Configuration.PairedDevices;
            CurrentDevice = ConfigManager.Configuration.CurrentDevice;

            SetPairedDeviceKey(CurrentDevice, publicKey);

            ExchangeKeysWithPairedDevices();
        }

        private void ExchangeKeysWithPairedDevices()
        {
            foreach(DeviceCredentials dc in PairedDevices)
            {
                if (!dc.PairedDevice.Equals(CurrentDevice))
                {
                    IAuditService s = new ClientBuilder<IAuditService>("AuditService", dc.PairedDevice.Port, dc.PairedDevice.Host).Proxy;
                    if (s != null)
                    {
                        string pKey = s.ExchangeKeys(CurrentDevice, publicKey);
                        dc.PublicKey = pKey;
                    }
                }
            }
            ConfigManager.Configuration.Save();
        }

        public string ExchangeKeys(Device device, string pubKey)
        {
            string ret = publicKey;
            SetPairedDeviceKey(device, pubKey);
            return ret;
        }

        private void SetPairedDeviceKey(Device device, string pubKey)
        {
            bool found = false;
            foreach(DeviceCredentials c in PairedDevices)
            {
                if (c.PairedDevice.Equals(device))
                {
                    found = true;
                    c.PublicKey = pubKey;
                    break;
                }
            }

            if (!found)
            {
                DeviceCredentials dc = new DeviceCredentials();
                dc.PairedDevice = device;
                dc.Authorized = false;
                dc.PublicKey = pubKey;

                if (device == CurrentDevice && publicKey == pubKey)
                {
                    dc.Password = Membership.GeneratePassword(Constants.PWD_LENGTH, Constants.NON_ALPHANUMERIC_CHARS_CNT);
                    dc.Authorized = true;
                }
                PairedDevices.Add(dc);
            }

            ConfigManager.Configuration.Save();
        }
    }
}
