using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core
{
    public class NodeAttribute : Attribute
    {
        public string Name { get; set; }

        public string Category { get; set; }

        public string Version { get; set; }

        //public string Help { get; set; } //better is to read /// help snippet

        public string Tags { get; set; }

        public string Author { get; set; }

        public string Credits { get; set; }

        public string Bugs { get; set; }

        public string Warnings { get; set; }

        public bool AutoEvaluate { get; set; }
    }

    public class PinAttribute : Attribute
    {
        public string Name { get; set; }

        public bool StrikedOutByDefault { get; set; }
    }
}
