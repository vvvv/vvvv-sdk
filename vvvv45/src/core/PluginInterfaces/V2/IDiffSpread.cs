using System;

namespace VVVV.PluginInterfaces.V2
{
	public delegate void SpreadChangedEventHander<T>(IDiffSpread<T> spread);
	
	public interface IDiffSpread<T> : ISpread<T>
	{
		event SpreadChangedEventHander<T> Changed;
		
		bool IsChanged
		{
			get;
		}
	}
}
