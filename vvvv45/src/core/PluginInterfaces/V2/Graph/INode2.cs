using System;
using VVVV.Core;

namespace VVVV.PluginInterfaces.V2.Graph
{
    public interface INode2 : IViewableList<INode2>, INamed, IDisposable
    {
        /// <summary>
        /// Reference to the internal COM interface. Use with caution.
        /// </summary>
        INode InternalCOMInterf
        {
            get;
        }
        
        INodeInfo NodeInfo
        {
            get;
        }
        
        int ID
        {
            get;
        }
        
        IViewableCollection<IPin2> Pins
        {
            get;
        }
        
        /// <summary>
		/// Gets the <see cref="IWindow2">window</see> of this node. Or null if
		/// this node doesn't have a window.
		/// </summary>
		IWindow2 Window
		{
			get;
		}
        
        /// <summary>
		/// Gets or sets the last runtime error that occured or null if there were no errors.
		/// </summary>
		string LastRuntimeError
		{
			get;
			set;
		}
		
		/// <summary>
		/// Provides access to the parent node.
		/// </summary>
		INode2 Parent
		{
			get;
		}
		
		bool HasPatch
		{
			get;
		}
		
		bool HasCode
		{
			get;
		}
		
		bool HasGUI
		{
			get;
		}
		
		bool ContainsMissingNodes
		{
			get;
		}
		
		bool ContainsBoygroupedNodes
		{
			get;
		}
		
		bool IsMissing
		{
			get;
		}
		
		bool IsBoygrouped
		{
			get;
		}
    }
}
