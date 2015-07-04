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
        public string Port { get; set; }
        public string Host { get; set; }
        public List<Service> Services { get; set; }

        public Config()
        {
            Port = string.Empty;
            Host = string.Empty;
            Services = new List<Service>();
        }

        public bool Save(string fileName)
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

        public bool Load(string fileName)
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
                Port = obj.Port;
                Host = obj.Host;
                Services = obj.Services;
            }
        }

        bool Validate()
        {
            return true;
        }
    }
}
