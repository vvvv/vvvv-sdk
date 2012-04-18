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
        void Flush();
    }
}
