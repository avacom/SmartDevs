using Common;
using Encryption;
using Interfaces.DataContracts;
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
                Save(fileName); 
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
                Save(fileName); 
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
                Save(fileName); 
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
                Save(fileName); 
            } 
        }

        string fileName;

        public Config(string FileName)
        {
            fileName = FileName;
            Load(fileName);
        }

        public Config()
        {
            fileName = string.Empty;
        }
        private bool Save(string fileName)
        {
            bool ret = Validate();
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
                catch
                {
                    ret = false;
                }
            }
            return ret;
        }

        public bool Save()
        {
            return Save(fileName);
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
            catch
            {
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
            }
        }

        bool Validate()
        {
            return true;
        }

        public int GetAccessLevel(string token)
        {
            int ret = 0;
            try
            {
                Token t = AsymmetricEncryption.DecryptCredentials(token, Constants.KEY_SIZE, PrivateKey);
                DeviceCredentials pd = GetPairedDevice(t);
                if (pd != null)
                {
                    ret = pd.AccessLevel;
                }
            }
            catch
            {
                ret = 0;
            }
            return ret;
        }

        public DeviceCredentials GetPairedDevice(Token t)
        {
            DeviceCredentials dc = null;
            if (t != null)
            {
                dc = PairedDevices.First(pd => pd.PairedDevice.ID == t.DeviceID && pd.Password == t.Password);
            }
            return dc;
        }

        public DeviceCredentials GetPairedDevice(Device device)
        {
            DeviceCredentials dc = null;
            if (device != null)
            {
                dc = PairedDevices.First(pd => pd.PairedDevice.Equals(device));
            }
            return dc;
        }

        public bool SetAccessLevelForDevice(Device device, int accessLvl)
        {
            bool ret = true;
            DeviceCredentials pd = GetPairedDevice(device);
            if (pd != null)
            {
                pd.AccessLevel = accessLvl;
                ret = Save(fileName);
            }
            else
            {
                ret = false;
            }
            return ret;
        }

        public bool InitPassword(Device device, string encyptedPwd)
        {
            bool ret = true;
            try
            {
                DeviceCredentials pd = GetPairedDevice(device);
                if (pd != null && string.IsNullOrEmpty(pd.Password) && pd.AccessLevel == 0)
                {
                    string pwd = AsymmetricEncryption.DecryptText(encyptedPwd, Constants.KEY_SIZE, PrivateKey);
                    pd.Password = pwd;
                    ret = Save(fileName);
                }
                else
                {
                    ret = false;
                }
            }
            catch
            {
                ret = false;
            }
            return ret;
        }
    }
}
