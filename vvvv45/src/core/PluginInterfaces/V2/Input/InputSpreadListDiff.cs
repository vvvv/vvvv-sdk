using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class InputSpreadListDiff<T> : SpreadListDiff<T>
	{
		public InputSpreadListDiff(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
		}
		
		//create a pin at position
		protected override IDiffSpread<T> CreatePin(int pos)
		{
			//create pin name
			var origName = FAttribute.Name;
			FAttribute.Name = origName + " " + pos;
			
			var ret	= new ObservableInputWrapperPin<T>(FHost, FAttribute as InputAttribute).ObservablePin;
			ret.PluginIO.Order = FOffsetCounter * 1000 + pos;
			ret.Updated += SpreadListDiff_Changed;
			
			//set attribute name back
			FAttribute.Name = origName;
			
			return ret;
		}
		
	}
}
