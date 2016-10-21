#region usings

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Logging;
using VVVV.Core.Model;
using VVVV.Core.Model.CS;
using VVVV.Core.Model.FX;
using VVVV.Core.Runtime;
using VVVV.Core.View.Table;
using VVVV.HDE.CodeEditor.ErrorView;
using VVVV.HDE.CodeEditor.Gui;
using VVVV.HDE.CodeEditor.LanguageBindings.CS;
using VVVV.HDE.CodeEditor.LanguageBindings.FX;
using VVVV.HDE.Viewer.WinFormsViewer;
using VVVV.Hosting.Factories;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils.ManagedVCL;
using SD = ICSharpCode.TextEditor.Document;

#endregion usings

namespace VVVV.HDE.CodeEditor
{
    [EditorInfo(".cs", ".fx", ".fxh", ".txt",".tfx",".gsfx")]
    public class CodeEditorPlugin : TopControl, IEditor, IDisposable, IQueryDelete, IPluginBase
    {
        private Form FCodeEditorForm;
        private TableViewer FErrorTableViewer;
        private ILogger FLogger;
        private ISolution FSolution;
        private IProject FAttachedProject;
        private CodeEditor FEditor;
        private ViewableCollection<object> FErrorList;
        private IHDEHost FHDEHost;
        private INode FNode;
        
        [Import]
        protected EditorFactory FEditorFactory;
        
        public static readonly ImageList CompletionIcons = new ImageList();
        
        [ImportingConstructor]
        public CodeEditorPlugin(IHDEHost host, INode node, ISolution solution, ILogger logger)
        {
            FHDEHost = host;
            FNode = node;
            FSolution = solution;
            FLogger = logger;
            FErrorList = new ViewableCollection<object>();
            
            if (CompletionIcons.Images.Count == 0)
            {
                var resources = new ComponentResourceManager(typeof(CodeEditorPlugin));
                CompletionIcons.TransparentColor = System.Drawing.Color.Transparent;
                CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.Class"));
                CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.Method"));
                CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.Property"));
                CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.Field"));
                CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.Enum"));
                CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.NameSpace"));
                CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.Event"));
                
                var path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\thirdparty"));
                var provider = new SD.FileSyntaxModeProvider(path);
                SD.HighlightingManager.Manager.AddSyntaxModeFileProvider(provider);
            }
            
            SuspendLayout();
            
            FCodeEditorForm = new Form();
            FCodeEditorForm.Location = new Point(0, 0);
            FCodeEditorForm.TopLevel = false;
            FCodeEditorForm.TopMost = false;
            FCodeEditorForm.Dock = DockStyle.Fill;
            FCodeEditorForm.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            FCodeEditorForm.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            FCodeEditorForm.BackColor = System.Drawing.Color.Silver;
            FCodeEditorForm.ClientSize = new System.Drawing.Size(881, 476);
            FCodeEditorForm.ControlBox = false;
            FCodeEditorForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            FCodeEditorForm.MaximizeBox = false;
            FCodeEditorForm.MinimizeBox = false;
            FCodeEditorForm.ShowIcon = false;
            FCodeEditorForm.ShowInTaskbar = false;
            FCodeEditorForm.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            FCodeEditorForm.TopMost = true;
            FCodeEditorForm.Show();
            
            FEditor = new CodeEditor(FCodeEditorForm, FLogger);
            FEditor.Dock = DockStyle.Fill;
            FCodeEditorForm.Controls.Add(FEditor);
            
            FErrorTableViewer = new TableViewer();
            FErrorTableViewer.Dock = DockStyle.Bottom;
            FErrorTableViewer.TabIndex = 0;
            FErrorTableViewer.DoubleClick += FErrorTableViewerDoubleClick;
            FErrorTableViewer.MaximumSize = new Size(0, 100);
            
            Controls.Add(FCodeEditorForm);
            Controls.Add(FErrorTableViewer);
            
            ResumeLayout(false);
            PerformLayout();
            
            var registry = new MappingRegistry();
            registry.RegisterDefaultMapping<IEnumerable<Column>, ErrorCollectionColumnProvider>();
            registry.RegisterMapping<CompilerError, IEnumerable<ICell>, ErrorCellProvider>();
            
            FErrorTableViewer.Registry = registry;
            FErrorTableViewer.Input = FErrorList;
            
            FEditor.LinkClicked += FEditor_LinkClicked;
            FEditor.SavePressed += FEditor_SavePressed;
        }

