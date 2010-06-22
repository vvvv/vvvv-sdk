using System;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using Microsoft.Practices.Unity;

using ManagedVCL;
using VVVV.PluginInterfaces.V1;
using VVVV.HDE.Model;
using VVVV.HDE.Model.CS;
using VVVV.HDE.Viewer;
using VVVV.HDE.Viewer.Model;
using VVVV.HDE.Model.Provider;
using VVVV.Utils.Event;

using Dom = ICSharpCode.SharpDevelop.Dom;

namespace VVVV.Nodes
{
	/// <summary>
	/// Description of CodeEditorPlugin.
	/// </summary>
	public partial class CodeEditorPlugin : ManagedVCL.TopControl, IHDEPlugin
	{
		#region Fields
		private INodeSelectionListener FNodeSelectionListener;
		private Dictionary<ITextDocument, TabPage> FOpenedDocuments;
		private IUnityContainer FUnityContainer;
		private IContentProvider FContentProvider;
		private ILabelProvider FLabelProvider;
		private IDragDropProvider FDragDropProvider;
		private IContextMenuProvider FContextMenuProvider;
		private Dom.ProjectContentRegistry FPCRegistry;
		private Dictionary<IProject, Dom.DefaultProjectContent> FProjects;
		private BackgroundParser FBGParser;
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
			
			FOpenedDocuments = new Dictionary<ITextDocument, TabPage>();
			FNodeSelectionListener = new NodeSelectionListener(this);
			
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
			
			FUnityContainer = HdeHost.UnityContainer.CreateChildContainer();
			FUnityContainer.AddNewExtension<HdeModelContainerExtension>();
			FContentProvider = new UnityContentProvider(FUnityContainer);
			FLabelProvider = new UnityLabelProvider(FUnityContainer);
			FDragDropProvider = new UnityDragDropProvider(FUnityContainer);
			FContextMenuProvider = new UnityContextMenuProvider(FUnityContainer);
			
			FProjectTreeViewer.SetContentProvider(FContentProvider);
			FProjectTreeViewer.SetLabelProvider(FLabelProvider);
			FProjectTreeViewer.SetDragDropProvider(FDragDropProvider);
			FProjectTreeViewer.SetContextMenuProvider(FContextMenuProvider);
			FProjectTreeViewer.SetRoot(HdeHost.Solution);
			FProjectTreeViewer.LeftDoubleClick += LeftDoubleClickCB;
			
			// Start to parse the solution.
			FPCRegistry = new Dom.ProjectContentRegistry(); // Default .NET 2.0 registry
			// Persistence lets SharpDevelop.Dom create a cache file on disk so that
			// future starts are faster.
			// It also caches XML documentation files in an on-disk hash table, thus
			// reducing memory usage.
			FPCRegistry.ActivatePersistence(Path.Combine(Path.GetTempPath(), "VVVVCodeEditor"));
			
			// Setup project contents for each C# project.
			FProjects = new Dictionary<IProject, Dom.DefaultProjectContent>();
			foreach (var project in HdeHost.Solution.Projects)
			{
				if (project is CSProject)
				{
					var projectContent = new Dom.DefaultProjectContent();
					FProjects.Add(project, projectContent);
					project.PropertyChanged += ProjectPropertyChangedCB;
				}
			}
			
			// Create the background assembly parser
			FBGParser = new BackgroundParser(FPCRegistry, FProjects, FStatusLabel);
			FBGParser.Parse(FProjects.Keys);
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
					FPluginInfo.ShortCut = "Ctrl+J";
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
		#endregion IHDEPlugin
		
		#region Methods
		/// <summary>
		/// Opens the specified ITextDocument in a new editor or if already opened brings editor to top.
		/// </summary>
		/// <param name="doc">The ITextDocument to open.</param>
		public void Open(ITextDocument doc)
		{
			if (!FOpenedDocuments.ContainsKey(doc))
			{
				var editor = new CodeEditor(this, doc, FImageList);
				var tabPage = new TabPage(FLabelProvider.GetText(doc));
				
				FTabControl.SuspendLayout();
				
				editor.Dock = DockStyle.Fill;
				FTabControl.Controls.Add(tabPage);
				tabPage.Controls.Add(editor);
				
				FTabControl.ResumeLayout();
	
				FOpenedDocuments[doc] = tabPage;
				doc.ContentChanged += DocumentContentChangedCB;
				doc.Saved += DocumentSavedCB;
			}
			FTabControl.SelectTab(FOpenedDocuments[doc]);
		}
		
		/// <summary>
		/// Returns the project content (contains parse information) for an IProject.
		/// </summary>
		public Dom.DefaultProjectContent GetProjectContent(IProject project)
		{
			if (FProjects.ContainsKey(project))
				return FProjects[project];
			return null;
		}
		
		protected string GetTabPageName(ITextDocument document)
		{
			if (document.IsDirty)
				return FLabelProvider.GetText(document) + "*";
			else
				return FLabelProvider.GetText(document);
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
		
		void DocumentContentChangedCB(ITextDocument document, string content)
		{
			FOpenedDocuments[document].Text = GetTabPageName(document);
		}
		
		void DocumentSavedCB(object sender, EventArgs args)
		{
			var document = sender as ITextDocument;
			FOpenedDocuments[document].Text = GetTabPageName(document);
		}
		
		void ProjectPropertyChangedCB(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "References")
			{
				// Tell the background parser to reparse this project
				FBGParser.Parse(sender as IProject);
			}
		}
		#endregion
		
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
					if (FProjects != null)
					{
						foreach (var project in FProjects.Keys)
							project.PropertyChanged -= ProjectPropertyChangedCB;
						FProjects.Clear();
					}
					
					if (FBGParser != null)
						FBGParser.CancelAsync();
					
					if (HdeHost != null)
						HdeHost.RemoveListener(FNodeSelectionListener);
					FNodeSelectionListener = null;
					
					if (FProjectTreeViewer != null)
						FProjectTreeViewer.LeftDoubleClick -= LeftDoubleClickCB;
					
					foreach (var entry in FOpenedDocuments)
					{
						entry.Key.ContentChanged -= DocumentContentChangedCB;
						entry.Key.Saved -= DocumentSavedCB;
					}
					
					FOpenedDocuments = null;
					
					if (components != null) {
						components.Dispose();
					}
					components = null;
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				
				if (PluginHost != null)
					PluginHost.Log(TLogType.Debug, "CodeEditorPlugin is being deleted");
				PluginHost = null;
			}
			FDisposed = true;
			
			base.Dispose(disposing);
		}
		#endregion IDisposable
	}
}
