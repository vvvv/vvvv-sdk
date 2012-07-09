using System;
using System.Collections.Generic;
using System.Text;

namespace MrVux.Structures.PriorityQueues
{
    internal class PriorityObject<T, P> : IComparable<P>
        where P : IComparable<P>
    {
        public PriorityObject()
        {

        }

        public PriorityObject(T element, P priority)
        {
            this.t = element;
            this.p = priority;
        }

        private T t;
        private P p;

        public T Element
        {
            get { return t; }
            set { t = value; }
        }

        public P Priority
        {
            get { return p; }
            set { p = value; }
        }

        public int CompareTo(P other)
        {
            return p.CompareTo(other);
        }
    }
}
