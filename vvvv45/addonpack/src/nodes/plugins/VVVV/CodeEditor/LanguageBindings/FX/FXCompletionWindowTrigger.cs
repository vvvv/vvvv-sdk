using System;
using ICSharpCode.TextEditor;

namespace VVVV.HDE.CodeEditor.LanguageBindings.FX
{
	public class FXCompletionWindowTrigger : DefaultCompletionWindowTrigger
	{
		public override bool TriggersCompletionWindow(TextEditorControl editorControl, char key)
		{
			return char.IsLetter(key) || key == '.';
		}
	}
}
