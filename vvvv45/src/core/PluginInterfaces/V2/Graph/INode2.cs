using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Drawing;

using VVVV.Core;

namespace VVVV.PluginInterfaces.V2.Graph
{
    [ComVisible(false)]
    public interface INode2 : IViewableList<INode2>, INamed
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
        
        /// <summary>
		/// Returns a slash-separated path of node IDs that uniquely identifies this node in the vvvv graph.
		/// </summary>
		/// <param name="useDescriptiveNames">If TRUE descriptive node names are used where available instead of the node ID.</param>
		/// <returns>A slash-separated path of node IDs that uniquely identifies this node in the vvvv graph.</returns>
		string GetNodePath(bool useDescriptiveNames);
		
		/// <summary>
		/// Returns the requested (node, box or window) bounds.
		/// </summary>
		/// <param name="boundsType">The type of bounds to be returned.</param>
		/// <returns></returns>
		Rectangle GetBounds(BoundsType boundsType);			
        
        IViewableCollection<IPin2> Pins
        {
            get;
        }
        
        IPin2 LabelPin
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
		
		StatusCode Status
		{
			get;
			set;
		}
		
		StatusCode InnerStatus
		{
			get;
		}
		
		event EventHandler StatusChanged;
		
		event EventHandler InnerStatusChanged;
    }

    [ComVisible(false)]
    public class BoundsChangedEventArgs : EventArgs
    {
        private BoundsType boundsType;

        public BoundsChangedEventArgs(BoundsType boundsType)
        {
            this.boundsType = boundsType;
        }

        public BoundsType BoundsType { get { return this.boundsType; } }
    }
	
    [ComVisible(false)]
	public static class Node2ExtensionMethods
	{
		public static bool ContainsMissingNodes(this INode2 node)
		{
			return (node.InnerStatus & StatusCode.IsMissing) == StatusCode.IsMissing;
		}
		
		public static bool ContainsBoygroupedNodes(this INode2 node)
		{
			return (node.InnerStatus & StatusCode.IsBoygrouped) == StatusCode.IsBoygrouped;
		}

        public static bool ContainsProblem(this INode2 node)
        {
			return (node.InnerStatus & (StatusCode.IsMissing | StatusCode.HasInvalidData | StatusCode.HasRuntimeError)) > 0;
        }
        
        public static bool IsConnected(this INode2 node)
		{
			return (node.InnerStatus & StatusCode.IsConnected) == StatusCode.IsConnected;
		}

        public static bool IsMissing(this INode2 node)
        {
            return (node.Status & StatusCode.IsMissing) == StatusCode.IsMissing;
        }

        public static bool IsBoygrouped(this INode2 node)
		{
			return (node.Status & StatusCode.IsBoygrouped) == StatusCode.IsBoygrouped;
		}
        
        public static bool IsExposed(this INode2 node)
		{
			return (node.Status & StatusCode.IsExposed) == StatusCode.IsExposed;
		}
        
        public static bool ContainsExposedNodes(this INode2 node)
		{
			return (node.InnerStatus & StatusCode.IsExposed) == StatusCode.IsExposed;
		}
        
        public static bool HasProblem(this INode2 node)
        {
            return (node.Status & (StatusCode.IsMissing | StatusCode.HasInvalidData | StatusCode.HasRuntimeError)) > 0;
        }
        
        public static IPin2 FindPin(this INode2 node, string name)
        {
            var query =
				from pin in node.Pins
            	where pin.Name == name || pin.NameByParent(node) == name
				select pin;
            return query.FirstOrDefault();
        }
    }
}
