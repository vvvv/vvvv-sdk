using System;
using ICSharpCode.SharpDevelop.Dom.CSharp;
using ICSharpCode.TextEditor;
using VVVV.Core.Model.CS;

namespace VVVV.HDE.CodeEditor.LanguageBindings.CS
{
	public class CSCompletionWindowTrigger : DefaultCompletionWindowTrigger
	{
		protected IDocumentLocator FDocumentLocator;
		
		public CSCompletionWindowTrigger(IDocumentLocator documentLocator)
		{
			FDocumentLocator = documentLocator;
		}
		
		public override bool TriggersCompletionWindow(TextEditorControl editorControl, char key)
		{
			if (IsInComment(editorControl))
				return false;
			
			return char.IsLetter(key) || key == '.';
		}
		
		bool IsInComment(TextEditorControl editor)
		{
			var sdDoc = editor.Document;
			var csDoc = FDocumentLocator.GetVDocument(sdDoc) as CSDocument;
			var expressionFinder = new CSharpExpressionFinder(csDoc.ParseInfo);
			int cursor = editor.ActiveTextAreaControl.Caret.Offset - 1;
			return expressionFinder.FilterComments(editor.Document.GetText(0, cursor + 1), ref cursor) == null;
		}
	}
}
