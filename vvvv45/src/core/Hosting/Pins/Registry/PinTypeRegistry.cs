using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Hosting.Pins
{
    [ComVisible(false)]
    public class PinTypeRegistry<A> where A : Attribute
    {
        [ComVisible(false)]
        public delegate object PinCreateDelegate(IPluginHost host, A attribute, Type closedGenericType);

        private Dictionary<Type, PinCreateDelegate> delegates = new Dictionary<Type, PinCreateDelegate>();

        private Dictionary<Type, PinCreateDelegate> delegatesbase = new Dictionary<Type, PinCreateDelegate>();

        public void RegisterBaseType(Type openGenericType, PinCreateDelegate creator)
        {
            delegatesbase[openGenericType] = creator;
        }

        public void RegisterType(Type openGenericType, PinCreateDelegate creator)
        {
            delegates[openGenericType] = creator;
        }

        public bool ContainsType(Type openGenericType)
        {
            if (this.delegates.ContainsKey(openGenericType))
            {
                return true;
            }
            else
            {
                foreach (Type t in delegatesbase.Keys)
                {
                    if (t.IsAssignableFrom(openGenericType)) { return true; }
                }
                return false;
            }
        }

        public object CreatePin(Type openGenericType, IPluginHost host, Type closedGenericType, A attribute)
        {
            if (this.delegates.ContainsKey(openGenericType))
            {
                return this.delegates[openGenericType](host, attribute, closedGenericType);
            }
            else
            {
                foreach (Type t in delegatesbase.Keys)
                {
                    if (t.IsAssignableFrom(openGenericType))
                    {
                        return this.delegatesbase[t](host, attribute, closedGenericType);
                    }
                }
                //Should never go there
                return null;
            } 
        }
    }
}
