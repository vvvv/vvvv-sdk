using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections
{
    public static class Empty
    {     
        static Empty ()
        {
            List = new List<int>().AsReadOnly();
            Enumerable = List;
        }

        public static IList List
        {
            get;
            private set;
        }

        public static IEnumerable Enumerable
        {
            get;
            private set;
        }

    }
}
