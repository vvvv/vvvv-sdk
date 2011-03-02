using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Hosting.Pins
{
    public class PinTypeRegistry<A> where A : Attribute
    {
        public delegate object PinCreateDelegate(IPluginHost host, A attribute);

        private Dictionary<Type, PinCreateDelegate> delegates = new Dictionary<Type, PinCreateDelegate>();

        public void RegisterType(Type type, PinCreateDelegate creator)
        {
            delegates[type] = creator;
        }

        public bool ContainsType(Type type)
        {
            return this.delegates.ContainsKey(type);
        }

        public object CreatePin(IPluginHost host, Type t, A attribute)
        {
            return delegates[t](host, attribute);
        }
    }
}
