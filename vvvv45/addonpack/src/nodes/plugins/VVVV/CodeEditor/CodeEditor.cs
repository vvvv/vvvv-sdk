#region licence/info

//////project name
//vvvv plugin template with gui

//////description
//basic vvvv plugin template with gui.
//Copy this an rename it, to write your own plugin node.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop

//////dependencies
//VVVV.PluginInterfaces.V1;

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Resources;
using System.Reflection;
using System.CodeDom.Compiler;
using System.IO;
using System.Diagnostics;

using VVVV.Core.Model;
using VVVV.Core.Model.CS;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils;


using ICSharpCode.TextEditor;
using SD = ICSharpCode.TextEditor.Document;
using Dom = ICSharpCode.SharpDevelop.Dom;

namespace VVVV.HDE.CodeEditor
{
	
	//class definition, inheriting from UserControl for the GUI stuff
	public class CodeEditor: UserControl
	{
		#region field declaration
		internal Dom.DefaultProjectContent FProjectContent;
		
		private ICSharpCode.TextEditor.TextEditorControl FTextEditorControl;
		private BackgroundCodeParser FBGCodeParser;
		private System.Windows.Forms.Timer FTimer;
		#endregion field declaration
		
		#region Properties
		public IParseInfoProvider ParseInfoProvider { get; private set; }
		
		public ImageList ImageList { get; private set; }
		
		public ITextDocument Document { get; private set; }
		
		public SD.IDocument SDDocument { get; private set; }
		
		public bool IsDirty
		{
			get
			{
				return Document.IsDirty;
			}
		}
		#endregion
		
		#region constructor/destructor
		public CodeEditor(IParseInfoProvider parseInfoProvider, ITextDocument doc, ImageList imageList, Dictionary<string, string> hlslReference, Dictionary<string, string> typeReference)
		{
			ParseInfoProvider = parseInfoProvider;
			Document = doc;
			ImageList = imageList;
			
			// The InitializeComponent() call is required for Windows Forms designer support.
			InitializeComponent();
			
			SDDocument = FTextEditorControl.Document;
			
			FTextEditorControl.TextEditorProperties.MouseWheelTextZoom = false;
			FTextEditorControl.TextEditorProperties.LineViewerStyle = SD.LineViewerStyle.FullRow;
			FTextEditorControl.TextEditorProperties.ShowMatchingBracket = true;
			FTextEditorControl.TextEditorProperties.AutoInsertCurlyBracket = true;
			
			var highlighter = SD.HighlightingManager.Manager.FindHighlighterForFile(doc.Location.LocalPath);
			if (highlighter.Name == "HLSL")
				FTextEditorControl.EnableFolding = false;

			FTextEditorControl.SetHighlighting(highlighter.Name);
			
			// Setup code folding
			var parseInfo = ParseInfoProvider.GetParseInfo(doc);
			FTextEditorControl.Document.FoldingManager.FoldingStrategy = new ParserFoldingStrategy();
			FTextEditorControl.Document.FormattingStrategy = new CSharpFormattingStrategy(parseInfoProvider);
			
			FProjectContent = ParseInfoProvider.GetProjectContent(Document.Project);
			FProjectContent.Language = Dom.LanguageProperties.CSharp;
			
			HostCallbackImplementation.Register(FProjectContent);
			CodeCompletionKeyHandler.Attach(parseInfoProvider, doc, imageList, FTextEditorControl, hlslReference, typeReference);
			ToolTipProvider.Attach(parseInfoProvider, doc, FTextEditorControl);
			
			FBGCodeParser = new BackgroundCodeParser(parseInfoProvider, doc, FTextEditorControl.Document);
			
			FTextEditorControl.ActiveTextAreaControl.TextArea.KeyDown += TextAreaKeyDownCB;
			FTextEditorControl.ActiveTextAreaControl.TextArea.MouseClick += FTextEditorControl_ActiveTextAreaControl_TextArea_MouseClick;
			FTextEditorControl.TextChanged += TextEditorControlTextChangedCB;
			
			// Everytime the project is compiled update the error highlighting.
			Document.Project.CompileCompleted += CompileCompletedCB;
			
			// Start parsing after 500ms have passed after last key stroke.
			FTimer = new System.Windows.Forms.Timer();
			FTimer.Interval = 500;
			FTimer.Tick += TimerTickCB;
		}

		void FTextEditorControl_ActiveTextAreaControl_TextArea_MouseClick(object sender, MouseEventArgs e)
		{
			// if (Control.ModifierKeys != Keys.Control)
			//     return;
			
			var line = FTextEditorControl.Document.GetLineSegment(FTextEditorControl.ActiveTextAreaControl.Caret.Line);
			if (line.Words.Count > 0 && line.Words[1].Word == "include")
			{
				var strings = line.Words.ConvertAll<string>(new Converter<SD.TextWord, string>(delegate(SD.TextWord word) {return word.Word;}));
				var text = string.Join("", strings.ToArray());
				text = text.Replace("#include\"", "").Trim(new char[1]{'"'});
				
				//open include document
				foreach (var doc in Document.Project.Documents)
				{
					if (doc.Name == text)
					{
						(ParseInfoProvider as CodeEditorForm).Open(doc as ITextDocument);
						break;
					}
				}
			}
		}
		#endregion constructor/destructor
		
