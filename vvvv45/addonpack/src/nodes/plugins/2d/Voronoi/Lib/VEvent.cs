using System;
using System.Collections.Generic;
using System.Text;

namespace BenTools.Mathematics
{
    internal abstract class VEvent : IComparable<VEvent>
    {
        public abstract double Y { get;}
        public abstract double X { get;}

        public int CompareTo(VEvent other)
        {
            int i = Y.CompareTo(other.Y);
            if (i != 0)
                return i;
            return X.CompareTo(other.X);
        }

        /*
        public int CompareTo(object obj)
        {
            if (!(obj is VEvent))
                throw new ArgumentException("obj not VEvent!");
            int i = Y.CompareTo(((VEvent)obj).Y);
            if (i != 0)
                return i;
            return X.CompareTo(((VEvent)obj).X);
        }*/
    }
}
