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

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using ICSharpCode.TextEditor;
using NRefactory = ICSharpCode.NRefactory;
using Dom = ICSharpCode.SharpDevelop.Dom;

using CSharpEditor;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition, inheriting from UserControl for the GUI stuff
	public class CSharpNodeEditor: UserControl, IPlugin
	{
		#region field declaration
		
		//the host (mandatory)
		private IPluginHost FHost;
		// Track whether Dispose has been called.
		private bool FDisposed = false;
		
		//input pin declaration
		private IStringIn FFilenameInput;
		
		internal Dom.ProjectContentRegistry FPcRegistry;
		internal Dom.DefaultProjectContent FMyProjectContent;
		internal Dom.ParseInformation FParseInformation = new Dom.ParseInformation();
		private Dom.ICompilationUnit FLastCompilationUnit;
		private Thread FParserThread;
		
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
		public CSharpNodeEditor()
		{
			// The InitializeComponent() call is required for Windows Forms designer support.
			InitializeComponent();
			
			HostCallbackImplementation.Register(this);
			CodeCompletionKeyHandler.Attach(this, FTextEditorControl);
			ToolTipProvider.Attach(this, FTextEditorControl);
			
			FPcRegistry = new Dom.ProjectContentRegistry(); // Default .NET 2.0 registry
			
			// Persistence lets SharpDevelop.Dom create a cache file on disk so that
			// future starts are faster.
			// It also caches XML documentation files in an on-disk hash table, thus
			// reducing memory usage.
			FPcRegistry.ActivatePersistence(Path.Combine(Path.GetTempPath(),
			                                            "CSharpCodeCompletion"));
			
			FMyProjectContent = new Dom.DefaultProjectContent();
			FMyProjectContent.Language = CurrentLanguageProperties;
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
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				
				FHost.Log(TLogType.Debug, "PluginGUITemplate is being deleted");
				
				// Note that this is not thread safe.
				// Another thread could start disposing the object
				// after the managed resources are disposed,
				// but before the disposed flag is set to true.
				// If thread safety is necessary, it must be
				// implemented by the client.
			}
			FDisposed = true;
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
					FPluginInfo.Name = "CSharpNodeEditor";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "VVVV";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "CompileRun";
					
					//the nodes author: your sign
					FPluginInfo.Author = "vvvv group";
					//describe the nodes function
					FPluginInfo.Help = "CSharpNodeEditor";
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
		
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.FParserThreadLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.FTextEditorControl = new ICSharpCode.TextEditor.TextEditorControl();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.FParserThreadLabel});
			this.statusStrip1.Location = new System.Drawing.Point(0, 147);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(310, 22);
			this.statusStrip1.TabIndex = 0;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// FParserThreadLabel
			// 
			this.FParserThreadLabel.Name = "FParserThreadLabel";
			this.FParserThreadLabel.Size = new System.Drawing.Size(118, 17);
			this.FParserThreadLabel.Text = "toolStripStatusLabel1";
			// 
			// FTextEditorControl
			// 
			this.FTextEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FTextEditorControl.IsReadOnly = false;
			this.FTextEditorControl.Location = new System.Drawing.Point(0, 0);
			this.FTextEditorControl.Name = "FTextEditorControl";
			this.FTextEditorControl.Size = new System.Drawing.Size(310, 147);
			this.FTextEditorControl.TabIndex = 1;
			this.FTextEditorControl.Text = "textEditorControl1";
			// 
			// imageList1
			// 
			this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// CSharpNodeEditor
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.Controls.Add(this.FTextEditorControl);
			this.Controls.Add(this.statusStrip1);
			this.DoubleBuffered = true;
			this.Name = "CSharpNodeEditor";
			this.Size = new System.Drawing.Size(310, 169);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.ComponentModel.IContainer components;
		internal System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.ToolStripStatusLabel FParserThreadLabel;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private ICSharpCode.TextEditor.TextEditorControl FTextEditorControl;
		
		#region pin creation
		
		//this method is called by vvvv when the node is created
		public void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FHost = Host;

			//create inputs
			FHost.CreateStringInput("String Input", TSliceMode.Dynamic, TPinVisibility.True, out FFilenameInput);
			FFilenameInput.SetSubType("node.cs", true);

			//create outputs
		}

		#endregion pin creation
		
		#region mainloop
		
		public void Configurate(IPluginConfig Input)
		{
			//nothing to configure in this plugin
			//only used in conjunction with inputs of type cmpdConfigurate
		}
		
		//here we go, thats the method called by vvvv each frame
		//all data handling should be in here
		public void Evaluate(int SpreadMax)
		{
			//if any of the inputs has changed
			//recompute the outputs
			if (FFilenameInput.PinIsChanged)
			{
				string filename;
				FFilenameInput.GetString(0, out filename);
				
				FTextEditorControl.LoadFile(filename);
				FTextEditorControl.EnableFolding = true;
				
				//redraw gui only if anything changed
				Invalidate();
			}
		}
		
		#endregion mainloop
		
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			
			FParserThread = new Thread(ParserThread);
			FParserThread.IsBackground = true;
			FParserThread.Start();
		}
		
		void ParserThread()
		{
			BeginInvoke(new MethodInvoker(delegate { FParserThreadLabel.Text = "Loading mscorlib..."; }));
			FMyProjectContent.AddReferencedContent(FPcRegistry.Mscorlib);
			
			// do one initial parser step to enable code-completion while other
			// references are loading
			ParseStep();
			
			string[] referencedAssemblies = {
				"System", "System.Data", "System.Drawing", "System.Xml", "System.Windows.Forms", "Microsoft.VisualBasic"
			};
			foreach (string assemblyName in referencedAssemblies) {
				string assemblyNameCopy = assemblyName; // copy for anonymous method
				BeginInvoke(new MethodInvoker(delegate { FParserThreadLabel.Text = "Loading " + assemblyNameCopy + "..."; }));
				Dom.IProjectContent referenceProjectContent = FPcRegistry.GetProjectContentForReference(assemblyName, assemblyName);
				FMyProjectContent.AddReferencedContent(referenceProjectContent);
				if (referenceProjectContent is Dom.ReflectionProjectContent) {
					(referenceProjectContent as Dom.ReflectionProjectContent).InitializeReferences();
				}
			}

			BeginInvoke(new MethodInvoker(delegate { FParserThreadLabel.Text = "Ready"; }));
			
			// Parse the current file every 2 seconds
			while (!IsDisposed) {
				ParseStep();
				
				Thread.Sleep(2000);
			}
		}
		
		void ParseStep()
		{
			string code = null;
			Invoke(new MethodInvoker(delegate {
			                         	code = FTextEditorControl.Text;
			                         }));
			TextReader textReader = new StringReader(code);
			Dom.ICompilationUnit newCompilationUnit;
			NRefactory.SupportedLanguage supportedLanguage;
			supportedLanguage = NRefactory.SupportedLanguage.CSharp;
			using (NRefactory.IParser p = NRefactory.ParserFactory.CreateParser(supportedLanguage, textReader)) {
				// we only need to parse types and method definitions, no method bodies
				// so speed up the parser and make it more resistent to syntax
				// errors in methods
				p.ParseMethodBodies = false;
				
				p.Parse();
				newCompilationUnit = ConvertCompilationUnit(p.CompilationUnit);
			}
			// Remove information from lastCompilationUnit and add information from newCompilationUnit.
			FMyProjectContent.UpdateCompilationUnit(FLastCompilationUnit, newCompilationUnit, DummyFileName);
			FLastCompilationUnit = newCompilationUnit;
			FParseInformation.SetCompilationUnit(newCompilationUnit);
		}
		
		Dom.ICompilationUnit ConvertCompilationUnit(NRefactory.Ast.CompilationUnit cu)
		{
			Dom.NRefactoryResolver.NRefactoryASTConvertVisitor converter;
			converter = new Dom.NRefactoryResolver.NRefactoryASTConvertVisitor(FMyProjectContent);
			cu.AcceptVisitor(converter, null);
			return converter.Cu;
		}
	}
}
