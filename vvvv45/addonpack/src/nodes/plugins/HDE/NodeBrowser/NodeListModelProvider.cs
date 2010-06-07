using System;
using VVVV.HDE.Viewer.Model;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.NodeBrowser
{
	/// <summary>
	/// Description of NodeListModelContentProvider.
	/// </summary>
	public class NodeListModelProvider: ITreeContentProvider, ILabelProvider
	{
		public NodeListModelProvider()
		{
		}
		
		public void Dispose()
		{
		}
		
		public string GetText(object element)
		{
		    return "root";
		}
	    
        System.Collections.IEnumerable ITreeContentProvider.GetChildren(object element)
        {
            return (element as NodeListModel).Categories;
        }
	    
        public event EventHandler ContentChanged;
	    
        public event EventHandler LabelChanged;
	    
        public string GetToolTip(object element)
        {
            throw new NotImplementedException();
        }
	}
}
