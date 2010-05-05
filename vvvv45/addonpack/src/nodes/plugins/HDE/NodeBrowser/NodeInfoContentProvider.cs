using System;
using VVVV.HDE.Viewer.Model;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.NodeBrowser
{
	/// <summary>
	/// Description of NodeInfoContentProvider.
	/// </summary>
	public class NodeInfoContentProvider: ITreeContentProvider, ILabelProvider
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
	    
        public event EventHandler OnContentChanged;
	    
        public event EventHandler OnLabelChanged;
	}
}
