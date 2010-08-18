using System;

namespace VVVV.PluginInterfaces.V2
{
	public delegate void SpreadChangedEventHander<T>(IDiffSpread<T> spread);
	
	/// <summary>
	/// Extension to the ISpread interface, to check if the data changes from frame to frame.
	/// </summary>
	public interface IDiffSpread<T> : ISpread<T>
	{
		/// <summary>
		/// Subscribe to this event to get noticed when the data changes.
		/// </summary>
		event SpreadChangedEventHander<T> Changed;
		
		/// <summary>
		/// Is true if the spread data has changed in this frame.
		/// </summary>
		bool IsChanged
		{
			get;
		}
	}
}
