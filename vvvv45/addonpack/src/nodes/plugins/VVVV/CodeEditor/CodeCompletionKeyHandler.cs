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
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

using VVVV.Nodes;

namespace CSharpEditor
{
	class CodeCompletionKeyHandler
	{
		CodeEditor FCodeEditor;
		TextEditorControl FEditorControl;
		CodeCompletionWindow codeCompletionWindow;
		
		private CodeCompletionKeyHandler(CodeEditor codeEditor, TextEditorControl editorControl)
		{
			this.FCodeEditor = codeEditor;
			this.FEditorControl = editorControl;
		}
		
		public static CodeCompletionKeyHandler Attach(CodeEditor mainForm, TextEditorControl editor)
		{
			CodeCompletionKeyHandler h = new CodeCompletionKeyHandler(mainForm, editor);
			
			editor.ActiveTextAreaControl.TextArea.KeyEventHandler += h.TextAreaKeyEventHandler;
			
			// When the editor is disposed, close the code completion window
			editor.Disposed += h.CloseCodeCompletionWindow;
			editor.Disposed += h.EditorDisposedCB;
			
			return h;
		}
		
		/// <summary>
		/// Return true to handle the keypress, return false to let the text area handle the keypress
		/// </summary>
		bool TextAreaKeyEventHandler(char key)
		{
			if (codeCompletionWindow != null && !codeCompletionWindow.IsDisposed) 
			{
				// If completion window is open and wants to handle the key, don't let the text area handle it.
				return codeCompletionWindow.ProcessKeyEvent(key);
			}
			
			if (char.IsLetter(key) || key == '.') 
			{
				ICompletionDataProvider completionDataProvider = new CodeCompletionProvider(FCodeEditor);

				codeCompletionWindow = CodeCompletionWindow.ShowCompletionWindow(
					FCodeEditor.DummyForm,					// The parent window for the completion window
					FEditorControl, 					// The text editor to show the window for
					FCodeEditor.Document.Location.AbsolutePath,		// Filename - will be passed back to the provider
					completionDataProvider,		// Provider to get the list of possible completions
					key							// Key pressed - will be passed to the provider
				);
				if (codeCompletionWindow != null) 
				{
					// ShowCompletionWindow can return null when the provider returns an empty list
					codeCompletionWindow.Closed += CloseCodeCompletionWindow;
				}
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
	}
}
