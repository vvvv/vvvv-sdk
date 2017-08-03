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
using VVVV.HDE.CodeEditor.Actions;
using VVVV.HDE.CodeEditor.Gui;
using VVVV.HDE.CodeEditor.LanguageBindings.CS;
using VVVV.PluginInterfaces.V2;
using Dom = ICSharpCode.SharpDevelop.Dom;
using NRefactory = ICSharpCode.NRefactory;
using SD = ICSharpCode.TextEditor.Document;
using System.Reactive.Linq;
using System.Threading;

namespace VVVV.HDE.CodeEditor
{
    public delegate void LinkEventHandler(object sender, Link link);
    
    //class definition, inheriting from UserControl for the GUI stuff
    public class CodeEditor: TextEditorControl
    {
        #region Fields
        private CodeCompletionWindow FCompletionWindow;
        private InsightWindow FInsightWindow;
        private System.Windows.Forms.Timer FTimer;
        private Form FParentForm;
        private SearchBar FSearchBar;
        private ReloadBar FReloadBar;
        
        private ITextDocument FTextDocument;
        private ICompletionBinding FCompletionBinding;
        private ILinkDataProvider FLinkDataProvider;
        private IToolTipProvider FToolTipProvider;
        
        private bool FNeedsKeyUp;
        private readonly SD.IFormattingStrategy FDefaultFormattingStrategy;
        #endregion
        
        #region Properties
        
        public ITextDocument TextDocument
        {
            get
            {
                return FTextDocument;
            }
            set
            {
                if (FTextDocument != null)
                    ShutdownTextDocument(FTextDocument);
                
                FTextDocument = value;
                
                if (FTextDocument != null)
                    InitializeTextDocument(FTextDocument);
            }
        }
        
        public bool IsDirty
        {
            get
            {
                return TextDocument.IsDirty;
            }
        }
        
        public ILogger Logger
        {
            get;
            private set;
        }
        
        public ICompletionBinding CompletionBinding
        {
            get
            {
                return FCompletionBinding;
            }
            set
            {
                if (FCompletionBinding != null)
                    ActiveTextAreaControl.TextArea.KeyEventHandler -= TextAreaKeyEventHandler;

                FCompletionBinding = value;
                
                if (FCompletionBinding != null)
                    ActiveTextAreaControl.TextArea.KeyEventHandler += TextAreaKeyEventHandler;
            }
        }
        
        public SD.IFormattingStrategy FormattingStrategy
        {
            get
            {
                return Document.FormattingStrategy;
            }
            set
            {
                // Never set it to null. Other stuff depends on it.
                if (value == null)
                    Document.FormattingStrategy = FDefaultFormattingStrategy;
                else
                    Document.FormattingStrategy = value;
            }
        }
        
        public SD.IFoldingStrategy FoldingStrategy
        {
            get
            {
                return Document.FoldingManager.FoldingStrategy;
            }
            set
            {
                if (FoldingStrategy != null)
                {
                    var csDoc = TextDocument as CSDocument;
                    if (csDoc != null)
                        csDoc.ContentChanged -= HandleTextContentChanged;
                    EnableFolding = false;
                }
                
                Document.FoldingManager.FoldingStrategy = value;
                
                if (FoldingStrategy != null)
                {
                    EnableFolding = true;
                    
                    // TODO: Do this via an interface to avoid asking for concrete implementation.
                    var csDoc = TextDocument as CSDocument;
                    if (csDoc != null)
                    {
                        csDoc.ContentChanged += HandleTextContentChanged;
                        HandleTextContentChanged(csDoc, new ContentChangedEventArgs(csDoc.Content));
                    }
                }
            }
        }
        
