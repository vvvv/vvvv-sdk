using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	public delegate void PinUpdatedEventHandler<T>(ObservablePin<T> pin);
	
	public abstract class ObservablePin<T> : Pin<T>, IObservableSpread<T>
	{
		public event SpreadChangedEventHander<T> Changed;
		public event PinUpdatedEventHandler<T> Updated;
		
		protected virtual void OnUpdated()
		{
			if (Updated != null) 
			{
				Updated(this);
			}
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
			
			OnUpdated();
		}
	}
}
