using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	public class InputSpreadList<T> : SpreadList<T>
	{
		public InputSpreadList(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
		}

        public Pin<T>[] Pins
        {
            get { return this.FPins; }
        }
		
		//create a pin at position
		protected override Pin<T> CreatePin(int pos)
		{
			//create pin name
			var origName = FAttribute.Name;
			FAttribute.Name = origName + " " + pos;
			
			var ret	= PinFactory.CreatePin<T>(FHost, FAttribute as InputAttribute);
			ret.PluginIO.Order = FOffsetCounter * 1000 + pos;
			
			//set attribute name back
			FAttribute.Name = origName;
			
			return ret;
           
		}
		
	}
}