        public ILinkDataProvider LinkDataProvider
        {
            get
            {
                return FLinkDataProvider;
            }
            set
            {
                if (FLinkDataProvider != null)
                {
                    ActiveTextAreaControl.TextArea.MouseMove -= MouseMoveCB;
                    ActiveTextAreaControl.TextArea.MouseClick -= LinkClickCB;
                }

                FLinkDataProvider = value;
                
                if (FLinkDataProvider != null)
                {
                    ActiveTextAreaControl.TextArea.MouseMove += MouseMoveCB;
                    ActiveTextAreaControl.TextArea.MouseClick += LinkClickCB;
                }
            }
        }
        
        public IToolTipProvider ToolTipProvider
        {
            get
            {
                return FToolTipProvider;
            }
            set
            {
                if (FToolTipProvider != null)
                    ActiveTextAreaControl.TextArea.ToolTipRequest -= OnToolTipRequest;

                FToolTipProvider = value;
                
                if (FToolTipProvider != null)
                    ActiveTextAreaControl.TextArea.ToolTipRequest += OnToolTipRequest;
            }
        }
        
        #endregion
        
        #region events
        
        public event LinkEventHandler LinkClicked;
        
        protected virtual void OnLinkClicked(Link link)
        {
            if (LinkClicked != null) {
                LinkClicked(this, link);
            }
        }

        public event EventHandler SavePressed;

        protected virtual void OnSavePressed()
        {
            if (SavePressed != null)
            {
                SavePressed(this, EventArgs.Empty);
            }
        }
        
        #endregion
        
        #region Constructor/Destructor
        public CodeEditor(Form parentForm, ILogger logger)
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            InitializeComponent();
            
            FParentForm = parentForm;
            Logger = logger;
            
            TextEditorProperties.MouseWheelTextZoom = false;
            TextEditorProperties.LineViewerStyle = SD.LineViewerStyle.FullRow;
            TextEditorProperties.ShowMatchingBracket = true;
            TextEditorProperties.AutoInsertCurlyBracket = true;
            
            // Backup some defaults
            FDefaultFormattingStrategy = Document.FormattingStrategy;
            
            // Setup bars
            FSearchBar = new SearchBar(this);
            Controls.Add(FSearchBar);
            FReloadBar = new ReloadBar(this);
            FReloadBar.Dock = DockStyle.Top;
            Controls.Add(FReloadBar);
            
            // Setup selection highlighting
            ActiveTextAreaControl.SelectionManager.SelectionChanged += FTextEditorControl_ActiveTextAreaControl_SelectionManager_SelectionChanged;
            
            ActiveTextAreaControl.TextArea.Resize += FTextEditorControl_ActiveTextAreaControl_TextArea_Resize;
            TextChanged += TextEditorControlTextChangedCB;
            
            // Start parsing after 500ms have passed after last key stroke.
            FTimer = new System.Windows.Forms.Timer();
            FTimer.Interval = 500;
            FTimer.Tick += TimerTickCB;
            
            // Setup actions
            var redo = editactions[Keys.Control | Keys.Y];
            editactions[Keys.Control | Keys.Shift | Keys.Z] = redo;
            editactions.Remove(Keys.Control | Keys.Y);
            
            var indentSelectionAction = new IndentSelectionAction();
            indentSelectionAction.Keys = new Keys[] { Keys.Control | Keys.I };
            editactions[indentSelectionAction.Keys[0]] = indentSelectionAction;
        }

        void FTextEditorControl_Scroll(object sender, ScrollEventArgs e)
        {
            Debug.WriteLine(string.Format("{0}: {1} -> {2}", e.ScrollOrientation, e.OldValue, e.NewValue));
        }

        #endregion constructor/destructor

