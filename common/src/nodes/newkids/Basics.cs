using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core;

namespace VVVV.Nodes
{
    public static class Basics
    {
        [Node]
        public static string ToString<T>(this T x)
        {
            return x.ToString();
        }
    }
}
