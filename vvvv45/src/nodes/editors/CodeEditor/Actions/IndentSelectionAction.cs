using System;
using System.Windows.Forms;
using ICSharpCode.TextEditor.Actions;

namespace VVVV.HDE.CodeEditor.Actions
{
	/// <summary>
	/// Indents selected lines.
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
					textArea.Document.FormattingStrategy.IndentLine(textArea, selection.StartPosition.Line);
				}
			}
			else
				textArea.Document.FormattingStrategy.IndentLines(textArea, 0, textArea.Document.TotalNumberOfLines - 1);
		}
	}
}
