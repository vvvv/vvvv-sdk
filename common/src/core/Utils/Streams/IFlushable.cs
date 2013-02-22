using System;
using System.Runtime.InteropServices;

namespace VVVV.Utils.Streams
{
    /// <summary>
    /// Interface which provides the ability to flush all buffered data
    /// of a stream to its backing data sink.
    /// </summary>
    [ComVisible(false)]
    public interface IFlushable
    {
        /// <summary>
        /// Flushes all buffered data to the backing data sink.
        /// </summary>
        /// <param name="force">
        /// Whether or not to force the flush. Many implementations keep a changed flag
        /// internally to reduce unnecessary method calls and copy operations.
        /// Forcing a flush should circumvent those internal flags and do the copy no 
        /// matter what.
        /// </param>
        void Flush(bool force = false);
    }
}
