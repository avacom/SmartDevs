using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.DataContracts
{
    [DataContract]
    public class Token
    {
        [DataMember]
        public string DeviceID { get; set; }
        [DataMember]
        public string Password { get; set; }
    }
}
