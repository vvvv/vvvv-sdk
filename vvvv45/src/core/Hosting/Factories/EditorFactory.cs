using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.Model;
using VVVV.Core.Model.CS;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Factories
{
	[Export(typeof(IAddonFactory))]
	public class EditorFactory : IAddonFactory, IMouseClickListener
	{
		[ImportMany(typeof(IEditor), AllowRecomposition = true)]
		protected List<ExportFactory<IEditor, IEditorInfo>> FNodeInfoExports;
		
		[Import]
		protected INodeInfoFactory FNodeInfoFactory;
		
		[Import]
		protected ISolution FSolution;
		
		private ILogger FLogger;
		private IHDEHost FHDEHost;
		private CompositionContainer FContainer;
		private HostExportProvider FHostExportProvider;
		private ExportProvider[] FExportProviders;
		private Dictionary<INodeInfo, ExportFactory<IEditor, IEditorInfo>> FNodeInfos;
		private Dictionary<IPluginHost2, ExportLifetimeContext<IEditor>> FExportLifetimeContexts;
		private int FMoveToLine;
		private INode FObservedNode;
		
		[ImportingConstructor]
		public EditorFactory(CompositionContainer parentContainer, ILogger logger, IHDEHost hdeHost)
		{
			FHostExportProvider = new HostExportProvider();
			FExportProviders = new ExportProvider[] { parentContainer, FHostExportProvider };
			FNodeInfos = new Dictionary<INodeInfo, ExportFactory<IEditor, IEditorInfo>>();
			FExportLifetimeContexts = new Dictionary<IPluginHost2, ExportLifetimeContext<IEditor>>();
			FLogger = logger;
			FHDEHost = hdeHost;
			FMoveToLine = -1;
			
			FHDEHost.AddListener(this);
		}
		
		public IEnumerable<INodeInfo> ExtractNodeInfos(string filename, string arguments)
		{
			// Present the user with all files associated with this filename.
			var nodeInfo = CreateNodeInfo(filename);
			if (nodeInfo != null)
				yield return nodeInfo;
			
			// Do we have a project file?
			var project = FSolution.FindProject(filename);
			if (project != null)
			{
				if (!project.IsLoaded)
					project.Load();
				
				foreach (var doc in project.Documents)
				{
					var docFilename = doc.Location.LocalPath;
					
					if (docFilename != filename)
					{
						nodeInfo = CreateNodeInfo(doc.Location.LocalPath);
						if (nodeInfo != null)
							yield return nodeInfo;
					}
				}
			}
		}
		
		private INodeInfo CreateNodeInfo(string filename)
		{
			var fileExtension = Path.GetExtension(filename);
			
			foreach (var nodeInfoExport in FNodeInfoExports)
			{
				if (nodeInfoExport.Metadata.FileExtensions.Contains(fileExtension))
				{
					var nodeInfo = FNodeInfoFactory.CreateNodeInfo(
						Path.GetFileName(filename),
						"Editor",
						string.Empty,
						filename);
					
					nodeInfo.BeginUpdate();
					nodeInfo.Type = NodeType.Text;
					nodeInfo.Ignore = true;
					nodeInfo.AutoEvaluate = true;
					nodeInfo.InitialBoxSize = new System.Drawing.Size(400, 300);
					nodeInfo.InitialWindowSize = new System.Drawing.Size(800, 600);
					nodeInfo.InitialComponentMode = TComponentMode.InAWindow;
					nodeInfo.CommitUpdate();
					
					FNodeInfos[nodeInfo] = nodeInfoExport;
					
					return nodeInfo;
				}
			}
			
			return null;
		}
		
		public bool Create(INodeInfo nodeInfo, IAddonHost host)
		{
			var editorHost = host as IPluginHost2;
			
			if (editorHost != null && FNodeInfos.ContainsKey(nodeInfo))
			{
				var nodeInfoExport = FNodeInfos[nodeInfo];
				var exportLifetimeContext = nodeInfoExport.CreateExport();
				FExportLifetimeContexts[editorHost] = exportLifetimeContext;
				
				var editor = exportLifetimeContext.Value;
				editorHost.Plugin = editor;
				
				editor.Open(nodeInfo.Filename);
				
				if (FMoveToLine >= 0)
					editor.MoveTo(FMoveToLine);
				
				if (FObservedNode != null)
					editor.LinkedNode = FObservedNode;
				
				return true;
			}
			
			return false;
		}
		
		public bool Delete(INodeInfo nodeInfo, IAddonHost host)
		{
			var editorHost = host as IPluginHost2;
			
			if (editorHost != null && FNodeInfos.ContainsKey(nodeInfo))
			{
				var exportLifetimeContext = FExportLifetimeContexts[editorHost];
				exportLifetimeContext.Dispose();
				FExportLifetimeContexts.Remove(editorHost);
			}
			
			return false;
		}
		
		public bool Clone(INodeInfo nodeInfo, string path, string name, string category, string version, out INodeInfo newNodeInfo)
		{
			// TODO: What to do here?
			newNodeInfo = null;
			return false;
		}
		
		public event NodeInfoEventHandler NodeInfoAdded;
		
		protected virtual void OnNodeInfoAdded(INodeInfo nodeInfo)
		{
			if (NodeInfoAdded != null)
				NodeInfoAdded(this, nodeInfo);
		}
		
		public event NodeInfoEventHandler NodeInfoRemoved;
		
		protected virtual void OnNodeInfoRemoved(INodeInfo nodeInfo)
		{
			if (NodeInfoRemoved != null)
				NodeInfoRemoved(this, nodeInfo);
		}
		
		public string JobStdSubPath {
			get {
				return "editors";
			}
		}
		
		public void AddDir(string dir)
		{
			try
			{
				var catalog = new DirectoryCatalog(dir);
				FContainer = new CompositionContainer(catalog, FExportProviders);
				FContainer.ComposeParts(this);
			}
			catch (ReflectionTypeLoadException e)
			{
				foreach (var f in e.LoaderExceptions)
					FLogger.Log(f);
				return;
			}
			catch (Exception e)
			{
				FLogger.Log(e);
				return;
			}
		}
		
		public void RemoveDir(string dir)
		{
			// Nothing todo. We didn't emit any new node info.
		}

		public event NodeInfoEventHandler NodeInfoUpdated;
		
		protected virtual void OnNodeInfoUpdated(IAddonFactory factory, INodeInfo nodeInfo)
		{
			if (NodeInfoUpdated != null) {
				NodeInfoUpdated(factory, nodeInfo);
			}
		}

		public void MouseDownCB(INode node, Mouse_Buttons button, Modifier_Keys keys)
		{
			if (node == null) return;
			
			if ((button == Mouse_Buttons.Left) && (keys == Modifier_Keys.Control))
			{
				// Let the user choose which file to open.
				
				var nodeInfo = node.GetNodeInfo();
				
				switch (nodeInfo.Type)
				{
					case NodeType.Text:
					case NodeType.Dynamic:
					case NodeType.Effect:
						// The following Open will trigger a call by vvvv to IInternalHDEHost.ExtractNodeInfos()
						// Force the hde host to collect node info only from us.
						var addonFactories = new List<IAddonFactory>(FHDEHost.AddonFactories);
						try
						{
							FHDEHost.AddonFactories.Clear();
							FHDEHost.AddonFactories.Add(this);
							FHDEHost.Open(nodeInfo.Filename, false);
						}
						finally
						{
							FHDEHost.AddonFactories.Clear();
							FHDEHost.AddonFactories.AddRange(addonFactories);
						}
						break;
				}
			}
			else if (button == Mouse_Buttons.Right)
			{
				// Try to locate exact file based on nodeinfo and navigate to its definition.
				
				var nodeInfo = node.GetNodeInfo();
				
				switch (nodeInfo.Type)
				{
					case NodeType.Text:
					case NodeType.Dynamic:
					case NodeType.Effect:
						var filename = nodeInfo.Filename;
						
						// Do we have a project file?
						var project = FSolution.FindProject(filename);
						if (project != null && project is CSProject)
						{
							// Find the document where this nodeinfo is defined.
							var doc = FindDefiningDocument(project as CSProject, nodeInfo);
							if (doc != null)
							{
								filename = doc.Location.LocalPath;
								FMoveToLine = FindDefiningLine(doc, nodeInfo);
								FObservedNode = node;
							}
						}
						
						// The following Open will trigger a call by vvvv to IInternalHDEHost.ExtractNodeInfos()
						// Force the hde host to collect node info only from us.
						var addonFactories = new List<IAddonFactory>(FHDEHost.AddonFactories);
						try
						{
							FHDEHost.AddonFactories.Clear();
							FHDEHost.AddonFactories.Add(this);
							FHDEHost.Open(filename, false);
						}
						finally
						{
							FMoveToLine = -1;
							FObservedNode = null;
							
							FHDEHost.AddonFactories.Clear();
							FHDEHost.AddonFactories.AddRange(addonFactories);
						}
						break;
				}
			}
		}
		
		public void MouseUpCB(INode node, Mouse_Buttons button, Modifier_Keys keys)
		{
		}
		
		protected CSDocument FindDefiningDocument(CSProject project, INodeInfo nodeInfo)
		{
			foreach (var doc in project.Documents)
			{
				var csDoc = doc as CSDocument;
				if (csDoc == null) continue;
				
				if (FindDefiningLine(csDoc, nodeInfo) >= 0)
					return csDoc;
			}
			
			return null;
		}
		
		protected int FindDefiningLine(CSDocument document, INodeInfo nodeInfo)
		{
			var parseInfo = document.ParseInfo;
			var compilationUnit = parseInfo.MostRecentCompilationUnit;
			if (compilationUnit == null) return -1;
			
			foreach (var clss in compilationUnit.Classes)
			{
				foreach (var attribute in clss.Attributes)
				{
					var attributeType = attribute.AttributeType;
					var pluginInfoName = typeof(PluginInfoAttribute).Name;
					var pluginInfoShortName = pluginInfoName.Replace("Attribute", "");
					if (attributeType.Name == pluginInfoName || attributeType.Name == pluginInfoShortName)
					{
						// Check name
						string name = null;
						if (attribute.NamedArguments.ContainsKey("Name"))
							name = (string) attribute.NamedArguments["Name"];
						else if (attribute.PositionalArguments.Count >= 0)
							name = (string) attribute.PositionalArguments[0];
						
						if (name != nodeInfo.Name)
							continue;
						
						// Check category
						string category = null;
						if (attribute.NamedArguments.ContainsKey("Category"))
							category = (string) attribute.NamedArguments["Category"];
						else if (attribute.PositionalArguments.Count >= 1)
							category = (string) attribute.PositionalArguments[1];
						
						if (category != nodeInfo.Category)
							continue;

						// Possible match
						bool match = true;
						
						// Check version
						if (nodeInfo.Version != null)
						{
							string version = null;
							if (attribute.NamedArguments.ContainsKey("Version"))
								version = (string) attribute.NamedArguments["Version"];
							else if (attribute.PositionalArguments.Count >= 2)
								version = (string) attribute.PositionalArguments[2];
							
							match = version == nodeInfo.Version;
						}
						
						if (match)
							return attribute.Region.BeginLine - 1;
					}
				}
			}
			
			return -1;
		}
	}
	
	public interface IEditorInfo
	{
		string[] FileExtensions { get; }
	}
}
