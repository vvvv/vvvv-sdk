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
using System.IO;
using System.Resources;
using System.Reflection;

using VVVV.HDE.Model;
using VVVV.HDE.Model.CS;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.HDE.Viewer.Model;
using VVVV.HDE.Model.Provider;
using VVVV.Utils;

using ICSharpCode.TextEditor;
using Dom = ICSharpCode.SharpDevelop.Dom;

using CSharpEditor;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition, inheriting from UserControl for the GUI stuff
	public class CodeEditor: UserControl
	{
		#region field declaration
		internal Dom.DefaultProjectContent FProjectContent;
		internal Dom.ParseInformation FParseInfo;
		
		private ICSharpCode.TextEditor.TextEditorControl FTextEditorControl;
		private BackgroundCodeParser FBGCodeParser;
		private System.Windows.Forms.Timer FTimer;
		#endregion field declaration
		
		#region Properties
		public CodeEditorPlugin EditorPlugin { get; private set; }
		
		public ImageList ImageList { get; private set; }
		
		public Form DummyForm { get; private set; }
		
		public ITextDocument Document { get; private set; }
		
		public bool IsDirty 
		{ 
			get
			{
				return Document.IsDirty;
			}
		}
		#endregion
		
		#region constructor/destructor
		public CodeEditor(CodeEditorPlugin editorPlugin, ITextDocument doc, ImageList imageList)
		{
			EditorPlugin = editorPlugin;
			Document = doc;
			ImageList = imageList;
			
			// The InitializeComponent() call is required for Windows Forms designer support.
			InitializeComponent();
			
			DummyForm = new Form();
			
			FTextEditorControl.SetHighlighting("C#");
			FTextEditorControl.Document.FoldingManager.FoldingStrategy = new ParserFoldingStrategy();
		
			FProjectContent = EditorPlugin.GetProjectContent(Document.Project);
			FProjectContent.Language = Dom.LanguageProperties.CSharp;
			
			HostCallbackImplementation.Register(FProjectContent);
			CodeCompletionKeyHandler.Attach(this, FTextEditorControl);
			ToolTipProvider.Attach(this, FTextEditorControl);
			
			// create dummy FParseInfo to prevent NullReferenceException when using CC before parsing
			// for the first time
			FParseInfo = new Dom.ParseInformation(new Dom.DefaultCompilationUnit(FProjectContent));
			
			FBGCodeParser = new BackgroundCodeParser(FProjectContent, FTextEditorControl.Document, Document.Location.AbsolutePath, FParseInfo);
			
			FTextEditorControl.TextChanged += TextEditorControlTextChangedCB;
			
			FTimer = new System.Windows.Forms.Timer();
			FTimer.Interval = 100;
			FTimer.Tick += TimerTickCB;
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
			this.FTextEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FTextEditorControl.IsReadOnly = false;
			this.FTextEditorControl.Location = new System.Drawing.Point(0, 0);
			this.FTextEditorControl.Name = "FTextEditorControl";
			this.FTextEditorControl.Size = new System.Drawing.Size(632, 453);
			this.FTextEditorControl.TabIndex = 2;
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
					if (Document != null)
						Document.ContentChanged -= TextDocumentContentChangedCB;
					
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
			Document.ContentChanged += TextDocumentContentChangedCB;
		}
		
		protected void TimerTickCB(object sender, EventArgs args)
		{
			FTimer.Stop();
			if (Document != null)
			{
				Document.ContentChanged -= TextDocumentContentChangedCB;
				Document.TextContent = FTextEditorControl.Document.TextContent;
				Document.ContentChanged += TextDocumentContentChangedCB;
				
				EditorPlugin.PluginHost.Log(TLogType.Debug, "Parsing " + Document.Location + " ...");
				FBGCodeParser.RunParserAsync(Document.TextContent);
			}
		}
		
		void TextEditorControlTextChangedCB(object sender, EventArgs e)
		{
			// Restart the timer.
			FTimer.Stop();
			FTimer.Start();
		}
		
		void TextDocumentContentChangedCB(IDocument doc, string content)
		{
			int length = FTextEditorControl.Document.TextContent.Length;
			FTextEditorControl.Document.Replace(0, length, content);
			FTextEditorControl.Refresh();
		}
	}
}
