using System;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace VVVV.HDE.CodeEditor
{
	public interface ICompletionProvider : ICompletionDataProvider
	{
		/// <summary>
		/// Determines whether the CompletionWindow should be displayed.
		/// </summary>
		bool TriggersCompletionWindow(TextEditorControl editorControl, char key);
	}
}
