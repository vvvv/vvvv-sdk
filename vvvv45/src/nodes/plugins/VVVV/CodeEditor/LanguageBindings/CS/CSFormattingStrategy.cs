using System;
using System.Collections.Generic;
using ICSharpCode.SharpDevelop.Dom.CSharp;
using ICSharpCode.TextEditor.Document;
using VVVV.Core.Model.CS;

namespace VVVV.HDE.CodeEditor.LanguageBindings.CS
{
	public class CSFormattingStrategy : DefaultFormattingStrategy
	{
		protected IDocumentLocator FDocumentLocator;
		
		public CSFormattingStrategy(IDocumentLocator documentLocator)
		{
			FDocumentLocator = documentLocator;
		}
		
		protected override int SmartIndentLine(ICSharpCode.TextEditor.TextArea textArea, int line)
		{
			return base.SmartIndentLine(textArea, line);
		}
		
		public override int SearchBracketBackward(IDocument document, int offset, char openBracket, char closingBracket)
		{
			if (IsInComment(document, offset))
				return -1;
			
			int baseResult = base.SearchBracketBackward(document, offset, openBracket, closingBracket);
			if (baseResult >= 0)
				return baseResult;
			
			bool inString = false;
			bool inLineComment = false;
			bool inBlockComment = false;
			
			var bracketStack = new Stack<int>();
			
			for (int i = 0; i < offset; i++)
			{
				char ch = document.GetCharAt(i);
				
				switch (ch)
				{
					case '/':
						if (!inString && !inLineComment && !inBlockComment)
						{
							inLineComment = document.GetCharAt(i + 1) == '/';
							if (!inLineComment)
								inBlockComment = document.GetCharAt(i + 1) == '*';
						}
						break;
					case '*':
						if (inBlockComment)
							inBlockComment = document.GetCharAt(i + 1) != '/';
						break;
					case '"':
						if (!inLineComment && !inBlockComment)
							inString = !inString;
						break;
					case '\n':
						inLineComment = false;
						inString = false;
						break;
					case '\r':
						inLineComment = false;
						inString = false;
						break;
					default:
						if (!inLineComment && !inBlockComment && !inString)
						{
							if (ch == openBracket)
								bracketStack.Push(i);
							else if (ch == closingBracket)
							{
								if (bracketStack.Count == 0)
									return -1;
								
								bracketStack.Pop();
							}
						}
						
						break;
				}
			}
			
			if (bracketStack.Count == 0)
				return -1;
			else
				return bracketStack.Pop();
		}
		
		public override int SearchBracketForward(IDocument document, int offset, char openBracket, char closingBracket)
		{
			if (IsInComment(document, offset))
				return -1;
			
			int baseResult = base.SearchBracketForward(document, offset, openBracket, closingBracket);
			if (baseResult >= 0)
				return baseResult;
			
			bool inString = false;
			bool inLineComment = false;
			bool inBlockComment = false;
			
			int brackets = 1;
		
			for (int i = offset; i < document.TextLength; i++)
			{
				char ch = document.GetCharAt(i);
				
				switch (ch)
				{
					case '/':
						if (!inString && !inLineComment && !inBlockComment)
						{
							inLineComment = document.GetCharAt(i + 1) == '/';
							if (!inLineComment)
								inBlockComment = document.GetCharAt(i + 1) == '*';
						}
						break;
					case '*':
						if (inBlockComment)
							inBlockComment = document.GetCharAt(i + 1) != '/';
						break;
					case '"':
						if (!inLineComment && !inBlockComment)
							inString = !inString;
						break;
					case '\n':
						inLineComment = false;
						inString = false;
						break;
					case '\r':
						inLineComment = false;
						inString = false;
						break;
					default:
						if (!inLineComment && !inBlockComment && !inString)
						{
							if (ch == openBracket)
								brackets++;
							else if (ch == closingBracket)
								brackets--;
							
							if (brackets == 0)
								return i;
						}
						
						break;
				}
			}
			
			return -1;
		}
		
		bool IsInComment(IDocument document, int offset)
		{
			var csDocument = FDocumentLocator.GetVDocument(document) as CSDocument;
			var expressionFinder = new CSharpExpressionFinder(csDocument.ParseInfo);
			return expressionFinder.FilterComments(document.GetText(0, offset + 1), ref offset) == null;
		}
	}
}
