using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{

	public abstract class DiffPin<T> : Pin<T>, IDiffSpread<T>
	{
		public DiffPin(IPluginHost host, PinAttribute attribute)
			: base(host, attribute)
		{
		}
		
		public event SpreadChangedEventHander<T> Changed;
		
		protected virtual void OnChanged()
		{
			if (Changed != null) 
				Changed(this);
		}
		
		public abstract bool IsChanged
		{
			get;
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
