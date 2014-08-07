
using System;

namespace VVVV.HDE.CodeEditor
{
	public interface ICompletionBinding
	{
        // Return true to handle the key press.
		bool HandleKeyPress(CodeEditor editor, char key);
	}
}
