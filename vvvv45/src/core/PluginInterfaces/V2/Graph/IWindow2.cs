using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2.Graph
{
    [ComVisible(false)]
    public interface IWindow2 : IEquatable<IWindow2>
    {
        /// <summary>
		/// Gets/sets the window's caption.
		/// </summary>
		string Caption
		{
			get;
			set;
		}
		
		/// <summary>
		/// Get the window's type.
		/// </summary>
		WindowType WindowType
		{
		    get;
		}
		
		/// <summary>
		/// Get the window's associated INode2.
		/// </summary>
		INode2 Node
		{
		    get;
		}
		
		/// <summary>
		/// Gets the window's visible state. 
		/// Returns true if this window is visible, false if not.
		/// </summary>
		bool IsVisible
		{
		    get;
		}
		
		/// <summary>
		/// Gets the window's bounds.
		/// </summary>
		Rectangle Bounds
		{
		    get;
		}
    }
}
