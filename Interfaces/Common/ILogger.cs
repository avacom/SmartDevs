﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface ILogger
    {
        void TraceMessage(string message);
        void TraceException(Exception ex);
    }
}
