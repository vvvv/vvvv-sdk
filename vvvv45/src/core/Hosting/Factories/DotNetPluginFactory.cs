using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.Model;
using VVVV.Core.Runtime;
using VVVV.Hosting;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Factories
{
	/// <summary>
	/// DotNetPluginFactory for V1 and V2 style plugins.
	/// V1 style plugins need to be loaded manually
	/// V2 style plugins are loaded via MEF
	/// </summary>
	[Export(typeof(IAddonFactory))]
	[Export(typeof(DotNetPluginFactory))]
	public class DotNetPluginFactory : AbstractFileFactory<IPluginHost2>
	{
		[ImportMany(typeof(IPluginBase), AllowRecomposition=true)]
		private List<ExportFactory<IPluginBase, INodeInfoStuff>> FNodeInfoExports { get; set; }
		
		[Import]
		protected ILogger FLogger;
		
		[Import]
		protected IHDEHost FHost;
		
		[Import]
		protected ISolution FSolution;
		
		private Dictionary<string, ExportFactory<IPluginBase, INodeInfoStuff>> FMEFPlugins = new Dictionary<string, ExportFactory<IPluginBase, INodeInfoStuff>>();
		private Dictionary<IPluginBase, ExportLifetimeContext<IPluginBase>> FPluginLifetimeContexts = new Dictionary<IPluginBase, ExportLifetimeContext<IPluginBase>>();
		private Dictionary<string, ComposablePartCatalog> FCatalogCache = new Dictionary<string, ComposablePartCatalog>();
		protected Regex FDynamicRegExp = new Regex(@"(.*)\._dynamic_\.[0-9]+\.dll$");
		
		protected HostExportProvider FHostExportProvider;
		private ExportProvider[] FExportProviders;
		
		#region Constructor
		[ImportingConstructor]
		public DotNetPluginFactory(CompositionContainer parentContainer)
			: this(parentContainer, ".dll")
		{
			
		}
		
		protected DotNetPluginFactory(CompositionContainer parentContainer, string fileExtension)
			: base(Shell.CallerPath.ConcatPath(@"..\..\plugins"), fileExtension)
		{
			FHostExportProvider = new HostExportProvider();
			FExportProviders = new ExportProvider[] { parentContainer, FHostExportProvider };
		}
		#endregion
		
		#region IAddonFactory
		
		protected override bool CreateNode(INodeInfo nodeInfo, IPluginHost2 pluginHost)
		{
			if (Path.GetExtension(nodeInfo.Filename) != FileExtension) return false;
			
			try
			{
				//make the host mark all its pins for possible deletion
				pluginHost.Plugin = null;
				
				//create the plugin
				pluginHost.Plugin = CreatePlugin(nodeInfo, pluginHost);
				return true;
			}
			catch (ReflectionTypeLoadException e)
			{
				foreach (var f in e.LoaderExceptions)
					FLogger.Log(f);
			}
			catch (Exception e)
			{
				FLogger.Log(e);
			}

			return false;
		}
		
		protected override bool DeleteNode(IPluginHost2 pluginHost)
		{
			var plugin = pluginHost.Plugin;
			
			if (plugin != null)
			{
				DisposePlugin(plugin);
				
				return true;
			}
			
			return false;
		}
		
		/// <summary>
		/// Called by AbstractFileFactory to extract all node infos in given file.
		/// </summary>
		protected override IEnumerable<INodeInfo> GetNodeInfos(string filename)
		{
			var nodeInfos = new List<INodeInfo>();
			
			// We can't handle dynamic plugins
			if (!IsDynamicAssembly(filename))
				LoadNodeInfosFromFile(filename, ref nodeInfos);
			
			return nodeInfos;
		}
		
		protected void LoadNodeInfosFromFile(string filename, ref List<INodeInfo> nodeInfos)
		{
			// See if it's a .net assembly
			if (!IsDotNetAssembly(filename))
			{
				FLogger.Log(LogType.Debug, "{0} is not a CLR assembly.", filename);
				return;
			}
			
			try
			{
				// Check for V2 style plugins
				if (!FCatalogCache.ContainsKey(filename))
					FCatalogCache[filename] = new AssemblyCatalog(filename);
				
				foreach (var nodeInfo in ExtractNodeInfosFromCatalog(FCatalogCache[filename]))
				{
					if (IsValidNodeInfo(nodeInfo))
					{
//						nodeInfo.Executable = new DotNetExecutable(null, new Lazy<Assembly>(() => Assembly.LoadFrom(filename)));
						nodeInfo.Filename = filename;
						nodeInfo.Type = NodeType.Plugin;
						nodeInfos.Add(nodeInfo);
					}
				}
				
				if (nodeInfos.Count == 0)
				{
					var assembly = Assembly.LoadFrom(filename);
					
					// Check for V1 style plugins
					foreach (var nodeInfo in ExtractNodeInfosFromAssembly(assembly))
					{
						if (IsValidNodeInfo(nodeInfo))
						{
//							nodeInfo.Executable = new DotNetExecutable(null, assembly);
							nodeInfo.Filename = filename;
							nodeInfo.Type = NodeType.Plugin;
							nodeInfos.Add(nodeInfo);
						}
					}
				}
			}
			catch (ReflectionTypeLoadException e)
			{
				FLogger.Log(LogType.Error, "Extracting node infos from {0} caused the following exception:", filename);
				foreach (var f in e.LoaderExceptions)
					FLogger.Log(f);
			}
			catch (Exception e)
			{
				FLogger.Log(LogType.Error, "Extracting node infos from {0} caused the following exception:", filename);
				FLogger.Log(e);
			}
		}
		
		#endregion
		
		private bool IsValidNodeInfo(INodeInfo nodeInfo)
		{
			var registeredInfo = FHost.GetNodeInfo(nodeInfo.Systemname);
			return registeredInfo == null || !IsLoaded(nodeInfo.Filename) || registeredInfo.Type == NodeType.Dynamic;
		}
		
		public IPluginBase CreatePlugin(INodeInfo nodeInfo, IPluginHost2 pluginHost)
		{
			if (!IsLoaded(nodeInfo.Filename))
				LoadAndCacheNodeInfos(nodeInfo.Filename);
			
			var systemName = nodeInfo.Systemname;
			
			//V2 plugin
			if (FMEFPlugins.ContainsKey(systemName))
			{
				FHostExportProvider.PluginHost = pluginHost;
				
				var lifetimeContext = FMEFPlugins[systemName].CreateExport();
				var plugin = lifetimeContext.Value;
				
				FPluginLifetimeContexts[plugin] = lifetimeContext;
				
				//Create a wrapper around dynamic plugins in order to catch all exceptions properly.
				if (nodeInfo.Type == NodeType.Dynamic && plugin is IPluginEvaluate)
					plugin = new DynamicPluginWrapperV2(plugin as IPluginEvaluate, pluginHost);
				
				return plugin;
			}
			//V1 plugin
			else
			{
				var assembly = Assembly.LoadFrom(nodeInfo.Filename);
				var plugin = (IPlugin) assembly.CreateInstance(nodeInfo.Arguments);
				
				//Create a wrapper around dynamic plugins in order to catch all exceptions properly.
				if (nodeInfo.Type == NodeType.Dynamic && plugin is IPlugin)
					plugin = new DynamicPluginWrapperV1(plugin as IPlugin);
				
				plugin.SetPluginHost(pluginHost);
				
				return plugin;
			}
			
			throw new InvalidOperationException(string.Format("Can't create plugin '{0}'.", systemName));
		}
		
		public void DisposePlugin(IPluginBase plugin)
		{
			if (plugin is DynamicPluginWrapperV2)
				plugin = ((DynamicPluginWrapperV2) plugin).WrappedPlugin;
			
			if (FPluginLifetimeContexts.ContainsKey(plugin))
			{
				FPluginLifetimeContexts[plugin].Dispose();
				FPluginLifetimeContexts.Remove(plugin);
			}
		}
		
		#region Helper functions
		
		protected IEnumerable<INodeInfo> ExtractNodeInfosFromCatalog(ComposablePartCatalog catalog)
		{
			var nodeInfos = new Dictionary<string, INodeInfo>();
			
			var container = new CompositionContainer(catalog, FExportProviders);
			container.ComposeParts(this);
			
			foreach (var pluginExport in FNodeInfoExports)
			{
				var info = new NodeInfo();
				info.Name = pluginExport.Metadata.Name;
				info.Category = pluginExport.Metadata.Category;
				info.Version = pluginExport.Metadata.Version;
				info.Shortcut = pluginExport.Metadata.Shortcut;
				info.Author = pluginExport.Metadata.Author;
				info.Help = pluginExport.Metadata.Help;
				info.Tags = pluginExport.Metadata.Tags;
				info.Bugs = pluginExport.Metadata.Bugs;
				info.Credits = pluginExport.Metadata.Credits;
				info.Warnings = pluginExport.Metadata.Warnings;
				info.InitialWindowSize = new Size(pluginExport.Metadata.InitialWindowWidth, pluginExport.Metadata.InitialWindowHeight);
				info.InitialBoxSize = new Size(pluginExport.Metadata.InitialBoxWidth, pluginExport.Metadata.InitialBoxHeight);
				info.InitialComponentMode = pluginExport.Metadata.InitialComponentMode;
				info.AutoEvaluate = pluginExport.Metadata.AutoEvaluate;
				info.Ignore = pluginExport.Metadata.Ignore;
				
				//a dynamic plugin may register the the same info with a new export
				//so remove an existing info
				var systemname = info.Systemname;
				if (FMEFPlugins.ContainsKey(systemname))
					FMEFPlugins.Remove(systemname);
				
				FMEFPlugins.Add(systemname, pluginExport);
				nodeInfos.Add(systemname, info);
			}
			
			// Set Arguments property on each INodeInfo.
			foreach (var part in catalog.Parts)
			{
				var lazyPartType = ReflectionModelServices.GetPartType(part);
				
				foreach (var exportDefinition in part.ExportDefinitions)
				{
					var lazyMemberInfo = ReflectionModelServices.GetExportingMember(exportDefinition);
					if (lazyMemberInfo.MemberType == MemberTypes.TypeInfo)
					{
						var exportedPluginTypes = lazyMemberInfo.GetAccessors()
							.Where(memberInfo => typeof(IPluginBase).IsAssignableFrom(memberInfo as Type))
							.Select(memberInfo => memberInfo as Type);
						foreach (var exportedPluginType in exportedPluginTypes)
						{
							var arguments = exportedPluginType.FullName;
							
							var pluginInfoAttribute = Attribute.GetCustomAttribute(exportedPluginType, typeof(PluginInfoAttribute)) as PluginInfoAttribute;
							if (pluginInfoAttribute != null)
								nodeInfos[pluginInfoAttribute.Systemname].Arguments = arguments;
						}
					}
				}
			}
			
			return nodeInfos.Values;
		}
		
		protected IEnumerable<INodeInfo> ExtractNodeInfosFromAssembly(Assembly assembly)
		{
			foreach (Type type in assembly.GetTypes())
			{
				// if type implements IPluginBase
				if (!type.IsAbstract && typeof(IPluginBase).IsAssignableFrom(type))
				{
					foreach (PropertyInfo info in type.GetProperties(BindingFlags.Public | BindingFlags.Static))
					{
						if (info.PropertyType == typeof(INodeInfo))
						{
							var nodeInfo = (INodeInfo) info.GetValue(null, null);
							
							nodeInfo.Arguments = type.Namespace + "." + type.Name;
							nodeInfo.Class = type.Name;
							nodeInfo.Namespace = type.Namespace;
							
							yield return nodeInfo;
							break;
						}
						// The old interface
						else if (info.PropertyType == typeof(IPluginInfo))
						{
							var pluginInfo = (IPluginInfo) info.GetValue(null, null);
							var nodeInfo = new NodeInfo(pluginInfo);
							
							nodeInfo.Arguments = type.Namespace + "." + type.Name;
							nodeInfo.Class = type.Name;
							nodeInfo.Namespace = type.Namespace;
							
							yield return nodeInfo;
						}
					}
				}
			}
		}
		
		// From http://www.anastasiosyal.com/archive/2007/04/17/3.aspx
		private bool IsDotNetAssembly(string fileName)
		{
			using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				try
				{
					using (BinaryReader binReader = new BinaryReader(fs))
					{
						try
						{
							fs.Position = 0x3C; //PE Header start offset
							uint headerOffset = binReader.ReadUInt32();

							fs.Position = headerOffset + 0x18;
							UInt16 magicNumber = binReader.ReadUInt16();

							int dictionaryOffset;
							switch (magicNumber)
							{
									case 0x010B: dictionaryOffset = 0x60; break;
									case 0x020B: dictionaryOffset = 0x70; break;
								default:
									throw new Exception("Invalid Image Format");
							}

							//position to RVA 15
							fs.Position = headerOffset + 0x18 + dictionaryOffset + 0x70;


							//Read the value
							uint rva15value = binReader.ReadUInt32();
							return rva15value != 0;
						}
						finally
						{
							binReader.Close();
						}
					}
				}
				finally
				{
					fs.Close();
				}

			}
		}
		
		private bool IsDynamicAssembly(string filename)
		{
			return FDynamicRegExp.IsMatch(filename);
		}
		#endregion
	}

	public interface INodeInfoStuff
	{
		string Name { get; }
		string Category { get; }
		string Version { get; }
		string Shortcut { get; }
		string Author { get; }
		string Help { get; }
		string Tags { get; }
		string Bugs { get; }
		string Credits { get; }
		string Warnings { get; }
		int InitialWindowWidth { get; }
		int InitialWindowHeight { get; }
		int InitialBoxWidth { get; }
		int InitialBoxHeight { get; }
		TComponentMode InitialComponentMode { get; }
		bool AutoEvaluate { get; }
		bool Ignore { get; }
	}
}