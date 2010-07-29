using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	public abstract class OutputPin<T> : Pin<T>
	{
		public abstract IPluginOut PluginOut
		{
			get;
		}
		
		public override int SliceCount 
		{
			get 
			{
				return PluginOut.SliceCount;
			}
			set 
			{
				PluginOut.SliceCount = value;
			}
		}
	}
}
