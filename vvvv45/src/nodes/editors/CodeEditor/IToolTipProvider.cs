
using System;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace VVVV.HDE.CodeEditor
{
	/// <summary>
	/// Used by the CodeEditor to show tool tips on mouse over.
	/// </summary>
	public interface IToolTipProvider
	{
		string GetToolTip(IDocument document, TextLocation textLocation);
	}
}
