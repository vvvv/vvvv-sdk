using System;

namespace VVVV.Utils.Streams
{
    public interface ISynchronizable
    {
        /// <summary>
        /// Synchronizes this object with the internal data source.
        /// </summary>
        /// <returns>
        /// Whether or not the data changed.
        /// Note: Most implementations will return true as the
        /// cost to check if the data changed might be too high.
        /// </returns>
        bool Sync();
    }
}
