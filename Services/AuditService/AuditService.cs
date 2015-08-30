using Client;
using Common;
using Configuration;
using Encryption;
using Interfaces.DataContracts;
using Interfaces.Services;
using Logger;
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

        public Device GetDeviceInfo()
        {
            return CurrentDevice;
        }

        public bool AddOrUpdatePairedDevice(DeviceCredentials pairedDevice, string token)
        {
            bool ret = true;
            AccessLevels al = ConfigManager.Configuration.GetAccessLevel(token);
            if (al == AccessLevels.CurrentDevice || al == AccessLevels.PowerfulPairedDevice)
            {
                ConfigManager.Configuration.AddOrUpdatePairedDevice(pairedDevice);
            }
            return ret;
        }

        private void ExchangeKeysWithPairedDevices()
        {
            foreach(DeviceCredentials dc in PairedDevices)
            {
                if (!dc.PairedDevice.Equals(CurrentDevice))
                {
                    try
                    {
                        IAuditService s = new ClientBuilder<IAuditService>("AuditService", dc.PairedDevice.Port, dc.PairedDevice.Host).Proxy;
                        if (s != null)
                        {
                            string pKey = s.ExchangeKeys(CurrentDevice, publicKey);
                            dc.PublicKey = pKey;
                        }
                    }
                    catch(Exception ex)
                    {
                        LoggerManager.Log.TraceMessage(string.Format("The device {0} {1}:{2} is not accessible", dc.PairedDevice.Name, dc.PairedDevice.Host, dc.PairedDevice.Host));
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

        public bool Authorize(Device device, AccessLevels accessLvl, string token)
        {
            bool ret = true;
            AccessLevels al = ConfigManager.Configuration.GetAccessLevel(token);
            if (al == AccessLevels.CurrentDevice)
            {
                ret = ConfigManager.Configuration.SetAccessLevelForDevice(device, accessLvl);
            }
            else
            {
                ret = false;
            }
            return ret;
        }

        public bool SetPassword(Device device, string oldPwdEncrypted, string newPwdEncrypted)
        {
            bool ret = true;
            ret = ConfigManager.Configuration.SetPassword(device, oldPwdEncrypted, newPwdEncrypted);
            return ret;
        }

        private void SetPairedDeviceKey(Device device, string pubKey)
        {
            bool found = false;
            DeviceCredentials dc = ConfigManager.Configuration.GetPairedDevice(device);

            if (dc == null)
            {
                dc = new DeviceCredentials();
                dc.PairedDevice = device;
                dc.AccessLevel = AccessLevels.NotAuthorized;

                if (device == CurrentDevice && publicKey == pubKey)
                {
                    dc.Password = Membership.GeneratePassword(Constants.PWD_LENGTH, Constants.NON_ALPHANUMERIC_CHARS_CNT);
                    dc.AccessLevel = AccessLevels.CurrentDevice;
                }
                ConfigManager.Configuration.AddOrUpdatePairedDevice(dc);
            }

            dc.PublicKey = pubKey;
            if (dc.PairedDevice == CurrentDevice && dc.PublicKey == publicKey && ConfigManager.Configuration.IsServer)
            {
                ConfigManager.Configuration.SetServer(dc);
            }
            ConfigManager.Configuration.Save();
        }
    }
}
