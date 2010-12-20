using System;
using VVVV.Core;

namespace VVVV.PluginInterfaces.V2.Graph
{
    public interface IPin2 : INamed, IDisposable
    {
        /// <summary>
        /// Gets the value at specified slice in a string representation.
        /// </summary>
        string this[int sliceIndex]
        {
            get;
        }
        
        /// <summary>
        /// Whether the pin is connected or not.
        /// </summary>
        bool IsConnected
        {
            get;
        }
        
        /// <summary>
        /// The changed event occurs when the pin's data changed.
        /// </summary>
        event EventHandler Changed;
    }
}
