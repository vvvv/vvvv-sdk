using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2.Graph
{
    [ComVisible(false)]
    public interface IWindow2 : IEquatable<IWindow2>
    {
        /// <summary>
        /// Reference to the internal COM interface. Use with caution.
        /// </summary>
        IWindow InternalCOMInterf
        {
            get;
        }

        /// <summary>
		/// Gets/sets the window's caption.
		/// </summary>
		string Caption
		{
			get;
			set;
		}
		
		/// <summary>
		/// Returns the window's type.
		/// </summary>
		WindowType WindowType
		{
		    get;
		}
		
		/// <summary>
		/// Returns the window's associated INode2.
		/// </summary>
		INode2 Node
		{
		    get;
		}
		
		/// <summary>
		/// Returns the window's visible state (true if visible, false if not).
		/// </summary>
		bool IsVisible
		{
		    get;
		}
		
		/// <summary>
		/// Returns the window's bounds.
		/// </summary>
		Rectangle Bounds
		{
		    get;
		}
		
		/// <summary>
		/// Returns the window's handle
		/// </summary>
		IntPtr Handle
		{
		    get;
		}
    }
}
