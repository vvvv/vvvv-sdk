using System;
using System.Collections.Generic;
using System.Text;

namespace MrVux.Structures.PriorityQueues
{
    public class PriorityQueue<T> : PriorityQueue<T, T>
        where T : IComparable<T>
    {
        public PriorityQueue(ePriorityMode mode) : base(mode) { }

        public PriorityQueue(ePriorityMode mode, IComparer<T> comparer) : base(mode,comparer) { }

        public void Push(T element)
        {
            this.Push(element, element);
        }
    }
}
