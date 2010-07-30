using System;
using System.Diagnostics;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Config
{
	public abstract class ConfigPin<T> : Pin<T>
	{
		public ConfigPin(ConfigAttribute attribute)
		{
			Debug.WriteLine(string.Format("Creating config pin {0}.", attribute.Name));
		}
		
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
			set 
			{
				PluginConfig.SliceCount = value;
			}
		}
	}
}