        #region Windows Forms designer
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // CodeEditor
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Name = "CodeEditor";
            this.Size = new System.Drawing.Size(632, 453);
            this.ResumeLayout(false);
        }
        #endregion Windows Forms designer
        
        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    CloseCodeCompletionWindow(this, EventArgs.Empty);
                    CloseInsightWindow(this, EventArgs.Empty);
                    
                    if (FTimer != null)
                    {
                        FTimer.Tick -= TimerTickCB;
                        FTimer.Dispose();
                        FTimer = null;
                    }
                    
                    if (FSearchBar != null)
                    {
                        FSearchBar.Dispose();
                        FSearchBar = null;
                    }

                    if (FReloadBar != null)
                    {
                        FReloadBar.Dispose();
                        FReloadBar = null;
                    }
                    
                    TextChanged -= TextEditorControlTextChangedCB;
                    ActiveTextAreaControl.TextArea.Resize -= FTextEditorControl_ActiveTextAreaControl_TextArea_Resize;
                    ActiveTextAreaControl.SelectionManager.SelectionChanged -= FTextEditorControl_ActiveTextAreaControl_SelectionManager_SelectionChanged;
                    TextDocument = null;
                }
            }
            
            base.Dispose(disposing);
        }
        
        #endregion IDisposable
        
        private IList<SD.TextMarker> FSelectionMarkers = new List<SD.TextMarker>();
        void FTextEditorControl_ActiveTextAreaControl_SelectionManager_SelectionChanged(object sender, EventArgs e)
        {
            var textAreaControl = ActiveTextAreaControl;
            var textArea = textAreaControl.TextArea;
            var doc = Document;
            
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

                            SD.TextMarker marker;
                            
                            if (this.HasForeGround("TypeHighlight"))
                            {
                                marker = new SD.TextMarker(offset, selectedText.Length, SD.TextMarkerType.SolidBlock,
                                    this.GetBackColor("TypeHighlight"), this.GetForeColor("TypeHighlight"));
                            }
                            else
                            {
                                marker = new SD.TextMarker(offset, selectedText.Length, SD.TextMarkerType.SolidBlock,
                                    this.GetBackColor("TypeHighlight"));
                            }
                            
                            FSelectionMarkers.Add(marker);
                            doc.MarkerStrategy.AddMarker(marker);
                            
                            doc.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToLineEnd, location));
                        }
                    }
                }
            }
            
            doc.CommitUpdate();
        }
        
        void HandleTextContentChanged(object sender, ContentChangedEventArgs args)
        {
            try
            {
                var document = sender as CSDocument;
                if (document.ParseInfo.MostRecentCompilationUnit != null)
                    Document.FoldingManager.UpdateFoldings(document.LocalPath, document.ParseInfo);
            }
            catch (Exception)
            {
                // Ignore
            }
        }
        
        void FTextEditorControl_ActiveTextAreaControl_TextArea_Resize(object sender, EventArgs e)
        {
            UpdateHScrollBar();
        }
        
        void UpdateHScrollBar()
        {
            var textAreaControl = ActiveTextAreaControl;
            var textArea = textAreaControl.TextArea;
            var doc = Document;
            
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
            var textView = ActiveTextAreaControl.TextArea.TextView;
            return textView.GetLogicalPosition(location.X - textView.DrawingPosition.Left, location.Y - textView.DrawingPosition.Top);
        }

        private SD.TextMarker FUnderlineMarker;
        private SD.TextMarker FHighlightMarker;
        private Link FLink = Link.Empty;
        void MouseMoveCB(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                var doc = Document;
                
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
                    
                    if (!location.IsEmpty)
                    {
                        FLink = FLinkDataProvider.GetLink(doc, location);
                        
                        if (!FLink.IsEmpty)
                        {
                            var hoverRegion = FLink.HoverRegion;
                            int offset = doc.PositionToOffset(hoverRegion.ToTextLocation());
                            int length = hoverRegion.EndColumn - hoverRegion.BeginColumn;

                            FUnderlineMarker = new SD.TextMarker(offset, length, SD.TextMarkerType.Underlined, Document.HighlightingStrategy.GetColorFor("Link").Color);
                            doc.MarkerStrategy.AddMarker(FUnderlineMarker);

                            FHighlightMarker = new SD.TextMarker(offset, length, SD.TextMarkerType.SolidBlock, Document.HighlightingStrategy.GetColorFor("Default").BackgroundColor, Document.HighlightingStrategy.GetColorFor("Link").Color);
                            doc.MarkerStrategy.AddMarker(FHighlightMarker);
                            
                            doc.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToLineEnd, doc.OffsetToPosition(offset)));
                            doc.CommitUpdate();
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore
            }
        }
        
        void LinkClickCB(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                if (!FLink.IsEmpty)
                {
                    OnLinkClicked(FLink);
                }
            }
            catch (Exception f)
            {
                Logger.Log(f);
            }
        }
        
        private void SyncControlWithDocument()
        {
            TextDocument.ContentChanged -= TextDocumentContentChangedCB;
            TextDocument.TextContent = Document.TextContent;
            TextDocument.ContentChanged += TextDocumentContentChangedCB;
        }

        /// <summary>
        /// Updates the underlying ITextDocument from changes made in the editor and parses the document.
        /// This method is called once after 500ms have passed after last key stroke (to save CPU cycles).
        /// </summary>
        void TimerTickCB(object sender, EventArgs args)
        {
            FTimer.Stop();
            if (TextDocument != null)
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
            if (!FIsSynchronizing)
                FTimer.Start();
        }
        
        /// <summary>
        /// Keeps the editor and the underlying ITextDocument in sync.
        /// </summary>
        void TextDocumentContentChangedCB(object sender, ContentChangedEventArgs args)
        {
            FIsSynchronizing = true;
            Document.Replace(0, Document.TextLength, TextDocument.TextContent);
            Refresh();
            FIsSynchronizing = false;
        }
        
        protected override bool ProcessKeyPreview(ref Message m)
        {
            KeyEventArgs ke = new KeyEventArgs((Keys)m.WParam.ToInt32() | ModifierKeys);
            FNeedsKeyUp = !(m.Msg == 0x101);
            
            if (ke.Control && ke.KeyCode == Keys.S && !ke.Alt && m.Msg == 0x100)
            {
                if (!TextDocument.IsReadOnly)
                {
                    SyncControlWithDocument();
                    TextDocument.Save();
                    // Trigger a recompile
                    var project = TextDocument.Project;
                    if (project != null)
                    {
                        project.Save();
                        project.CompileAsync();
                    }
                    OnSavePressed();
                }
                return true;
            }
            else if (ke.Control && ke.KeyCode == Keys.S && ke.Alt && m.Msg == 0x100)
            {
                string ext = Path.GetExtension(TextDocument.Project.LocalPath);

                var saveDialog = new System.Windows.Forms.SaveFileDialog();
                saveDialog.InitialDirectory = Path.GetDirectoryName(TextDocument.Project.LocalPath);
                saveDialog.Filter = String.Format("{0} Document (*{1})|*{1}", ext.ToUpper(), ext.ToLower());
                saveDialog.AddExtension = true;

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveDialog.FileName, TextDocument.TextContent);
                }
                return true;
            }
            else if (ke.Control && ke.KeyCode == Keys.F && m.Msg == 0x100)
            {
                // Show search bar
                FSearchBar.ShowSearchBar();
                return true;
            }
            else if ((ke.Control && !ke.Alt) && (ke.KeyCode == Keys.Add || ke.KeyCode == Keys.Oemplus))
            {
                if (!FNeedsKeyUp)
                {
                    FNeedsKeyUp = true;
                    this.Font = new Font(this.Font.Name, this.Font.Size + 1);
                }
                return true;
            }
            else if ((ke.Control && !ke.Alt) && (ke.KeyCode == Keys.Subtract || ke.KeyCode == Keys.OemMinus))
            {
                if (!FNeedsKeyUp)
                {
                    FNeedsKeyUp = true;
                    this.Font = new Font(this.Font.Name, this.Font.Size - 1);
                }
                return true;
            }
            else if ((ke.Control && !ke.Alt) && (ke.KeyCode == Keys.NumPad0 || ke.KeyCode == Keys.D0))
            {
                this.Font = new Font(this.Font.Name, 10);
                return true;
            }
            else if (ke.KeyCode == Keys.F1 && m.Msg == 0x100)
            {
                if (TextDocument is CSDocument)
                    Process.Start("http://vvvv.org/documentation/plugins");
                else
                    Process.Start("http://vvvv.org/documentation/effects");						
                return true;
            }
            else
                return base.ProcessKeyPreview(ref m);
        }
        
        void AddErrorMarker(List<SD.TextMarker> errorMarkers, int column, int line, Color color)
        {
            column = Math.Max(column, 0);
            line = Math.Max(line, 0);
            
            var doc = Document;
            var location = new TextLocation(column, line);
            var offset = doc.PositionToOffset(location);
            var segment = doc.GetLineSegment(location.Line);
            var length = segment.Length - offset + segment.Offset;
            var marker = new SD.TextMarker(offset, length, SD.TextMarkerType.WaveLine, color);
            doc.MarkerStrategy.AddMarker(marker);
            doc.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, segment.LineNumber));
            errorMarkers.Add(marker);
        }
        
        void ClearErrorMarkers(List<SD.TextMarker> errorMarkers)
        {
            var doc = Document;
            foreach (var marker in errorMarkers)
            {
                try
                {
                    doc.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, doc.GetLineNumberForOffset(marker.Offset)));
                }
                catch (Exception)
                {
                    doc.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
                }
                doc.MarkerStrategy.RemoveMarker(marker);
            }
            errorMarkers.Clear();
        }
        
        List<SD.TextMarker> FCompilerErrorMarkers = new List<SD.TextMarker>();
        internal void ShowCompilerErrors(IEnumerable<CompilerError> compilerErrors)
        {
            foreach (var compilerError in compilerErrors)
            {
                var color = compilerError.IsWarning
                    ? this.GetForeColor("Warning")
                    : this.GetForeColor("Error");
                try
                {
                    if (string.Compare(compilerError.FileName, TextDocument.LocalPath, StringComparison.InvariantCultureIgnoreCase) == 0)
                        AddErrorMarker(FCompilerErrorMarkers, compilerError.Column - 1, compilerError.Line - 1, color);
                }
                catch (Exception)
                {
                    // Better show the error with illegal filename than crash
                    AddErrorMarker(FCompilerErrorMarkers, compilerError.Column - 1, compilerError.Line - 1, color);
                }
            }

            Document.CommitUpdate();
        }
        
        internal void ClearCompilerErrors()
        {
            ClearErrorMarkers(FCompilerErrorMarkers);
            Document.CommitUpdate();
        }

        void OnToolTipRequest(object sender, ToolTipRequestEventArgs e)
        {
            if (e.InDocument && !e.ToolTipShown) {
                try
                {
                    string toolTipText = FToolTipProvider.GetToolTip(Document, e.LogicalPosition);
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
            ActiveTextAreaControl.ScrollTo(line, column);
            ActiveTextAreaControl.Caret.Line = line;
            ActiveTextAreaControl.Caret.Column = column;
        }

        private bool HasForeGround(string key)
        {
            return Document.HighlightingStrategy.GetColorFor(key).HasForeground;
        }

        private bool HasBackGround(string key)
        {
            return Document.HighlightingStrategy.GetColorFor(key).HasBackground;
        }

        private Color GetForeColor(string key)
        {
            return Document.HighlightingStrategy.GetColorFor(key).Color;
        }


        private Color GetBackColor(string key)
        {
            return Document.HighlightingStrategy.GetColorFor(key).BackgroundColor;
        }
        
        #region Code completion
        
        public void ShowCompletionWindow(ICompletionDataProvider completionDataProvider, char key)
        {
            try
            {
                FCompletionWindow = CodeCompletionWindow.ShowCompletionWindow(
                    FParentForm,					// The parent window for the completion window
                    this, 				// The text editor to show the window for
                    TextDocument.LocalPath,		// Filename - will be passed back to the provider
                    completionDataProvider,				// Provider to get the list of possible completions
                    key									// Key pressed - will be passed to the provider
                );
                if (FCompletionWindow != null)
                {
                    // ShowCompletionWindow can return null when the provider returns an empty list
                    FCompletionWindow.Closed += CloseCodeCompletionWindow;
                    ActiveTextAreaControl.DoHandleMousewheel = false;
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }
        
        public void CloseCompletionWindow()
        {
            if (FCompletionWindow != null && !FCompletionWindow.IsDisposed)
            {
                FCompletionWindow.Close();
            }
        }

        protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (FCompletionWindow != null && !FCompletionWindow.IsDisposed)
            {
                FCompletionWindow.HandleMouseWheel(e);
            }
        }
   
        public void ShowInsightWindow(IInsightDataProvider insightDataProvider)
        {
            try
            {
                if (FInsightWindow == null || FInsightWindow.IsDisposed) {
                    FInsightWindow = new InsightWindow(FParentForm, this);
                    FInsightWindow.Closed += new EventHandler(CloseInsightWindow);
                }
                FInsightWindow.AddInsightDataProvider(insightDataProvider, TextDocument.LocalPath);
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
        private bool FIsSynchronizing;
        bool TextAreaKeyEventHandler(char key)
        {
            if (inHandleKeyPress)
                return false;
            
            inHandleKeyPress = true;
            
            try
            {
                if (FCompletionWindow != null && !FCompletionWindow.IsDisposed) {
                    // If completion window is open and wants to handle the key, don't let the text area handle it.
                    if (FCompletionWindow.ProcessKeyEvent(key)) {
                        return true;
                    }
                    if (FCompletionWindow != null && !FCompletionWindow.IsDisposed) {
                        // code-completion window is still opened but did not want to handle
                        // the keypress -> don't try to restart code-completion
                        return false;
                    }
                }
                
                return FCompletionBinding.HandleKeyPress(this, key);
            }
            finally
            {
                inHandleKeyPress = false;
            }
        }
        
        void CloseCodeCompletionWindow(object sender, EventArgs e)
        {
            if (FCompletionWindow != null)
            {
                FCompletionWindow.Closed -= CloseCodeCompletionWindow;
                FCompletionWindow.Dispose();
                FCompletionWindow = null;
            }
            ActiveTextAreaControl.DoHandleMousewheel = true;
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

        #region Initialization
        
        private void InitializeTextDocument(ITextDocument doc)
        {
            var fileName = doc.LocalPath;
            Document.ReadOnly = doc.IsReadOnly;
            
            // Setup code highlighting
            var highlighter = SD.HighlightingManager.Manager.FindHighlighterForFile(fileName);

            if (Path.GetExtension(fileName) == ".tfx" || Path.GetExtension(fileName) == ".gsfx")
                this.SetHighlighting("HLSL");
            else
                SetHighlighting(highlighter.Name);
            
            Document.TextContent = doc.TextContent;
            doc.ContentChanged += TextDocumentContentChangedCB;

            FSubscription = Observable.FromEventPattern<EventArgs>(doc, nameof(doc.FileChanged))
                .ObserveOn(SynchronizationContext.Current)
                .Do(e =>
                {
                    FReloadBar.ShowBar();
                })
                .Subscribe();
        }
        
        private void ShutdownTextDocument(ITextDocument doc)
        {
            doc.ContentChanged -= TextDocumentContentChangedCB;
            Document.TextContent = string.Empty;
            if (FSubscription != null)
            {
                FSubscription.Dispose();
                FSubscription = null;
            }
        }

        IDisposable FSubscription;

        #endregion
    }
}
