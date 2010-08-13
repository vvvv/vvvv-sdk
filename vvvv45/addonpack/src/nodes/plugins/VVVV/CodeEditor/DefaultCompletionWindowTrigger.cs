using System;
using ICSharpCode.TextEditor;

namespace VVVV.HDE.CodeEditor
{
	public class DefaultCompletionWindowTrigger : ICompletionWindowTrigger
	{
		public virtual bool TriggersCompletionWindow(TextEditorControl editorControl, char key)
		{
			return false;
		}
	}
}
