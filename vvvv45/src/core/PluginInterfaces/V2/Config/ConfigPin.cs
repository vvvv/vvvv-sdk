using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	public abstract class ConfigPin<T> : Pin<T>
	{
		public abstract IPluginConfig PluginConfig
		{
			get;
		}
		
		public override int SliceCount 
		{
			get 
			{
				return PluginConfig.SliceCount;
			}
			set {
				PluginConfig.SliceCount = value;
			}
		}
	}
}
