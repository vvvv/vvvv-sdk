using System;

using VVVV.HDE.Viewer.Model;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.NodeBrowser
{
	/// <summary>
	/// Description of CategoryEntryContentProvider.
	/// </summary>
	public class CategoryEntryContentProvider: ITreeContentProvider, ILabelProvider
	{
		public CategoryEntryContentProvider()
		{
		}
		
		public void Dispose()
		{
		}
		
		public object[] GetChildren(object element)
		{
			return (element as CategoryEntry).GetNodeInfos();
		}
		
		public string GetText(object element)
		{
		    return (element as CategoryEntry).Name;
		}
	    
        public event EventHandler LabelChanged;
	    
        public event EventHandler ContentChanged;
	}
}
