#region usings

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
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
using VVVV.HDE.CodeEditor.Gui.Dialogs;
using VVVV.HDE.CodeEditor.LanguageBindings.CS;
using VVVV.HDE.CodeEditor.LanguageBindings.FX;
using VVVV.HDE.Viewer.WinFormsViewer;
using VVVV.Hosting.Factories;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.ManagedVCL;
using SD = ICSharpCode.TextEditor.Document;

#endregion usings

namespace VVVV.HDE.CodeEditor
{
	[EditorInfo(".cs", ".fx", ".fxh", ".txt")]
	public class CodeEditorPlugin : TopControl, IEditor, IDisposable, IQueryDelete, IPluginEvaluate
	{
		private SplitContainer FSplitContainer;
		private Form FCodeEditorForm;
		private TableViewer FErrorTableViewer;
		private ILogger FLogger;
		private ISolution FSolution;
		private INode FAttachedNode;
		private CodeEditor FEditor;
		private ViewableCollection<object> FErrorList;
		private Dictionary<string, RuntimeError> FRuntimeErrors;
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
			FRuntimeErrors = new Dictionary<string, RuntimeError>();
			
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
				
				var path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\bin"));
				var provider = new SD.FileSyntaxModeProvider(path);
				SD.HighlightingManager.Manager.AddSyntaxModeFileProvider(provider);
			}
			
			SuspendLayout();
			
			FSplitContainer = new SplitContainer();
			FSplitContainer.Location = new Point(0, 0);
			FSplitContainer.Dock = DockStyle.Fill;
			FSplitContainer.Orientation = Orientation.Horizontal;
			FSplitContainer.Panel2Collapsed = true;
			
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
			FErrorTableViewer.Dock = DockStyle.Top;
			FErrorTableViewer.Location = new System.Drawing.Point(0, 0);
			FErrorTableViewer.TabIndex = 0;
			FErrorTableViewer.DoubleClick += FErrorTableViewerDoubleClick;
			FErrorTableViewer.Resize += FErrorTableViewer_Resize;
			FErrorTableViewer.AutoSize = true;
			FErrorTableViewer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			
			FSplitContainer.Panel1.Controls.Add(FCodeEditorForm);
			FSplitContainer.Panel2.Controls.Add(FErrorTableViewer);
			Controls.Add(FSplitContainer);
			
			ResumeLayout(false);
			PerformLayout();
			
			var registry = new MappingRegistry();
			registry.RegisterDefaultMapping<IEnumerable<Column>, ErrorCollectionColumnProvider>();
			registry.RegisterMapping<CompilerError, IEnumerable<Cell>, ErrorCellProvider>();
			registry.RegisterMapping<RuntimeError, IEnumerable<Cell>, RuntimeErrorCellProvider>();
			
			FErrorTableViewer.Registry = registry;
			FErrorTableViewer.Input = FErrorList;
			
