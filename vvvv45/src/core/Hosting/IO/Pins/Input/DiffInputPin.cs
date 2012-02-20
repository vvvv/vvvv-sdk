using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	class DiffInputPin<T> : InputPin<T>, IDiffSpread<T>
	{
		public DiffInputPin(IPluginIn pluginIn,  ManagedIOStream<T> stream)
			: base(pluginIn, stream)
		{
		}
		
		public DiffInputPin(IPluginIn pluginIn, IInStream<T> stream)
		    : this(pluginIn, new ManagedInputIOStream<T>(stream))
		{
		    
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
			get;
			private set;
		}
		
		public override bool Sync()
		{
			IsChanged = base.Sync();
			
			if (IsChanged)
			{
				OnChanged();
			}
			
			return IsChanged;
		}
	}
}
