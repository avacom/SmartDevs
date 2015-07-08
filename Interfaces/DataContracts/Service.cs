using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.DataContracts
{
    [DataContract]
    public class Service
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string Contract { get; set; }
        [DataMember]
        public bool Advanced { get; set; }
    }
}
