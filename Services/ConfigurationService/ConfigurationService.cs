using Common;
using Configuration;
using Interfaces;
using Interfaces.DataContracts;
using Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ConfigurationService : IConfigurationService
    {
        Config config = null;
        public Device GetCurrentDevice()
        {
            return SysConfig.CurrentDevice;
        }

        Config SysConfig
        {
            get
            {
                if (config == null)
                {
                    config = new Config();
                    config.Load(Constants.CONFIG_FILENAME);
                }
                return config;
            }
        }
    }
}
