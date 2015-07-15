using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configuration
{
    public static class ConfigManager
    {
        public static Config Configuration;
        static ConfigManager()
        {
            Configuration = new Config(Constants.CONFIG_FILENAME);
        }
    }
}
