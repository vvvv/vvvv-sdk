using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	public abstract class InputPin<T> : Pin<T>
	{
		public abstract IPluginIn PluginIn
		{
			get;
		}
		
		public override int SliceCount 
		{
			get 
			{
				return PluginIn.SliceCount;
			}
			set {
				throw new NotImplementedException();
			}
		}
	}
}
