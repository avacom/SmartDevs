using Interfaces.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.Services
{
    [ServiceContract]
    public interface IConfigurationService
    {
        [OperationContract]
        Device GetCurrentDevice();
    }
}
