using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Streams.Registry
{
    [ComVisible(false)]
    public class StreamRegistry<A> where A : ImportAttribute
    {
        [ComVisible(false)]
        public delegate IStream StreamCreateDelegate(IPluginHost host, A attribute, Type closedGenericType);

        private Dictionary<Type, StreamCreateDelegate> delegates = new Dictionary<Type, StreamCreateDelegate>();

        public void RegisterType(Type openGenericType, StreamCreateDelegate creator)
        {
            delegates[openGenericType] = creator;
        }

        public bool ContainsType(Type openGenericType)
        {
            return this.delegates.ContainsKey(openGenericType);
        }

        public IStream CreateStream(Type openGenericType, IPluginHost host, Type closedGenericType, A attribute)
        {
            return delegates[openGenericType](host, attribute, closedGenericType);
        }
    }
}
