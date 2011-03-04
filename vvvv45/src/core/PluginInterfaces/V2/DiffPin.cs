using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
	public abstract class DiffPin<T> : Pin<T>, IDiffSpread<T>
	{
		private bool IsReconnected;
		
		public DiffPin(IPluginHost host, PinAttribute attribute)
			: base(host, attribute)
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
		
		protected abstract bool IsInternalPinChanged
		{
			get;
		}
		
		protected abstract void DoUpdate();
		
		public override void Connect(IPin otherPin)
		{
			IsReconnected = true;
			base.Connect(otherPin);
		}
		
		public override void Disconnect(IPin otherPin)
		{
			IsReconnected = true;
			base.Disconnect(otherPin);
		}
		
		public override sealed void Update()
		{
			IsChanged = IsReconnected || IsInternalPinChanged;
			
			try
			{
				if (IsChanged)
				{
					DoUpdate();
					OnChanged();
				}
				
				base.Update();
			}
			finally
			{
				IsReconnected = false;
			}
		}
		
		protected override void DisposeManaged()
		{
			Changed = null;
			base.DisposeManaged();
		}
	}
}
