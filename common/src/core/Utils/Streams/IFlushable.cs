using System;

namespace VVVV.Utils.Streams
{
    public interface IFlushable
    {
        /// <summary>
        /// Flushes all buffered data to the underlying data sink.
        /// </summary>
        void Flush();
    }
}
