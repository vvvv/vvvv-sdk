using System;

namespace VVVV.PluginInterfaces.V2
{
	public delegate void SpreadChangedEventHander<T>(IObservableSpread<T> spread);
	
	public interface IObservableSpread<T> : ISpread<T>
	{
		event SpreadChangedEventHander<T> Changed;
		
		bool IsChanged
		{
			get;
		}
	}
}
