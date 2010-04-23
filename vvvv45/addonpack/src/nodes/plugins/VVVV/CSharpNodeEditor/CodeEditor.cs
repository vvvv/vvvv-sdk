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
		private bool FDisposed = false;
		private ToolStripStatusLabel FParserLabel;
		private StatusStrip statusStrip1;
		private ICSharpCode.TextEditor.TextEditorControl FTextEditorControl;
		private BackgroundParser FBGParser;
		private BackgroundCodeParser FBGCodeParser;
		private Form FWrapperForm;
		
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
			/*FPcRegistry.ActivatePersistence(Path.Combine(Path.GetTempPath(),
			                                            "CSharpCodeCompletion"));*/
			
			FProjectContent = new Dom.DefaultProjectContent();
			FProjectContent.Language = CurrentLanguageProperties;
			
			// create dummy FParseInfo to prevent NullReferenceException when using CC before parsing
			// for the first time
			FParseInfo = new Dom.ParseInformation(new Dom.DefaultCompilationUnit(FProjectContent));
			
			FBGParser = new BackgroundParser(FPCRegistry, FProjectContent, FParserLabel);
			FBGCodeParser = new BackgroundCodeParser(FProjectContent, FTextEditorControl.Document, DummyFileName, FParseInfo);
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
					HostCallbackImplementation.UnRegister(this.FProjectContent);
					FBGParser.CancelAsync();
					FTextEditorControl.Dispose();
					FWrapperForm.Dispose();
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				
				if (FHost != null)
					FHost.Log(TLogType.Debug, "CodeEditor is being deleted");
				
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
					FPluginInfo.InitialWindowSize = new Size(400, 300);
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
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.FParserLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.FWrapperForm = new System.Windows.Forms.Form();
			this.FTextEditorControl = new ICSharpCode.TextEditor.TextEditorControl();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.statusStrip1.SuspendLayout();
			this.FWrapperForm.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.FParserLabel});
			this.statusStrip1.Location = new System.Drawing.Point(0, 458);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(640, 22);
			this.statusStrip1.TabIndex = 0;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// FParserLabel
			// 
			this.FParserLabel.Name = "FParserLabel";
			this.FParserLabel.Size = new System.Drawing.Size(109, 17);
			this.FParserLabel.Text = "toolStripStatusLabel1";
			// 
			// FWrapperForm
			// 
			this.FWrapperForm.ClientSize = new System.Drawing.Size(640, 458);
			this.FWrapperForm.Controls.Add(this.FTextEditorControl);
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
			// FTextEditorControl
			// 
			this.FTextEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FTextEditorControl.IsReadOnly = false;
			this.FTextEditorControl.Location = new System.Drawing.Point(0, 0);
			this.FTextEditorControl.Name = "FTextEditorControl";
			this.FTextEditorControl.Size = new System.Drawing.Size(640, 458);
			this.FTextEditorControl.TabIndex = 0;
			this.FTextEditorControl.TextChanged += new System.EventHandler(this.FTextEditorControlTextChangedCB);
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
			// CSharpNodeEditor
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.Controls.Add(this.FWrapperForm);
			this.Controls.Add(this.statusStrip1);
			this.DoubleBuffered = true;
			this.Name = "CSharpNodeEditor";
			this.Size = new System.Drawing.Size(640, 480);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.FWrapperForm.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
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
		}
		
		public void SetPluginHost(IPluginHost Host)
		{
			// We don't need the PluginHost for now.
		}
		#endregion IHDEPlugin
		
		void FTextEditorControlTextChangedCB(object sender, EventArgs e)
		{
			TextEditorControl control = sender as TextEditorControl;
			
			FBGCodeParser.RunParserAsync(control.Text);
		}
	}
}
