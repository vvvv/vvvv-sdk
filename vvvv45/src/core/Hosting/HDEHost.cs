using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using VVVV.Core;
using VVVV.Core.Commands;
using VVVV.Core.Logging;
using VVVV.Core.Model;
using VVVV.Hosting;
using VVVV.Hosting.Factories;
using VVVV.Hosting.Graph;
using VVVV.Hosting.IO;
using VVVV.Hosting.Interfaces.EX9;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.InteropServices.EX9;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils.Linq;
using VVVV.Utils.Network;
using NuGetAssemblyLoader;
using Nito.Async;
using VVVV.Utils.Reflection;
using System.Collections;
using System.Text;

namespace VVVV.Hosting
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IHDEHost))]
    [ComVisible(false)]
    class HDEHost : IInternalHDEHost, IHDEHost,
    IMouseClickListener, INodeSelectionListener, IWindowListener, IComponentModeListener, IWindowSelectionListener, IEnumListener
    {
        #region SynchronizationContext hack
        // See issue described here: http://stackoverflow.com/questions/32439669/hosting-net-and-winforms-synchronizationcontexts-is-reset-when-showdialog-of
        class MySynchronizationContext : SynchronizationContext
        {
            private readonly SynchronizationContext context = new WindowsFormsSynchronizationContext();

            static MySynchronizationContext()
            {
                // This tells the GenericSynchronizingObject class implementing ISynchronizeInvoke used by our file watchers that an invoke is indeed required
                SynchronizationContextRegister.Register(typeof(MySynchronizationContext), SynchronizationContextProperties.Standard);
            }

            public override SynchronizationContext CreateCopy()
            {
                return context.CreateCopy();
            }

            public override bool Equals(object obj)
            {
                return context.Equals(obj);
            }

            public override int GetHashCode()
            {
                return context.GetHashCode();
            }

            public override void OperationCompleted()
            {
                context.OperationCompleted();
            }

            public override void OperationStarted()
            {
                context.OperationStarted();
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                context.Post(d, state);
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                context.Send(d, state);
            }

            public override string ToString()
            {
                return "Wrapped";
            }

            public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
            {
                return context.Wait(waitHandles, waitAll, millisecondsTimeout);
            }
        }
        #endregion

        public const string ENV_VVVV = "VVVV45";
        
        const string WINDOW_SWITCHER = "WindowSwitcher (VVVV)";
        const string KOMMUNIKATOR = "Kommunikator (VVVV)";
        const string NODE_BROWSER = "NodeBrowser (VVVV)";
        
        private IVVVVHost FVVVVHost;
        private IPluginBase FWindowSwitcher, FKommunikator, FNodeBrowser;
        private readonly List<Window> FWindows = new List<Window>();
        private INetworkTimeSync FNetTimeSync;
        private INode2[] FSelectedNodes;
        
        [Export]
        public CompositionContainer Container { get; protected set; }
        
        [Export(typeof(ILogger))]
        public DefaultLogger Logger { get; private set; }

        [Export(typeof(IORegistry))]
        [Export(typeof(IIORegistry))]
        public IORegistry IORegistry { get; private set; }
        
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

#pragma warning disable 0649
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
        public NodeCollection NodeCollection { get; protected set; }

        [Import]
        private IStartableRegistry FStartableRegistry; 
#pragma warning restore

        private List<string> FAssemblySearchPaths = new List<string>();
        
        public HDEHost()
        {
            // Set vvvv.exe path
            ExePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName((typeof(HDEHost).Assembly.Location)), @"..\.."));

            //add repository paths from commandline
            //from commandline
            var repoArg = "/package-repositories";
            var packageRepositories = ExtractPaths(repoArg);
            AssemblyLoader.AddPackageRepositories(packageRepositories);

            //the built-in one
            if (Directory.Exists(PacksPath))
                AssemblyLoader.AddPackageRepository(PacksPath);
            //the one where the user is supposed to install packages
            if (Directory.Exists(UserPacksPatch))
                AssemblyLoader.AddPackageRepository(UserPacksPatch);

            // Set name to vvvv thread for easier debugging.
            Thread.CurrentThread.Name = "vvvv";

            // Create a windows forms sync context (FileSystemWatcher runs asynchronously).
            SynchronizationContext.SetSynchronizationContext(new MySynchronizationContext());

            // Register at least one ICommandHistory for top level element ISolution
            var mappingRegistry = new MappingRegistry();
            mappingRegistry.RegisterMapping<ISolution, ICommandHistory, CommandHistory>(MapInstantiation.PerInstanceAndItsChilds);
            
            var location = Shell.CallerPath.ConcatPath(@"..\..\lib\nodes\plugins").ConcatPath("Solution.sln");
            Solution = new Solution(location, mappingRegistry);
            
            EnumManager.SetHDEHost(this);
            
            Logger = new DefaultLogger();

            IORegistry = new IORegistry();

            // Will tell Windows Forms that a message loop is indeed running
            Application.RegisterMessageLoop(IsSendingMessages);
        }

        private string[] ExtractPaths(string key)
        {
            //from commandline
            var paths = AssemblyLoader.ParseCommandLine(key);

            //from args.txt
            var argsFile = Path.Combine(ExePath, "args.txt");
            if (File.Exists(argsFile))
            {
                var args = File.ReadAllText(argsFile).Split(new Char[] { ' ', '\r' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                paths = paths.Concat(AssemblyLoader.ParseLines(args, key)).ToArray();
            }

            //make relative paths absolute
            for (int i = 0; i < paths.Length; i++)
            {
                if (Path.IsPathRooted(paths[i]))
                    paths[i] = Path.Combine(ExePath, paths[i]);
            }

            return paths;
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
            // Used for Windows Forms message loop
            FIsRunning = true;

            //set blackbox mode?
            this.IsBlackBoxMode = vvvvHost.IsBlackBoxMode;

            // Set VVVV45 to this running vvvv.exe
            Environment.SetEnvironmentVariable(ENV_VVVV, Path.GetFullPath(Shell.CallerPath.ConcatPath("..").ConcatPath("..")));

            FVVVVHost = vvvvHost;
            NodeInfoFactory = new ProxyNodeInfoFactory(vvvvHost.NodeInfoFactory);

            FVVVVHost.AddMouseClickListener(this);
            FVVVVHost.AddNodeSelectionListener(this);
            FVVVVHost.AddWindowListener(this);
            FVVVVHost.AddWindowSelectionListener(this);
            FVVVVHost.AddComponentModeListener(this);
            FVVVVHost.AddEnumListener(this);

            NodeInfoFactory.NodeInfoUpdated += factory_NodeInfoUpdated;

            // Route log messages to vvvv
            Logger.AddLogger(new VVVVLogger(FVVVVHost));

            DeviceService = new DeviceService(vvvvHost.DeviceService);
            MainLoop = new MainLoop(vvvvHost.MainLoop);
            MainLoop.OnInitFrame += MainLoop_OnInitFrame;

            ExposedNodeService = new ExposedNodeService(vvvvHost.ExposedNodeService, NodeInfoFactory);

            NodeBrowserHost = new ProxyNodeBrowserHost(nodeBrowserHost, NodeInfoFactory);
            WindowSwitcherHost = windowSwitcherHost;
            KommunikatorHost = kommunikatorHost;

            //do not add the entire directory for faster startup
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(HDEHost).Assembly.Location));
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(NodeCollection).Assembly.Location));
            //allow plugin writers to add their own factories (deprecated, see below)
            var factoriesPath = ExePath.ConcatPath(@"lib\factories");
            if (Directory.Exists(factoriesPath))
                catalog.Catalogs.Add(new DirectoryCatalog(factoriesPath));

            //register custom assembly resolvers which look also in the PACK_NAME/core and PACK_NAME/core/[x86|x64] folders
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;

            //search for packs, add factories dir to this catalog, add core dir to assembly search path,
            //add nodes to nodes search path
            var packsPath = Path.Combine(ExePath, "packs");
            if (Directory.Exists(packsPath))
                LoadFactoriesFromLegacyPackages(packsPath, catalog);
            //new package loading system
            LoadFactoriesFromPackages(catalog);

            Container = new CompositionContainer(catalog);
            Container.ComposeParts(this);

            PluginFactory.AddFile(ExePath.ConcatPath(@"lib\nodes\plugins\VVVV.Nodes.dll"));

            //Get node infos from core plugins here to avoid looping all node infos
            var windowSwitcherNodeInfo = GetNodeInfo(WINDOW_SWITCHER);
            var kommunikatorNodeInfo = GetNodeInfo(KOMMUNIKATOR);
            var nodeBrowserNodeInfo = GetNodeInfo(NODE_BROWSER);

            foreach (var factory in AddonFactories)
                if (factory is PatchFactory)
                    NodeCollection.Add(string.Empty, ExePath.ConcatPath(@"lib\nodes\native\"), factory, true, false);

            //now instantiate a NodeBrowser, a Kommunikator and a WindowSwitcher
            FWindowSwitcher = PluginFactory.CreatePlugin(windowSwitcherNodeInfo, null);
            FKommunikator = PluginFactory.CreatePlugin(kommunikatorNodeInfo, null);
            FNodeBrowser = PluginFactory.CreatePlugin(nodeBrowserNodeInfo, null);

            this.IsBoygroupClient = FVVVVHost.IsBoygroupClient;
            if (IsBoygroupClient)
            {
                this.BoygroupServerIP = FVVVVHost.BoygroupServerIP;
            }

            var clockport = 3334;

            try
            {
                if (Environment.CommandLine.Contains("/clockport"))
                {
                    var env = Environment.GetCommandLineArgs();
                    var idx = Array.IndexOf(env, "/clockport") + 1;
                    clockport = int.Parse(env[idx]);
                }
            }
            catch (Exception)
            {
                throw new Exception("Could not parse clockport, make sure you have the right syntax, e.g. '/clockport 3344' ");
            }

            //start time server of client
            FNetTimeSync = IsBoygroupClient ? new UDPTimeClient(BoygroupServerIP, clockport) : new UDPTimeServer(clockport);
            FNetTimeSync.Start();

            //now that all basics are set up, see if there are any node search paths to add
            //from the installed packs
            if (Directory.Exists(packsPath))
                LoadNodesFromLegacyPackages(packsPath);
            LoadNodesFromPackages();
        }

        private void MainLoop_OnInitFrame(object sender, EventArgs e)
        {
            Application.RaiseIdle(EventArgs.Empty);
        }

        bool IsSendingMessages()
        {
            return FIsRunning;
        }
        bool FIsRunning;

        public unsafe bool OnThreadMessage(IntPtr msgPtr)
        {
            // In case someone installs a filter on the Windows Forms application this way it gets properly notified about it
            var winformsMsg = *(Message*)msgPtr.ToPointer();
            if (Application.FilterMessage(ref winformsMsg))
                return true;
            return false;
        }

        private void LoadFactoriesFromPackages(AggregateCatalog catalog)
        {
            foreach (var package in AssemblyLoader.Repository.GetPackages())
            {
                var packagePath = package.GetPathOfPackage();
                //check for new nuget package format (skip packages without a version.info file - see http://vvvv.org/blog/patch-conversions-pack-versioning)
                var versionInfoPath = Path.Combine(packagePath, "version.info");
                if (File.Exists(versionInfoPath))
                {
                    foreach (var assembly in package.GetCompatibleAssemblyFiles())
                        catalog.Catalogs.Add(new AssemblyCatalog(assembly.SourcePath));
                }
            }
        }

        private void LoadNodesFromPackages()
        {
            foreach (var package in AssemblyLoader.Repository.GetPackages())
            {
                var packagePath = package.GetPathOfPackage();
                //check if the package contains a legacy package
                var vvvvPath = Path.Combine(packagePath, "vvvv");
                if (Directory.Exists(vvvvPath))
                    LoadNodesFromLegacyPackage(vvvvPath);
            }
        }

        private void LoadNodesFromLegacyPackages(string packsPath)
        {
            foreach (var packPath in Directory.EnumerateDirectories(packsPath))
                LoadNodesFromLegacyPackage(packPath);
        }

        private void LoadNodesFromLegacyPackage(string packagePath)
        {
            var nodesPath = Path.Combine(packagePath, "nodes");
            if (Directory.Exists(nodesPath))
                NodeCollection.AddJob(nodesPath, true);
        }

        private void LoadFactoriesFromLegacyPackages(string packsPath, AggregateCatalog catalog)
        {
            foreach (var packPath in Directory.EnumerateDirectories(packsPath))
            {
                var corePath = Path.Combine(packPath, "core");
                if (Directory.Exists(corePath))
                {
                    FAssemblySearchPaths.Add(corePath);
                    Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + corePath);
                    var platformDir = IntPtr.Size == 4 ? "x86" : "x64";
                    var platformPath = Path.Combine(corePath, platformDir);
                    if (Directory.Exists(platformPath))
                    {
                        FAssemblySearchPaths.Add(platformPath);
                        Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + platformPath);
                    }
                }
                var factoriesPath = Path.Combine(packPath, "factories");
                if (Directory.Exists(factoriesPath))
                    catalog.Catalogs.Add(new DirectoryCatalog(factoriesPath));

                // We look for nodes later
            }
        }

        Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            foreach (var searchPath in FAssemblySearchPaths)
            {
                foreach (var assemblyLocation in GetAssemblyLocations(searchPath, assemblyName))
                    if (File.Exists(assemblyLocation))
                        return Assembly.ReflectionOnlyLoadFrom(assemblyLocation);
            }
            return null;
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            foreach (var searchPath in FAssemblySearchPaths)
            {
                foreach (var assemblyLocation in GetAssemblyLocations(searchPath, assemblyName))
                    if (File.Exists(assemblyLocation))
                        return Assembly.LoadFrom(assemblyLocation);
            }
            return null;
        }

        IEnumerable<string> GetAssemblyLocations(string searchPath, AssemblyName assemblyName)
        {
            yield return Path.Combine(searchPath, assemblyName.Name + ".dll");
            yield return Path.Combine(searchPath, assemblyName.Name + ".exe");
        }
        
        private INodeInfo GetNodeInfo(string systemName)
        {
            return NodeInfoFactory.NodeInfos.Where(ni => ni.Systemname == systemName).First();
        }

        public void GetHDEPlugins(out IPluginBase nodeBrowser, out IPluginBase windowSwitcher, out IPluginBase kommunikator)
        {
            // HACK hack hack :/
            nodeBrowser = ((PluginContainer) FNodeBrowser).PluginBase;
            ((INodeBrowser) nodeBrowser).IsStandalone = false;
            ((INodeBrowser) nodeBrowser).DragDrop(false);
            windowSwitcher = ((PluginContainer) FWindowSwitcher).PluginBase;
            kommunikator = ((PluginContainer) FKommunikator).PluginBase;
        }
        
        public void ExtractNodeInfos(string filename, string arguments, out INodeInfo[] result)
        {
            HashSet<ProxyNodeInfo> nodeInfos = null;
            
            nodeInfos = LoadNodeInfos(filename, arguments);
            
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
            return nodeInfo.Factory.Create(nodeInfo, node);
        }
        
        public bool DestroyNode(INode node)
        {
            var nodeInfo = NodeInfoFactory.ToProxy(node.GetNodeInfo());
            
            var factory = nodeInfo.Factory;
            if (factory.Delete(nodeInfo, node))
                return true;
            
            return false;
        }
        
        public INodeInfo Clone(INodeInfo nodeInfo, string path, string name, string category, string version)
        {
            if (!(nodeInfo is ProxyNodeInfo))
                nodeInfo = NodeInfoFactory.ToProxy(nodeInfo);
            
            try
            {
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
        
        public void Shutdown()
        {
            FStartableRegistry.ShutDown();
            Container.Dispose();
            FIsRunning = false;
        }
        
        public void RunRefactor()
        {
            //run Refactorer
            var refactorer = new PatchRefactorer(this, FSelectedNodes, NodeBrowserHost, NodeInfoFactory);
        }

        #endregion IInternalHDEHost
        
        #region IHDEHost
        
        public event NodeSelectionEventHandler NodeSelectionChanged;
        
        protected virtual void OnNodeSelectionChanged(NodeSelectionEventArgs args)
        {
            if (NodeSelectionChanged != null) {
                NodeSelectionChanged(this, args);
            }
        }
        
        public event VVVV.PluginInterfaces.V2.MouseEventHandler MouseUp;
        
        protected virtual void OnMouseUp(VVVV.PluginInterfaces.V2.MouseEventArgs args)
        {
            if (MouseUp != null) {
                MouseUp(this, args);
            }
        }
        
        public event VVVV.PluginInterfaces.V2.MouseEventHandler MouseDown;
        
        protected virtual void OnMouseDown(VVVV.PluginInterfaces.V2.MouseEventArgs args)
        {
            if (MouseDown != null) {
                MouseDown(this, args);
            }
        }
        
        public event WindowEventHandler WindowSelectionChanged;
        protected virtual void OnWindowSelectionChanged(WindowEventArgs args)
        {
            if (WindowSelectionChanged != null) {
                WindowSelectionChanged(this, args);
            }
        }
        
        public event ComponentModeEventHandler BeforeComponentModeChange;
        protected virtual void OnBeforeComponentModeChange(ComponentModeEventArgs args)
        {
            if (BeforeComponentModeChange != null) {
                BeforeComponentModeChange(this, args);
            }
        }
        
        public event ComponentModeEventHandler AfterComponentModeChange;
        protected virtual void OnAfterComponentModeChange(ComponentModeEventArgs args)
        {
            if (AfterComponentModeChange != null) {
                AfterComponentModeChange(this, args);
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
            if (FWindowAdded != null) {
                FWindowAdded(this, args);
            }
        }
        
        public event WindowEventHandler WindowRemoved;
        protected virtual void OnWindowRemoved(WindowEventArgs args)
        {
            if (WindowRemoved != null) {
                WindowRemoved(this, args);
            }
        }

        public event EnumEventHandler EnumChanged;
        protected virtual void OnEnumChanged(EnumEventArgs args)
        {
            if (EnumChanged != null)
            {
                EnumChanged(this, args);
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
                    FRootNode = Node.Create(FVVVVHost.Root, NodeInfoFactory);
                return FRootNode;
            }
        }
        
        public INode2 GetNodeFromPath(string nodePath)
        {
            var ids = nodePath.Split('/');
            
            var result = RootNode[0];
            for (int i = 1; i < ids.Length; i++)
            {
                try
                {
                    var id = int.Parse(ids[i]);
                    result = (from node in result where node.ID == id select node).First();
                }
                catch
                {
                    result = null;
                    break;
                }       			
            }

            return result;
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

        public double GetCurrentTime() => FrameTime;
        
        public double FrameTime
        {
            get
            {
                double currentTime;
                FVVVVHost.GetCurrentTime(out currentTime);
                return currentTime;
            }
        }

        public double RealTime
        {
            get
            {
                return FNetTimeSync.ElapsedSeconds;
            }
        }
        
        public void SetRealTime(double time = 0)
        {
            FNetTimeSync.SetTime(time);
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
        
        public string GetXMLSnippetFromSelection()
        {
            return FVVVVHost.GetXMLSnippetFromSelection();
        }
        
        public void SendXMLSnippet(string fileName, string message, bool undoable)
        {
            FVVVVHost.SendXMLSnippet(fileName, message, undoable);
        }
        
        public string ExePath
        {
            get;
            private set;
        }

        public string PacksPath => Path.Combine(ExePath, "lib", "packs");
        public string UserPacksPatch => Path.Combine(ExePath, "packs");
        
        private Window FActivePatchWindow;
        public IWindow2 ActivePatchWindow
        {
            get
            {
                var internalWindow = FVVVVHost.ActivePatchWindow;
                if (internalWindow != null)
                    FActivePatchWindow = Window.Create(internalWindow);
                else
                    FActivePatchWindow = null;
                
                return FActivePatchWindow;
            }
        }
        
        public IExposedNodeService ExposedNodeService
        {
            get;
            private set;
        }
        
        [Export(typeof(IDXDeviceService))]
        public IDXDeviceService DeviceService
        {
            get;
            private set;
        }

        [Export(typeof(IMainLoop))]
        public IMainLoop MainLoop
        {
            get;
            private set;
        }
        
        public bool IsBoygroupClient 
        {
            get; 
            private set;
        }
        
        public string BoygroupServerIP 
        {
            get;
            private set;
        }
        
        public bool IsBlackBoxMode
        {
        	get;
        	private set;
        }
        
        #endregion 
        
        protected IEnumerable<INode2> GetAffectedNodes(INodeInfo nodeInfo)
        {
            return
                from node in RootNode.AsDepthFirstEnumerable()
                where nodeInfo == node.NodeInfo
                select node;
        }
        
        public void factory_NodeInfoUpdated(object sender, INodeInfo info)
        {
            var factory = info.Factory;
            
            if (factory != null)
            {
                // More of a hack. Find cleaner solution: EditorFactory shouldn't update node infos
                // every time.
                if (factory is EditorFactory) return;
                
                if (!(info.Type == NodeType.Dynamic || info.Type == NodeType.Effect || info.Type == NodeType.VL)) return;
                
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
        }

        public bool IsRunningInBackground
        {
            get { return FVVVVHost.IsInBackground; }
        }
        
        #region Listeners
        
        public void MouseDownCB(INode internalNode, Mouse_Buttons button, Modifier_Keys keys)
        {
            if (internalNode != null)
                OnMouseDown(new VVVV.PluginInterfaces.V2.MouseEventArgs(Node.Create(internalNode, NodeInfoFactory), button, keys));
            else
                OnMouseDown(new VVVV.PluginInterfaces.V2.MouseEventArgs(null, button, keys));
        }
        
        public void MouseUpCB(INode internalNode, Mouse_Buttons button, Modifier_Keys keys)
        {
            if (internalNode != null)
                OnMouseUp(new VVVV.PluginInterfaces.V2.MouseEventArgs(Node.Create(internalNode, NodeInfoFactory), button, keys));
            else
                OnMouseUp(new VVVV.PluginInterfaces.V2.MouseEventArgs(null, button, keys));
        }
        
        public void NodeSelectionChangedCB(INode[] internalNodes)
        {
            if (internalNodes != null)
            {
                FSelectedNodes = new INode2[internalNodes.Length];
                for (int i = 0; i < FSelectedNodes.Length; i++)
                    FSelectedNodes[i] = Node.Create(internalNodes[i], NodeInfoFactory);
                OnNodeSelectionChanged(new NodeSelectionEventArgs(FSelectedNodes));
            }
            else
                OnNodeSelectionChanged(new NodeSelectionEventArgs(new INode2[0]));
        }
        
        public void WindowAddedCB(IWindow internalWindow)
        {
            var window = Window.Create(internalWindow);
            FWindows.Add(window);
            OnWindowAdded(new WindowEventArgs(window));
        }
        
        public void WindowRemovedCB(IWindow internalWindow)
        {
            var window = Window.Create(internalWindow);
            FWindows.Remove(window);
            OnWindowRemoved(new WindowEventArgs(window));
        }
        
        public void WindowSelectionChangeCB(IWindow internalWindow)
        {
            OnWindowSelectionChanged(new WindowEventArgs(Window.Create(internalWindow)));
        }
        
        public void BeforeComponentModeChangedCB(IWindow internalWindow, ComponentMode componentMode)
        {
            if (internalWindow == null)
                OnBeforeComponentModeChange(new ComponentModeEventArgs(null, componentMode));
            else
                OnBeforeComponentModeChange(new ComponentModeEventArgs(Window.Create(internalWindow), componentMode));
        }
        
        public void AfterComponentModeChangedCB(IWindow internalWindow, ComponentMode componentMode)
        {
            if (internalWindow == null)
                OnAfterComponentModeChange(new ComponentModeEventArgs(null, componentMode));
            else
                OnAfterComponentModeChange(new ComponentModeEventArgs(Window.Create(internalWindow), componentMode));
        }
        
        #endregion

        public void DisableShortCuts()
        {
            FVVVVHost.DisableShortCuts();
        }

        public void EnableShortCuts()
        {
            FVVVVHost.EnableShortCuts();
        }
        
        public IQueryDelete FiftyEditor
        {
            set {FVVVVHost.FiftyEditor = value;}
        }

        class DummyTimeProvider : ITimeProvider
        {
            private Func<double, double> timeProvider;

            public DummyTimeProvider(Func<double, double> timeProvider)
            {
                this.timeProvider = timeProvider;
            }

            public double GetTime(double originalNewFrameTime) => timeProvider(originalNewFrameTime);
        }

        public void SetFrameTimeProvider(Func<double, double> timeProvider) => FVVVVHost.SetTimeProvider(new DummyTimeProvider(timeProvider));
        public void SetFrameTimeProvider(ITimeProvider timeProvider) => FVVVVHost.SetTimeProvider(timeProvider);

        public void EnumChangeCB(string enumName)
        {
            OnEnumChanged(new EnumEventArgs(enumName));
        }

        public string GetInfoString([MarshalAs(UnmanagedType.IUnknown)] object obj)
        {
            return string.Format(" [{0}]{1}", GetDataType(obj).GetCSharpName(), GetValueRendering(obj));
        }

        Type GetDataType(object obj) => GetDataType(GetType(obj));

        static Type GetDataType(Type ioType)
        {
            // this should probably be spelled out specifically.
            // it's about getting rid of ISpread<> and alikes not about any generic type.
            if (ioType.IsGenericType)
                return ioType.GetGenericArguments()[0];
            return GetDataType(ioType.BaseType);
        }

        static Type GetType(object obj)
        {
            var wrapper = obj as DynamicTypeWrapper;
            if (wrapper != null)
                return wrapper.Value.GetType();
            return obj.GetType();
        }

        static string GetValueRendering(object obj)
        {
            var enumerable = obj as IEnumerable;
            if (enumerable == null)
            {
                var wrapper = obj as DynamicTypeWrapper;
                if (wrapper != null)
                    enumerable = wrapper.Value as IEnumerable;
            }
            if (enumerable != null)
            {
                var enumerator = enumerable.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    var slice = enumerator.Current;
                    if (slice == null)
                        return " : null";
                    var s = slice.ToString();
                    var defaultRendering = slice.GetType()?.ToString();
                    if (s == defaultRendering)
                        return "";
                    return $" : {s}";
                }
                else
                    return " : Ø";
            }
            return "";
        }

        public void ShowVLEditor()
        {
            FVVVVHost.ShowVLEditor();
        }

        public double OriginalFrameTime => FVVVVHost.GetOriginalFrameTime();

        public Version Version
        {
            get
            {
                if (FVersion == null)
                {
                    var p = Path.Combine(ExePath, "vvvv.exe");
                    if (File.Exists(p))
                    {
                        var versionInfo = FileVersionInfo.GetVersionInfo(p);
                        if (versionInfo != null)
                            FVersion = new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart);
                    }
                    if (FVersion == null)
                        FVersion = new Version();
                }
                return FVersion;
            }
        }
        Version FVersion;
    }
}