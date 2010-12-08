using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using System.Linq;

using Microsoft.Practices.Unity;
using VVVV.Core;
using VVVV.Core.Commands;
using VVVV.Core.Logging;
using VVVV.Core.Model;
using VVVV.Hosting;
using VVVV.Hosting.Factories;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting
{
	[PartCreationPolicy(CreationPolicy.Shared)]
	[Export(typeof(IHDEHost))]
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
		private List<INode> FNodes;
		private IPluginBase FNodeBrowser, FWindowSwitcher, FKommunikator;
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
		public NodeCollection NodeCollection {get; protected set;}
		
		public HDEHost()
		{
			AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblyCB;
			
			//set vvvv.exe path
			ExePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName((typeof(HDEHost).Assembly.Location)), @"..\.."));
			
			//set cache file name
			var filepath = Path.Combine(Path.GetTempPath(), "vvvv_cache");
			
			uint hash;
			unchecked
			{
				hash = (uint)ExePath.GetHashCode();
			}
			
			CacheFileName = Path.Combine(filepath, "node_info_" + hash + ".cache");
			
			//setup cache save timer
			FCacheTimer = new System.Windows.Forms.Timer();
			FCacheTimer.Interval = 3000;
			FCacheTimer.Tick += new EventHandler(FCacheTimer_Tick);
			
			// Set name to vvvv thread for easier debugging.
			Thread.CurrentThread.Name = "vvvv";
			
			// Create a windows forms sync context (FileSystemWatcher runs asynchronously).
			var context = SynchronizationContext.Current;
			if (context == null)
				SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
			
