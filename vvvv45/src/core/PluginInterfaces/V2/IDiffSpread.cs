using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace VVVV.PluginInterfaces.V2
{
 
    [ComVisible(false)]
    public delegate void SpreadChangedEventHander<T>(IDiffSpread<T> spread);
    
    /// <summary>
    /// Extension to the <see cref="ISpread{T}"/> interface, to check if the data changes from frame to frame.
    /// </summary>
    [ComVisible(false)]
    public interface IDiffSpread<T> : ISpread<T>, IDiffSpread
    {
        /// <summary>
        /// Subscribe to this event to get noticed when the data changes.
        /// </summary>
        /// <remarks>
        /// Only data from this spread is valid in an event handler.
        /// If you access data from another spread, don't expect it to be valid.
        /// </remarks>
        new event SpreadChangedEventHander<T> Changed;
        
        /// <summary>
        /// Is true if the spread data has changed in this frame.
        /// </summary>
        new bool IsChanged
        {
            get;
        }
    }
}
