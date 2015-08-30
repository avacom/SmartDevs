using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public enum AccessLevels
    {
        NotAuthorized = 0,
        CurrentDevice = 10,
        PowerfulPairedDevice = 20,
        PairedDevice = 30
    }
}
