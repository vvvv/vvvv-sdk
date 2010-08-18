using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Config
{
	public abstract class ConfigPin<T> : ObservablePin<T>
	{
		protected bool FIsChanged;
		
		public ConfigPin(IPluginHost host, PinAttribute attribute)
			: base(host, attribute)
		{
		}
			
		
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
				if (FAttribute.SliceMode != SliceMode.Single)
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
