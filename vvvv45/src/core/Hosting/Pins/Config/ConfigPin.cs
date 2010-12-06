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
			
		/// <summary>
		/// Must be called by subclass at end of constructor.
		/// </summary>
		protected void Initialize(IPluginConfig pluginConfig)
		{
			PluginConfig = pluginConfig;
			base.InitializeInternalPin(pluginConfig);
		}
		
		protected IPluginConfig PluginConfig
		{
			get;
			private set;
		}
		
		public override int SliceCount 
		{
			get 
			{
				return PluginConfig.SliceCount;
			}
			set 
			{
				base.SliceCount = value;
				
				if (FAttribute.SliceMode != SliceMode.Single)
					PluginConfig.SliceCount = FSliceCount;
			}
		}
		
		protected override void DoUpdate()
		{
			// Config pins read from internal pin directly
		}
		
		protected override bool IsInternalPinChanged 
		{
			get 
			{
				return PluginConfig.PinIsChanged;
			}
		}
	}
}
