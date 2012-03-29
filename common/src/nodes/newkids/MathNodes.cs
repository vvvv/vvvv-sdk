using System;
using System.Collections.Generic;
using System.Linq;
using VVVV.Core;

namespace VVVV.Nodes.Math
{
    public static class MathNodes
    {
        [Node]
        public static int Inc(int a)
        {
            return ++a;
        }
        
        [Node]
        public static int Dec(int a)
        {
            return --a;
        }
        
        [Node]
        public static int Add(int a, int b)
        {
            return a + b;
        }
        
        [Node]
        public static int Subtract(int a, int b)
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
        public static bool Eq(int a, int b)
        {
            return a == b;
        }
        
        [Node]
        public static int One()
        {
            return 1;
        }
        
        [Node]
        public static int Two()
        {
            return 2;
        }
        
        [Node]
        public static int Three()
        {
            return 3;
        }
        
        [Node]
        public static int Four()
        {
            return 4;
        }
        
        [Node]
        public static int AddS(IEnumerable<int> xs)
        {
            return xs.Aggregate(0, (x, y) => x + y);
        }
        
        [Node]
        public static T FoldL<T>(IEnumerable<T> xs, Func<T, T, T> func)
        {
            return xs.Aggregate(func);
        }
        
        [Node]
        public static T Identity<T>(T x)
        {
            return x;
        }
        
        [Node]
        public static IEnumerable<int> Range(int start, int count)
        {
            return Enumerable.Range(start, count);
        }
        
        [Node]
        public static Func<int, int, int> AddFunc()
        {
            return Add;
        }
    }
}