		#region Windows Forms designer
		private void InitializeComponent()
		{
			this.FTextEditorControl = new ICSharpCode.TextEditor.TextEditorControl();
			this.SuspendLayout();
			// 
			// FTextEditorControl
			// 
			this.FTextEditorControl.AutoScroll = true;
			this.FTextEditorControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.FTextEditorControl.ConvertTabsToSpaces = true;
			this.FTextEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FTextEditorControl.IsReadOnly = false;
			this.FTextEditorControl.Location = new System.Drawing.Point(0, 0);
			this.FTextEditorControl.Name = "FTextEditorControl";
			this.FTextEditorControl.Size = new System.Drawing.Size(632, 453);
			this.FTextEditorControl.TabIndex = 2;
			this.FTextEditorControl.Text = "float";
			// 
			// CodeEditor
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.Controls.Add(this.FTextEditorControl);
			this.Name = "CodeEditor";
			this.Size = new System.Drawing.Size(632, 453);
			this.ResumeLayout(false);
		}
		#endregion Windows Forms designer
		
		#region IDisposable
		private bool FDisposed = false;
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the
		// runtime from inside the finalizer and you should not reference
		// other objects. Only unmanaged resources can be disposed.
		protected override void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!FDisposed)
			{
				if(disposing)
				{
					// Dispose managed resources.
					if (FTextEditorControl != null)
						FTextEditorControl.ActiveTextAreaControl.TextArea.KeyDown -= TextAreaKeyDownCB;
					
					if (Document != null)
					{
						Document.ContentChanged -= TextDocumentContentChangedCB;
						Document.Project.CompileCompleted -= CompileCompletedCB;
					}
					
					if (FTimer != null)
						FTimer.Dispose();
					
					HostCallbackImplementation.UnRegister(this.FProjectContent);
					FTextEditorControl.Dispose();
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				
				// Note that this is not thread safe.
				// Another thread could start disposing the object
				// after the managed resources are disposed,
				// but before the disposed flag is set to true.
				// If thread safety is necessary, it must be
				// implemented by the client.
			}
			FDisposed = true;
			
			base.Dispose(disposing);
		}
		#endregion IDisposable
		
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			FTextEditorControl.Document.TextContent = Document.TextContent;
			var parserInfo = ParseInfoProvider.GetParseInfo(Document);
			if (parserInfo != null)
				FTextEditorControl.Document.FoldingManager.UpdateFoldings(Document.Location.LocalPath, parserInfo);
			Document.ContentChanged += TextDocumentContentChangedCB;
		}
		
		private void SyncControlWithDocument()
		{
			Document.ContentChanged -= TextDocumentContentChangedCB;
			Document.TextContent = FTextEditorControl.Document.TextContent;
			Document.ContentChanged += TextDocumentContentChangedCB;
		}

		/// <summary>
		/// Updates the underlying ITextDocument from changes made in the editor and parses the document.
		/// This method is called once after 500ms have passed after last key stroke (to save CPU cycles).
		/// </summary>
		void TimerTickCB(object sender, EventArgs args)
		{
			FTimer.Stop();
			if (Document != null)
			{
				SyncControlWithDocument();
				
//				EditorPlugin.PluginHost.Log(TLogType.Debug, "Parsing " + Document.Location + " ...");
				FBGCodeParser.RunParserAsync();
			}
		}
		
		/// <summary>
		/// Restarts the timer everytime the content of the editor changes.
		/// </summary>
		void TextEditorControlTextChangedCB(object sender, EventArgs e)
		{
			FTimer.Stop();
			FTimer.Start();
		}
		
		/// <summary>
		/// Keeps the editor and the underlying ITextDocument in sync.
		/// </summary>
		void TextDocumentContentChangedCB(IDocument doc, string content)
		{
			int length = FTextEditorControl.Document.TextContent.Length;
			FTextEditorControl.Document.Replace(0, length, content);
			FTextEditorControl.Refresh();
		}
		
		void TextAreaKeyDownCB(object sender, KeyEventArgs args)
		{
			if (args.Control && args.KeyCode == Keys.S)
			{
				SyncControlWithDocument();
				Document.Save();
				args.Handled = true;
			}
			else
				args.Handled = false;
		}
		
		List<SD.TextMarker> FErrorMarkers = new List<SD.TextMarker>();
		void CompileCompletedCB(IProject project, CompilerResults results)
		{
			var doc = FTextEditorControl.Document;
			
			// Clear all previous error markers.
			foreach (var marker in FErrorMarkers)
			{
				doc.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, doc.GetLineNumberForOffset(marker.Offset)));
				doc.MarkerStrategy.RemoveMarker(marker);
			}
			FErrorMarkers.Clear();
			
			if (results.Errors.HasErrors)
			{
				foreach (var error in results.Errors)
				{
					if (error is CompilerError)
					{
						var compilerError = error as CompilerError;
						var path = Path.GetFullPath(compilerError.FileName);

						if (path.ToLower() == Document.Location.LocalPath.ToLower())
						{
							var location = new TextLocation(compilerError.Column - 1, compilerError.Line - 1);
							var offset = doc.PositionToOffset(location);
							var segment = doc.GetLineSegment(location.Line);
							var length = segment.Length - offset + segment.Offset;
							var marker = new SD.TextMarker(offset, length, SD.TextMarkerType.WaveLine);
							doc.MarkerStrategy.AddMarker(marker);
							doc.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, segment.LineNumber));
							FErrorMarkers.Add(marker);
						}
					}
				}
			}

			FTextEditorControl.Document.CommitUpdate();
		}
		
		public void JumpTo(int line)
		{
			JumpTo(line, 0);
		}
		
		public void JumpTo(int line, int column)
		{
			FTextEditorControl.ActiveTextAreaControl.Caret.Line = line;
			FTextEditorControl.ActiveTextAreaControl.Caret.Column = column;
			FTextEditorControl.ActiveTextAreaControl.ScrollTo(line, column);
		}
	}
}
