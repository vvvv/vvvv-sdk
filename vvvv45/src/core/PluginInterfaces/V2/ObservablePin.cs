using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{

	public abstract class ObservablePin<T> : Pin<T>, IDiffSpread<T>
	{
		public event SpreadChangedEventHander<T> Changed;
		
		public ObservablePin(IPluginHost host, PinAttribute attribute)
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
	}
}
