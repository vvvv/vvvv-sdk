using System;

using ICSharpCode.TextEditor.Document;

namespace VVVV.HDE.CodeEditor
{
	public class FXFormattingStrategy : DefaultFormattingStrategy
	{
		public FXFormattingStrategy (CodeEditor editor)
			: base (editor)
		{
		}
		
		protected override bool IsInComment (ICSharpCode.TextEditor.Document.IDocument document, int offset)
		{
			if (offset < 0 || offset > document.TextContent.Length) return false;
			
			int lineNumber = document.GetLineNumberForOffset(offset);
			string line = TextUtilities.GetLineAsString(document, lineNumber);
			return line.StartsWith("//");
		}
	}
}