			FEditor.LinkClicked += new LinkEventHandler(FEditor_LinkClicked);
		}

		void FErrorTableViewer_Resize(object sender, EventArgs e)
		{
			FSplitContainer.SplitterDistance = Math.Max(FSplitContainer.ClientSize.Height - FErrorTableViewer.ClientSize.Height - FSplitContainer.SplitterWidth, 0);
		}
		
		void FEditor_LinkClicked(object sender, Link link)
		{
			var fileName = link.FileName;
			
			if (!File.Exists(fileName))
			{
				var dir = Path.GetDirectoryName(FEditor.TextDocument.Location.LocalPath);
				fileName = dir.ConcatPath(fileName);
			}
			
			if (File.Exists(fileName))
			{
				if (new Uri(fileName) == FEditor.TextDocument.Location)
					MoveTo(link.Location.Line + 1, link.Location.Column + 1);
				else
					FEditorFactory.Open(fileName, link.Location.Line + 1, link.Location.Column + 1, FAttachedNode, FNode.Window);
			}
		}
		
		protected override void Dispose(bool disposing)
		{
			try
			{
				Close();
				base.Dispose(disposing);
			}
			catch (Exception e)
			{
				FLogger.Log(e);
			}
		}
		
		public bool DeleteMe()
		{
			// Save the current SynchronizationContext. ShowDialog method changes the
			// current SynchronizationContext. Later RunWorkerCompleted callbacks from
			// BackgroundWorker objects do not return to the GUI (vvvv) thread.
			var syncContext = SynchronizationContext.Current;
			
			// Check if opened document needs to be saved.
			var doc = FEditor.TextDocument;
			if (doc.IsDirty)
			{
				var saveDialog = new SaveDialog(doc.Location.LocalPath);
				if (saveDialog.ShowDialog(this) == DialogResult.OK)
				{
					// Resore the old SynchronizationContext.
					SynchronizationContext.SetSynchronizationContext(syncContext);
					
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
					FEditor.FormattingStrategy = new SD.DefaultFormattingStrategy();
					FEditor.LinkDataProvider = new FXLinkDataProvider();
				}
				
				var project = document.Project;
				if (project != null)
				{
					project.CompileCompleted += Project_CompileCompleted;
					
					// Fake a compilation in order to show error messages on startup.
					Project_CompileCompleted(project, new CompilerEventArgs(project.CompilerResults));
				}
				
				document.ContentChanged += document_ContentChanged;
				document.Saved += document_Saved;
				document.Renamed += document_Renamed;
				
				SynchronizationContext.Current.Post((o) => UpdateWindowCaption(document, document.Name), null);
			}
			else
			{
				FLogger.Log(LogType.Warning, "Can't open \0", filename);
			}
		}

		void document_Renamed(INamed sender, string newName)
		{
			UpdateWindowCaption(sender as ITextDocument, newName);
		}

		void document_Saved(object sender, EventArgs e)
		{
			document_ContentChanged(sender as ITextDocument, string.Empty);
		}

		void document_ContentChanged(ITextDocument doc, string content)
		{
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
				
				if (FAttachedNode != null)
				{
					caption += string.Format(" - attached to {1} [id: {0}]", FAttachedNode.GetID(), FAttachedNode.GetNodeInfo().Username);
				}
				
				window.Caption = caption;
			}
		}
		
		public void Close()
		{
			var document = FEditor.TextDocument;
			
			if (document != null)
			{
				OpenedFile = null;
				
				document.ContentChanged -= document_ContentChanged;
				document.Saved -= document_Saved;
				document.Renamed -= document_Renamed;
				
				FEditor.TextDocument = null;
				FEditor.CompletionBinding = null;
				FEditor.FormattingStrategy = null;
				FEditor.FoldingStrategy = null;
				FEditor.LinkDataProvider = null;
				FEditor.ToolTipProvider = null;
				
				var project = document.Project;
				if (project != null)
					project.CompileCompleted -= Project_CompileCompleted;
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
		
		public INode AttachedNode
		{
			get
			{
				return FAttachedNode;
			}
			set
			{
				FAttachedNode = value;
				
				var doc = FEditor.TextDocument;
				if (doc != null)
				{
					UpdateWindowCaption(doc, doc.Name);
				}
			}
		}
		
		public void Evaluate(int SpreadMax)
		{
			if (FAttachedNode != null)
			{
				var lastRuntimeErrorString = FAttachedNode.LastRuntimeError;
				
				if (lastRuntimeErrorString != null)
				{
					int runtimeErrorsCount = FRuntimeErrors.Count;
					
					if (!FRuntimeErrors.ContainsKey(lastRuntimeErrorString))
					{
						var runtimeError = new RuntimeError(lastRuntimeErrorString);
						FRuntimeErrors.Add(lastRuntimeErrorString, runtimeError);
						FErrorList.Add(runtimeError);
					}
					
					if (runtimeErrorsCount != FRuntimeErrors.Count)
						FEditor.ShowRuntimeErrors(FRuntimeErrors.Values);
				}
				else
					ClearRuntimeErrors();
			}
			else
				ClearRuntimeErrors();
			
			// Show or hide error table
			if (IsErrorTableVisible)
			{
				if (FErrorList.Count == 0)
					HideErrorTable();
			}
			else if (FErrorList.Count > 0)
				ShowErrorTable();
		}
		
		private void ClearRuntimeErrors()
		{
			if (FRuntimeErrors.Count > 0)
			{
				FErrorList.RemoveRange(FRuntimeErrors.Values);
				FRuntimeErrors.Clear();
				FEditor.ClearRuntimeErrors();
			}
		}
		
		private void ShowErrorTable()
		{
			FSplitContainer.Panel2Collapsed = false;
		}
		
		private void HideErrorTable()
		{
			FSplitContainer.Panel2Collapsed = true;
		}
		
		private bool IsErrorTableVisible
		{
			get
			{
				return !FSplitContainer.Panel2Collapsed;
			}
		}
		
		private void FErrorTableViewerDoubleClick(IModelMapper sender, System.Windows.Forms.MouseEventArgs e)
		{
			var fileName = string.Empty;
			var line = 0;
			
			if (sender.Model is CompilerError)
			{
				var compilerError = sender.Model as CompilerError;
				fileName = compilerError.FileName;
				line = compilerError.Line;
			}
			else
			{
				var runtimeError = sender.Model as RuntimeError;
				fileName = runtimeError.FileName;
				line = runtimeError.Line;
			}
			
			if (File.Exists(fileName))
			{
				if (new Uri(fileName) == FEditor.TextDocument.Location)
					MoveTo(line, 1);
				else
					FEditorFactory.Open(fileName, line, 1, FAttachedNode, FNode.Window);
			}
		}
		
		private void Project_CompileCompleted(object sender, CompilerEventArgs args)
		{
			ClearCompilerErrors();
			
			var results = args.CompilerResults;
			if (results != null)
				FErrorList.AddRange(results.Errors);
			
			if (FErrorList.Count > 0)
				ShowErrorTable();
			else
				HideErrorTable();
		}
		
		private void ClearCompilerErrors()
		{
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
	}
}
