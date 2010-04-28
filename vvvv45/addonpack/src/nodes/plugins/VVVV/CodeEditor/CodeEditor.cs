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

using ICSharpCode.TextEditor;
using Dom = ICSharpCode.SharpDevelop.Dom;

using CSharpEditor;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition, inheriting from UserControl for the GUI stuff
	public class CodeEditor: ManagedVCL.TopControl, IHDEPlugin
	{
		#region field declaration
		internal Dom.ProjectContentRegistry FPCRegistry;
		internal Dom.DefaultProjectContent FProjectContent;
		internal Dom.ParseInformation FParseInfo;
		internal System.Windows.Forms.ImageList imageList1;
		
		private IHDEHost FHost;
		private IPluginHost FPluginHost;
		private bool FDisposed = false;
		private ToolStripStatusLabel FParserLabel;
		private ICSharpCode.TextEditor.TextEditorControl FTextEditorControl;
		private BackgroundParser FBGParser;
		private BackgroundCodeParser FBGCodeParser;
		private ITextDocument FActiveTextDocument;
		private TextDocumentEventHandler FTextDocumentEventHandler;
		private INodeSelectionListener FNodeSelectionListener;
		
		public Form WrapperForm
		{
			get
			{
				return FWrapperForm;
			}
		}
		
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
		
		#region constructor/destructor
		public CodeEditor()
		{
			// The InitializeComponent() call is required for Windows Forms designer support.
			InitializeComponent();
			
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
			
			FBGParser = new BackgroundParser(FPCRegistry, FProjectContent, FParserLabel);
			FBGCodeParser = new BackgroundCodeParser(FProjectContent, FTextEditorControl.Document, DummyFileName, FParseInfo);
			
			FTextEditorControl.TextChanged += new EventHandler(TextEditorControlTextChangedCB);
			FTextDocumentEventHandler = new TextDocumentEventHandler(TextDocumentContentChangedCB);
			FNodeSelectionListener = new NodeSelectionListener(this);
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
						FActiveTextDocument.OnContentChanged -= FTextDocumentEventHandler;
					
					if (FHost != null)
						FHost.RemoveListener(FNodeSelectionListener);
					
					HostCallbackImplementation.UnRegister(this.FProjectContent);
					FBGParser.CancelAsync();
					FTextEditorControl.Dispose();
					FWrapperForm.Dispose();
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				
				if (FPluginHost != null)
					FPluginHost.Log(TLogType.Debug, "CodeEditor is being deleted");
				
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
		
		#region node name and infos
		
		//provide node infos
		private static IPluginInfo FPluginInfo;
		public static IPluginInfo PluginInfo
		{
			get
			{
				if (FPluginInfo == null)
				{
					//fill out nodes info
					//see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
					FPluginInfo = new PluginInfo();
					
					//the nodes main name: use CamelCaps and no spaces
					FPluginInfo.Name = "CodeEditor";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "VVVV";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "CompileRun";
					
					//the nodes author: your sign
					FPluginInfo.Author = "vvvv group";
					//describe the nodes function
					FPluginInfo.Help = "CodeEditor";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "";
					//any known problems?
					FPluginInfo.Bugs = "";
					//any known usage of the node that may cause troubles?
					FPluginInfo.Warnings = "";
					
					//define the nodes initial size in box-mode
					FPluginInfo.InitialBoxSize = new Size(200, 100);
					//define the nodes initial size in window-mode
					FPluginInfo.InitialWindowSize = new Size(800, 600);
					//define the nodes initial component mode
					FPluginInfo.InitialComponentMode = TComponentMode.InAWindow;
					
					//leave below as is
					System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
					System.Diagnostics.StackFrame sf = st.GetFrame(0);
					System.Reflection.MethodBase method = sf.GetMethod();
					FPluginInfo.Namespace = method.DeclaringType.Namespace;
					FPluginInfo.Class = method.DeclaringType.Name;
					//leave above as is
				}
				return FPluginInfo;
			}
		}
		
		public bool AutoEvaluate
		{
			//return true if this node needs to calculate every frame even if nobody asks for its output
			get {return true;}
		}
		
		#endregion node name and infos
		
		#region Windows Forms designer
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CodeEditor));
			this.FWrapperForm = new System.Windows.Forms.Form();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.FProjectTreeView = new System.Windows.Forms.TreeView();
			this.FTextEditorControl = new ICSharpCode.TextEditor.TextEditorControl();
			this.FStatusStrip = new System.Windows.Forms.StatusStrip();
			this.FParserLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.FWrapperForm.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.FStatusStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// FWrapperForm
			// 
			this.FWrapperForm.ClientSize = new System.Drawing.Size(640, 458);
			this.FWrapperForm.Controls.Add(this.splitContainer1);
			this.FWrapperForm.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FWrapperForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.FWrapperForm.Location = new System.Drawing.Point(0, 0);
			this.FWrapperForm.MaximizeBox = false;
			this.FWrapperForm.MinimizeBox = false;
			this.FWrapperForm.Name = "FWrapperForm";
			this.FWrapperForm.ShowIcon = false;
			this.FWrapperForm.ShowInTaskbar = false;
			this.FWrapperForm.Visible = false;
			this.FWrapperForm.TopLevel = false;
			this.FWrapperForm.Parent = this;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.FProjectTreeView);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.FTextEditorControl);
			this.splitContainer1.Size = new System.Drawing.Size(640, 458);
			this.splitContainer1.SplitterDistance = 213;
			this.splitContainer1.TabIndex = 0;
			// 
			// FProjectTreeView
			// 
			this.FProjectTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FProjectTreeView.Location = new System.Drawing.Point(0, 0);
			this.FProjectTreeView.Name = "FProjectTreeView";
			this.FProjectTreeView.Size = new System.Drawing.Size(213, 458);
			this.FProjectTreeView.TabIndex = 0;
			this.FProjectTreeView.DoubleClick += new System.EventHandler(this.ProjectTreeViewDoubleClick);
			// 
			// FTextEditorControl
			// 
			this.FTextEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FTextEditorControl.IsReadOnly = false;
			this.FTextEditorControl.Location = new System.Drawing.Point(0, 0);
			this.FTextEditorControl.Name = "FTextEditorControl";
			this.FTextEditorControl.Size = new System.Drawing.Size(423, 458);
			this.FTextEditorControl.TabIndex = 1;
			// 
			// FStatusStrip
			// 
			this.FStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.FParserLabel});
			this.FStatusStrip.Location = new System.Drawing.Point(0, 458);
			this.FStatusStrip.Name = "FStatusStrip";
			this.FStatusStrip.Size = new System.Drawing.Size(640, 22);
			this.FStatusStrip.TabIndex = 0;
			this.FStatusStrip.Text = "StatusStrip";
			// 
			// FParserLabel
			// 
			this.FParserLabel.Name = "FParserLabel";
			this.FParserLabel.Size = new System.Drawing.Size(109, 17);
			this.FParserLabel.Text = "toolStripStatusLabel1";
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "Icons.16x16.Class.png");
			this.imageList1.Images.SetKeyName(1, "Icons.16x16.Method.png");
			this.imageList1.Images.SetKeyName(2, "Icons.16x16.Property.png");
			this.imageList1.Images.SetKeyName(3, "Icons.16x16.Field.png");
			this.imageList1.Images.SetKeyName(4, "Icons.16x16.Enum.png");
			this.imageList1.Images.SetKeyName(5, "Icons.16x16.NameSpace.png");
			this.imageList1.Images.SetKeyName(6, "Icons.16x16.Event.png");
			// 
			// CodeEditor
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.Controls.Add(this.FWrapperForm);
			this.Controls.Add(this.FStatusStrip);
			this.DoubleBuffered = true;
			this.Name = "CodeEditor";
			this.Size = new System.Drawing.Size(640, 480);
			this.FWrapperForm.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.FStatusStrip.ResumeLayout(false);
			this.FStatusStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private Form FWrapperForm;
		private System.Windows.Forms.TreeView FProjectTreeView;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.StatusStrip FStatusStrip;
		private System.ComponentModel.IContainer components;
		#endregion Windows Forms designer
		
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			
			FWrapperForm.Show();
			
			FBGParser.RunParserAsync();
		}
		
		#region IHDEPlugin
		public void SetHDEHost(IHDEHost host)
		{
			FHost = host;
			FHost.AddListener(FNodeSelectionListener);
			FillProjectTreeView();
		}
		
		public void SetPluginHost(IPluginHost host)
		{
			FPluginHost = host;
		}
		#endregion IHDEPlugin
		
		public void Open(ITextDocument doc)
		{
			if (FActiveTextDocument == doc)
				return;
			
			if (FActiveTextDocument != null)
				FActiveTextDocument.OnContentChanged -= FTextDocumentEventHandler;
			
			FActiveTextDocument = doc;
			FTextEditorControl.Document.TextContent = FActiveTextDocument.TextContent;
			FActiveTextDocument.OnContentChanged += FTextDocumentEventHandler;
		}
		
		void FillProjectTreeView()
		{
			FProjectTreeView.BeginUpdate();
			
			ISolution solution = FHost.Solution;
			foreach (IProject project in solution.Projects)
			{
				TreeNode node = FProjectTreeView.Nodes.Add("Project");
				foreach (IDocument doc in project.Documents)
				{
					TreeNode newNode = node.Nodes.Add(Path.GetFileName(doc.Location.AbsolutePath));
					newNode.Tag = doc;
				}
			}
			
			FProjectTreeView.EndUpdate();
		}
		
		void ProjectTreeViewDoubleClick(object sender, EventArgs e)
		{
			TreeNode node = FProjectTreeView.SelectedNode;
			if (node != null && node.Tag is ITextDocument)
			{
				Open(node.Tag as ITextDocument);
			}
		}
		
		void TextEditorControlTextChangedCB(object sender, EventArgs e)
		{
			TextEditorControl control = sender as TextEditorControl;
			
			FBGCodeParser.RunParserAsync(control.Text);
			
			if (FActiveTextDocument != null)
			{
				FActiveTextDocument.OnContentChanged -= FTextDocumentEventHandler;
				FActiveTextDocument.TextContent = control.Document.TextContent;
				FActiveTextDocument.OnContentChanged += FTextDocumentEventHandler;
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
