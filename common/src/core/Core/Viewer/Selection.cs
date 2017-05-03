using System;
using System.Collections;

namespace VVVV.Core.Viewer
{
    public class Selection : ISelection
    {
        private static readonly ISelection FEmpty = new Selection();
        public static ISelection Empty
        {
            get
            {
                return FEmpty;
            }
        }
        
        public static ISelection Single(object item)
        {
            return new Selection(new ArrayList() { item });
        }
        
        private readonly IEnumerable FEnumerable;
        
        public Selection(IEnumerable enumerable)
        {
            FEnumerable = enumerable;
        }
        
        private Selection()
            : this(new ArrayList())
        {
            
        }
        
        public IEnumerator GetEnumerator()
        {
            return FEnumerable.GetEnumerator();
        }
    }
}
