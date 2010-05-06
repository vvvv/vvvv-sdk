
using System;
using VVVV.PluginInterfaces.V1;
using VVVV.HDE.Model;

namespace VVVV.Nodes
{
	public class NodeSelectionListener : INodeSelectionListener
	{
		private CodeEditorPlugin FCodeEditorPlugin;
		
		public NodeSelectionListener(CodeEditorPlugin codeEditorPlugin)
		{
			FCodeEditorPlugin = codeEditorPlugin;
		}
		
		public void NodeSelectionChangedCB(INode[] nodes)
		{
			foreach (var node in nodes)
			{
				var nodeInfo = node.GetNodeInfo();
				var executable = nodeInfo.Executable;
				
				if (executable == null)
					continue;
				
				var project = executable.Project;
				
				if (project == null)
					continue;
				
				foreach (var doc in project.Documents)
				{
					if (doc is ITextDocument)
					{
						// TODO: FCodeEditorPlugin.Open(doc as ITextDocument, nodeInfo);
						FCodeEditorPlugin.Open(doc as ITextDocument);
					}
				}
			}
		}
	}
}
