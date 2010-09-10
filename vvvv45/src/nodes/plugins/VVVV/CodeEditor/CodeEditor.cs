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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using ICSharpCode.TextEditor.Gui.InsightWindow;
using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.Model;
using VVVV.Core.Model.CS;
using VVVV.Core.Model.FX;
using VVVV.Core.Runtime;
using VVVV.HDE.CodeEditor.Gui.SearchBar;
using VVVV.HDE.CodeEditor.LanguageBindings.CS;
using Dom = ICSharpCode.SharpDevelop.Dom;
using NRefactory = ICSharpCode.NRefactory;
using SD = ICSharpCode.TextEditor.Document;

namespace VVVV.HDE.CodeEditor
{
	
	//class definition, inheriting from UserControl for the GUI stuff
	public class CodeEditor: UserControl
	{
		#region Fields
		
		private ICSharpCode.TextEditor.TextEditorControl FTextEditorControl;
		private CodeCompletionWindow FCodeCompletionWindow;
		private InsightWindow FInsightWindow;
		private System.Windows.Forms.Timer FTimer;
		private CodeEditorForm FCodeEditorForm;
		private SearchBar FSearchBar;
		
		private ICompletionBinding FCompletionBinding;
		private ILinkDataProvider FLinkDataProvider;
		private IToolTipProvider FToolTipProvider;
		
		#endregion
		
		#region Properties
		
		public ITextDocument Document { get; private set; }
		
		public SD.IDocument SDDocument { get; private set; }
		
		public bool IsDirty
		{
			get
			{
				return Document.IsDirty;
			}
		}
		
		public TextEditorControl TextEditorControl
		{
			get
			{
				return FTextEditorControl;
			}
		}
		
		public string TextContent
		{
			get
			{
				return SDDocument.TextContent;
			}
		}
		
		public ILogger Logger
		{
			get;
			private set;
		}
		
		#endregion
		
		#region Constructor/Destructor
		public CodeEditor(
			ILogger logger,
			CodeEditorForm codeEditorForm,
			ITextDocument doc,
			ICompletionBinding completionBinding,
			SD.IFormattingStrategy formattingStrategy,
			SD.IFoldingStrategy foldingStrategy,
			ILinkDataProvider linkDataProvider,
			IToolTipProvider toolTipProvider)
		{
			Logger = logger;
			
			// The InitializeComponent() call is required for Windows Forms designer support.
			InitializeComponent();
			
			FCodeEditorForm = codeEditorForm;
			
			Document = doc;
			SDDocument = FTextEditorControl.Document;
			
			FTextEditorControl.TextEditorProperties.MouseWheelTextZoom = false;
			FTextEditorControl.TextEditorProperties.LineViewerStyle = SD.LineViewerStyle.FullRow;
			FTextEditorControl.TextEditorProperties.ShowMatchingBracket = true;
			FTextEditorControl.TextEditorProperties.AutoInsertCurlyBracket = true;
			
			var fileName = doc.Location.LocalPath;
			
			// Setup search bar
			FSearchBar = new SearchBar(FTextEditorControl);
			
			// Setup code highlighting
			var highlighter = SD.HighlightingManager.Manager.FindHighlighterForFile(fileName);
			FTextEditorControl.SetHighlighting(highlighter.Name);
			
			// Setup code completion
			FCompletionBinding = completionBinding;
			FTextEditorControl.ActiveTextAreaControl.TextArea.KeyEventHandler += TextAreaKeyEventHandler;
			
			// Setup code formatting
			if (formattingStrategy != null)
				FTextEditorControl.Document.FormattingStrategy = formattingStrategy;
			
			// Setup code folding
			if (foldingStrategy != null)
			{
				FTextEditorControl.EnableFolding = true;
				FTextEditorControl.Document.FoldingManager.FoldingStrategy = foldingStrategy;
				
				// TODO: Do this via an interface to avoid asking for concrete implementation.
				if (Document is CSDocument)
				{
					var csDoc = Document as CSDocument;
					csDoc.ParseCompleted += CSDocument_ParseCompleted;
				}
			}
			else
			{
				FTextEditorControl.EnableFolding = false;
			}
			
			// Setup hovering (ILinkDataProvider)
			if (linkDataProvider != null)
			{
				FLinkDataProvider = linkDataProvider;
				FTextEditorControl.ActiveTextAreaControl.TextArea.MouseMove += MouseMoveCB;
				FTextEditorControl.ActiveTextAreaControl.TextArea.MouseClick += LinkClickCB;
			}
			
			// Setup tool tips
			if (toolTipProvider != null)
			{
				FToolTipProvider = toolTipProvider;
				FTextEditorControl.ActiveTextAreaControl.TextArea.ToolTipRequest += OnToolTipRequest;
			}
			
			// Setup selection highlighting
			FTextEditorControl.ActiveTextAreaControl.SelectionManager.SelectionChanged += FTextEditorControl_ActiveTextAreaControl_SelectionManager_SelectionChanged;
			
			FTextEditorControl.ActiveTextAreaControl.TextArea.Resize += FTextEditorControl_ActiveTextAreaControl_TextArea_Resize;
			FTextEditorControl.TextChanged += TextEditorControlTextChangedCB;
			
			// Start parsing after 500ms have passed after last key stroke.
			FTimer = new Timer();
			FTimer.Interval = 500;
			FTimer.Tick += TimerTickCB;
		}

