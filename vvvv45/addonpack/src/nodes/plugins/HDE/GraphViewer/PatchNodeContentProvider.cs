using System;
using VVVV.HDE.Viewer.Model;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Graph.Provider
{
	/// <summary>
	/// Description of DocumentContentProvider.
	/// </summary>
	public class PatchNodeContentProvider: ITreeContentProvider, ILabelProvider
	{
		public PatchNodeContentProvider()
		{
		}
		
		public void Dispose()
		{
		}
		
		public object[] GetChildren(object element)
		{
			INode self = element as INode;
			return self.GetChildren();
		}
		
		public string GetText(object element)
		{
		    INode self = element as INode;
		    
		    var ni = self.GetNodeInfo();
		    if (ni != null)
		        return self.GetNodeInfo().Username;
		    else
		        return self.ToString();
		}
	}
}
