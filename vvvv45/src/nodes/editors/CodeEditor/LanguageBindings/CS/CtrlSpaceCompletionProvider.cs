using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.NRefactoryResolver;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using VVVV.Core.Model.CS;
using Dom = ICSharpCode.SharpDevelop.Dom;

namespace VVVV.HDE.CodeEditor.LanguageBindings.CS
{
	public class CtrlSpaceCompletionProvider : DefaultCompletionProvider
	{
		private CodeEditor FEditor;
		
		public CtrlSpaceCompletionProvider(CodeEditor editor)
		{
			FEditor = editor;
		}
		
		public override ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
		{
			var resultList = new List<ICompletionData>();
			
			var document = FEditor.TextDocument as CSDocument;
            var parseInfo = document.ParseInfo;
            var finder = document.ExpressionFinder;
            var text = textArea.Document.GetText(0, textArea.Caret.Offset);
            var expressionResult = finder.FindExpression(text, textArea.Caret.Offset);
            if (expressionResult.Region.IsEmpty) {
                expressionResult.Region = new Dom.DomRegion(textArea.Caret.Line + 1, textArea.Caret.Column + 1);
            }
            var resolver = new NRefactoryResolver(LanguageProperties.CSharp);
            
            Debug.WriteLine(string.Format("Generating C# CTRL space completion data for expression result {0}", expressionResult));
			
			PreSelection = string.Empty;
			var completionData = resolver.CtrlSpace(textArea.Caret.Line + 1,
			                                        textArea.Caret.Column + 1,
			                                        parseInfo,
			                                        textArea.Document.TextContent,
			                                        expressionResult.Context);

            var result = Enumerable.Empty<ICompletionData>();
            if (completionData != null)
                // Add the async keyword
                result = new ICompletionData[] { new DefaultCompletionData("async", 5) }
                    .Concat(GetCompletionData(completionData, expressionResult.Context));
            return result.ToArray();
		}
		
		IEnumerable<ICompletionData> GetCompletionData(ArrayList completionData, Dom.ExpressionContext context)
        {
            // used to store the method names for grouping overloads
            Dictionary<string, CSCompletionData> nameDictionary = new Dictionary<string, CSCompletionData>();

            // Add the completion data as returned by SharpDevelop.Dom to the
            // list for the text editor
            foreach (object obj in completionData) {
                if (!context.ShowEntry(obj))
                    continue;
                
                if (obj is string) {
                    // namespace names are returned as string
                    yield return new DefaultCompletionData((string)obj, "namespace " + obj, 5);
                } else if (obj is Dom.IClass) {
                    Dom.IClass c = (Dom.IClass)obj;
                    yield return new CSCompletionData(c);
                } else if (obj is Dom.IMember) {
                    Dom.IMember m = (Dom.IMember)obj;
                    if (m is Dom.IMethod && ((m as Dom.IMethod).IsConstructor)) {
                        // Skip constructors
                        continue;
                    }
                    // Group results by name and add "(x Overloads)" to the
                    // description if there are multiple results with the same name.
                    
                    CSCompletionData data;
                    if (nameDictionary.TryGetValue(m.Name, out data)) {
                        data.AddOverload();
                    } else {
                        nameDictionary[m.Name] = data = new CSCompletionData(m);
                        yield return data;
                    }
                } else {
                    // Current ICSharpCode.SharpDevelop.Dom should never return anything else
                    throw new NotSupportedException();
                }
            }
        }
	}
}