		void FTextEditorControl_Scroll(object sender, ScrollEventArgs e)
		{
			Debug.WriteLine(string.Format("{0}: {1} -> {2}", e.ScrollOrientation, e.OldValue, e.NewValue));
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
					CloseCodeCompletionWindow(this, EventArgs.Empty);
					CloseInsightWindow(this, EventArgs.Empty);
					
					if (FSearchBar != null)
					{
						FSearchBar.Dispose();
						FSearchBar = null;
					}
					
					if (FTextEditorControl != null)
					{
						FTextEditorControl.TextChanged -= TextEditorControlTextChangedCB;
						FTextEditorControl.ActiveTextAreaControl.TextArea.Resize -= FTextEditorControl_ActiveTextAreaControl_TextArea_Resize;
						FTextEditorControl.ActiveTextAreaControl.TextArea.KeyEventHandler -= TextAreaKeyEventHandler;
						FTextEditorControl.ActiveTextAreaControl.SelectionManager.SelectionChanged -= FTextEditorControl_ActiveTextAreaControl_SelectionManager_SelectionChanged;
						
						if (FLinkDataProvider != null)
						{
							FTextEditorControl.ActiveTextAreaControl.TextArea.MouseMove -= MouseMoveCB;
							FTextEditorControl.ActiveTextAreaControl.TextArea.MouseClick -= LinkClickCB;
						}
						
						if (FToolTipProvider != null)
						{
							FTextEditorControl.ActiveTextAreaControl.TextArea.ToolTipRequest -= OnToolTipRequest;
						}
						
						FTextEditorControl.Dispose();
						FTextEditorControl = null;
					}
					
					if (Document != null)
					{
						Document.ContentChanged -= TextDocumentContentChangedCB;
						Document.Project.CompileCompleted -= CompileCompletedCB;
						
						var executable = FCodeEditorForm.GetExecutable(Document.Project);
						if (executable != null)
						{
							executable.RuntimeErrors.Added -= executable_RuntimeErrors_Added;
							executable.RuntimeErrors.Removed -= executable_RuntimeErrors_Removed;
						}
						
						if (Document is CSDocument)
						{
							var csDoc = Document as CSDocument;
							csDoc.ParseCompleted -= CSDocument_ParseCompleted;
						}
						
						Document = null;
					}
					
					if (FTimer != null)
					{
						FTimer.Tick -= TimerTickCB;
						FTimer.Dispose();
						FTimer = null;
					}
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
			Document.ContentChanged += TextDocumentContentChangedCB;
			
			// TODO: Do this via an interface
			if (Document is CSDocument)
			{
				var csDoc = Document as CSDocument;
				CSDocument_ParseCompleted(csDoc);
			}
			
			// Everytime the project is compiled update the error highlighting.
			Document.Project.CompileCompleted += CompileCompletedCB;
			
			// We're also interested in runtime errors.
			var executable = FCodeEditorForm.GetExecutable(Document.Project);
			if (executable != null)
			{
				executable.RuntimeErrors.Added += executable_RuntimeErrors_Added;
				executable.RuntimeErrors.Removed += executable_RuntimeErrors_Removed;
				ShowRuntimeErrors(executable.RuntimeErrors);
			}
		}
		
