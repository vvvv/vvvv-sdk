using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Config
{
	public abstract class ConfigPin<T> : ObservablePin<T>
	{
		protected bool FIsChanged;
		
		public override IPluginIO PluginIO
		{
			get
			{
				return PluginConfig;
			}
		}
		
		protected abstract IPluginConfig PluginConfig
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
		
		public override bool IsChanged 
		{
			get 
			{
				return FIsChanged;
			}
		}
	}
}
