using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;

using VVVV.Utils.Streams;

namespace VVVV.PluginInterfaces.V2.NonGeneric
{
    [ComVisible(false)]
    public interface ISpread : IEnumerable, ICloneable, ISynchronizable, IFlushable
    {
        /// <summary>
        /// Provides random read/write access to the actual data.
        /// </summary>
        object this[int index]
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set the size of this spread.
        /// </summary>
        int SliceCount
        {
            get;
            set;
        }
    }

    [ComVisible(false)]
    public delegate void SpreadChangedEventHander(IDiffSpread spread);

    /// <summary>
    /// Extension to the non-generic <see cref="ISpread"/> interface, to check if the data changes from frame to frame.
    /// </summary>
    [ComVisible(false)]
    public interface IDiffSpread : ISpread
    {
        /// <summary>
        /// Subscribe to this event to get notified when the data changes.
        /// </summary>
        /// <remarks>
        /// Only data from this spread is valid in an event handler.
        /// If you access data from another spread, don't expect it to be valid.
        /// </remarks>
        event SpreadChangedEventHander Changed;

        /// <summary>
        /// Is true if the spread data has changed in this frame.
        /// </summary>
        new bool IsChanged
        {
            get;
        }
    }
}