        void FEditor_SavePressed(object sender, EventArgs e)
        {
            document_ContentChanged(FEditor.TextDocument, new ContentChangedEventArgs(FEditor.TextDocument.Content));
        }

        void FEditor_LinkClicked(object sender, Link link)
        {
            var fileName = link.FileName;
            
            if (!File.Exists(fileName))
            {
                var dir = Path.GetDirectoryName(FEditor.TextDocument.LocalPath);
                fileName = dir.ConcatPath(fileName);
            }
            
            if (File.Exists(fileName))
            {
                if (string.Compare(fileName, FEditor.TextDocument.LocalPath, StringComparison.InvariantCultureIgnoreCase) == 0)
                    MoveTo(link.Location.Line + 1, link.Location.Column + 1);
                else
                    FEditorFactory.Open(fileName, link.Location.Line + 1, link.Location.Column + 1, FNode.Window);
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            Close();
            
            base.Dispose(disposing);
        }
        
        public bool DeleteMe()
        {
            // Check if opened document needs to be saved.
            var doc = FEditor.TextDocument;
            if (doc != null && doc.IsDirty)
            {
                var saveDialog = new SaveDialog(doc.LocalPath);
                if (saveDialog.ShowDialog(this) == DialogResult.OK)
                {
                    var result = saveDialog.SaveOptionResult;
                        
                    switch (result)
                    {
                        case SaveOption.Save:
                            doc.Save();
                            break;
                        case SaveOption.DontSave:
                            // Do nothing
                            break;
                        default:
                            // Cancel
                            return false;
                    }
                }
                else
                {
                    // Cancel
                    return false;
                }
            }
            
            return true;
        }
        
        public string OpenedFile
        {
            get;
            private set;
        }
        
        public void Open(string filename)
        {
            var document = FSolution.FindDocument(filename) as ITextDocument;
            if (document == null)
                document = DocumentFactory.CreateDocumentFromFile(filename) as ITextDocument;
            
            if (document != null)
            {
                OpenedFile = filename;
                FEditor.TextDocument = document;
                AttachedProject = document.Project;
                
                if (document is CSDocument)
                {
                    var csDoc = document as CSDocument;
                    FEditor.CompletionBinding = new CSCompletionBinding(FEditor);
                    FEditor.FormattingStrategy = new CSFormattingStrategy(FEditor);
                    FEditor.FoldingStrategy = new CSFoldingStrategy();
                    FEditor.LinkDataProvider = new CSLinkDataProvider(FEditor);
                    FEditor.ToolTipProvider = new CSToolTipProvider(FEditor);
                }
                else if (document is FXDocument)
                {
                    FEditor.CompletionBinding = new FXCompletionBinding(FEditor);
                    FEditor.FormattingStrategy = new FXFormattingStrategy(FEditor);
                    FEditor.LinkDataProvider = new FXLinkDataProvider(Path.GetDirectoryName(filename), Path.Combine(FHDEHost.ExePath, "lib", "nodes"));
                }
                
                document.ContentChanged += document_ContentChanged;
                document.Renamed += document_Renamed;
                document.Disposed += document_Disposed;
                
                SynchronizationContext.Current.Post((o) => UpdateWindowCaption(document, document.Name), null);
            }
            else
            {
                FLogger.Log(LogType.Warning, "Can't open \0", filename);
            }
        }

        void document_Disposed(object sender, EventArgs e)
        {
            Close();
        }
        
        void document_Renamed(INamed sender, string newName)
        {
            UpdateWindowCaption(sender as ITextDocument, newName);
        }

        void document_ContentChanged(object sender, ContentChangedEventArgs args)
        {
            var doc = sender as ITextDocument;
            UpdateWindowCaption(doc, doc.Name);
        }
        
        void UpdateWindowCaption(ITextDocument doc, string name)
        {
            var window = FNode.Window;
            if (window != null)
            {
                var caption = name;
                if (doc.IsDirty)
                    caption = name + "*";
                else if (doc.IsReadOnly)
                    caption = name + "+";
                else
                    caption = name;
                
                window.Caption = caption;
            }
        }
        
        public void Close()
        {
            var document = FEditor.TextDocument;
            
            if (document != null)
            {
                OpenedFile = null;
                AttachedProject = null;
                
                document.ContentChanged -= document_ContentChanged;
                document.Renamed -= document_Renamed;
                document.Disposed -= document_Disposed;
                
                FEditor.LinkClicked -= FEditor_LinkClicked;
                FEditor.CompletionBinding = null;
                FEditor.FormattingStrategy = null;
                FEditor.FoldingStrategy = null;
                FEditor.LinkDataProvider = null;
                FEditor.ToolTipProvider = null;
                FEditor.TextDocument = null;
            }
        }
        
        public void Save()
        {
            throw new NotImplementedException();
        }
        
        public void SaveAs(string filename)
        {
            throw new NotImplementedException();
        }
        
        /// <remarks>Line counting and coloumn count starts at 1.</remarks>
        public void MoveTo(int lineNumber, int column)
        {
            if (lineNumber < 1)
                lineNumber = 1;
            
            if (column < 1)
                column = 1;
            
            FEditor.JumpTo(lineNumber - 1, column - 1);
            FEditor.Focus();
        }
        
        // where we get the compile errors from
        public IProject AttachedProject
        {
            get
            {
                return FAttachedProject;
            }
            set
            {
                if (FAttachedProject != null)
                    FAttachedProject.CompileCompleted -= Project_CompileCompleted;
                
                FAttachedProject = value;
                
                if (FAttachedProject != null)
                {
                    FAttachedProject.CompileCompleted += Project_CompileCompleted;
                    // Fake a compilation in order to show error messages on startup.
                    Project_CompileCompleted(FAttachedProject, new CompilerEventArgs(FAttachedProject.CompilerResults));
                }
            }
        }
        
        private void ClearCompilerErrors()
        {
            FEditor.ClearCompilerErrors();
            
            var compilerErrors = new List<CompilerError>();
            foreach (var error in FErrorList)
            {
                var compilerError = error as CompilerError;
                if (compilerError != null)
                    compilerErrors.Add(compilerError);
            }
            
            foreach (var compilerError in compilerErrors)
                FErrorList.Remove(compilerError);
        }
        
        private void ShowErrorTable()
        {
            FErrorTableViewer.Visible = true;
        }
        
        private void HideErrorTable()
        {
            FErrorTableViewer.Visible = false;
        }
        
        private bool IsErrorTableVisible
        {
            get
            {
                return FErrorTableViewer.Visible;
            }
        }
        
        private void FErrorTableViewerDoubleClick(ModelMapper sender, System.Windows.Forms.MouseEventArgs e)
        {
            var fileName = string.Empty;
            var line = 0;
            
            if (sender.Model is CompilerError)
            {
                var compilerError = sender.Model as CompilerError;
                fileName = compilerError.FileName;
                line = compilerError.Line;
            }
            
            if (File.Exists(fileName))
            {
                if (string.Compare(fileName, FEditor.TextDocument.LocalPath, StringComparison.InvariantCultureIgnoreCase) == 0)
                    MoveTo(line, 1);
                else
                    FEditorFactory.Open(fileName, line, 1, FNode.Window);
            }
        }
        
        private void Project_CompileCompleted(object sender, CompilerEventArgs args)
        {
            ClearCompilerErrors();
            
            var results = args.CompilerResults;
            if (results != null && (results.Errors.HasErrors || results.Errors.HasWarnings))
            {
                var compilerErrors =
                    from CompilerError error in results.Errors
                    select error as CompilerError;
                
                FEditor.ShowCompilerErrors(compilerErrors);
                FErrorList.AddRange(results.Errors);
            }
            
            if (FErrorList.Count > 0)
                ShowErrorTable();
            else
                HideErrorTable();
        }
    }
}
