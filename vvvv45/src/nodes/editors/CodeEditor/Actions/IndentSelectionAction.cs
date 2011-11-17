using System;
using System.Windows.Forms;
using ICSharpCode.TextEditor.Actions;

namespace VVVV.HDE.CodeEditor.Actions
{
	/// <summary>
	/// Indents selected lines or all lines if nothing is selected.
	/// </summary>
	public class IndentSelectionAction : AbstractEditAction
	{
		public override void Execute(ICSharpCode.TextEditor.TextArea textArea)
		{
			var selectionManager = textArea.SelectionManager;
			if (selectionManager.HasSomethingSelected)
			{
				foreach (var selection in selectionManager.SelectionCollection)
				{
					textArea.Document.FormattingStrategy.IndentLines(textArea, selection.StartPosition.Line, selection.EndPosition.Line);
				}
			}
			else
				textArea.Document.FormattingStrategy.IndentLines(textArea, 0, textArea.Document.TotalNumberOfLines - 1);
		}
	}
}
