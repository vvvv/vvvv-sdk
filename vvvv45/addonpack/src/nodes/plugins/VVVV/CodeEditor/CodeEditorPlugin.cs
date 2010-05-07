using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using ManagedVCL;
using VVVV.PluginInterfaces.V1;
using VVVV.HDE.Model;
using VVVV.Utils.Adapter;
using VVVV.HDE.Viewer;
using VVVV.HDE.Viewer.Model;
using VVVV.HDE.Model.Provider;

namespace VVVV.Nodes
{
	/// <summary>
	/// Description of CodeEditorPlugin.
	/// </summary>
	public partial class CodeEditorPlugin : ManagedVCL.TopControl, IHDEPlugin
	{
		#region Fields
		private bool FDisposed = false;
		private INodeSelectionListener FNodeSelectionListener;
		private Dictionary<IDocument, CodeEditor> FOpenedDocuments;
		private IAdapterFactory FAdapterFactory;
		private IContentProvider FContentProvider;
		private ILabelProvider FLabelProvider;
		private IDragDropProvider FDragDropProvider;
		#endregion
		
		#region Properties
		public IHDEHost HdeHost { get; private set; }
		public IPluginHost PluginHost { get; private set; }
		#endregion
		
		#region Constructor
		public CodeEditorPlugin()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			FOpenedDocuments = new Dictionary<IDocument, CodeEditor>();
			FNodeSelectionListener = new NodeSelectionListener(this);
			FAdapterFactory = new HdeAdapterFactory();
			FContentProvider = new AdapterFactoryContentProvider(FAdapterFactory);
			FLabelProvider = new AdapterFactoryLabelProvider(FAdapterFactory);
			FDragDropProvider = new AdapterFactoryDragDropProvider(FAdapterFactory);
			
			var resources = new ComponentResourceManager(typeof(CodeEditorPlugin));
			FImageList.ImageStream = ((ImageListStreamer)(resources.GetObject("FImageList.ImageStream")));
			FImageList.TransparentColor = System.Drawing.Color.Transparent;
			FImageList.Images.SetKeyName(0, "Icons.16x16.Class.png");
			FImageList.Images.SetKeyName(1, "Icons.16x16.Method.png");
			FImageList.Images.SetKeyName(2, "Icons.16x16.Property.png");
			FImageList.Images.SetKeyName(3, "Icons.16x16.Field.png");
			FImageList.Images.SetKeyName(4, "Icons.16x16.Enum.png");
			FImageList.Images.SetKeyName(5, "Icons.16x16.NameSpace.png");
			FImageList.Images.SetKeyName(6, "Icons.16x16.Event.png");
		}
		#endregion Constructor
		
		#region IHDEPlugin
		public void SetHDEHost(IHDEHost host)
		{
			HdeHost = host;
			HdeHost.AddListener(FNodeSelectionListener);
			
			FProjectTreeViewer.SetContentProvider(FContentProvider);
			FProjectTreeViewer.SetLabelProvider(FLabelProvider);
			FProjectTreeViewer.SetDragDropProvider(FDragDropProvider);
			FProjectTreeViewer.SetRoot(HdeHost.Solution);
			FProjectTreeViewer.LeftDoubleClick += LeftDoubleClickCB;
		}
		
		public void SetPluginHost(IPluginHost host)
		{
			PluginHost = host;
		}
		
		#region Node name and infos
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
		
		#region IDisposable
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
					if (components != null) {
						components.Dispose();
					}
					
					if (HdeHost != null)
						HdeHost.RemoveListener(FNodeSelectionListener);
					
					if (FProjectTreeViewer != null)
						FProjectTreeViewer.LeftDoubleClick -= LeftDoubleClickCB;
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				
				if (PluginHost != null)
					PluginHost.Log(TLogType.Debug, "CodeEditorPlugin is being deleted");
				
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
		#endregion IHDEPlugin
		
		#region Methods
		/// <summary>
		/// Opens the specified ITextDocument in a new editor or if already opened brings editor to top.
		/// </summary>
		/// <param name="doc">The ITextDocument to open.</param>
		public void Open(ITextDocument doc)
		{
			if (FOpenedDocuments.ContainsKey(doc))
			{
				// TODO: Bring it to top
				return;
			}
			
			CodeEditor editor = new CodeEditor(FStatusLabel, FImageList);
			editor.Dock = DockStyle.Fill;
			editor.Open(doc);
			
			TabPage tabPage = new TabPage(FLabelProvider.GetText(doc));
			FTabControl.Controls.Add(tabPage);
			tabPage.Controls.Add(editor);

			FOpenedDocuments[doc] = editor;
		}
		#endregion
		
		#region Callbacks
		void LeftDoubleClickCB(object sender, EventArgs args)
		{
			if (sender is ITextDocument)
			{
				Open(sender as ITextDocument);
			}
		}
		#endregion
	}
}
