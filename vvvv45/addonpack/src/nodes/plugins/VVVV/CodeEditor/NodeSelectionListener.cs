
using System;
using VVVV.PluginInterfaces.V1;
using VVVV.HDE.Model;

namespace VVVV.Nodes
{
	public class NodeSelectionListener : INodeSelectionListener
	{
		private CodeEditor FCodeEditor;
		
		public NodeSelectionListener(CodeEditor codeEditor)
		{
			FCodeEditor = codeEditor;
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
						FCodeEditor.Open(doc as ITextDocument);
					}
				}
			}
		}
	}
}
