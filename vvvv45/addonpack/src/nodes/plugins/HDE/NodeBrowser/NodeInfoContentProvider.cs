using System;
using VVVV.HDE.Viewer.Model;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.NodeBrowser
{
	/// <summary>
	/// Description of NodeInfoContentProvider.
	/// </summary>
	public class NodeInfoContentProvider: ITreeContentProvider, ILabelProvider, IDragDropProvider
	{
		public NodeInfoContentProvider()
		{
		}
		
		public void Dispose()
		{
		}
		
		public object[] GetChildren(object element)
		{
		    return new object[0];
		}
		
		public string GetText(object element)
		{
		    return (element as NodeInfoDummy).Username;
		}
	    
        public event EventHandler ContentChanged;
	    
        public event EventHandler LabelChanged;
	    
        public bool AllowDrag(object element)
        {
            return true;
        }
	    
        public object DragItem(object element)
        {
            return GetText(element);
        }
	    
        public bool AllowDrop(object element, System.Collections.Generic.Dictionary<string, object> dropItems)
        {
            return false;
        }
	    
        public void DropItem(object element, System.Collections.Generic.Dictionary<string, object> dropItems)
        {
            throw new NotImplementedException();
        }
	}
}
