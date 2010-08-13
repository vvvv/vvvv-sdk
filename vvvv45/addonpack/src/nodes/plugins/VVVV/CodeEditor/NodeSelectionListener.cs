using System;
using VVVV.Core.Model;
using VVVV.Core.Model.CS;
using VVVV.PluginInterfaces.V2;

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
					if (doc is CSDocument)
					{
						var csDoc = doc as CSDocument;
						var parseInfo = csDoc.ParseInfo;
						var compilationUnit = parseInfo.MostRecentCompilationUnit;
						
						if (compilationUnit != null)
						{
							foreach (var clss in compilationUnit.Classes)
							{
								foreach (var attribute in clss.Attributes)
								{
									var attributeType = attribute.AttributeType;
									if (attributeType.Name == typeof(PluginInfoAttribute).Name)
									{
										if ((string) attribute.NamedArguments["Name"] == nodeInfo.Name &&
										    (string) attribute.NamedArguments["Category"] == nodeInfo.Category &&
										    (string) attribute.NamedArguments["Version"] == nodeInfo.Version)
										{
											var tabPage = FCodeEditorForm.Open(csDoc);
											var codeEditor = tabPage.Controls[0] as CodeEditor;
											codeEditor.JumpTo(attribute.Region.BeginLine - 1);
										}
									}
								}
							}
						}
					}
					else if (doc is ITextDocument)
					{
						// TODO: FCodeEditorPlugin.Open(doc as ITextDocument, nodeInfo);
						FCodeEditorForm.Open(doc as ITextDocument);
					}
				}
			}
		}
	}
}
