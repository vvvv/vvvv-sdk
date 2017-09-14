using System;
using System.Runtime.InteropServices;
using VVVV.Core;

namespace VVVV.PluginInterfaces.V2.Graph
{
    [ComVisible(false)]
    public interface IPin2: INamed
    {
        /// <summary>
        /// Reference to the internal COM interface. Use with caution.
        /// </summary>
        IPin InternalCOMInterf
        {
            get;
        }

    	/// <summary>
    	/// Gets/Sets a string representation of the specified slice.
    	/// </summary>
    	string this[int sliceIndex]
    	{
    		get;
    		set;
    	}
    	
    	/// <summary>
		/// Returns the pins name as seen by the given parent node. This makes sense for pins of modules which have two parents: the IOBox and the Module.
		/// </summary>
		/// <param name="parentNode">The node for which to ask the pinname from.</param>
		/// <returns>The pins name.</returns>
   		string NameByParent(INode2 parentNode);

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
        /// Returns the pins datatype as a string.
        /// </summary>
        string Type
        {
        	get;
        }
        
        /// <summary>
        /// Returns the pins clr type and null in case of native pins.
        /// </summary>
        Type CLRType
        {
        	get;
        }
        
        /// <summary>
        /// Returns the pins subtype.
        	/// values: guiType, dimension, default, min, max, stepSize, unitName, precision
        	/// strings: guiType, default, fileMask, maxChars
        	/// colors: guiType, default, hasAlpha
        	/// enums: guiType, enumName, default
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
        /// Gets/Sets the pins visibility. 
        /// </summary>
        PinVisibility Visibility
        {
            get; set;
        }
        
        /// <summary>
        /// Returns a list of connected pins. For Inputs this is a maximum of one.
        /// </summary>
        IViewableCollection<IPin2> ConnectedPins
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
		/// Returns the Pins parent node that lies in the give patch. This makes sense for pins of modules which have two parents: the IOBox and the Module.
		/// </summary>
		/// <param name="patch">The given patch.</param>
		/// <returns>The pins parent in the given patch.</returns>
		INode2 ParentNodeByPatch(INode2 patch);
		        
        /// <summary>
        /// The changed event occurs when the data of the unconnected input pin changes through an user interaction.
        /// </summary>
        event EventHandler Changed;
        
        /// <summary>
        /// The SubtypeChanged event occurs when the pins subtype changed.
        /// </summary>
        event EventHandler SubtypeChanged;
        
        /// <summary>
        /// The connected event occurs when the pin gets connected.
        /// </summary>
        event PinConnectionEventHandler Connected;
        
        /// <summary>
        /// The disconnected event occurs when the pin gets disconnected.
        /// </summary>
        event PinConnectionEventHandler Disconnected;

        /// <summary>
        /// The status changed event occurs when the pins status changed.
        /// </summary>
        event EventHandler StatusChanged;
    }
	
    [ComVisible(false)]
	public static class Pin2ExtensionMethods
	{
		public static bool IsConnected(this IPin2 pin)
		{
            return pin.ConnectedPins.Count > 0;
		}
	}
}
