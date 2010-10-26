using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

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
	[Export(typeof(HDEHost))]
	public class HDEHost : IInternalHDEHost, IHDEHost
	{
		const string WINDOW_SWITCHER = "WindowSwitcher (VVVV)";
		const string KOMMUNIKATOR = "Kommunikator (VVVV)";
		const string NODE_BROWSER = "NodeBrowser (VVVV)";
		
		private IVVVVHost FVVVVHost;
		private Dictionary<INodeInfo, List<IAddonHost>> FRunningPluginHostsMap;
		private Dictionary<string, INodeInfo> FRegisteredNodeInfos;
		private Dictionary<INodeInfo, IAddonFactory> FNodeInfoFactoryMap;
		private IPluginBase FNodeBrowser, FWindowSwitcher, FKommunikator;
		
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
		public IGraphViewerHost GraphViewerHost { get; protected set; }
		
		[Export]
		public ISolution Solution { get; set; }
		
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
			
			// Set name to vvvv thread for easier debugging.
			Thread.CurrentThread.Name = "vvvv";
			
			// Create a windows forms sync context (FileSystemWatcher runs asynchronously).
			var context = SynchronizationContext.Current;
			if (context == null)
				SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
			
			FRunningPluginHostsMap = new Dictionary<INodeInfo, List<IAddonHost>>();
			FRegisteredNodeInfos = new Dictionary<string, INodeInfo>();
			FNodeInfoFactoryMap = new Dictionary<INodeInfo, IAddonFactory>();
			
			// Register at least one ICommandHistory for top level element ISolution
			var mappingRegistry = new MappingRegistry();
			mappingRegistry.RegisterMapping<ISolution, ICommandHistory, CommandHistory>(MapInstantiation.PerInstanceAndItsChilds);
			
			var location = new Uri(Shell.CallerPath.ConcatPath(@"..\..\plugins").ConcatPath("Solution.sln"));
			Solution = new Solution(location, mappingRegistry);
			
			EnumManager.SetHDEHost(this);
			
			Logger = new DefaultLogger();
			
		}
		
		#region IInternalHDEHost
		public void Initialize(IVVVVHost vvvvHost, INodeBrowserHost nodeBrowserHost, IWindowSwitcherHost windowSwitcherHost, IKommunikatorHost kommunikatorHost)
		{
			FVVVVHost = vvvvHost;
			
			// Route log messages to vvvv
			Logger.AddLogger(new VVVVLogger(FVVVVHost));
			
			NodeBrowserHost = nodeBrowserHost;
			WindowSwitcherHost = windowSwitcherHost;
			KommunikatorHost = kommunikatorHost;
			GraphViewerHost = vvvvHost as IGraphViewerHost;
			
			try
			{
				var catalog = new DirectoryCatalog(Shell.CallerPath);
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

			//initialize the addonfactories
			foreach (var factory in AddonFactories)
				AddFactory(factory);
			
			NodeCollection.AddJob(Shell.CallerPath.Remove(Shell.CallerPath.LastIndexOf(@"bin\managed")));
			NodeCollection.Collect();
			
			//now instantiate a NodeBrowser, a Kommunikator and a WindowSwitcher
			try
			{
				FWindowSwitcher = PluginFactory.CreatePlugin(GetNodeInfo(WINDOW_SWITCHER), null);
				FKommunikator = PluginFactory.CreatePlugin(GetNodeInfo(KOMMUNIKATOR), null);
				FNodeBrowser = PluginFactory.CreatePlugin(GetNodeInfo(NODE_BROWSER), null);
				(FNodeBrowser as INodeBrowser).DragDrop(false);
			}
			catch (Exception e)
			{
				Logger.Log(e);
			}
		}
		
		public void AddFactory(IAddonFactory factory)
		{
			try
			{
				factory.NodeInfoAdded += factory_NodeInfoAdded;
				factory.NodeInfoRemoved += factory_NodeInfoRemoved;
				factory.NodeInfoUpdated += factory_NodeInfoUpdated;
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
		
		public void ExtractNodeInfos(string filename, out INodeInfo[] result)
		{
			var nodeInfos = new List<INodeInfo>();
			
			foreach(IAddonFactory factory in AddonFactories)
			{
				foreach (var nodeInfo in factory.ExtractNodeInfos(filename))
					nodeInfos.Add(nodeInfo);
			}
			
			result = nodeInfos.ToArray();
		}
		
		public void Add(IAddonHost host, INodeInfo nodeInfo)
		{
			try
			{
				if (!FNodeInfoFactoryMap.ContainsKey(nodeInfo))
				{
					foreach (var factory in AddonFactories)
					{
						if (factory.Create(nodeInfo, host))
						{
							FNodeInfoFactoryMap[nodeInfo] = factory;
							break;
						}
					}
				}
				else
				{
					var factory = FNodeInfoFactoryMap[nodeInfo];
					factory.Create(nodeInfo, host);
				}
				
				if (!FRunningPluginHostsMap.ContainsKey(nodeInfo))
					FRunningPluginHostsMap[nodeInfo] = new List<IAddonHost>();

				FRunningPluginHostsMap[nodeInfo].Add(host);
			}
			catch (Exception e)
			{
				Logger.Log(e);
			}
		}
		
		public void Remove(IAddonHost host, INodeInfo nodeInfo)
		{
			try
			{
				if (!FNodeInfoFactoryMap.ContainsKey(nodeInfo))
				{
					foreach (var factory in AddonFactories)
					{
						if (factory.Delete(nodeInfo, host))
						{
							FNodeInfoFactoryMap[nodeInfo] = factory;
							break;
						}
					}
				}
				else
				{
					var factory = FNodeInfoFactoryMap[nodeInfo];
					factory.Delete(nodeInfo, host);
				}
				
				FRunningPluginHostsMap[nodeInfo].Remove(host);
				
				if (FRunningPluginHostsMap[nodeInfo].Count == 0)
					FRunningPluginHostsMap.Remove(nodeInfo);
			}
			catch (Exception e)
			{
				Logger.Log(e);
			}
		}
		
		public void Clone(INodeInfo nodeInfo, string path, string name, string category, string version)
		{
			try
			{
				foreach (var factory in AddonFactories)
				{
					if (factory.Clone(nodeInfo, path, name, category, version))
						break;
				}
			}
			catch (Exception e)
			{
				Logger.Log(e);
			}
		}
		
		#endregion IInternalHDEHost
		
		#region IHDEHost
		public void AddListener(IListener listener)
		{
			FVVVVHost.AddListener(listener);
		}
		
		public void RemoveListener(IListener listener)
		{
			FVVVVHost.RemoveListener(listener);
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
		
		public IEnumerable<INodeInfo> NodeInfos
		{
			get
			{
				return FRegisteredNodeInfos.Values;
			}
		}
		
		public INodeInfo GetNodeInfo(string systemname)
		{
			INodeInfo result = null;
			FRegisteredNodeInfos.TryGetValue(systemname.ToUpper(), out result);
			return result;
		}
		
		public void Open(string file, bool inActivePatch)
		{
			FVVVVHost.Open(file, inActivePatch);
		}
		#endregion
		
		#region helper methods
		protected IAddonHost[] GetAffectedHosts(INodeInfo nodeInfo)
		{
			List<IAddonHost> affectedHosts;
			if (FRunningPluginHostsMap.TryGetValue(nodeInfo, out affectedHosts))
			{
				return affectedHosts.ToArray();
			}
			else
			{
				return new IPluginHost2[0];
			}
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
		
		protected void UpdateNodeInfo(ref INodeInfo oldInfo, ref INodeInfo newInfo)
		{
			Type infoType = oldInfo.GetType();
			
			foreach (PropertyInfo p in GetAllProperties(infoType))
			{
				if (p.CanWrite && p.CanRead)
				{
					object val = infoType.InvokeMember(p.Name, BindingFlags.GetProperty, null, newInfo, new object[0]);
					infoType.InvokeMember(p.Name, BindingFlags.SetProperty, null, oldInfo, new object[] { val });
				}
			}
		}
		#endregion helper methods
		
		#region event handler
		protected void factory_NodeInfoAdded(IAddonFactory factory, INodeInfo info)
		{
			// Check if already registered.
			INodeInfo registeredInfo = null;
			if (!FRegisteredNodeInfos.TryGetValue(info.Systemname.ToUpper(), out registeredInfo))
			{
				// Register the NodeInfo in the NodeRegistry and add it to our local list.
				FVVVVHost.Add(info);
				FRegisteredNodeInfos.Add(info.Systemname.ToUpper(), info);
				FNodeInfoFactoryMap[info] = factory;
			}
		}
		
		protected void factory_NodeInfoRemoved(IAddonFactory factory, INodeInfo info)
		{
			INodeInfo registeredInfo = null;
			if (FRegisteredNodeInfos.TryGetValue(info.Systemname.ToUpper(), out registeredInfo))
			{
				if (info.Filename == registeredInfo.Filename)
				{
					FVVVVHost.Remove(registeredInfo);
					FRegisteredNodeInfos.Remove(registeredInfo.Systemname.ToUpper());
					FNodeInfoFactoryMap.Remove(registeredInfo);
				}
			}
		}
		
		public void factory_NodeInfoUpdated(IAddonFactory factory, INodeInfo info)
		{
			// Check if already registered.
			INodeInfo registeredInfo = null;
			if (FRegisteredNodeInfos.TryGetValue(info.Systemname.ToUpper(), out registeredInfo))
			{
				// Do not overwrite plugins with dynamic plugins.
				if (registeredInfo.Type == NodeType.Plugin && info.Type == NodeType.Dynamic)
					return;
				
				UpdateNodeInfo(ref registeredInfo, ref info);
				
				// Go through all the running hosts using this changed node info
				// and create a new plugin for them.
				foreach (IAddonHost host in GetAffectedHosts(registeredInfo))
				{
					factory.Create(info, host);
					
					//for effects need to update only one affected host
					//others will be updated vvvv internally
					if (factory is EffectsFactory)
						break;
				}
				
				FVVVVHost.Update(registeredInfo);
			}
			else
				factory_NodeInfoAdded(factory, info);
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
	}
}