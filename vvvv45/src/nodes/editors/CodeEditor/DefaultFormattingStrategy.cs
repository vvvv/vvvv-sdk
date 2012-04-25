using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.CSharp;
using ICSharpCode.TextEditor.Actions;
using ICSharpCode.TextEditor.Document;

using VVVV.Core.Logging;

namespace VVVV.HDE.CodeEditor
{
	public abstract class DefaultFormattingStrategy : ICSharpCode.TextEditor.Document.DefaultFormattingStrategy
	{
		protected CodeEditor FEditor;
		
		public DefaultFormattingStrategy(CodeEditor editor)
		{
			FEditor = editor;
		}
		
		protected override int AutoIndentLine(ICSharpCode.TextEditor.TextArea textArea, int lineNumber)
		{
			return base.AutoIndentLine(textArea, lineNumber);
		}
		
		protected override int SmartIndentLine(ICSharpCode.TextEditor.TextArea textArea, int line)
		{
			var doc = textArea.Document;
			var lineSegment = doc.GetLineSegment(line);
			
			int bracketOffset = SearchBracketBackward(doc, lineSegment.Offset - 1, '{', '}');
			int bracketLine = bracketOffset >= 0 ? doc.GetLineNumberForOffset(bracketOffset) : 0;
			
			string indentationString = Tab.GetIndentationString(doc);
			if (LineStartsWithClosingBracket(textArea, line))
				indentationString = "";
			
			string indentation = bracketLine != 0 ? GetIndentation(textArea, bracketLine) + indentationString : "";
			string newLineText = indentation + TextUtilities.GetLineAsString(doc, line).Trim();
			
			SmartReplaceLine(doc, lineSegment, newLineText);
			
			return indentation.Length;
		}
		
		public override void FormatLine(ICSharpCode.TextEditor.TextArea textArea, int line, int cursorOffset, char ch)
		{
			try {
				if (ch == '\n')
				{
					int offset = Math.Min(textArea.Document.TextLength - 1, cursorOffset);
					int backwardBracketOffset = SearchBracketBackward(textArea.Document, offset - 1, '{', '}');
					int forwardBracketOffset = SearchBracketForward(textArea.Document, offset, '{', '}');
					
					if (backwardBracketOffset >= 0)
					{
						bool needClosingBracket = false;
						if (forwardBracketOffset >= 0)
						{
							// See if indentations of brackets match
							var backwardIndent = GetIndentation(textArea, textArea.Document.GetLineNumberForOffset(backwardBracketOffset)).Length;
							var forwardIndent = GetIndentation(textArea, textArea.Document.GetLineNumberForOffset(forwardBracketOffset)).Length;
							needClosingBracket = backwardIndent > forwardIndent;
						}
						else
							needClosingBracket = true;
						
						if (needClosingBracket)
						{
							textArea.Document.Insert(cursorOffset, "\n}");
							IndentLine(textArea, line + 1);
						}
					}
					
					textArea.Caret.Column = IndentLine(textArea, line);
				}
			} catch (Exception e) {
				FEditor.Logger.Log(e);
			}
		}
		
		protected bool LineStartsWithClosingBracket(ICSharpCode.TextEditor.TextArea textArea, int line)
		{
			return TextUtilities.GetLineAsString(textArea.Document, line).Trim().StartsWith("}");
		}
		
		protected bool LineEndsWithOpeningBracket(ICSharpCode.TextEditor.TextArea textArea, int line)
		{
			return TextUtilities.GetLineAsString(textArea.Document, line).Trim().EndsWith("{");
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
		
		protected abstract bool IsInComment(IDocument document, int offset);
	}
}
