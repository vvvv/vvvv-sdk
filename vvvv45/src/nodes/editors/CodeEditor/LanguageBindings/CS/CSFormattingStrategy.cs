using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.CSharp;
using ICSharpCode.TextEditor.Actions;
using ICSharpCode.TextEditor.Document;
using VVVV.Core.Logging;
using VVVV.Core.Model.CS;

namespace VVVV.HDE.CodeEditor.LanguageBindings.CS
{
	public class CSFormattingStrategy : DefaultFormattingStrategy
	{
		public CSFormattingStrategy(CodeEditor editor)
			: base(editor)
		{
			FEditor = editor;
		}
		
		protected override bool IsInComment(IDocument document, int offset)
		{
			var csDocument = FEditor.TextDocument as CSDocument;
			var expressionFinder = new CSharpExpressionFinder(csDocument.ParseInfo);
			int length = offset + 1;
			if (length <= document.TextLength)
				return expressionFinder.FilterComments(document.GetText(0, length), ref offset) == null;
			else
			{
				return false;
			}
		}
	}
}
