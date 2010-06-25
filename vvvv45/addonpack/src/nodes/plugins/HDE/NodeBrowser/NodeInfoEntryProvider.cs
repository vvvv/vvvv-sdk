using System;
//using VVVV.HDE.Viewer.Model;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.NodeBrowser
{
	/// <summary>
	/// Description of NodeInfoContentProvider.
	/// </summary>
	public class NodeInfoEntryProvider//: ITreeContentProvider, ILabelProvider, IDragDropProvider
	{
		public NodeInfoEntryProvider()
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
		    return (element as NodeInfoEntry).Username;
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
	    
//        System.Collections.IEnumerable ITreeContentProvider.GetChildren(object element)
//        {
//            return new object[0];
//        }
	    
        public string GetToolTip(object element)
        {
            string tip = "";
            var ni = (element as NodeInfoEntry).NodeInfo;
            if (!string.IsNullOrEmpty(ni.ShortCut))
                tip = "(" + ni.ShortCut + ") " ;
            if (!string.IsNullOrEmpty(ni.Help))
                tip += ni.Help;
            if (!string.IsNullOrEmpty(ni.Warnings))
                tip += "\n WARNINGS: " + ni.Warnings;
            if (!string.IsNullOrEmpty(ni.Bugs))
                tip += "\n BUGS: " + ni.Bugs;
            if ((!string.IsNullOrEmpty(ni.Author)) && (ni.Author != "vvvv group"))
                tip += "\n AUTHOR: " + ni.Author;
            if (!string.IsNullOrEmpty(ni.Credits))
                tip += "\n CREDITS: " + ni.Credits;
            return tip;
        }
	}
}
