// CSharp Editor Example with Code Completion
// Copyright (c) 2006, Daniel Grunwald
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are
// permitted provided that the following conditions are met:
// 
// - Redistributions of source code must retain the above copyright notice, this list
//   of conditions and the following disclaimer.
// 
// - Redistributions in binary form must reproduce the above copyright notice, this list
//   of conditions and the following disclaimer in the documentation and/or other materials
//   provided with the distribution.
// 
// - Neither the name of the ICSharpCode team nor the names of its contributors may be used to
//   endorse or promote products derived from this software without specific prior written
//   permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS &AS IS& AND ANY EXPRESS
// OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
// IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
// OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using ICSharpCode.SharpDevelop.Dom.CSharp;
using VVVV.Core.Model;

namespace VVVV.HDE.CodeEditor
{
	class CodeCompletionKeyHandler
	{
		protected TextEditorControl FEditorControl;
		protected CodeCompletionWindow codeCompletionWindow;
		protected IParseInfoProvider FParseInfoProvider;
		protected ITextDocument FDocument;
		protected ImageList FImageList;
		protected Dictionary<string, string> FHLSLReference;
		protected Dictionary<string, string> FTypeReference;
		
		private CodeCompletionKeyHandler(
			IParseInfoProvider parseInfoProvider,
			ITextDocument document,
			ImageList imageList,
			TextEditorControl editorControl,
			Dictionary<string, string> hlslReference,
			Dictionary<string, string> typeReference)
		{
			FParseInfoProvider = parseInfoProvider;
			FDocument = document;
			FImageList = imageList;
			FEditorControl = editorControl;
			FHLSLReference = hlslReference;
			FTypeReference = typeReference;
		}
		
		public static CodeCompletionKeyHandler Attach(
			IParseInfoProvider parseInfoProvider,
			ITextDocument document,
			ImageList imageList,
			TextEditorControl editorControl,
			Dictionary<string, string> hlslReference,
			Dictionary<string, string> typeReference)
		{
			CodeCompletionKeyHandler h = new CodeCompletionKeyHandler(parseInfoProvider, document, imageList, editorControl, hlslReference, typeReference);
			
			editorControl.ActiveTextAreaControl.TextArea.KeyEventHandler += h.TextAreaKeyEventHandler;
			
			// When the editor is disposed, close the code completion window
			editorControl.Disposed += h.CloseCodeCompletionWindow;
			editorControl.Disposed += h.EditorDisposedCB;
			
			return h;
		}

		/// <summary>
		/// Return true to handle the keypress, return false to let the text area handle the keypress
		/// </summary>
		bool inHandleKeyPress;
		bool TextAreaKeyEventHandler(char key)
		{
			if (inHandleKeyPress)
				return false;
			
			inHandleKeyPress = true;
			
			try
			{
				if (codeCompletionWindow != null && !codeCompletionWindow.IsDisposed) {
					// If completion window is open and wants to handle the key, don't let the text area handle it.
					if (codeCompletionWindow.ProcessKeyEvent(key)) {
						return true;
					}
					if (codeCompletionWindow != null && !codeCompletionWindow.IsDisposed) {
						// code-completion window is still opened but did not want to handle
						// the keypress -> don't try to restart code-completion
						return false;
					}
				}
				
				if (IsInComment(FEditorControl))
					return false;
				
				if (char.IsLetter(key) || key == '.')
				{
					// Delete selected text (which will be overwritten anyways) before starting completion.
					if (FEditorControl.ActiveTextAreaControl.SelectionManager.HasSomethingSelected) {
						// allow code completion when overwriting an identifier
						int cursor = FEditorControl.ActiveTextAreaControl.SelectionManager.SelectionCollection[0].Offset;
						int endOffset = FEditorControl.ActiveTextAreaControl.SelectionManager.SelectionCollection[0].EndOffset;
						// but block code completion when overwriting only part of an identifier
						if (endOffset < FEditorControl.Document.TextLength && char.IsLetterOrDigit(FEditorControl.Document.GetCharAt(endOffset)))
							return false;
						FEditorControl.ActiveTextAreaControl.SelectionManager.RemoveSelectedText();
						FEditorControl.ActiveTextAreaControl.Caret.Position = FEditorControl.Document.OffsetToPosition(cursor);
					}
					
					var completionDataProvider = new CodeCompletionProvider(FParseInfoProvider, FDocument, FImageList, FHLSLReference, FTypeReference);

					codeCompletionWindow = CodeCompletionWindow.ShowCompletionWindow(
						FEditorControl.FindForm(),					// The parent window for the completion window
						FEditorControl, 					// The text editor to show the window for
						FDocument.Location.AbsolutePath,		// Filename - will be passed back to the provider
						completionDataProvider,		// Provider to get the list of possible completions
						key							// Key pressed - will be passed to the provider
					);
					if (codeCompletionWindow != null)
					{
						// ShowCompletionWindow can return null when the provider returns an empty list
						codeCompletionWindow.Closed += CloseCodeCompletionWindow;
					}
				}
			}
			finally
			{
				inHandleKeyPress = false;
			}
			
			return false;
		}
		
		void CloseCodeCompletionWindow(object sender, EventArgs e)
		{
			if (codeCompletionWindow != null)
			{
				codeCompletionWindow.Closed -= CloseCodeCompletionWindow;
				codeCompletionWindow.Dispose();
				codeCompletionWindow = null;
			}
		}
		
		void EditorDisposedCB(object sender, EventArgs e)
		{
			var editor = sender as TextEditorControl;
			editor.Disposed -= CloseCodeCompletionWindow;
			editor.ActiveTextAreaControl.TextArea.KeyEventHandler -= TextAreaKeyEventHandler;
		}
		
		bool IsInComment(TextEditorControl editor)
		{
			var expressionFinder = new CSharpExpressionFinder(FParseInfoProvider.GetParseInfo(FDocument));
			int cursor = editor.ActiveTextAreaControl.Caret.Offset - 1;
			return expressionFinder.FilterComments(editor.Document.GetText(0, cursor + 1), ref cursor) == null;
		}
	}
}
