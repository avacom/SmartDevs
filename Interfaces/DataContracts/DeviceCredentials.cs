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
        /// <summary>
        /// Access level:
        /// 0 - not authorized
        /// 1 - Current device - most powerful
        /// 2 - Paired device
        /// </summary>
        [DataMember]
        public int AccessLevel { get; set; }
        [DataMember]
        public bool Online { get; set; }

        public Token GetToken()
        {
            Token ret = new Token();
            ret.DeviceID = PairedDevice.ID;
            ret.Password = Password;
            return ret;
        }
    }
}
