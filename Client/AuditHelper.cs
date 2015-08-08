using Common;
using Configuration;
using Encryption;
using Interfaces.DataContracts;
using Interfaces.Services;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace Client
{
    public class AuditHelper
    {
        public void RegisterDevice(string host, string port)
        {
            LoggerManager.Log.TraceMessage(string.Format("Registering the device {0}:{1}", host, port));
            DeviceCredentials dc = ConfigManager.Configuration.GetPairedDevice(host, port);
            if (dc == null)
            {
                IAuditService s = new ClientBuilder<IAuditService>("AuditService", port, host).Proxy;
                LoggerManager.Log.TraceMessage(string.Format("Getting the Device info from {0}:{1}", host, port));
                Device hostDevice = s.GetDeviceInfo();
                if (hostDevice != null)
                {
                    LoggerManager.Log.TraceMessage(string.Format("Exchanging public keys with {0}:{1}", host, port));
                    string hostKey = s.ExchangeKeys(ConfigManager.Configuration.CurrentDevice, ConfigManager.Configuration.PublicKey);
                    string pwd = Membership.GeneratePassword(Constants.PWD_LENGTH, Constants.NON_ALPHANUMERIC_CHARS_CNT);
                    LoggerManager.Log.TraceMessage(string.Format("Setting the device password on {0}:{1}", host, port));
                    if (s.SetPassword(ConfigManager.Configuration.CurrentDevice, null, AsymmetricEncryption.EncryptText(pwd, Constants.KEY_SIZE, hostKey)))
                    {
                        DeviceCredentials newDc = new DeviceCredentials();
                        newDc.PairedDevice = hostDevice;
                        newDc.PublicKey = hostKey;
                        newDc.Password = pwd;

                        ConfigManager.Configuration.PairedDevices.Add(newDc);
                        ConfigManager.Configuration.SetServer(newDc);
                    }
                    else
                    {
                        LoggerManager.Log.TraceMessage(string.Format("Unable to set the device password on {0}:{1}", host, port));
                    }
                }
                else
                {
                    LoggerManager.Log.TraceMessage(string.Format("ERROR: cannot get the Device info from {0}:{1}", host, port));
                }

            }
            else
            {
                LoggerManager.Log.TraceMessage(string.Format("The device {0}:{1} is already registered. No need to retry", host, port));
            }
            LoggerManager.Log.TraceMessage("RegisterDevice() end");
        }
    }
}
