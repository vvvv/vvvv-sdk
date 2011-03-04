using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Hosting.Pins
{
    public class PinTypeRegistry<A> where A : Attribute
    {
        public delegate object PinCreateDelegate(IPluginHost host, A attribute, Type closedGenericType);

        private Dictionary<Type, PinCreateDelegate> delegates = new Dictionary<Type, PinCreateDelegate>();

        public void RegisterType(Type openGenericType, PinCreateDelegate creator)
        {
            delegates[openGenericType] = creator;
        }

        public bool ContainsType(Type openGenericType)
        {
            return this.delegates.ContainsKey(openGenericType);
        }

        public object CreatePin(Type openGenericType, IPluginHost host, Type closedGenericType, A attribute)
        {
            return delegates[openGenericType](host, attribute, closedGenericType);
        }
    }
}
