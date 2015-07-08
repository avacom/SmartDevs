using Interfaces;
using Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class TestService : ITestService
    {
        public string GetString()
        {
            string retVal = "Hello world!";
            return retVal;
        }
    }
}
