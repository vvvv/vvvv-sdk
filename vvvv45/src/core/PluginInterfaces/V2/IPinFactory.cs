using System;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
	public delegate ISpread<T> SpreadCreator<T>(IPluginHost2 host, PinAttribute attribute);
	
	[ComVisible(false)]
	public delegate IDiffSpread<T> DiffSpreadCreator<T>(IPluginHost2 host, PinAttribute attribute);
	
	[ComVisible(false)]
	public delegate Pin<T> PinCreator<T>(IPluginHost2 host, PinAttribute attribute);
	
	[ComVisible(false)]
	public delegate DiffPin<T> DiffPinCreator<T>(IPluginHost2 host, PinAttribute attribute);
	
	[ComVisible(false)]
	public interface IPinFactory
	{
		ISpread<T> CreateSpread<T>(PinAttribute attribute);
		IDiffSpread<T> CreateDiffSpread<T>(PinAttribute attribute);
		Pin<T> CreatePin<T>(PinAttribute attribute);
		DiffPin<T> CreateDiffPin<T>(PinAttribute attribute);
		
		void RegisterSpreadCreator<T>(SpreadCreator<T> creator);
		void RegisterDiffSpreadCreator<T>(DiffSpreadCreator<T> creator);
		void RegisterPinCreator<T>(PinCreator<T> creator);
		void RegisterDiffPinCreator<T>(DiffPinCreator<T> creator);
	}
}
