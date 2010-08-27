using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins
{

	public abstract class DiffPin<T> : Pin<T>, IDiffSpread<T>
	{
		public event SpreadChangedEventHander<T> Changed;
		
		public DiffPin(IPluginHost host, PinAttribute attribute)
			: base(host, attribute)
		{
		}
		
		public abstract bool IsChanged
		{
			get;
		}
		
		protected virtual void OnChanged()
		{
			if (Changed != null) 
				Changed(this);
		}
		
		public override void Update()
		{
			if (IsChanged)
				OnChanged();
			
			base.Update();
		}
		
		protected override void DisposeManaged()
		{
			Changed = null;
			base.DisposeManaged();
		}
	}
}
