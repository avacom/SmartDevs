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
    public interface IAuditService
    {
        [OperationContract]
        string ExchangeKeys(Device device, string publicKey);
        [OperationContract]
        bool Authorize(Device device, int accessLvl, string token);
        [OperationContract]
        bool SetPassword(Device device, string oldPwdEncrypted, string newPwdEncrypted);
        [OperationContract]
        Device GetDeviceInfo();
    }
}
