using System;
using ICSharpCode.TextEditor;

namespace VVVV.HDE.CodeEditor
{
	public interface ICompletionWindowTrigger
	{
		/// <summary>
		/// Determines whether the CompletionWindow should be displayed.
		/// </summary>
		bool TriggersCompletionWindow(TextEditorControl editorControl, char key);
	}
}
