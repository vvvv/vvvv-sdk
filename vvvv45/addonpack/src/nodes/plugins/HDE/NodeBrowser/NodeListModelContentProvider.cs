using System;
using VVVV.HDE.Viewer.Model;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.NodeBrowser
{
	/// <summary>
	/// Description of NodeListModelContentProvider.
	/// </summary>
	public class NodeListModelContentProvider: ITreeContentProvider, ILabelProvider
	{
		public NodeListModelContentProvider()
		{
		}
		
		public void Dispose()
		{
		}
		
		public object[] GetChildren(object element)
		{
		    return (element as NodeListModel).NodeList.ToArray();
		}
		
		public string GetText(object element)
		{
		    return "";
		}
	    
        public event EventHandler OnContentChanged;

        public event EventHandler OnLabelChanged;
	}
}
