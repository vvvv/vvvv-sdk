using System;
using System.Runtime.InteropServices;
using VVVV.Hosting.Pins.Input;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Config
{
	[ComVisible(false)]
	class ConfigPin<T> : Pin<T>, IDiffSpread<T>
	{
		private readonly IPluginConfig FPluginConfig;
		
		public ConfigPin(IPluginHost host, IPluginConfig pluginConfig, IIOStream<T> stream)
			: base(host, pluginConfig, stream)
		{
			FPluginConfig = pluginConfig;
			SliceCount = 1;
		}
		
		public event SpreadChangedEventHander<T> Changed;
		
		protected SpreadChangedEventHander FChanged;
		event SpreadChangedEventHander IDiffSpread.Changed
		{
			add
			{
				FChanged += value;
			}
			remove
			{
				FChanged -= value;
			}
		}
		
		protected virtual void OnChanged()
		{
			if (Changed != null)
				Changed(this);
			if (FChanged != null)
				FChanged(this);
		}
		
		public bool IsChanged
		{
			get
			{
				return FPluginConfig.PinIsChanged;
			}
		}
		
		public override bool Sync()
		{
			if (base.Sync())
			{
				OnChanged();
				return true;
			}
			
			return false;
		}
	}
}
