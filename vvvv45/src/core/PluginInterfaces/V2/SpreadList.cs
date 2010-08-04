using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// base class for spread lists
	/// </summary>
	public class SpreadList<T> : ISpreadList<T>
	{
		protected Pin<T>[] FPins;
		protected IPluginHost FHost;
		protected PinAttribute FAttribute;
		
		public SpreadList(IPluginHost host, PinAttribute attribute)
		{
			FHost = host;
			FAttribute = attribute;
			FPins = new Pin<T>[0];
		}
			
		public ISpread<T>[] Spreads 
		{ 
			get
			{
				return FPins as ISpread<T>[];
			}
		}
	}
}
