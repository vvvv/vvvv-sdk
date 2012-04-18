
using System;
using VVVV.Core.Logging;

namespace VVVV.HDE.CodeEditor.LanguageBindings.FX
{
	/// <summary>
	/// Description of FXCompletionBinding.
	/// </summary>
	public class FXCompletionBinding : ICompletionBinding
	{
		private FXCompletionProvider FCompletionProvider;
		
		public FXCompletionBinding(CodeEditor editor)
		{
			FCompletionProvider = new FXCompletionProvider(editor);
		}
		
		public void HandleKeyPress(CodeEditor editor, char key)
		{
		    // Do not show completion window for now if we're in a comment.
			if (IsInComment(editor))
				return;
		    
			if (char.IsLetter(key))
			{
				editor.ShowCompletionWindow(FCompletionProvider, key);
			}
		}
		
		bool IsInComment(CodeEditor editor)
		{
			for (int i = editor.ActiveTextAreaControl.Caret.Offset - 2; i >= 0; i--)
			{
			    string comment = editor.Text[i].ToString() + editor.Text[i+1].ToString();
			    if (comment == @"/*")
			        return true;
			    else if (comment == @"*/")
			        break;
			}
			
			var line = editor.Document.GetLineSegment(editor.ActiveTextAreaControl.Caret.Line);
			var text = editor.Document.GetText(line.Offset, editor.ActiveTextAreaControl.Caret.Offset - line.Offset);
			if (text.Contains(@"//"))
			    return true;
			   
			//else
			return false;			    
		}
	}
}
