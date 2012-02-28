using System;
using System.Runtime.InteropServices;
using VVVV.Core;

namespace VVVV.PluginInterfaces.V2.Graph
{
    [ComVisible(false)]
    public interface IPin2 : INamed
    {
        /// <summary>
        /// Gets a string representation of the specified slice.
        /// </summary>
        /// <param name="sliceIndex">The slice index.</param>
        /// <returns>A string representation of the specified slice.</returns>
        string GetSlice(int sliceIndex);
        /// <summary>
        /// Sets a specified slice as its string representation. 
        /// </summary>
        /// <param name="sliceIndex">The slice index.</param>
        /// <param name="slice">The slices value in its string representation</param>
        /// <param name="undoable">If TRUE this operation can be undone by the user in the patch using the UNDO command.</param>
		void SetSlice(int sliceIndex, string slice, bool undoable);

		/// <summary>
		/// Gets the whole spread as a string with commaseparated slices.
		/// </summary>
		/// <returns>A commaseparated string with all slices of the spread.</returns>
        string GetSpread();
        
        /// <summary>
        /// Sets the whole spread as a string with commaseparated slices.
        /// </summary>
        /// <param name="spread">The commaseparated string.</param>
        /// <param name="undoable">If TRUE this operation can be undone by the user in the patch using the UNDO command.</param>
        void SetSpread(string spread, bool undoable);
        
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
        /// The changed event occurs when the pin's data changed.
        /// </summary>
        event EventHandler Changed;
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