		private IList<SD.TextMarker> FSelectionMarkers = new List<SD.TextMarker>();
		void FTextEditorControl_ActiveTextAreaControl_SelectionManager_SelectionChanged(object sender, EventArgs e)
		{
			var textAreaControl = FTextEditorControl.ActiveTextAreaControl;
			var textArea = textAreaControl.TextArea;
			var doc = FTextEditorControl.Document;
			
			// Clear previous selection markers
			foreach (var marker in FSelectionMarkers)
			{
				var location = doc.OffsetToPosition(marker.Offset);
				doc.MarkerStrategy.RemoveMarker(marker);
				doc.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToLineEnd, location));
			}
			doc.CommitUpdate();
			FSelectionMarkers.Clear();
			
			var selectionManager = textAreaControl.SelectionManager;
			foreach (var selection in selectionManager.SelectionCollection)
			{
				// Ignore selection spanning over multiple lines.
				if (selection.StartPosition.Line != selection.EndPosition.Line)
					continue;
				
				var selectedText = selection.SelectedText.Trim();
				
				// Ignore empty strings
				if (selectedText == string.Empty)
					continue;
				
				for (int lineNumber = 0; lineNumber < doc.TotalNumberOfLines; lineNumber++)
				{
					var lineSegment = doc.GetLineSegment(lineNumber);
					// Highlight text which matches the selection
					foreach (var word in lineSegment.Words)
					{
						var text = word.Word;
						var start = text.IndexOf(selectedText);
						if (start >= 0)
						{
							var offset = lineSegment.Offset + word.Offset + start;
							var location = doc.OffsetToPosition(offset);
							
							var marker = new SD.TextMarker(offset, selectedText.Length, SD.TextMarkerType.SolidBlock, Color.LightGray);
							FSelectionMarkers.Add(marker);
							doc.MarkerStrategy.AddMarker(marker);
							
							doc.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToLineEnd, location));
						}
					}
				}
			}
			
			doc.CommitUpdate();
		}
		
		void CSDocument_ParseCompleted(CSDocument document)
		{
			FTextEditorControl.Document.FoldingManager.UpdateFoldings(document.Location.LocalPath, document.ParseInfo);
		}
		
		void FTextEditorControl_ActiveTextAreaControl_TextArea_Resize(object sender, EventArgs e)
		{
			UpdateHScrollBar();
		}
		
		void UpdateHScrollBar()
		{
			var textAreaControl = FTextEditorControl.ActiveTextAreaControl;
			var textArea = textAreaControl.TextArea;
			var doc = FTextEditorControl.Document;
			
			// At startup this property seems to be invalid.
			if (textArea.TextView.VisibleColumnCount == -1)
				textArea.Refresh(textArea.TextView);
			
			var visibleColumnCount = textArea.TextView.VisibleColumnCount;
			
			int firstLine = textArea.TextView.FirstVisibleLine;
			int lastLine = doc.GetFirstLogicalLine(textArea.TextView.FirstPhysicalLine + textArea.TextView.VisibleLineCount);
			if (lastLine >= doc.TotalNumberOfLines)
				lastLine = doc.TotalNumberOfLines - 1;
			
			int max = textArea.Caret.Column + 20;
			for (int lineNumber = firstLine; lineNumber <= lastLine; lineNumber++)
			{
				if (doc.FoldingManager.IsLineVisible(lineNumber))
				{
					var lineSegment = doc.GetLineSegment(lineNumber);
					int visualLength = textArea.TextView.GetVisualColumnFast(lineSegment, lineSegment.Length);
					max = Math.Max(max, visualLength);
					
					if (max >= visibleColumnCount)
						break;
				}
			}
			
			if (max < visibleColumnCount)
			{
				textAreaControl.HScrollBar.Hide();
				textAreaControl.TextArea.Bounds =
					new Rectangle(0, 0,
					              textAreaControl.Width - SystemInformation.HorizontalScrollBarArrowWidth,
					              textAreaControl.Height);
			}
			else
			{
				textAreaControl.TextArea.Bounds =
					new Rectangle(0, 0,
					              textAreaControl.Width - SystemInformation.HorizontalScrollBarArrowWidth,
					              textAreaControl.Height - SystemInformation.VerticalScrollBarArrowHeight);
				textAreaControl.ResizeTextArea();
				textAreaControl.HScrollBar.Show();
			}
		}
		
		public TextLocation GetTextLocationAtMousePosition(Point location)
		{
			var textView = FTextEditorControl.ActiveTextAreaControl.TextArea.TextView;
			return textView.GetLogicalPosition(location.X - textView.DrawingPosition.Left, location.Y - textView.DrawingPosition.Top);
		}

		private SD.TextMarker FUnderlineMarker;
		private SD.TextMarker FHighlightMarker;
		private Link FLink = Link.Empty;
		void MouseMoveCB(object sender, MouseEventArgs e)
		{
			try
			{
				var doc = FTextEditorControl.Document;
				
				if (FUnderlineMarker != null)
				{
					doc.MarkerStrategy.RemoveMarker(FUnderlineMarker);
					doc.MarkerStrategy.RemoveMarker(FHighlightMarker);
					var lastMarkerLocation = doc.OffsetToPosition(FUnderlineMarker.Offset);
					doc.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToLineEnd, lastMarkerLocation));
					doc.CommitUpdate();
					
					FUnderlineMarker = null;
					FHighlightMarker = null;
					FLink = Link.Empty;
				}
				
				if (Control.ModifierKeys == Keys.Control)
				{
					var location = GetTextLocationAtMousePosition(e.Location);
					FLink = FLinkDataProvider.GetLink(doc, location);
					
					if (!FLink.IsEmpty)
					{
						var hoverRegion = FLink.HoverRegion;
						int offset = doc.PositionToOffset(hoverRegion.ToTextLocation());
						int length = hoverRegion.EndColumn - hoverRegion.BeginColumn;
						
						FUnderlineMarker = new SD.TextMarker(offset, length, SD.TextMarkerType.Underlined, Color.Blue);
						doc.MarkerStrategy.AddMarker(FUnderlineMarker);
						
						FHighlightMarker = new SD.TextMarker(offset, length, SD.TextMarkerType.SolidBlock, FTextEditorControl.Document.HighlightingStrategy.GetColorFor("Default").BackgroundColor, Color.Blue);
						doc.MarkerStrategy.AddMarker(FHighlightMarker);
						
						doc.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToLineEnd, doc.OffsetToPosition(offset)));
						doc.CommitUpdate();
					}
				}
			}
			catch (Exception f)
			{
				Logger.Log(f);
			}
		}
		
		void LinkClickCB(object sender, MouseEventArgs e)
		{
			try
			{
				if (!FLink.IsEmpty)
				{
					var textDocument = FCodeEditorForm.GetVDocument(FLink.FileName) as ITextDocument;
					
					if (textDocument != null)
					{
						var tabPage = FCodeEditorForm.Open(textDocument);
						FCodeEditorForm.BringToFront(tabPage);
						var codeEditor = tabPage.Controls[0] as CodeEditor;
						codeEditor.FTextEditorControl.ActiveTextAreaControl.TextArea.Focus();
						codeEditor.JumpTo(FLink.Location.Line, FLink.Location.Column);
					}
				}
			}
			catch (Exception f)
			{
				Debug.WriteLine(f.StackTrace);
			}
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
			}
		}
		
		/// <summary>
		/// Restarts the timer everytime the content of the editor changes.
		/// </summary>
		void TextEditorControlTextChangedCB(object sender, EventArgs e)
		{
			UpdateHScrollBar();
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
		
		protected override bool ProcessKeyPreview(ref Message m)
		{
			KeyEventArgs ke = new KeyEventArgs((Keys)m.WParam.ToInt32() | ModifierKeys);
			if (ke.Control && ke.KeyCode == Keys.S)
			{
				SyncControlWithDocument();
				Document.Save();
				// Trigger a recompile
				Document.Project.Save();
				return true;
			}
			else if (ke.Control && ke.KeyCode == Keys.F)
			{
				// Show search bar
				FSearchBar.ShowSearchBar();
				return true;
			}
			else
				return base.ProcessKeyPreview(ref m);
		}
		
		List<SD.TextMarker> FErrorMarkers = new List<SD.TextMarker>();
		void CompileCompletedCB(IProject project, CompilerResults results)
		{
			// Clear all previous error markers.
			ClearErrorMarkers();
			
			if (results.Errors.HasErrors)
			{
				foreach (var error in results.Errors)
				{
					if (error is CompilerError)
					{
						var compilerError = error as CompilerError;
						var path = Path.GetFullPath(compilerError.FileName);

						if (path.ToLower() == Document.Location.LocalPath.ToLower())
							AddErrorMarker(compilerError.Column - 1, compilerError.Line - 1);
					}
				}
			}

			FTextEditorControl.Document.CommitUpdate();
		}
		
		void AddErrorMarker(int column, int line)
		{
			var doc = FTextEditorControl.Document;
			var location = new TextLocation(column, line);
			var offset = doc.PositionToOffset(location);
			var segment = doc.GetLineSegment(location.Line);
			var length = segment.Length - offset + segment.Offset;
			var marker = new SD.TextMarker(offset, length, SD.TextMarkerType.WaveLine);
			doc.MarkerStrategy.AddMarker(marker);
			doc.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, segment.LineNumber));
			FErrorMarkers.Add(marker);
		}
		
		void ClearErrorMarkers()
		{
			var doc = FTextEditorControl.Document;
			foreach (var marker in FErrorMarkers)
			{
				doc.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, doc.GetLineNumberForOffset(marker.Offset)));
				doc.MarkerStrategy.RemoveMarker(marker);
			}
			FErrorMarkers.Clear();
		}
		
		void executable_RuntimeErrors_Added(IViewableCollection<RuntimeError> collection, RuntimeError item)
		{
			ShowRuntimeErrors(collection);
		}
		
		void executable_RuntimeErrors_Removed(IViewableCollection<RuntimeError> collection, RuntimeError item)
		{
			ShowRuntimeErrors(collection);
		}
		
		void ShowRuntimeErrors(IViewableCollection<RuntimeError> runtimeErros)
		{
			// Clear all previous error markers.
			ClearErrorMarkers();
			
			foreach (var runtimeError in runtimeErros)
			{
				var path = Path.GetFullPath(runtimeError.FileName);

				if (path.ToLower() == Document.Location.LocalPath.ToLower())
					AddErrorMarker(0, runtimeError.Line - 1);
			}

			FTextEditorControl.Document.CommitUpdate();
		}

		void OnToolTipRequest(object sender, ToolTipRequestEventArgs e)
		{
			if (e.InDocument && !e.ToolTipShown) {
				try
				{
					string toolTipText = FToolTipProvider.GetToolTip(SDDocument, e.LogicalPosition);
					if (toolTipText != null)
						e.ShowToolTip(toolTipText);
				}
				catch (Exception)
				{
					// Ignore
				}
			}
		}
		
		public void JumpTo(int line)
		{
			JumpTo(line, 0);
		}
		
		public void JumpTo(int line, int column)
		{
			FTextEditorControl.ActiveTextAreaControl.ScrollTo(line, column);
			FTextEditorControl.ActiveTextAreaControl.Caret.Line = line;
			FTextEditorControl.ActiveTextAreaControl.Caret.Column = column;
		}
		
		#region Code completion
		
		public void ShowCompletionWindow(ICompletionDataProvider completionDataProvider, char key)
		{
			try
			{
				FCodeCompletionWindow = CodeCompletionWindow.ShowCompletionWindow(
					FCodeEditorForm,					// The parent window for the completion window
					FTextEditorControl, 				// The text editor to show the window for
					Document.Location.LocalPath,		// Filename - will be passed back to the provider
					completionDataProvider,				// Provider to get the list of possible completions
					key									// Key pressed - will be passed to the provider
				);
				if (FCodeCompletionWindow != null)
				{
					// ShowCompletionWindow can return null when the provider returns an empty list
					FCodeCompletionWindow.Closed += CloseCodeCompletionWindow;
				}
			}
			catch (Exception e)
			{
				Logger.Log(e);
			}
		}
		
		public void ShowInsightWindow(IInsightDataProvider insightDataProvider)
		{
			try
			{
				if (FInsightWindow == null || FInsightWindow.IsDisposed) {
					FInsightWindow = new InsightWindow(FCodeEditorForm, FTextEditorControl);
					FInsightWindow.Closed += new EventHandler(CloseInsightWindow);
				}
				FInsightWindow.AddInsightDataProvider(insightDataProvider, Document.Location.LocalPath);
				FInsightWindow.ShowInsightWindow();
			}
			catch (Exception e)
			{
				Logger.Log(e);
			}
		}
		
		public void CloseInsightWindow()
		{
			if (FInsightWindow != null && !FInsightWindow.IsDisposed)
			{
				FInsightWindow.Close();
			}
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
				
				FCompletionBinding.HandleKeyPress(this, key);
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
		
		void CloseInsightWindow(object sender, EventArgs e)
		{
			if (FInsightWindow != null)
			{
				FInsightWindow.Closed -= CloseInsightWindow;
				FInsightWindow.Dispose();
				FInsightWindow = null;
			}
		}
		
		#endregion
	}
}
