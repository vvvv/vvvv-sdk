using System;
using VVVV.Core.Model;
using VVVV.Core.Model.CS;
using VVVV.Core.Model.FX;
using VVVV.PluginInterfaces.V2;

namespace VVVV.HDE.CodeEditor
{
    public class MouseClickListener : IMouseClickListener
    {
        private CodeEditorForm FCodeEditorForm;
        
        public MouseClickListener(CodeEditorForm codeEditorForm)
        {
            FCodeEditorForm = codeEditorForm;
        }
        
        public void MouseDownCB(INode node, Mouse_Buttons button, Modifier_Keys keys)
        {
            if ((node != null) && (button == Mouse_Buttons.Right))
            {
                var nodeInfo = node.GetNodeInfo();
                var executable = nodeInfo.Executable;
                
                if (executable == null)
                    return;
                
                var project = executable.Project;
                
                if (project == null)
                    return;
                
                if (project is CSProject)
                {
                    // Try to find file where NodeInfo is defined.
                    bool found = false;
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
                                        var pluginInfoName = typeof(PluginInfoAttribute).Name;
                                        var pluginInfoShortName = pluginInfoName.Replace("Attribute", "");
                                        if (attributeType.Name == pluginInfoName || attributeType.Name == pluginInfoShortName)
                                        {
                                            // Check name
                                            string name = null;
                                            if (attribute.NamedArguments.ContainsKey("Name"))
                                                name = (string) attribute.NamedArguments["Name"];
                                            else if (attribute.PositionalArguments.Count >= 0)
                                                name = (string) attribute.PositionalArguments[0];
                                            
                                            if (name != nodeInfo.Name)
                                                continue;
                                            
                                            // Check category
                                            string category = null;
                                            if (attribute.NamedArguments.ContainsKey("Category"))
                                                category = (string) attribute.NamedArguments["Category"];
                                            else if (attribute.PositionalArguments.Count >= 1)
                                                category = (string) attribute.PositionalArguments[1];
                                            
                                            if (category != nodeInfo.Category)
                                                continue;

                                            // Possible match
                                            bool match = true;
                                            
                                            // Check version
                                            if (nodeInfo.Version != null)
                                            {
                                                string version = null;
                                                if (attribute.NamedArguments.ContainsKey("Version"))
                                                    version = (string) attribute.NamedArguments["Version"];
                                                else if (attribute.PositionalArguments.Count >= 2)
                                                    version = (string) attribute.PositionalArguments[2];
                                                
                                                match = version == nodeInfo.Version;
                                            }
                                            
                                            if (match)
                                            {
                                                found = true;
                                                var tabPage = FCodeEditorForm.Open(csDoc);
                                                var codeEditor = tabPage.Controls[0] as CodeEditor;
                                                codeEditor.JumpTo(attribute.Region.BeginLine - 1);
                                                break;
                                            }
                                        }
                                    }
                                    
                                    if (found) break;
                                }
                            }
                        }
                        
                        if (found) break;
                    }
                }
                else if (project is FXProject)
				{
					// Open FX file only
					foreach (var doc in project.Documents)
					{
						if (doc is FXDocument)
						{
							FCodeEditorForm.Open(doc as ITextDocument);
						}
					}
				}
                else
                {
                    // Open all documents
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
        
        public void MouseUpCB(INode node, Mouse_Buttons button, Modifier_Keys keys)
        {
            throw new NotImplementedException();
        }
    }
}
