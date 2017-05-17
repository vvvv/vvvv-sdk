using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Hosting.IO
{
    class DynamicTypeWrapper
    {
        public readonly object Value;

        public DynamicTypeWrapper(object value)
        {
            this.Value = value;
        }
    }
}
