using Common;
using Encryption;
using Interfaces.DataContracts;
using Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Configuration
{
    public class Config
    {
        Device _currentDevice;
        public Device CurrentDevice 
        { 
            get 
            {
                return _currentDevice; 
            } 
            set 
            {
                _currentDevice = value; 
                Save(_fileName); 
            } 
        }
        string _publicKey;
        public string PublicKey 
        { 
            get 
            {
                return _publicKey; 
            } 
            set 
            {
                _publicKey = value; 
                Save(_fileName); 
            } 
        }
        string _privateKey;
        public string PrivateKey 
        { 
            get 
            {
                return _privateKey; 
            } 
            set 
            {
                _privateKey = value; 
                Save(_fileName); 
            } 
        }

        bool _isServer;
        public bool IsServer
        {
            get
            {
                return _isServer;
            }
            set
            {
                _isServer = value;
                Save(_fileName);
            }
        }

        List<DeviceCredentials> _pairedDevices;
        public List<DeviceCredentials> PairedDevices 
        { 
            get 
            {
                return _pairedDevices; 
            } 
            set 
            {
                _pairedDevices = value; 
                Save(_fileName); 
            } 
        }

        List<DeviceCredentials> _temporarySessions;
        [XmlIgnore]
        public List<DeviceCredentials> TemporarySessions
        {
            get
            {
                return _temporarySessions;
            }
            set
            {
                _temporarySessions = value;
            }
        }

        string _fileName;

        public Config(string fileName)
        {
            _fileName = fileName;
            Load(fileName);
        }

        public Config()
        {
            _fileName = string.Empty;
        }
        private bool Save(string fileName)
        {
            bool ret = Validate();
            if (!string.IsNullOrEmpty(fileName))
            {
                if (ret)
                {
                    try
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(Config));
                        using (TextWriter writer = new StreamWriter(fileName))
                        {
                            serializer.Serialize(writer, this);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerManager.Log.TraceMessage(string.Format("ERROR: cannot save {0}", fileName));
                        LoggerManager.Log.TraceException(ex);
                        ret = false;
                    }
                }
            }
            else
            {
                ret = false;
            }

            return ret;
        }

        public bool Save()
        {
            return Save(_fileName);
        }

        private bool Load(string fileName)
        {
            bool ret = true;
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(Config));
                TextReader reader = new StreamReader(fileName);
                object obj = deserializer.Deserialize(reader);
                Config loaded = (Config)obj;
                LoadFromObject(loaded);
                reader.Close();

                if (string.IsNullOrEmpty(CurrentDevice.Host))
                {
                    CurrentDevice.Host = System.Net.Dns.GetHostName();
                }

                if (string.IsNullOrEmpty(CurrentDevice.Name))
                {
                    CurrentDevice.Name = Environment.MachineName;
                }

                if (string.IsNullOrEmpty(CurrentDevice.ID))
                {
                    CurrentDevice.ID = Guid.NewGuid().ToString();
                }
                Save(fileName);
            }
            catch(Exception ex)
            {
                LoggerManager.Log.TraceMessage(string.Format("ERROR: cannot load {0}", fileName));
                LoggerManager.Log.TraceException(ex);
                ret = false;
            }

            return ret;
        }

        private void LoadFromObject(Config obj)
        {
            if (obj != null)
            {
                _currentDevice = obj.CurrentDevice;
                _privateKey = obj.PrivateKey;
                _publicKey = obj.PublicKey;
                _pairedDevices = obj.PairedDevices;
                _isServer = obj.IsServer;
            }
        }

        bool Validate()
        {
            return true;
        }

        public AccessLevels GetAccessLevel(string token)
        {
            AccessLevels ret = 0;
            try
            {
                Token t = AsymmetricEncryption.DecryptCredentials(token, Constants.KEY_SIZE, PrivateKey);
                DeviceCredentials pd = GetPairedDevice(t);
                if (pd != null)
                {
                    ret = pd.AccessLevel;
                }
            }
            catch(Exception ex)
            {
                LoggerManager.Log.TraceMessage("ERROR: cannot get the access level");
                LoggerManager.Log.TraceException(ex);
                ret = AccessLevels.NotAuthorized;
            }
            return ret;
        }

        public DeviceCredentials GetPairedDevice(Token t)
        {
            DeviceCredentials dc = null;
            if (t != null)
            {
                dc = PairedDevices.FirstOrDefault(pd => pd.PairedDevice.ID == t.DeviceID && pd.Password == t.Password);
            }
            return dc;
        }

        public DeviceCredentials GetPairedDevice(string host, string port)
        {
            DeviceCredentials dc = null;
            dc = PairedDevices.FirstOrDefault(pd => pd.PairedDevice.Host.ToLower() == host.ToLower() && pd.PairedDevice.Port == port);
            return dc;
        }

        public DeviceCredentials GetPairedDevice(Device device)
        {
            return GetPairedDevice(device, PairedDevices);
        }

        public DeviceCredentials GetPairedDevice(Device device, List<DeviceCredentials> listToSearch)
        {
            DeviceCredentials dc = null;
            if (device != null)
            {
                dc = listToSearch.FirstOrDefault(pd => pd.PairedDevice.Equals(device));
            }
            return dc;
        }

        public bool SetAccessLevelForDevice(Device device, AccessLevels accessLvl)
        {
            bool ret = true;
            DeviceCredentials pd = GetPairedDevice(device);
            if (pd != null)
            {
                pd.AccessLevel = accessLvl;
                ret = Save(_fileName);
            }
            else
            {
                ret = false;
            }
            return ret;
        }

        public bool SetServer(Device device)
        {
            bool ret = true;
            DeviceCredentials target = GetPairedDevice(device);
            ret = SetServer(target);
            return ret;
        }

        public bool SetServer(DeviceCredentials devCred)
        {
            bool ret = true;
            if (devCred != null && PairedDevices.Contains(devCred))
            {
                DeviceCredentials srv = GetServer();
                if (srv != null)
                {
                    srv.IsServer = false;
                }
                devCred.IsServer = true;
            }
            else
            {
                ret = false;
            }
            ret = Save(_fileName);
            return ret;
        }

        public DeviceCredentials GetServer()
        {
            return PairedDevices.FirstOrDefault(dc => dc.IsServer);
        }

        public void AddOrUpdatePairedDevice(DeviceCredentials dc)
        {
            AddOrUpdatePairedDevice(dc, false);
        }
        public void AddOrUpdatePairedDevice(DeviceCredentials dc, bool temp)
        {
            if (dc != null)
            {
                List<DeviceCredentials> dcList = temp ? TemporarySessions : PairedDevices;

                DeviceCredentials current = GetPairedDevice(dc.PairedDevice, dcList);
                if (current != null)
                {
                    current.PairedDevice = dc.PairedDevice;
                    current.Password = dc.Password;
                    current.PublicKey = dc.PublicKey;
                    current.AccessLevel = dc.AccessLevel;
                }
                else
                {
                    dcList.Add(dc);
                }
            }
            if (!temp)
                Save(_fileName);
        }

        public bool SetPassword(Device device, string oldPwdEncrypted, string newPwdEncrypted)
        {
            bool ret = true;
            try
            {
                DeviceCredentials pd = GetPairedDevice(device);
                if (pd != null)
                {
                    if (pd != null &&
                            (
                                (string.IsNullOrEmpty(oldPwdEncrypted) && string.IsNullOrEmpty(pd.Password)) ||
                                (AsymmetricEncryption.DecryptText(oldPwdEncrypted, Constants.KEY_SIZE, PrivateKey) == pd.Password)
                            )
                        )
                    {
                        string pwd = AsymmetricEncryption.DecryptText(newPwdEncrypted, Constants.KEY_SIZE, PrivateKey);
                        pd.Password = pwd;
                        ret = Save(_fileName);
                    }
                }
                else
                {
                    ret = false;
                }
            }
            catch(Exception ex)
            {
                LoggerManager.Log.TraceMessage(string.Format("ERROR: cannot set a password for a device {0} {1}", device.Name, device.ID));
                LoggerManager.Log.TraceException(ex);
                ret = false;
            }
            return ret;
        }
    }
}
