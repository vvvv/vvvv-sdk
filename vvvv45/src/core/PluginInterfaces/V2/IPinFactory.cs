
using System;

namespace VVVV.PluginInterfaces.V2
{
	public delegate ISpread<T> PinCreator<T>(IPluginHost2 host, PinAttribute attribute);
	
	public interface IPinFactory
	{
		ISpread<T> CreateSpread<T>(PinAttribute attribute);
		IDiffSpread<T> CreateDiffSpread<T>(PinAttribute attribute);
		Pin<T> CreatePin<T>(PinAttribute attribute);
		DiffPin<T> CreateDiffPin<T>(PinAttribute attribute);
		
		void RegisterPin<T>(PinCreator<T> creator);
	}
}
