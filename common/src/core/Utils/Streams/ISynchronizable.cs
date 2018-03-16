using System;
using System.Runtime.InteropServices;

namespace VVVV.Utils.Streams
{
    /// <summary>
    /// Interface which provides the ability to synchronize a stream
    /// with its backing data source.
    /// </summary>
    [ComVisible(false)]
    public interface ISynchronizable
    {
        /// <summary>
        /// Synchronizes this object with the backing data source.
        /// </summary>
        /// <returns>
        /// Whether or not the data changed.
        /// Note: Most implementations will return true as the
        /// cost to check if the data changed might be too high.
        /// </returns>
        bool Sync();
        
        /// <summary>
        /// Whether or not the data changed during last synchronization.
        /// </summary>
        bool IsChanged { get; }
    }
}
