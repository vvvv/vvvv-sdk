using System;
using VVVV.Core;

namespace VVVV.PluginInterfaces.V2.Graph
{
    public interface IPin2 : INamed
    {
        /// <summary>
        /// Gets the value at specified slice in a string representation.
        /// </summary>
        string this[int sliceIndex]
        {
            get;
        }
		
		/// <summary>
		/// Gets the status of the pin.
		/// </summary>
		/// <value>
		/// The status.
		/// </value>
		StatusCode Status
		{
			get;
		}
        
        /// <summary>
        /// The changed event occurs when the pin's data changed.
        /// </summary>
        event EventHandler Changed;
    }
	
	public static class Pin2ExtensionMethods
	{
		public static bool IsConnected(this IPin2 pin)
		{
			return (pin.Status & StatusCode.IsConnected) == StatusCode.IsConnected;
		}
	}
}
