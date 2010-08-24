
using System;

namespace VVVV.HDE.CodeEditor
{
	public interface ICompletionBinding
	{
		void HandleKeyPress(CodeEditor editor, char key);
	}
}
