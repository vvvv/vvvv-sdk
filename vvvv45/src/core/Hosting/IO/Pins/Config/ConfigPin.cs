using System;
using System.Runtime.InteropServices;
using VVVV.Hosting.Pins.Input;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace VVVV.Hosting.Pins.Config
{
	[ComVisible(false)]
	class ConfigPin<T> : Pin<T>, IDiffSpread<T>
	{
		private readonly IPluginConfig FPluginConfig;
		
		public ConfigPin(IIOFactory factory, IPluginConfig pluginConfig, MemoryIOStream<T> stream)
			: base(factory, pluginConfig, stream)
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
		
		public override bool Sync()
		{
		    var isChanged = base.Sync();
			if (isChanged)
			{
				OnChanged();
			}
			return isChanged;
		}
	}
}