//			FRunningPluginHostsMap = new Dictionary<INodeInfo, List<IAddonHost>>();
			FNodes = new List<INode>();
			
			// Register at least one ICommandHistory for top level element ISolution
			var mappingRegistry = new MappingRegistry();
			mappingRegistry.RegisterMapping<ISolution, ICommandHistory, CommandHistory>(MapInstantiation.PerInstanceAndItsChilds);
			
			var location = new Uri(Shell.CallerPath.ConcatPath(@"..\..\plugins").ConcatPath("Solution.sln"));
			Solution = new Solution(location, mappingRegistry);
			
			EnumManager.SetHDEHost(this);
			
			Logger = new DefaultLogger();
			
		}

		//serialize cache when timer has ended
		void FCacheTimer_Tick(object sender, EventArgs e)
		{
			FCacheTimer.Stop();
			SerializeNodeInfoCache();
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
			FVVVVHost.AddMouseClickListener(this);
			FVVVVHost.AddNodeSelectionListener(this);
			FVVVVHost.AddWindowListener(this);
			FVVVVHost.AddWindowSelectionListener(this);
			
			NodeInfoFactory = new ProxyNodeInfoFactory(vvvvHost.NodeInfoFactory);
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
			
			try
			{
				//do not add the entire directory for faster startup
				var catalog = new AggregateCatalog();
				catalog.Catalogs.Add(new AssemblyCatalog(typeof(HDEHost).Assembly.Location));
				catalog.Catalogs.Add(new AssemblyCatalog(typeof(NodeCollection).Assembly.Location));
				Container = new CompositionContainer(catalog);
				Container.ComposeParts(this);
			}
			catch (ReflectionTypeLoadException e)
			{
				foreach (var f in e.LoaderExceptions)
					Logger.Log(f);
				return;
			}
			catch (Exception e)
			{
				Logger.Log(e);
				return;
			}
			
			//NodeCollection.AddJob(Shell.CallerPath.Remove(Shell.CallerPath.LastIndexOf(@"bin\managed")));
			PluginFactory.AddFile(ExePath.ConcatPath(@"plugins\Finder.dll"));
			PluginFactory.AddFile(ExePath.ConcatPath(@"plugins\Kommunikator.dll"));
			PluginFactory.AddFile(ExePath.ConcatPath(@"plugins\NodeBrowser.dll"));
			PluginFactory.AddFile(ExePath.ConcatPath(@"plugins\NodeCollector.dll"));
			PluginFactory.AddFile(ExePath.ConcatPath(@"plugins\WindowSwitcher.dll"));
//			NodeCollection.AddUnsorted(Shell.CallerPath.Remove(Shell.CallerPath.LastIndexOf(@"bin\managed")));
//			NodeCollection.Collect();
			
			foreach (var nodeInfo in NodeInfoFactory.NodeInfos)
			{
				if (nodeInfo.Systemname == WINDOW_SWITCHER)
					FWinSwNodeInfo = nodeInfo;
				else if (nodeInfo.Systemname == KOMMUNIKATOR)
					FKomNodeInfo = nodeInfo;
				else if (nodeInfo.Systemname == NODE_BROWSER)
					FNodeBrowserNodeInfo = nodeInfo;
			}
			
			//now instantiate a NodeBrowser, a Kommunikator and a WindowSwitcher
			var nodeInfoFactory = FVVVVHost.NodeInfoFactory;
			try
			{
				UpdateNodeInfos(FWinSwNodeInfo.Filename);
				FWindowSwitcher = PluginFactory.CreatePlugin(FWinSwNodeInfo, null);
				UpdateNodeInfos(FKomNodeInfo.Filename);
				FKommunikator = PluginFactory.CreatePlugin(FKomNodeInfo, null);
				UpdateNodeInfos(FNodeBrowserNodeInfo.Filename);
				FNodeBrowser = PluginFactory.CreatePlugin(FNodeBrowserNodeInfo, null);
				(FNodeBrowser as INodeBrowser).DragDrop(false);
			}
			catch (Exception e)
			{
				Logger.Log(e);
			}
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
		
		public bool CreateNode(INode node)
		{
			var nodeInfo = NodeInfoFactory.ToProxy(node.GetNodeInfo());
			
			try
			{
				// We don't know if nodeInfo was cached. Some properties like UserData, Factory
				// might be not set -> Update the nodeInfo.
				UpdateNodeInfos(nodeInfo.Filename);
				
				var factory = nodeInfo.Factory;
				if (factory.Create(nodeInfo, node))
					return true;
			}
			catch (Exception e)
			{
				Logger.Log(e);
				throw e;
			}
			finally
			{
				FNodes.Add(node);
				OnNodeAdded(new NodeEventArgs(node));
			}
			
			return false;
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
			finally
			{
				FNodes.Remove(node);
				OnNodeRemoved(new NodeEventArgs(node));
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
			NodeCollection.AddCombined(path);
			NodeCollection.Collect();
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
		
		public event NodeEventHandler NodeAdded;
		
		protected virtual void OnNodeAdded(NodeEventArgs args)
		{
			if (NodeAdded != null) {
				NodeAdded(this, args);
			}
		}
		
		public event NodeEventHandler NodeRemoved;
		
		protected virtual void OnNodeRemoved(NodeEventArgs args)
		{
			if (NodeRemoved != null) {
				NodeRemoved(this, args);
			}
		}
		
		public void GetRoot(out INode root)
		{
			FVVVVHost.GetRoot(out root);
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
		
		public void SelectNodes(INode[] nodes)
		{
			FVVVVHost.SelectNodes(nodes);
		}
		
		public void ShowEditor(INode node)
		{
			FVVVVHost.ShowEditor(node);
		}
		
		public void ShowGUI(INode node)
		{
			FVVVVHost.ShowGUI(node);
		}
		
		public void ShowHelpPatch(INodeInfo nodeInfo)
		{
			FVVVVHost.ShowHelpPatch(nodeInfo);
		}
		
		public void ShowNodeReference(INodeInfo nodeInfo)
		{
			FVVVVHost.ShowNodeReference(nodeInfo);
		}
		
		public void SetComponentMode(INode node, ComponentMode componentMode)
		{
			FVVVVHost.SetComponentMode(node, componentMode);
		}
		
		public string ExePath
		{
			get;
			private set;
		}
		
		public IWindow SelectedPatchWindow
		{
			get
			{
				return FVVVVHost.SelectedPatchWindow;
			}
		}
		
		#endregion
		
		#region helper methods
		protected IEnumerable<INode> GetAffectedNodes(INodeInfo nodeInfo)
		{
			return
				from node in FNodes
				let ni = NodeInfoFactory.ToProxy(node.GetNodeInfo())
				where ni == nodeInfo
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
			
			if(FCacheTimer.Enabled)
				FCacheTimer.Stop();
			
			FCacheTimer.Start();
		}
		
		protected void factory_NodeInfoRemoved(object sender, INodeInfo info)
		{
			//remove from cache
			var filename = info.Filename;
			if (FNodeInfoCache.ContainsKey(filename))
			{
				var cache = FNodeInfoCache[filename];
				
				//remove from info list
				var result = cache.Remove((ProxyNodeInfo) info);

				//also remove list if empty now
				if(cache.Count == 0)
					FNodeInfoCache.Remove(filename);
			}
			
			FLoadedFiles[filename] = false;
		}
		
		public void factory_NodeInfoUpdated(object sender, INodeInfo info)
		{
			try
			{
				var factory = info.Factory;

				// More of a hack. Find cleaner solution: EditorFactory shouldn't update node infos
				// every time.
				if (factory is EditorFactory) return;
				
				// Go through all the running hosts using this changed node info
				// and create a new plugin for them.
				foreach (var node in GetAffectedNodes(info))
				{
					factory.Create(info, node);
					
					//for effects need to update only one affected host
					//others will be updated vvvv internally
					if (factory is EffectsFactory)
						break;
				}
				
				factory_NodeInfoAdded(sender, info);
			}
			catch (Exception e)
			{
				Logger.Log(e);
			}
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
			
			string path = Path.Combine(Shell.CallerPath, partialName + ".dll");
			if (File.Exists(path)) {
				try {
					return Assembly.LoadFrom(path);
				} catch (Exception e) {
					Logger.Log(e);
					return null;
				}
			}
			else
			{
				path = Path.Combine(Shell.CallerPath + "\\..\\..\\plugins", partialName + ".dll");
				if (File.Exists(path)) {
					try {
						return Assembly.LoadFrom(path);
					} catch (Exception e) {
						Logger.Log(e);
						return null;
					}
				}
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
			
			Logger.Log(LogType.Debug, "Loaded cached node infos for: " + filename);
			
			return FNodeInfoCache[filename];
		}
		
		public void InvalidateCache(string filename)
		{
			if (FNodeInfoCache.ContainsKey(filename))
				FNodeInfoCache.Remove(filename);
			
			if (FDeserializedNodeInfoCache.ContainsKey(filename))
				FDeserializedNodeInfoCache.Remove(filename);
			
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
		
		public void MouseDownCB(INode node, Mouse_Buttons button, Modifier_Keys keys)
		{
			OnMouseDown(new VVVV.PluginInterfaces.V2.MouseEventArgs(node, button, keys));
		}
		
		public void MouseUpCB(INode node, Mouse_Buttons button, Modifier_Keys keys)
		{
			OnMouseUp(new VVVV.PluginInterfaces.V2.MouseEventArgs(node, button, keys));
		}
		
		public void NodeSelectionChangedCB(INode[] nodes)
		{
			OnNodeSelectionChanged(new NodeSelectionEventArgs(nodes));
		}
		
		private List<IWindow> FWindows = new List<IWindow>();
		public void WindowAddedCB(IWindow window)
		{
			FWindows.Add(window);
			OnWindowAdded(new WindowEventArgs(window));
		}
		
		public void WindowRemovedCB(IWindow window)
		{
			FWindows.Remove(window);
			OnWindowRemoved(new WindowEventArgs(window));
		}
		
		public void WindowSelectionChangeCB(IWindow window)
		{
			OnWindowSelectionChanged(new WindowEventArgs(window));
		}
		
		#endregion
	}
}