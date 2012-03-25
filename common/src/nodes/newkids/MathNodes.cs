using System;
using System.Collections.Generic;
using System.Linq;
using VVVV.Core;

namespace VVVV.Nodes.Math
{
    public static class MathNodes
    {
        [Node]
        public static int Add(int a, int b)
        {
            return a + b;
        }
        
        [Node]
        public static int Substract(int a, int b)
        {
            return a - b;
        }
        
        [Node]
        public static int Multiply(int a, int b)
        {
            return a * b;
        }
        
        [Node]
        public static int Divide(int a, int b)
        {
            return a / b;
        }
        
        [Node]
        public static int AddS(IEnumerable<int> xs)
        {
            return xs.Aggregate(0, (x, y) => x + y);
        }
        
        [Node]
        public static T Identity<T>(T x)
        {
            return x;
        }
    }
}
