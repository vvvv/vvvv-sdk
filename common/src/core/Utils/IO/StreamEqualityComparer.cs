using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace System.IO
{
    public class StreamEqualityComparer : EqualityComparer<Stream>
    {
        public static readonly StreamEqualityComparer Instance = new StreamEqualityComparer();

        public override bool Equals(Stream x, Stream y)
        {
            return x.StreamEquals(y);
        }

        public override int GetHashCode(Stream obj)
        {
            unchecked
            {
                return (int)obj.Length;
            }
        }
    }
}
