﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configuration
{
    public class Service
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Contract { get; set; }

        public Service()
        {
            Name = string.Empty;
            Type = string.Empty;
            Contract = string.Empty;
        }

        public Service(string name, string type, string contract)
        {
            Name = name;
            Type = type;
            Contract = contract;
        }
    }
}
