using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Microsoft.Practices.Unity;
using VVVV.Core;
using VVVV.Core.Commands;
using VVVV.Core.Logging;
using VVVV.Core.Model;
using VVVV.Hosting;
using VVVV.Hosting.Factories;
using VVVV.Hosting.Graph;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils.Linq;

namespace VVVV.Hosting
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IHDEHost))]
    [ComVisible(false)]
    class HDEHost : IInternalHDEHost, IHDEHost, IDisposable,
    IMouseClickListener, INodeSelectionListener, IWindowListener, IWindowSelectionListener
    {
        public const string ENV_VVVV = "VVVV45";
        
        const string WINDOW_SWITCHER = "WindowSwitcher (VVVV)";
        const string KOMMUNIKATOR = "Kommunikator (VVVV)";
        const string NODE_BROWSER = "NodeBrowser (VVVV)";
        
        private INodeInfo FWinSwNodeInfo;
        private INodeInfo FKomNodeInfo;
        private INodeInfo FNodeBrowserNodeInfo;
        
        private IVVVVHost FVVVVHost;
        private INodeBrowser FNodeBrowser;
        private IPluginBase FWindowSwitcher, FKommunikator;
        protected Dictionary<string, HashSet<ProxyNodeInfo>> FNodeInfoCache;
        protected Dictionary<string, HashSet<ProxyNodeInfo>> FDeserializedNodeInfoCache;
        private Dictionary<string, bool> FLoadedFiles = new Dictionary<string, bool>();
        
        [Export]
        public CompositionContainer Container { get; protected set; }
        
        [Export(typeof(ILogger))]
        public DefaultLogger Logger { get; private set; }
        
        [Export]
        public INodeBrowserHost NodeBrowserHost { get; protected set; }
        
        [Export]
        public IWindowSwitcherHost WindowSwitcherHost { get; protected set; }
        
        [Export]
        public IKommunikatorHost KommunikatorHost { get; protected set; }
        
        [Export]
        public ISolution Solution { get; set; }
        
        [Export(typeof(INodeInfoFactory))]
        public ProxyNodeInfoFactory NodeInfoFactory { get; set; }
        
        [ImportMany]
        public List<IAddonFactory> AddonFactories
        {
            get;
            private set;
        }
        
        [Import]
        private DotNetPluginFactory PluginFactory { get; set; }
        
        [Import]
        private EditorFactory EditorFactory { get; set; }

        [Import]
        public NodeCollection NodeCollection {get; protected set;}
        
        public HDEHost()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblyCB;
            
            //set vvvv.exe path
            ExePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName((typeof(HDEHost).Assembly.Location)), @"..\.."));
            
            //set cache file name
            var filepath = Path.Combine(Path.GetTempPath(), "vvvv");
            
            uint hash;
            unchecked
            {
                hash = (uint)ExePath.GetHashCode();
            }
            
            CacheFileName = Path.Combine(filepath, "node_info_" + hash + ".cache");
            
            //setup cache save timer
            FCacheTimer = new System.Windows.Forms.Timer();
            FCacheTimer.Interval = 10000;
            FCacheTimer.Tick += new EventHandler(FCacheTimer_Tick);
            
            // Set name to vvvv thread for easier debugging.
            Thread.CurrentThread.Name = "vvvv";
            
            // Create a windows forms sync context (FileSystemWatcher runs asynchronously).
            var context = SynchronizationContext.Current;
            if (context == null)
            {
                // We need to create a user control to get a sync context.
                var control = new UserControl();
                context = SynchronizationContext.Current;
                control.Dispose();
                
                Debug.Assert(context != null, "SynchronizationContext not set.");
            }
            
            // Register at least one ICommandHistory for top level element ISolution
            var mappingRegistry = new MappingRegistry();
            mappingRegistry.RegisterMapping<ISolution, ICommandHistory, CommandHistory>(MapInstantiation.PerInstanceAndItsChilds);
            
            var location = new Uri(Shell.CallerPath.ConcatPath(@"..\..\plugins").ConcatPath("Solution.sln"));
            Solution = new Solution(location, mappingRegistry);
            
            EnumManager.SetHDEHost(this);
            
            Logger = new DefaultLogger();
        }

        //serialize cache when timer has ended
        private uint FLastTimestamp;
        void FCacheTimer_Tick(object sender, EventArgs e)
        {
            var timestamp = NodeInfoFactory.Timestamp;
            if (timestamp != FLastTimestamp)
            {
                SerializeNodeInfoCache();
            }
            FLastTimestamp = timestamp;
        }
        
        private bool IsLoaded(string filename)
        {
            return FLoadedFiles.ContainsKey(filename) && FLoadedFiles[filename];
        }
        
        private void UpdateNodeInfos(string filename)
        {
            if (!IsLoaded(filename))
            {
                LoadNodeInfos(filename, string.Empty);
                FLoadedFiles[filename] = true;
            }
        }
        
        private HashSet<ProxyNodeInfo> LoadNodeInfos(string filename, string arguments)
        {
            var nodeInfos = new HashSet<ProxyNodeInfo>();
            
            foreach(IAddonFactory factory in AddonFactories)
            {
                foreach (var nodeInfo in factory.ExtractNodeInfos(filename, arguments))
                {
                    nodeInfos.Add((ProxyNodeInfo) nodeInfo);
                }
            }
            
            return nodeInfos;
        }
        
        #region IInternalHDEHost
        
        public void Initialize(IVVVVHost vvvvHost, INodeBrowserHost nodeBrowserHost, IWindowSwitcherHost windowSwitcherHost, IKommunikatorHost kommunikatorHost)
        {
            // Set VVVV45 to this running vvvv.exe
            Environment.SetEnvironmentVariable(ENV_VVVV, Path.GetFullPath(Shell.CallerPath.ConcatPath("..").ConcatPath("..")));
            
            FVVVVHost = vvvvHost;
            NodeInfoFactory = new ProxyNodeInfoFactory(vvvvHost.NodeInfoFactory, this);
            
            FVVVVHost.AddMouseClickListener(this);
            FVVVVHost.AddNodeSelectionListener(this);
            FVVVVHost.AddWindowListener(this);
            FVVVVHost.AddWindowSelectionListener(this);
            
            NodeInfoFactory.NodeInfoAdded += factory_NodeInfoAdded;
            NodeInfoFactory.NodeInfoRemoved += factory_NodeInfoRemoved;
            NodeInfoFactory.NodeInfoUpdated += factory_NodeInfoUpdated;

            // Route log messages to vvvv
            Logger.AddLogger(new VVVVLogger(FVVVVHost));
            
            NodeBrowserHost = new ProxyNodeBrowserHost(nodeBrowserHost, NodeInfoFactory);
            WindowSwitcherHost = windowSwitcherHost;
            KommunikatorHost = kommunikatorHost;
            
            //deserialize node info cache dict
            DeserializeNodeInfoCache();
            
            //do not add the entire directory for faster startup
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(HDEHost).Assembly.Location));
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(NodeCollection).Assembly.Location));
            //allow plugin writers to add their own factories
            var factoriesPath = Path.GetDirectoryName(typeof(HDEHost).Assembly.Location).ConcatPath("factories");
            if (Directory.Exists(factoriesPath))
                catalog.Catalogs.Add(new DirectoryCatalog(factoriesPath));
            Container = new CompositionContainer(catalog);
            Container.ComposeParts(this);
            
            NodeInfoFactory.NodeInfoAdded += NodeInfoFactory_NodeInfoAdded;
            
            //NodeCollection.AddJob(Shell.CallerPath.Remove(Shell.CallerPath.LastIndexOf(@"bin\managed")));
            PluginFactory.AddFile(ExePath.ConcatPath(@"plugins\Finder.dll"));
            PluginFactory.AddFile(ExePath.ConcatPath(@"plugins\Kommunikator.dll"));
            PluginFactory.AddFile(ExePath.ConcatPath(@"plugins\NodeBrowser.dll"));
            PluginFactory.AddFile(ExePath.ConcatPath(@"plugins\NodeCollector.dll"));
            PluginFactory.AddFile(ExePath.ConcatPath(@"plugins\WindowSwitcher.dll"));
            foreach (var factory in AddonFactories)
                if (factory is PatchFactory)
                    NodeCollection.Add(ExePath.ConcatPath(@"help\"), factory, true, false);
//			NodeCollection.Collect();
            
            NodeInfoFactory.NodeInfoAdded -= NodeInfoFactory_NodeInfoAdded;
            
            //now instantiate a NodeBrowser, a Kommunikator and a WindowSwitcher
            var nodeInfoFactory = FVVVVHost.NodeInfoFactory;
            UpdateNodeInfos(FWinSwNodeInfo.Filename);
            FWindowSwitcher = PluginFactory.CreatePlugin(FWinSwNodeInfo, null);
            UpdateNodeInfos(FKomNodeInfo.Filename);
            FKommunikator = PluginFactory.CreatePlugin(FKomNodeInfo, null);
            UpdateNodeInfos(FNodeBrowserNodeInfo.Filename);
            FNodeBrowser = (INodeBrowser) PluginFactory.CreatePlugin(FNodeBrowserNodeInfo, null);
            FNodeBrowser.IsStandalone = false;
            FNodeBrowser.DragDrop(false);
            
            FCacheTimer.Start();
        }

        void NodeInfoFactory_NodeInfoAdded(object sender, INodeInfo nodeInfo)
        {
            if (nodeInfo.Systemname == WINDOW_SWITCHER)
                FWinSwNodeInfo = nodeInfo;
            else if (nodeInfo.Systemname == KOMMUNIKATOR)
                FKomNodeInfo = nodeInfo;
            else if (nodeInfo.Systemname == NODE_BROWSER)
                FNodeBrowserNodeInfo = nodeInfo;
        }

        public void GetHDEPlugins(out IPluginBase nodeBrowser, out IPluginBase windowSwitcher, out IPluginBase kommunikator)
        {
            nodeBrowser = FNodeBrowser;
            windowSwitcher = FWindowSwitcher;
            kommunikator = FKommunikator;
        }
        
        public void ExtractNodeInfos(string filename, string arguments, out INodeInfo[] result)
        {
            HashSet<ProxyNodeInfo> nodeInfos = null;
            
            if(HasCachedNodeInfos(filename))
            {
                nodeInfos = GetCachedNodeInfos(filename);
            }
            else
            {
                //not in cache, so load from file
                nodeInfos = LoadNodeInfos(filename, arguments);
                FNodeInfoCache[filename] = nodeInfos;
            }
            
            //convert to internal and write into result
            result = new INodeInfo[nodeInfos.Count];
            var i = 0;
            foreach(var info in nodeInfos)
            {
                result[i++] = NodeInfoFactory.ToInternal(info);
            }
        }
        
        // Return false if the wrapper should be deleted by vvvv. Used for example
        // by the EditorFactory if an editor is already opened.
        public bool CreateNode(INode node)
        {
            var nodeInfo = NodeInfoFactory.ToProxy(node.GetNodeInfo());
            
            // We don't know if nodeInfo was cached. Some properties like UserData, Factory
            // might be not set -> Update the nodeInfo.
            // If this call throws an exception, vvvv will create a red node.
            UpdateNodeInfos(nodeInfo.Filename);
            
            try
            {
                return nodeInfo.Factory.Create(nodeInfo, node);
            }
            catch (Exception e)
            {
                Logger.Log(e);
                node.LastRuntimeError = e.Message;
                return true;
            }
        }
        
        public bool DestroyNode(INode node)
        {
            var nodeInfo = NodeInfoFactory.ToProxy(node.GetNodeInfo());
            
            try
            {
                var factory = nodeInfo.Factory;
                if (factory.Delete(nodeInfo, node))
                    return true;
            }
            catch (Exception e)
            {
                Logger.Log(e);
                throw e;
            }
            
            return false;
        }
        
        public INodeInfo Clone(INodeInfo nodeInfo, string path, string name, string category, string version)
        {
            if (!(nodeInfo is ProxyNodeInfo))
                nodeInfo = NodeInfoFactory.ToProxy(nodeInfo);
            
            try
            {
                // We don't know if nodeInfo was cached. Some properties like UserData, Factory
                // might be not set -> Update the nodeInfo.
                UpdateNodeInfos(nodeInfo.Filename);
                
                foreach (var factory in AddonFactories)
                {
                    INodeInfo newNodeInfo;
                    if (factory.Clone(nodeInfo, path, name, category, version, out newNodeInfo))
                    {
                        return NodeInfoFactory.ToInternal(newNodeInfo);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
            
            return null;
        }
        
        public void AddSearchPath(string path)
        {
            NodeCollection.AddCombined(path, false);
            NodeCollection.Collect();
        }

        #endregion IInternalHDEHost
        
        #region IHDEHost
        
        public event NodeSelectionEventHandler NodeSelectionChanged;
        
        protected virtual void OnNodeSelectionChanged(NodeSelectionEventArgs args)
        {
            try
            {
                if (NodeSelectionChanged != null) {
                    NodeSelectionChanged(this, args);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
                throw e;
            }
        }
        
        public event VVVV.PluginInterfaces.V2.MouseEventHandler MouseUp;
        
        protected virtual void OnMouseUp(VVVV.PluginInterfaces.V2.MouseEventArgs args)
        {
            try
            {
                if (MouseUp != null) {
                    MouseUp(this, args);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
                throw e;
            }
        }
        
        public event VVVV.PluginInterfaces.V2.MouseEventHandler MouseDown;
        
        protected virtual void OnMouseDown(VVVV.PluginInterfaces.V2.MouseEventArgs args)
        {
            try
            {
                if (MouseDown != null) {
                    MouseDown(this, args);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
                throw e;
            }
        }
        
        public event WindowEventHandler WindowSelectionChanged;
        
        protected virtual void OnWindowSelectionChanged(WindowEventArgs args)
        {
            try
            {
                if (WindowSelectionChanged != null) {
                    WindowSelectionChanged(this, args);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
                throw e;
            }
        }
        
        private event WindowEventHandler FWindowAdded;
        public event WindowEventHandler WindowAdded
        {
            add
            {
                FWindowAdded += value;
                foreach (var window in FWindows)
                    value.Invoke(this, new WindowEventArgs(window));
            }
            remove
            {
                FWindowAdded -= value;
            }
        }
        
        protected virtual void OnWindowAdded(WindowEventArgs args)
        {
            try
            {
                if (FWindowAdded != null) {
                    FWindowAdded(this, args);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
                throw e;
            }
        }
        
        public event WindowEventHandler WindowRemoved;
        
        protected virtual void OnWindowRemoved(WindowEventArgs args)
        {
            try
            {
                if (WindowRemoved != null) {
                    WindowRemoved(this, args);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
                throw e;
            }
        }
        
        public INode Root
        {
            get
            {
                return FVVVVHost.Root;
            }
        }
        
        private INode2 FRootNode;
        public INode2 RootNode
        {
            get
            {
                if (FRootNode == null)
                    FRootNode = Node.Create(null, FVVVVHost.Root, NodeInfoFactory);
                return FRootNode;
            }
        }
        
        public void UpdateEnum(string EnumName, string Default, string[] EnumEntries)
        {
            FVVVVHost.UpdateEnum(EnumName, Default, EnumEntries);
        }
        
        public int GetEnumEntryCount(string EnumName)
        {
            int entryCount;
            FVVVVHost.GetEnumEntryCount(EnumName, out entryCount);
            return entryCount;
        }
        
        public string GetEnumEntry(string EnumName, int Index)
        {
            string entryName;
            FVVVVHost.GetEnumEntry(EnumName, Index, out entryName);
            return entryName;
        }
        
        public double GetCurrentTime()
        {
            double currentTime;
            FVVVVHost.GetCurrentTime(out currentTime);
            return currentTime;
        }
        
        public void Open(string file, bool inActivePatch, IWindow window)
        {
            FVVVVHost.Open(file, inActivePatch, window);
        }
        
        public void SelectNodes(INode2[] nodes)
        {
            var query =
                from node in nodes
                select node.InternalCOMInterf;
            FVVVVHost.SelectNodes(query.ToArray());
        }
        
        public void ShowEditor(INode2 node)
        {
            // TODO: Kind of a hack
            switch (node.NodeInfo.Type)
            {
                case NodeType.Dynamic:
                case NodeType.Effect:
                    EditorFactory.OpenEditor(node.InternalCOMInterf);
                    break;
                default:
                    FVVVVHost.ShowEditor(node.InternalCOMInterf);
                    break;
            }
        }
        
        public void ShowGUI(INode2 node)
        {
            FVVVVHost.ShowGUI(node.InternalCOMInterf);
        }
        
        public void ShowHelpPatch(INodeInfo nodeInfo)
        {
            FVVVVHost.ShowHelpPatch(nodeInfo);
        }
        
        public void ShowNodeReference(INodeInfo nodeInfo)
        {
            FVVVVHost.ShowNodeReference(nodeInfo);
        }
        
        public void SetComponentMode(INode2 node, ComponentMode componentMode)
        {
            FVVVVHost.SetComponentMode(node.InternalCOMInterf, componentMode);
        }
        
        public string ExePath
        {
            get;
            private set;
        }
        
        private Window FActivePatchWindow;
        public IWindow2 ActivePatchWindow
        {
            get
            {
                var internalWindow = FVVVVHost.ActivePatchWindow;
                if (internalWindow != null)
                {
                    if (FActivePatchWindow == null)
                        FActivePatchWindow = FindWindow(internalWindow);
                    else if (FActivePatchWindow.InternalCOMInterf != FVVVVHost.ActivePatchWindow)
                        FActivePatchWindow = FindWindow(internalWindow);
                }
                else
                    FActivePatchWindow = null;
                
                return FActivePatchWindow;
            }
        }
        
        #endregion
        
        #region helper methods
        protected IEnumerable<INode2> GetAffectedNodes(INodeInfo nodeInfo)
        {
            return
                from node in RootNode.AsDepthFirstEnumerable()
                where nodeInfo == node.NodeInfo
                select node;
        }
        
        /// <summary>
        /// From http://beaucrawford.net/post/Using-Reflection-to-get-inherited-properties-for-an-interface.aspx
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Returns all the inherited properties of an interface type.</returns>
        protected static PropertyInfo[] GetAllProperties(Type type)
        {
            List<Type> typeList = new List<Type>();
            typeList.Add(type);
            
            if (type.IsInterface)
            {
                typeList.AddRange(type.GetInterfaces());
            }
            
            List<PropertyInfo> propertyList = new List<PropertyInfo>();
            
            foreach (Type interfaceType in typeList)
            {
                foreach (PropertyInfo property in interfaceType.GetProperties())
                {
                    propertyList.Add(property);
                }
            }
            
            return propertyList.ToArray();
        }
        
        protected INode2 FindNode(IWindow internalWindow)
        {
            var query =
                from node in RootNode.AsDepthFirstEnumerable()
                let window = node.Window as Window
                where window != null && window.InternalCOMInterf == internalWindow
                select node;
            return query.First();
        }
        
        protected INode2 FindNode(INode internalNode)
        {
            var query =
                from node in RootNode.AsDepthFirstEnumerable()
                where node.InternalCOMInterf == internalNode
                select node;
            return query.First();
        }
        
        protected Window FindWindow(IWindow internalWindow)
        {
            var query =
                from w in FWindows
                let window = w as Window
                where window.InternalCOMInterf == internalWindow
                select window;
            return query.First();
        }
        #endregion helper methods
        
        #region event handler
        protected void factory_NodeInfoAdded(object sender, INodeInfo info)
        {
            // do not cache node of type text and unknown.
            if (info.Type == NodeType.Text) return;
            if (info.Type == NodeType.Unknown) return;
            
            //add to cache
            var filename = info.Filename;
            if (!FNodeInfoCache.ContainsKey(filename))
                FNodeInfoCache[filename] = new HashSet<ProxyNodeInfo>();
            
            var nodeInfos = FNodeInfoCache[filename];
            var nodeInfo = (ProxyNodeInfo) info;
            
            if (!nodeInfos.Contains(nodeInfo))
                nodeInfos.Add(nodeInfo);
        }
        
        protected void factory_NodeInfoRemoved(object sender, INodeInfo info)
        {
            InvalidateCache(info.Filename);
        }
        
        public void factory_NodeInfoUpdated(object sender, INodeInfo info)
        {
            var factory = info.Factory;
            
            if (factory != null)
            {
                // More of a hack. Find cleaner solution: EditorFactory shouldn't update node infos
                // every time.
                if (factory is EditorFactory) return;
                
                // Go through all the running hosts using this changed node info
                // and create a new plugin for them.
                foreach (var node in GetAffectedNodes(info))
                {
                    try
                    {
                        factory.Create(info, node.InternalCOMInterf);
                        
                        //for effects need to update only one affected host
                        //others will be updated vvvv internally
                        if (factory is EffectsFactory)
                            break;
                    }
                    catch (Exception e)
                    {
                        node.LastRuntimeError = e.ToString();
                    }
                }
            }
            
            factory_NodeInfoAdded(sender, info);
        }
        
        protected Assembly ResolveAssemblyCB(object sender, ResolveEventArgs args)
        {
            AppDomain domain = AppDomain.CurrentDomain;
            // TODO: Clean this up a little.
            string fullName = args.Name.Trim();
            string partialName = fullName;
            if (fullName.IndexOf(',') >= 0)
                partialName = fullName.Substring(0, fullName.IndexOf(','));
            
            if (partialName == "_PluginInterfaces")
                partialName = "VVVV.PluginInterfaces";
            
            if (partialName == "PluginInterfaces")
                partialName = "VVVV.PluginInterfaces";
            
            if (partialName == "_Utils")
                partialName = "VVVV.Utils";
            
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                AssemblyName name = assembly.GetName();
                if (name.Name == partialName)
                    return assembly;
            }
            
            return null;
        }
        #endregion
        
        #region Caching
        
        //check if there is a valid node info in cache
        public bool HasCachedNodeInfos(string filename)
        {
            return FNodeInfoCache.ContainsKey(filename) || (FDeserializedNodeInfoCache.ContainsKey(filename) &&
                                                            (File.GetLastWriteTime(filename) < File.GetLastWriteTime(CacheFileName)));
        }
        
        //return node infos from cache
        public HashSet<ProxyNodeInfo> GetCachedNodeInfos(string filename)
        {
            if(!FNodeInfoCache.ContainsKey(filename))
            {
                var cache = new HashSet<ProxyNodeInfo>();
                
                if (FDeserializedNodeInfoCache.ContainsKey(filename))
                {
                    foreach (var cachedInfo in FDeserializedNodeInfoCache[filename])
                    {
                        var newInfo = NodeInfoFactory.CreateNodeInfo(cachedInfo.Name, cachedInfo.Category, cachedInfo.Version, filename, true) as ProxyNodeInfo;
                        if (!cache.Contains(newInfo))
                        {
                            newInfo.UpdateFromNodeInfo(cachedInfo);
                            
                            cache.Add(newInfo);
                        }
                        newInfo.CommitUpdate();
                    }
                }
                
                FNodeInfoCache[filename] = cache;
            }
            
            return FNodeInfoCache[filename];
        }
        
        public void MarkFileAsEmpty(string filename)
        {
            FNodeInfoCache[filename] = new HashSet<ProxyNodeInfo>();
        }
        
        public void InvalidateCache(string filename)
        {
            if (FNodeInfoCache.ContainsKey(filename))
                FNodeInfoCache.Remove(filename);
            
            if (FDeserializedNodeInfoCache.ContainsKey(filename))
                FDeserializedNodeInfoCache.Remove(filename);
            
            FLoadedFiles[filename] = false;
            
            Logger.Log(LogType.Debug, "Invalidated cache for {0}.", filename);
        }
        
        //path to cache file
        protected string CacheFileName
        {
            get;
            private set;
        }
        
        //load cache from disk
        protected void DeserializeNodeInfoCache()
        {
            FNodeInfoCache = new Dictionary<string, HashSet<ProxyNodeInfo>>();

            try
            {
                var formatter = new BinaryFormatter();
                using (var stream = new FileStream(CacheFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    FDeserializedNodeInfoCache = (Dictionary<string, HashSet<ProxyNodeInfo>>) formatter.Deserialize(stream);
                }
                
            }
            catch
            {
                FDeserializedNodeInfoCache = new Dictionary<string, HashSet<ProxyNodeInfo>>();
            }
        }
        
        //save cache to temp dir
        private System.Windows.Forms.Timer FCacheTimer;
        protected void SerializeNodeInfoCache()
        {
            try
            {
                // Write nodeInfoList to cache file.
                var filename = CacheFileName;
                
                if (!Directory.Exists(Path.GetDirectoryName(filename)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));
                }
                
                var formatter = new BinaryFormatter();
                using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    formatter.Serialize(stream, FNodeInfoCache);
                }
                
                Logger.Log(LogType.Debug, "Saved node info cache to Temp: " + filename);
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Serialization of node infos failed:");
                Logger.Log(e);
            }
        }
        
        public void Dispose()
        {
            //write dict to temp
        }
        
        #endregion Caching
        
        #region Listeners
        
        public void MouseDownCB(INode internalNode, Mouse_Buttons button, Modifier_Keys keys)
        {
            if (internalNode != null)
                OnMouseDown(new VVVV.PluginInterfaces.V2.MouseEventArgs(FindNode(internalNode), button, keys));
            else
                OnMouseDown(new VVVV.PluginInterfaces.V2.MouseEventArgs(null, button, keys));
        }
        
        public void MouseUpCB(INode internalNode, Mouse_Buttons button, Modifier_Keys keys)
        {
            if (internalNode != null)
                OnMouseUp(new VVVV.PluginInterfaces.V2.MouseEventArgs(FindNode(internalNode), button, keys));
            else
                OnMouseUp(new VVVV.PluginInterfaces.V2.MouseEventArgs(null, button, keys));
        }
        
        public void NodeSelectionChangedCB(INode[] internalNodes)
        {
            if (internalNodes != null)
            {
                INode2[] nodes = new INode2[internalNodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                    nodes[i] = FindNode(internalNodes[i]);
                OnNodeSelectionChanged(new NodeSelectionEventArgs(nodes));
            }
            else
                OnNodeSelectionChanged(new NodeSelectionEventArgs(new INode2[0]));
        }
        
        private List<IWindow2> FWindows = new List<IWindow2>();
        public void WindowAddedCB(IWindow internalWindow)
        {
            var window = Window.Create(internalWindow);
            FWindows.Add(window);
            OnWindowAdded(new WindowEventArgs(window));
        }
        
        public void WindowRemovedCB(IWindow internalWindow)
        {
            var window = FindWindow(internalWindow);
            FWindows.Remove(window);
            OnWindowRemoved(new WindowEventArgs(window));
            window.Dispose();
        }
        
        public void WindowSelectionChangeCB(IWindow internalWindow)
        {
            OnWindowSelectionChanged(new WindowEventArgs(FindWindow(internalWindow)));
        }
        
        #endregion
    }
}