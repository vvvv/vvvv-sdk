using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	class DiffInputPin<T> : InputPin<T>, IDiffSpread<T>
	{
		public DiffInputPin(IIOFactory factory, IPluginIn pluginIn, MemoryIOStream<T> stream)
			: base(factory, pluginIn, stream)
		{
		}
		
		public DiffInputPin(IIOFactory factory, IPluginIn pluginIn, IInStream<T> stream)
		    : this(factory, pluginIn, new BufferedInputIOStream<T>(stream))
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
