using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.Model;
using VVVV.Core.Model.CS;
using VVVV.Core.Model.FX;
using VVVV.Core.Runtime;
using VVVV.Core.View;
using VVVV.Core.View.Table;
using VVVV.HDE.CodeEditor.ErrorView;
using VVVV.HDE.CodeEditor.LanguageBindings.CS;
using VVVV.HDE.CodeEditor.LanguageBindings.FX;
using VVVV.PluginInterfaces.V2;
using Dom = ICSharpCode.SharpDevelop.Dom;
using SD = ICSharpCode.TextEditor.Document;

namespace VVVV.HDE.CodeEditor
{
	/// <summary>
	/// Sharpdevelop texteditor needs a Form as one of its parent controls
	/// in order to work properly.
	/// </summary>
	public partial class CodeEditorForm : Form//, IDocumentLocator
	{
		public CodeEditor Editor
		{
			get;
			private set;
		}
		
		public ILogger Logger
		{
			get;
			private set;
		}
		
		public CodeEditorForm(ILogger logger)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			Editor = new CodeEditor(this);
			Editor.Dock = DockStyle.Fill;
			Controls.Add(Editor);
			
			var path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), @"..\bin"));
			var provider = new SD.FileSyntaxModeProvider(path);
			SD.HighlightingManager.Manager.AddSyntaxModeFileProvider(provider);
			
			Logger = logger;
		}
		
		/// <summary>
		/// Opens the specified ITextDocument.
		/// </summary>
		/// <param name="doc">The ITextDocument to open.</param>
		public void Open(ITextDocument doc)
		{
			Editor.TextDocument = doc;
			
			if (doc is CSDocument)
			{
				var csDoc = doc as CSDocument;
				Editor.CompletionBinding = new CSCompletionBinding(Editor);
				Editor.FormattingStrategy = new CSFormattingStrategy(Editor);
				Editor.FoldingStrategy = new CSFoldingStrategy();
				Editor.LinkDataProvider = new CSLinkDataProvider(Editor);
				Editor.ToolTipProvider = new CSToolTipProvider(Editor);
			}
			else if (doc is FXDocument)
			{
				Editor.CompletionBinding = new FXCompletionBinding(Editor);
				Editor.FormattingStrategy = new SD.DefaultFormattingStrategy();
				Editor.LinkDataProvider = new FXLinkDataProvider();
			}
			
			doc.ContentChanged += DocumentContentChangedCB;
			doc.Saved += DocumentSavedCB;
		}

		public void Close(ITextDocument doc)
		{
			// TODO
//			if (FOpenedDocuments.ContainsKey(doc))
//			{
//				var tabPage = FOpenedDocuments[doc];
//				var codeEditor = tabPage.Controls[0] as CodeEditor;
//				FSDDocToDocMap.Remove(codeEditor.Document);
//				FTabControl.Controls.Remove(tabPage);
//				tabPage.Dispose();
//
//				FOpenedDocuments.Remove(doc);
//
//				var project = doc.Project;
//				FProjectDocCount[project]--;
//				var executable = GetExecutable(project);
//				if (executable != null && FProjectDocCount[project] == 0)
//				{
//					executable.RuntimeErrors.Added -= executable_RuntimeErrors_Added;
//					executable.RuntimeErrors.Removed -= executable_RuntimeErrors_Removed;
//				}
//			}
		}
		
//		public void BringToFront(TabPage tabPage)
//		{
//			FTabControl.SelectTab(tabPage);
//		}
		//TODO
//		protected string GetTabPageName(ITextDocument document)
//		{
//			var name = document.Mapper.Map<INamed>().Name;
//
//			var fileName = document.Location.LocalPath;
//			var isReadOnly = (File.GetAttributes(fileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
//
//			if (isReadOnly)
//				name = name + "+";
//
//			if (document.IsDirty)
//				return name + "*";
//			else
//				return name;
//		}
		
		void DocumentContentChangedCB(ITextDocument document, string content)
		{
			// TODO
//			FOpenedDocuments[document].Text = GetTabPageName(document);
		}
		
		void DocumentSavedCB(object sender, EventArgs args)
		{
			// TODO
//			var document = sender as ITextDocument;
//			FOpenedDocuments[document].Text = GetTabPageName(document);
		}
		
		#region IDisposable
		
		protected override void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!IsDisposed)
			{
				if(disposing)
				{
					if (components != null) {
						components.Dispose();
					}
					components = null;
				}
			}
			
			base.Dispose(disposing);
		}
		
		#endregion IDisposable
	}
}
