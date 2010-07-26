using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Model;

namespace VVVV.HDE.CodeEditor
{
	public class NodeSelectionListener : INodeSelectionListener
	{
		private CodeEditorForm FCodeEditorForm;
		
		public NodeSelectionListener(CodeEditorForm codeEditorForm)
		{
			FCodeEditorForm = codeEditorForm;
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
						FCodeEditorForm.Open(doc as ITextDocument);
					}
				}
			}
		}
	}
}
