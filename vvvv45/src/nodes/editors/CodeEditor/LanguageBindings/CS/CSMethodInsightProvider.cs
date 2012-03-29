using System;
using System.Collections.Generic;
using System.Diagnostics;

using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.CSharp;
using ICSharpCode.SharpDevelop.Dom.NRefactoryResolver;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.InsightWindow;
using VVVV.Core.Model.CS;

namespace VVVV.HDE.CodeEditor.LanguageBindings.CS
{
	public class CSMethodInsightProvider : IInsightDataProvider
	{
		private CodeEditor FEditor;
		private List<IMethodOrProperty> FMethods;
		private IAmbience FAmbience;
		private int FOffset;
		private List<int> FCommaOffsets;
		private TextArea FActiveTextArea;
		
		public CSMethodInsightProvider(CodeEditor editor, int offset)
			: this(editor, offset, new List<int>())
		{
		}
		
		public CSMethodInsightProvider(CodeEditor editor, int offset, List<int> commaOffsets)
		{
			FEditor = editor;
			FOffset = offset;
			FCommaOffsets = commaOffsets;
			
			FMethods = new List<IMethodOrProperty>();
			FAmbience = new CSharpAmbience();
		}
		
		public int InsightDataCount {
			get
			{
				return FMethods.Count;
			}
		}
		
		public int DefaultIndex {
			get;
			private set;
		}
		
		public void SetupDataProvider(string fileName, TextArea textArea)
		{
			FActiveTextArea = textArea;
			FMethods.Clear();
			
			var document = textArea.Document;
			var csDoc = FEditor.TextDocument as CSDocument;
			var parseInfo = csDoc.ParseInfo;
			var finder = csDoc.ExpressionFinder;
			var compilationUnit = parseInfo.MostRecentCompilationUnit;
			var projectContent = compilationUnit.ProjectContent;
			var language = projectContent.Language;
			
			var expressionResult = finder.FindExpression(document.TextContent, FOffset);
			Debug.WriteLine(string.Format("Generating C# method insight data for expression: {0}", expressionResult));
			
			var resolver = new NRefactoryResolver(language);
			var resolveResult = resolver.Resolve(expressionResult, parseInfo, csDoc.TextContent);
			
			if (resolveResult != null)
			{
				Debug.WriteLine(string.Format("Resolve result: {0}", resolveResult));
				
				var methodGroupResolveResult = resolveResult as MethodGroupResolveResult;
				if (methodGroupResolveResult == null) return;
				
				bool classIsInInheritanceTree = false;
				var callingClass = methodGroupResolveResult.CallingClass;
				if (callingClass != null)
					classIsInInheritanceTree = callingClass.IsTypeInInheritanceTree(methodGroupResolveResult.ContainingType.GetUnderlyingClass());
				
				foreach (var methodGroup in  methodGroupResolveResult.Methods)
				{
					foreach (var method in methodGroup)
					{
						if (language.NameComparer.Equals(method.Name, methodGroupResolveResult.Name))
						{
							if (method.IsAccessible(methodGroupResolveResult.CallingClass, classIsInInheritanceTree))
							{
								FMethods.Add(method);
							}
						}
					}
				}
				
				// Set default index dependant on parameters.
				var arguments = new List<IReturnType>();
				
				foreach (var commaOffset in FCommaOffsets)
				{
					var argumentExpression = finder.FindExpression(document.TextContent, commaOffset);
					var argResolveResult = resolver.Resolve(argumentExpression, parseInfo, document.TextContent);
					if (argResolveResult != null)
					{
						arguments.Add(argResolveResult.ResolvedType);
						Debug.WriteLine(string.Format("Parameter: {0}", argResolveResult));
					}
				}
				
				bool overloadIsSure;
				var bestMatchingMethod = OverloadResolution.FindOverload(FMethods, arguments.ToArray(), true, false, out overloadIsSure);
				DefaultIndex = FMethods.IndexOf(bestMatchingMethod);
			}
		}
		
		public bool CaretOffsetChanged()
		{
			int caretOffset = FActiveTextArea.Caret.Offset;
			
			if (caretOffset <= FOffset) 
				return true;
			
			var document = FActiveTextArea.Document;
			char currentChar = document.GetCharAt(caretOffset);
			if (currentChar == ')' || currentChar == ';')
				return true;
			
			return false;
		}
		
		public string GetInsightData(int number)
		{
			var method = FMethods[number];
			return FAmbience.Convert(method);
		}
	}
}
