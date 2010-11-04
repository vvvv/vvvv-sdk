#region usings

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
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
using VVVV.HDE.Viewer.WinFormsViewer;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.ManagedVCL;

#endregion usings

namespace VVVV.HDE.CodeEditor
{
	[EditorInfo(".cs", ".fx", ".fxh")]
	public class CodeEditorPlugin : TopControl, IEditor, IDisposable, IQueryDelete, IPluginEvaluate
	{
		private SplitContainer FSplitContainer;
		private CodeEditorForm FCodeEditorForm;
		private TableViewer FErrorTableViewer;
		private ILogger FLogger;
		private ISolution FSolution;
		private INode FLinkedNode;
		private CodeEditor FEditor;
		private ViewableCollection<object> FErrorList;
		private Dictionary<string, RuntimeError> FRuntimeErrors;
		private IHDEHost FHDEHost;
		
		public static readonly ImageList CompletionIcons = new ImageList();
		
		[ImportingConstructor]
		public CodeEditorPlugin(IHDEHost host, ISolution solution, ILogger logger)
		{
			FHDEHost = host;
			FSolution = solution;
			FLogger = logger;
			FErrorList = new ViewableCollection<object>();
			FRuntimeErrors = new Dictionary<string, RuntimeError>();
			
			if (CompletionIcons.Images.Count == 0)
			{
				var resources = new ComponentResourceManager(typeof(CodeEditorForm));
				CompletionIcons.TransparentColor = System.Drawing.Color.Transparent;
				CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.Class"));
				CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.Method"));
				CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.Property"));
				CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.Field"));
				CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.Enum"));
				CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.NameSpace"));
				CompletionIcons.Images.Add((System.Drawing.Bitmap) resources.GetObject("Icons.16x16.Event"));
			}
			
			SuspendLayout();
			
			FSplitContainer = new SplitContainer();
			FSplitContainer.Location = new Point(0, 0);
			FSplitContainer.Dock = DockStyle.Fill;
			FSplitContainer.Orientation = Orientation.Horizontal;
			FSplitContainer.Panel2Collapsed = true;
			
			FCodeEditorForm = new CodeEditorForm(logger);
			FCodeEditorForm.Location = new Point(0, 0);
			FCodeEditorForm.TopLevel = false;
			FCodeEditorForm.TopMost = false;
			FCodeEditorForm.Dock = DockStyle.Fill;
			FCodeEditorForm.Show();
			
			FErrorTableViewer = new TableViewer();
			FErrorTableViewer.Dock = DockStyle.Fill;
			FErrorTableViewer.Location = new System.Drawing.Point(0, 0);
			FErrorTableViewer.RowHeight = 16;
			FErrorTableViewer.TabIndex = 0;
			FErrorTableViewer.DoubleClick += FErrorTableViewerDoubleClick;
			
			FSplitContainer.Panel1.Controls.Add(FCodeEditorForm);
			FSplitContainer.Panel2.Controls.Add(FErrorTableViewer);
			Controls.Add(FSplitContainer);
			
			ResumeLayout(false);
			PerformLayout();
			
			var registry = new MappingRegistry();
			registry.RegisterDefaultMapping<IEnumerable<IColumn>, ErrorCollectionColumnProvider>();
			registry.RegisterMapping<CompilerError, IEnumerable<ICell>, ErrorCellProvider>();
			registry.RegisterMapping<RuntimeError, IEnumerable<ICell>, RuntimeErrorCellProvider>();
			
			FErrorTableViewer.Registry = registry;
			FErrorTableViewer.Input = FErrorList;
			
			FEditor = FCodeEditorForm.Editor;
			FEditor.LinkClicked += new LinkEventHandler(FEditor_LinkClicked);
		}

		void FEditor_LinkClicked(object sender, Link link)
		{
			// TODO: implement this
			FLogger.Log(LogType.Debug, "Implement this!");
		}
		
		protected override void Dispose(bool disposing)
		{
			try
			{
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
			var doc = FCodeEditorForm.Editor.TextDocument;
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
		
		public void Open(string filename)
		{
			var document = FSolution.FindDocument(filename) as ITextDocument;
			if (document == null)
				document = DocumentFactory.CreateDocumentFromFile(filename) as ITextDocument;
			
			if (document != null)
			{
				if (!document.IsLoaded)
					document.Load();
				
				FCodeEditorForm.Open(document);
				
				var project = document.Project;
				if (project != null)
				{
					project.CompileCompleted += Project_CompileCompleted;
					
					// Fake a compilation in order to show error messages on startup.
					Project_CompileCompleted(project);
				}
			}
			else
			{
				FLogger.Log(LogType.Warning, "Can't open \0", filename);
			}
		}
		
		public void Close()
		{
			throw new NotImplementedException();
		}
		
		public void Save()
		{
			throw new NotImplementedException();
		}
		
		public void SaveAs(string filename)
		{
			throw new NotImplementedException();
		}
		
		public void MoveTo(int lineNumber)
		{
			FEditor.JumpTo(lineNumber);
		}
		
		public INode LinkedNode
		{
			get
			{
				return FLinkedNode;
			}
			set
			{
				FLinkedNode = value;
			}
		}
		
		public void Evaluate(int SpreadMax)
		{
			if (FLinkedNode != null)
			{
				var lastRuntimeErrorString = FLinkedNode.LastRuntimeError;
				
				if (lastRuntimeErrorString != null)
				{
					if (!FRuntimeErrors.ContainsKey(lastRuntimeErrorString))
					{
						var runtimeError = new RuntimeError(lastRuntimeErrorString);
						FRuntimeErrors.Add(lastRuntimeErrorString, runtimeError);
						FErrorList.Add(runtimeError);
					}
					
					if (!IsErrorTableVisible())
					{
						FEditor.ShowRuntimeErrors(FRuntimeErrors.Values);
						ShowErrorTable();
					}
				}
				else
				{
					if (IsErrorTableVisible())
					{
						FErrorList.RemoveRange(FRuntimeErrors.Values);
						FRuntimeErrors.Clear();
						FEditor.ClearRuntimeErrors();
						HideErrorTable();
					}
				}
			}
		}
		
		private void ShowErrorTable()
		{
			// TODO: Find better way to calculate splitter distance
			FSplitContainer.SplitterDistance = FSplitContainer.Height - (FErrorTableViewer.RowCount + 2) * (FErrorTableViewer.RowHeight + 2);
			FSplitContainer.Panel2Collapsed = false;
		}
		
		private void HideErrorTable()
		{
			FSplitContainer.Panel2Collapsed = true;
		}
		
		private bool IsErrorTableVisible()
		{
			return !FSplitContainer.Panel2Collapsed;
		}
		
		private void FErrorTableViewerDoubleClick(IModelMapper sender, MouseEventArgs e)
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
			
			
			// Find the document which caused the compiler error.
			var doc = FSolution.FindDocument(fileName) as ITextDocument;
			
			if (doc == null) return;

			FHDEHost.Open(doc.Location.LocalPath, false);
		}
		
		private void Project_CompileCompleted(IProject project)
		{
			ClearCompilerErrors();
			
			var results = project.CompilerResults;
			if (results != null && results.Errors.Count > 0)
			{
				FErrorList.AddRange(results.Errors);
				ShowErrorTable();
			}
			else
			{
				HideErrorTable();
			}
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
