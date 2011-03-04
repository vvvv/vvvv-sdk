using System;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
    public delegate void SpreadChangedEventHander(IDiffSpread spread);
    
    [ComVisible(false)]
    public delegate void SpreadChangedEventHander<T>(IDiffSpread<T> spread);
    
    /// <summary>
    /// Extension to the non-generic <see cref="ISpread"/> interface, to check if the data changes from frame to frame.
    /// </summary>
    [ComVisible(false)]
    public interface IDiffSpread : ISpread
    {
        /// <summary>
        /// Subscribe to this event to get noticed when the data changes.
        /// </summary>
        /// <remarks>
        /// Only data from this spread is valid in an event handler.
        /// If you access data from another spread, don't expect it to be valid.
        /// </remarks>
        event SpreadChangedEventHander Changed;
        
        /// <summary>
        /// Is true if the spread data has changed in this frame.
        /// </summary>
        bool IsChanged
        {
            get;
        }
    }
    
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
    }
}
