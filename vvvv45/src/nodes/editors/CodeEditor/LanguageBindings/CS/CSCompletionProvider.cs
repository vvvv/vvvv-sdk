using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.CSharp;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using VVVV.Core.Model.CS;
using Dom = ICSharpCode.SharpDevelop.Dom;
using NRefactoryResolver = ICSharpCode.SharpDevelop.Dom.NRefactoryResolver.NRefactoryResolver;

namespace VVVV.HDE.CodeEditor.LanguageBindings.CS
{
	public class CSCompletionProvider : DefaultCompletionProvider
	{
		private CodeEditor FEditor;
		
		public CSCompletionProvider(CodeEditor editor)
		{
			FEditor = editor;
		}
		
		public override ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
		{
			// We can return code-completion items like this:
			
			//return new ICompletionData[] {
			//	new DefaultCompletionData("Text", "Description", 1)
			//};
			
			var resultList = new List<ICompletionData>();
			
			var document = FEditor.TextDocument as CSDocument;
			var parseInfo = document.ParseInfo;
			var projectContent = parseInfo.MostRecentCompilationUnit.ProjectContent;
			
			var finder = document.ExpressionFinder;
			var text = textArea.Document.GetText(0, textArea.Caret.Offset);
			var expressionResult = finder.FindExpression(text, textArea.Caret.Offset);
			if (expressionResult.Region.IsEmpty) {
				expressionResult.Region = new Dom.DomRegion(textArea.Caret.Line + 1, textArea.Caret.Column + 1);
			}
			var resolver = new NRefactoryResolver(LanguageProperties.CSharp);
			
			Debug.WriteLine(string.Format("Generating C# completion data for expression result {0}", expressionResult));

			ArrayList completionData = null;
			PreSelection = null;
			var rr = resolver.Resolve(expressionResult,
			                           parseInfo,
			                           textArea.MotherTextEditorControl.Text);
			
			if (rr != null)
				completionData = rr.GetCompletionData(projectContent);
			
			if (completionData != null)
				AddCompletionData(ref resultList, completionData, expressionResult.Context);
			
			return resultList.ToArray();
		}
		
		void AddCompletionData(ref List<ICompletionData> resultList, ArrayList completionData, Dom.ExpressionContext context)
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
					resultList.Add(new DefaultCompletionData((string)obj, "namespace " + obj, 5));
				} else if (obj is Dom.IClass) {
					Dom.IClass c = (Dom.IClass)obj;
					resultList.Add(new CSCompletionData(c));
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
						resultList.Add(data);
					}
				} else {
					// Current ICSharpCode.SharpDevelop.Dom should never return anything else
					throw new NotSupportedException();
				}
			}
		}
	}
}
