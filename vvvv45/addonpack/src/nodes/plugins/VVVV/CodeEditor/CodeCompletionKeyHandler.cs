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
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace VVVV.HDE.CodeEditor
{
	/// <summary>
	/// Triggers the CompletionWindow.
	/// </summary>
	internal class CodeCompletionKeyHandler
	{
		protected TextEditorControl FEditorControl;
		protected ICompletionDataProvider FCompletionDataProvider;
		protected ICompletionWindowTrigger FCompletionWindowTrigger;
		protected string FFilename;
		
		protected CodeCompletionWindow FCodeCompletionWindow;
		
		private CodeCompletionKeyHandler(
			TextEditorControl editorControl,
			ICompletionDataProvider completionDataProvider,
			ICompletionWindowTrigger completionWindowTrigger,
			string filename)
		{
			FEditorControl = editorControl;
			FCompletionDataProvider = completionDataProvider;
			FCompletionWindowTrigger = completionWindowTrigger;
			FFilename = filename;
		}
		
		public static CodeCompletionKeyHandler Attach(
			TextEditorControl editorControl,
			ICompletionDataProvider completionDataProvider,
			ICompletionWindowTrigger completionWindowTrigger,
			string filename)
		{
			var handler = new CodeCompletionKeyHandler(editorControl, completionDataProvider, completionWindowTrigger, filename);
			editorControl.ActiveTextAreaControl.TextArea.KeyEventHandler += handler.TextAreaKeyEventHandler;
			
			// When the editor is disposed, close the code completion window
			editorControl.Disposed += handler.CloseCodeCompletionWindow;
			editorControl.Disposed += handler.EditorDisposedCB;
			
			return handler;
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
				if (FCodeCompletionWindow != null && !FCodeCompletionWindow.IsDisposed) {
					// If completion window is open and wants to handle the key, don't let the text area handle it.
					if (FCodeCompletionWindow.ProcessKeyEvent(key)) {
						return true;
					}
					if (FCodeCompletionWindow != null && !FCodeCompletionWindow.IsDisposed) {
						// code-completion window is still opened but did not want to handle
						// the keypress -> don't try to restart code-completion
						return false;
					}
				}
				
				if (FCompletionWindowTrigger.TriggersCompletionWindow(FEditorControl, key))
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
					
					FCodeCompletionWindow = CodeCompletionWindow.ShowCompletionWindow(
						FEditorControl.FindForm(),					// The parent window for the completion window
						FEditorControl, 					// The text editor to show the window for
						FFilename,		// Filename - will be passed back to the provider
						FCompletionDataProvider,		// Provider to get the list of possible completions
						key							// Key pressed - will be passed to the provider
					);
					if (FCodeCompletionWindow != null)
					{
						// ShowCompletionWindow can return null when the provider returns an empty list
						FCodeCompletionWindow.Closed += CloseCodeCompletionWindow;
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
			if (FCodeCompletionWindow != null)
			{
				FCodeCompletionWindow.Closed -= CloseCodeCompletionWindow;
				FCodeCompletionWindow.Dispose();
				FCodeCompletionWindow = null;
			}
		}
		
		void EditorDisposedCB(object sender, EventArgs e)
		{
			var editor = sender as TextEditorControl;
			editor.Disposed -= CloseCodeCompletionWindow;
			editor.ActiveTextAreaControl.TextArea.KeyEventHandler -= TextAreaKeyEventHandler;
		}
	}
}
