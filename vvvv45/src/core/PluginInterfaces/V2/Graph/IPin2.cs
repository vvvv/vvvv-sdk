using System;
using System.Runtime.InteropServices;
using VVVV.Core;

namespace VVVV.PluginInterfaces.V2.Graph
{
    [ComVisible(false)]
    public interface IPin2: INamed
    {
    	/// <summary>
    	/// Gets/Sets a string representation of the specified slice.
    	/// </summary>
    	string this[int sliceIndex]
    	{
    		get;
    		set;
    	}

    	/// <summary>
    	/// Gets/Sets the whole spread as a string with commaseparated slices.
    	/// </summary>
    	string Spread
    	{
    		get;
    		set;
    	}
        
        /// <summary>
        /// Returns the pins slicecount.
        /// </summary>
        int SliceCount
        {
        	get;
        }
        
        /// <summary>
        /// Returns the pins datatype.
        /// </summary>
        string Type
        {
        	get;
        }
        
        /// <summary>
        /// Returns the pins subtype.
        /// </summary>
        string SubType
	    {
	    	get;
	    }
        
        /// <summary>
        /// Returns the pins direction. 
        /// </summary>
        PinDirection Direction
        {
        	get;
        }
		
		/// <summary>
		/// Returns the status of the pin.
		/// </summary>
		StatusCode Status
		{
			get;
		}
		
		/// <summary>
		/// Returns the pins parent node.
		/// </summary>
		INode2 ParentNode
		{
			get;
		}
        
        /// <summary>
        /// The changed event occurs when the pins data changed.
        /// </summary>
        event EventHandler Changed;
        
        /// <summary>
        /// The SubtypeChanged event occurs when the pins subtype changed.
        /// </summary>
        event EventHandler SubtypeChanged;
    }
	
    [ComVisible(false)]
	public static class Pin2ExtensionMethods
	{
		public static bool IsConnected(this IPin2 pin)
		{
			return (pin.Status & StatusCode.IsConnected) == StatusCode.IsConnected;
		}
	}
}
