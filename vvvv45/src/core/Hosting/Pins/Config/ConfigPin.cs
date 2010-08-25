using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Config
{
	public abstract class ConfigPin<T> : DiffPin<T>
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
