using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.DataContracts
{
    [DataContract]
    public class Device
    {
        [DataMember]
        public string Port { get; set; }
        [DataMember]
        public string Host { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<Service> Services { get; set; }
    }
}
