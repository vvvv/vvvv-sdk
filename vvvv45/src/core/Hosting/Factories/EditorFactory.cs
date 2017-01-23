using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.Model;
using VVVV.Core.Model.CS;
using VVVV.Hosting.Interfaces;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils.Linq;

namespace VVVV.Hosting.Factories
{
    [Export(typeof(IAddonFactory))]
    [Export(typeof(EditorFactory))]
    [ComVisible(false)]
    public class EditorFactory : IAddonFactory
    {
        [ImportMany(typeof(IEditor), AllowRecomposition = true)]
        protected List<ExportFactory<IEditor, IEditorInfo>> FChangingNodeInfoExports;
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
        private Dictionary<IInternalPluginHost, ExportLifetimeContext<IEditor>> FExportLifetimeContexts;
        private int FMoveToLine;
        private int FMoveToColumn;
        private bool FInOpen;
        
        [ImportingConstructor]
        public EditorFactory(CompositionContainer parentContainer, ILogger logger, IHDEHost hdeHost)
        {
            FHostExportProvider = new HostExportProvider();
            FExportProviders = new ExportProvider[] { parentContainer, FHostExportProvider };
            FNodeInfos = new Dictionary<INodeInfo, ExportFactory<IEditor, IEditorInfo>>();
            FExportLifetimeContexts = new Dictionary<IInternalPluginHost, ExportLifetimeContext<IEditor>>();
            FNodeInfoExports = new List<ExportFactory<IEditor, IEditorInfo>>();
            FLogger = logger;
            FHDEHost = hdeHost;
            FMoveToLine = -1;
            FMoveToColumn = -1;
            
            FHDEHost.MouseDown += HandleMouseDown;
        }
        
        public string Name
        {
            get
            {
                return ToString();
            }
        }
        
        public bool AllowCaching
        {
            get
            {
                return false;
            }
        }

        public bool GetNodeListAttribute(INodeInfo nodeInfo, out string name, out string value)
        {
            name = string.Empty;
            value = string.Empty;
            return false;
        }

        public void ParseNodeEntry(System.Xml.XmlReader xmlReader, INodeInfo nodeInfo)
        {
            
        }

        public INodeInfo[] ExtractNodeInfos(string filename, string arguments)
        {
            var result = new List<INodeInfo>();
            
            // Present the user with all files associated with this filename.
            var nodeInfo = CreateNodeInfo(filename);
            if (nodeInfo != null)
                result.Add(nodeInfo);
            
            if (FInOpen)
            {
                // Do we have a project file?
                // TODO: Do not hardcode project extension.
                if (Path.GetExtension(filename) == ".csproj")
                {
                    var project = FSolution.FindProject(filename);
                    if (project != null)
                    {
                        foreach (var doc in project.Documents.OfType<ITextDocument>())
                        {
                            var docFilename = doc.LocalPath;
                            
                            if (docFilename != filename)
                            {
                                nodeInfo = CreateNodeInfo(doc.LocalPath);
                                if (nodeInfo != null)
                                    result.Add(nodeInfo);
                            }
                        }
                    }
                }
            }
            
            return result.ToArray();
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
                        filename,
                        true);
                    
                    nodeInfo.Type = NodeType.Text;
                    nodeInfo.Factory = this;
                    nodeInfo.Ignore = true;
                    nodeInfo.AutoEvaluate = true;
                    nodeInfo.InitialBoxSize = new System.Drawing.Size(400, 300);
                    nodeInfo.InitialWindowSize = new System.Drawing.Size(800, 950);
                    nodeInfo.InitialComponentMode = TComponentMode.InAWindow;
                    nodeInfo.CommitUpdate();
                    
                    FNodeInfos[nodeInfo] = nodeInfoExport;
                    
