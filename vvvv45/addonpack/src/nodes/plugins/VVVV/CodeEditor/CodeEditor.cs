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
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.IO;
using System.Resources;
using System.Reflection;

using VVVV.HDE.Model;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.Adapter;
using VVVV.HDE.Viewer.Model;
using VVVV.HDE.Model.Provider;

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
		internal Dom.ProjectContentRegistry FPCRegistry;
		internal Dom.DefaultProjectContent FProjectContent;
		internal Dom.ParseInformation FParseInfo;
		
		private bool FDisposed = false;
		private ICSharpCode.TextEditor.TextEditorControl FTextEditorControl;
		private BackgroundParser FBGParser;
		private BackgroundCodeParser FBGCodeParser;
		private ITextDocument FActiveTextDocument;
		
		/// <summary>
		/// Many SharpDevelop.Dom methods take a file name, which is really just a unique identifier
		/// for a file - Dom methods don't try to access code files on disk, so the file does not have
		/// to exist.
		/// SharpDevelop itself uses internal names of the kind "[randomId]/Class1.cs" to support
		/// code-completion in unsaved files.
		/// </summary>
		public const string DummyFileName = "edited.cs";
		
		static readonly Dom.LanguageProperties CurrentLanguageProperties = Dom.LanguageProperties.CSharp;
		
		#endregion field declaration
		
		#region Properties
		public ImageList ImageList { get; private set; }
		public Form DummyForm { get; private set; }
		#endregion
		
		#region constructor/destructor
		public CodeEditor(ToolStripStatusLabel statusLabel, ImageList imageList)
		{
			ImageList = imageList;
//			TopLevel = false;
			
			// The InitializeComponent() call is required for Windows Forms designer support.
			InitializeComponent();
			
			DummyForm = new Form();
			
			FTextEditorControl.SetHighlighting("C#");
			FTextEditorControl.Document.FoldingManager.FoldingStrategy = new ParserFoldingStrategy();
			
			HostCallbackImplementation.Register(this.FProjectContent);
			CodeCompletionKeyHandler.Attach(this, FTextEditorControl);
			ToolTipProvider.Attach(this, FTextEditorControl);
			
			FPCRegistry = new Dom.ProjectContentRegistry(); // Default .NET 2.0 registry
			
			// Persistence lets SharpDevelop.Dom create a cache file on disk so that
			// future starts are faster.
			// It also caches XML documentation files in an on-disk hash table, thus
			// reducing memory usage.
			FPCRegistry.ActivatePersistence(Path.Combine(Path.GetTempPath(), "VVVVCodeEditor"));
			
			FProjectContent = new Dom.DefaultProjectContent();
			FProjectContent.Language = CurrentLanguageProperties;
			
			// create dummy FParseInfo to prevent NullReferenceException when using CC before parsing
			// for the first time
			FParseInfo = new Dom.ParseInformation(new Dom.DefaultCompilationUnit(FProjectContent));
			
			FBGParser = new BackgroundParser(FPCRegistry, FProjectContent, statusLabel);
			FBGCodeParser = new BackgroundCodeParser(FProjectContent, FTextEditorControl.Document, DummyFileName, FParseInfo);
			
			FTextEditorControl.TextChanged += new EventHandler(TextEditorControlTextChangedCB);
		}
		
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
					if (FActiveTextDocument != null)
						FActiveTextDocument.ContentChanged -= TextDocumentContentChangedCB;
					
					HostCallbackImplementation.UnRegister(this.FProjectContent);
					FBGParser.CancelAsync();
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
		
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			FBGParser.RunParserAsync();
		}
		
		/// <summary>
		/// Opens the specified ITextDocument in the editor.
		/// </summary>
		/// <param name="doc">The ITextDocument to open.</param>
		public void Open(ITextDocument doc)
		{
			if (FActiveTextDocument == doc)
				return;
			
			if (FActiveTextDocument != null)
				FActiveTextDocument.ContentChanged -= TextDocumentContentChangedCB;
			
			FActiveTextDocument = doc;
			FTextEditorControl.Document.TextContent = FActiveTextDocument.TextContent;
			FActiveTextDocument.ContentChanged += TextDocumentContentChangedCB;
		}
		
		/// <summary>
		/// Opens the specified ITextDocument in the editor and scrolls to the line
		/// where this particular INodeInfo was defined.
		/// </summary>
		/// <param name="doc">The ITextDocument to open.</param>
		/// <param name="nodeInfo">The INodeInfo to scroll to.</param>
		public void Open(ITextDocument doc, INodeInfo nodeInfo)
		{
			Open(doc);
			FTextEditorControl.ActiveTextAreaControl.ScrollTo(100);
		}
		
		void TextEditorControlTextChangedCB(object sender, EventArgs e)
		{
			TextEditorControl control = sender as TextEditorControl;
			
			FBGCodeParser.RunParserAsync(control.Text);
			
			if (FActiveTextDocument != null)
			{
				FActiveTextDocument.ContentChanged -= TextDocumentContentChangedCB;
				FActiveTextDocument.TextContent = control.Document.TextContent;
				FActiveTextDocument.ContentChanged += TextDocumentContentChangedCB;
			}
		}
		
		void TextDocumentContentChangedCB(IDocument doc, string content)
		{
			int length = FTextEditorControl.Document.TextContent.Length;
			FTextEditorControl.Document.Replace(0, length, content);
			FTextEditorControl.Refresh();
		}
	}
}
