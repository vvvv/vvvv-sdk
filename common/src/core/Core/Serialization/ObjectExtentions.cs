using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VVVV.Core.Serialization
{
    public static class ObjectExtentions
    {
        public static bool IsSerializable(this object obj)
        {
            if (obj == null) return false;
            return obj.GetType().IsSerializable || (obj is ISerializable);
        }
    }
}
