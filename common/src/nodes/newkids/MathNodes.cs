using System;
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
    }
}
