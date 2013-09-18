
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.NRefactoryResolver;
using ICSharpCode.TextEditor;
using VVVV.Core.Model.CS;

namespace VVVV.HDE.CodeEditor.LanguageBindings.CS
{
	public class CSCompletionBinding : ICompletionBinding
	{
		private CSCompletionProvider FCSCompletionProvider;
		private CtrlSpaceCompletionProvider FCtrlSpaceCompletionProvider;
		
		public CSCompletionBinding(CodeEditor editor)
		{
			FCSCompletionProvider = new CSCompletionProvider(editor);
			FCtrlSpaceCompletionProvider = new CtrlSpaceCompletionProvider(editor);
		}
		
		public void HandleKeyPress(CodeEditor editor, char key)
		{
			var sdDocument = editor.Document;
			var textAreaControl = editor.ActiveTextAreaControl;
			var textArea = textAreaControl.TextArea;
			
			// Do not show completion window for now if we're in a comment.
			if (IsInComment(editor))
				return;
			
			if (key == '(')
			{
				// Show MethodInsight window
				editor.ShowInsightWindow(new CSMethodInsightProvider(editor, textArea.Caret.Offset));
			}
			else if (key == ')')
			{
				editor.CloseInsightWindow();
			}
			else if (key == ',')
			{
				var parseInfo = ((CSDocument) editor.TextDocument).ParseInfo;
				var resolver = new NRefactoryResolver(LanguageProperties.CSharp);
				if (resolver.Initialize(parseInfo, textArea.Caret.Line + 1, textArea.Caret.Column + 1))
				{
					var textReader = resolver.ExtractCurrentMethod(textArea.Document.TextContent);
					
					// Find the function name from current offset (go backwards) and save all commas on this bracket level.
					int bracketCount = -1;
					int offset = textArea.Caret.Offset;
					var commaOffsets = new List<int>();
					var document = textArea.Document;
					
					commaOffsets.Add(offset);
					
					while (offset >= 0)
					{
						char currentChar = document.GetCharAt(offset);
						if (currentChar == '(')
							bracketCount++;
						else if (currentChar == ')')
							bracketCount--;
						else if (currentChar == ',' && bracketCount == -1)
							commaOffsets.Insert(0, offset);
						
						if (bracketCount == 0)
							break;
						else
							offset--;
					}
					
					// Show MethodInsight window
					if (offset >= 0)
						editor.ShowInsightWindow(new CSMethodInsightProvider(editor, offset, commaOffsets));
				}
			}
			else if (key == '.')
			{
				editor.ShowCompletionWindow(FCSCompletionProvider, key);
			}
			else if (char.IsLetter(key))
			{
				// Delete selected text (which will be overwritten anyways) before starting completion.
				if (textAreaControl.SelectionManager.HasSomethingSelected) {
					// allow code completion when overwriting an identifier
					int cursor = textAreaControl.SelectionManager.SelectionCollection[0].Offset;
					int endOffset = textAreaControl.SelectionManager.SelectionCollection[0].EndOffset;
					// but block code completion when overwriting only part of an identifier
					if (endOffset < sdDocument.TextLength && char.IsLetterOrDigit(sdDocument.GetCharAt(endOffset)))
						return;
					textAreaControl.SelectionManager.RemoveSelectedText();
					textAreaControl.Caret.Position = sdDocument.OffsetToPosition(cursor);
				}
				
				var document = editor.TextDocument as CSDocument;
				var finder = document.ExpressionFinder;
				
				
				try {
					var expressionResult = finder.FindExpression(editor.Document.TextContent, editor.ActiveTextAreaControl.Caret.Offset);
					
					if (expressionResult.Context != ExpressionContext.IdentifierExpected)
					{
						editor.ShowCompletionWindow(FCtrlSpaceCompletionProvider, key);
					}
				} catch (Exception e) {
					Debug.WriteLine(e.Message);
				}
				
			}
		}
		
		bool IsInComment(CodeEditor editor)
		{
			var sdDoc = editor.Document;
			var csDoc = editor.TextDocument as CSDocument;
			var expressionFinder = csDoc.ExpressionFinder;
			int cursor = editor.ActiveTextAreaControl.Caret.Offset - 1;
			return expressionFinder.FilterComments(sdDoc.GetText(0, cursor + 1), ref cursor) == null;
		}
	}
}