                    return nodeInfo;
                }
            }
            
            return null;
        }
        
        public bool Create(INodeInfo nodeInfo, INode host)
        {
            bool result = false;
            
            var editorHost = host as IInternalPluginHost;
            
            if (editorHost != null && FNodeInfos.ContainsKey(nodeInfo))
            {
                // Try to find an existing editor which has opened this file
                var entries =
                    from entry in FExportLifetimeContexts
                    let e = entry.Value.Value
                    where !(string.IsNullOrEmpty(e.OpenedFile) || string.IsNullOrEmpty(nodeInfo.Filename))
                    where new Uri(e.OpenedFile) == new Uri(nodeInfo.Filename)
                    select entry;
                
                if (entries.Any())
                {
                    return ShowEditor(entries.FirstOrDefault());
                }
                
                // We didn't find a suitable editor, create a new one.
                FHostExportProvider.PluginHost = host as IInternalPluginHost;
                
                var nodeInfoExport = FNodeInfos[nodeInfo];
                var exportLifetimeContext = nodeInfoExport.CreateExport();
                FExportLifetimeContexts[editorHost] = exportLifetimeContext;
                
                var editor = exportLifetimeContext.Value;
                editorHost.Plugin = editor;
                editorHost.Win32Window = editor as System.Windows.Forms.IWin32Window;
                editor.Open(nodeInfo.Filename);
                editor.MoveTo(FMoveToLine, FMoveToColumn);
                
                result = true;
                
                FMoveToLine = -1;
            }
            
            return result;
        }
        
        private bool ShowEditor(KeyValuePair<IInternalPluginHost, ExportLifetimeContext<IEditor>> entry)
        {
            var editor = entry.Value.Value;
            var editorNode = entry.Key as INode;
            editor.MoveTo(FMoveToLine, FMoveToColumn);
            
            FHDEHost.ShowGUI(FindNodeFromInternal(editorNode));
            return false;
        }
        
        public bool Delete(INodeInfo nodeInfo, INode host)
        {
            var editorHost = host as IInternalPluginHost;
            
            if (editorHost != null && FNodeInfos.ContainsKey(nodeInfo) && FExportLifetimeContexts.ContainsKey(editorHost))
            {
                var exportLifetimeContext = FExportLifetimeContexts[editorHost];
                exportLifetimeContext.Dispose();
                FExportLifetimeContexts.Remove(editorHost);
            }
            
            return true;
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
        
        public void AddDir(string dir, bool recursive)
        {
            // Ignore non editor directories to save performance.
            if (!dir.EndsWith(JobStdSubPath)) return;
            
            var catalog = new DirectoryCatalog(dir);
            FContainer = new CompositionContainer(catalog, FExportProviders);
            FContainer.ComposeParts(this);
            
            FNodeInfoExports.AddRange(FChangingNodeInfoExports);
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
        
        void HandleMouseDown(object sender, MouseEventArgs args)
        {
            var node = args.Node;
            if (node == null) return;
            
            var button = args.Button;
            var keys = args.ModifierKey;
            
            if ((button == Mouse_Buttons.Left) && (keys == Modifier_Keys.Shift))
            {
                // Let the user choose which file to open.
                
                var nodeInfo = node.NodeInfo;
                
                switch (nodeInfo.Type)
                {
                    case NodeType.Text:
                    case NodeType.Dynamic:
                    case NodeType.Effect:
                        Open(nodeInfo.Filename, -1, -1);
                        break;
                }
            }
            else if (button == Mouse_Buttons.Right)
            {
                OpenEditor(node);
            }
        }
        
        private INode2 FindNodeFromInternal(INode internalNode)
        {
            return
                (
                    from n in FHDEHost.RootNode.AsDepthFirstEnumerable()
                    where n.InternalCOMInterf == internalNode
                    select n
                   ).FirstOrDefault();
        }
        
        public void OpenEditor(INode internalNode)
        {
            var node = FindNodeFromInternal(internalNode);
            if (node == null) return;
            OpenEditor(node);
        }
        
        public void OpenEditor(INode2 node)
        {
            var nodeInfo = node.NodeInfo;
            switch (nodeInfo.Type)
            {
                case NodeType.Unknown:
                case NodeType.Dynamic:
                case NodeType.Effect:
                    // Try to locate exact file based on nodeinfo and navigate to its definition.
                    var filename = nodeInfo.Filename;
                    var line = -1;
                    
                    // Do we have a project file?
                    var project = FSolution.FindProject(filename) as CSProject;
                    if (project != null)
                    {
                        // Find the document where this nodeinfo is defined.
                        var doc = FindDefiningDocument(project, nodeInfo);
                        if (doc != null)
                        {
                            filename = doc.LocalPath;
                            line = FindDefiningLine(doc, nodeInfo);
                        }
                    }
                    else
                    {
                        // Do not try to open the file if there's no editor
                        // registered for this file extension.
                        var fileExtension = Path.GetExtension(filename);
                        
                        var editorFindQuery =
                            from editorExport in FNodeInfoExports
                            let editorInfo = editorExport.Metadata
                            where editorInfo.FileExtensions.Contains(fileExtension)
                            select editorExport.Metadata;
                        
                        if (!editorFindQuery.Any()) return;
                    }
                    
                    Open(filename, line, 0);
                    break;
            }
        }
        
        public void Open(string filename)
        {
            Open(filename, -1);
        }
        
        public void Open(string filename, int line)
        {
            Open(filename, line, -1);
        }
        
        public void Open(string filename, int line, int column)
        {
            Open(filename, line, column, null);
        }
        
        public void Open(string filename, int line, int column, IWindow window)
        {
            if (!File.Exists(filename))
            {
                FLogger.Log(LogType.Error, "File {0} doesn't exist!", filename);
                return;
            }
            
            foreach (var entry in FExportLifetimeContexts)
            {
                var editorNode = entry.Key as INode;
                var editor = entry.Value.Value;
                        
                if (new Uri(editor.OpenedFile) == new Uri(filename))
                {
                    editor.MoveTo(line, column);
                    FHDEHost.ShowGUI(FindNodeFromInternal(editorNode));
                    return;
                }
            }
            
//			if (window == null)
//			{
//				// Before we open the editor in a new window, see if the file to
//				// open is a project file and search for an editor already
//				// editing this project.
//				// If we find one, open the editor there.
//				var project = FSolution.FindProject(filename);
//				if (project != null && project.IsLoaded)
//				{
//					foreach (var doc in project.Documents)
//					{
//						var docFilename = doc.Location.LocalPath;
//
//						var editorNodes =
//							from entry in FExportLifetimeContexts
//							let e = entry.Value.Value
//							where new Uri(e.OpenedFile) == new Uri(docFilename)
//							select entry.Key as INode;
//
//						var editorNode = editorNodes.First();
//						if (editorNode != null)
//						{
//							window = editorNode.Window;
//							break;
//						}
//					}
//				}
//			}
            
            // The following Open will trigger a call by vvvv to IInternalHDEHost.ExtractNodeInfos()
            // Force the hde host to collect node infos from us only.
            var addonFactories = new List<IAddonFactory>(FHDEHost.AddonFactories);
            try
            {
                FMoveToLine = line;
                FMoveToColumn = column;
                FInOpen = true;
                
                FHDEHost.AddonFactories.Clear();
                FHDEHost.AddonFactories.Add(this);
                FHDEHost.Open(filename, false, window);
            }
            finally
            {
                FInOpen = false;
                FHDEHost.AddonFactories.Clear();
                FHDEHost.AddonFactories.AddRange(addonFactories);
                foreach (var factory in addonFactories)
                {
                    try
                    {
                        factory.ExtractNodeInfos(filename, null).ToList();
                    }
                    catch (Exception)
                    {
                        // Swallow exceptions
                    }
                }
            }
        }
        
        protected static CSDocument FindDefiningDocument(CSProject project, INodeInfo nodeInfo)
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
        
        protected static int FindDefiningLine(CSDocument document, INodeInfo nodeInfo)
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
                        if (!string.IsNullOrEmpty(nodeInfo.Version))
                        {
                            string version = null;
                            if (attribute.NamedArguments.ContainsKey("Version"))
                                version = (string) attribute.NamedArguments["Version"];
                            else if (attribute.PositionalArguments.Count >= 2)
                                version = (string) attribute.PositionalArguments[2];
                            
                            match = version == nodeInfo.Version;
                        }
                        
                        if (match)
                            return attribute.Region.BeginLine;
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
