using Common;
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
        /// 10 - Current device - most powerful
        /// 20 - Server
        /// 30 - Paired device
        /// </summary>
        [DataMember]
        public AccessLevels AccessLevel { get; set; }
        [DataMember]
        public bool IsServer { get; set; }

        public Token GetToken()
        {
            Token ret = new Token();
            ret.DeviceID = PairedDevice.ID;
            ret.Password = Password;
            return ret;
        }
    }
}
