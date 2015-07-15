using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.DataContracts
{
    [DataContract]
    public class DeviceCredentials
    {
        [DataMember]
        public Device PairedDevice { get; set; }
        [DataMember]
        public string PublicKey { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public bool Authorized { get; set; }
        [DataMember]
        public bool Online { get; set; }
    }
}
