using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	public class DiffInputPin<T> : InputPin<T>, IDiffSpread<T>
	{
		private readonly IPluginIn FPluginIn;
	    private bool FIsChanged;
		private bool FIsReconnected;
		
		public DiffInputPin(IPluginHost host, IPluginIn pluginIn, IInStream<T> stream)
			: base(host, pluginIn, stream)
		{
			FPluginIn = pluginIn;
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
			    return FIsChanged;
			}
		}
		
		protected virtual void DoUpdate()
		{
			
		}
		
		public override void Connect(IPin otherPin)
		{
			FIsReconnected = true;
			base.Connect(otherPin);
		}
		
		public override void Disconnect(IPin otherPin)
		{
			FIsReconnected = true;
			base.Disconnect(otherPin);
		}
		
		public override sealed void Update()
		{
			FIsChanged = FIsReconnected || FPluginIn.PinIsChanged;
			
			try
			{
				if (FIsChanged)
				{
					FStream.Sync();
					DoUpdate();
					OnChanged();
				}
				
				FStream.Reset();
				OnUpdated();
			}
			finally
			{
				FIsReconnected = false;
			}
		}
		
		protected override void DisposeManaged()
		{
			Changed = null;
			base.DisposeManaged();
		}
	}
}
