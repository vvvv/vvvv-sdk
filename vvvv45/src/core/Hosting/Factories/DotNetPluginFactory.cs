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
	public class DotNetPluginFactory : AbstractFileFactory
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
		private Dictionary<string, List<INodeInfo>> FNodeInfoCache = new Dictionary<string, List<INodeInfo>>();
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
		
		public override bool Create(INodeInfo nodeInfo, IAddonHost host)
		{
			if (Path.GetExtension(nodeInfo.Filename) != FileExtension)
				return false;
			
			try
			{
				//make the host mark all its pins for possible deletion
				(host as IPluginHost2).Plugin = null;
				
				IPluginBase plugin = null;
				
				//V2 plugin
				if (FMEFPlugins.ContainsKey(nodeInfo.Systemname))
				{
					FHostExportProvider.PluginHost = host as IPluginHost;
					plugin = InstantiateV2Plugin(nodeInfo.Systemname);
					
					//Create a wrapper around dynamic plugins in order to catch all exceptions properly.
					if (nodeInfo.Type == NodeType.Dynamic && plugin is IPluginEvaluate)
						plugin = new DynamicPluginWrapperV2(plugin as IPluginEvaluate, host as IPluginHost);
				}
				//V1 plugin
				else if (nodeInfo.Executable is DotNetExecutable)
				{
					Assembly assembly = ((DotNetExecutable) nodeInfo.Executable).Assembly;
					plugin = (IPluginBase) assembly.CreateInstance(nodeInfo.Arguments);
					
					//Create a wrapper around dynamic plugins in order to catch all exceptions properly.
					if (nodeInfo.Type == NodeType.Dynamic && plugin is IPlugin)
						plugin = new DynamicPluginWrapperV1(plugin as IPlugin);
					
					(plugin as IPlugin).SetPluginHost(host as IPluginHost);
				}
				
				(host as IPluginHost2).Plugin = plugin;
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
		
		public override bool Delete(IAddonHost host)
		{
			if (host is IPluginHost2)
			{
				var plugin = (host as IPluginHost2).Plugin;
				
				if (plugin != null)
				{
					if (plugin is DynamicPluginWrapperV2)
						DisposeV2Plugin(((DynamicPluginWrapperV2) plugin).WrappedPlugin);
					else
						DisposeV2Plugin(plugin);
					
					return true;
				}
			}
			return base.Delete(host);
		}
		
		/// <summary>
		/// Called by AbstractFileFactory to extract all node infos in given file.
		/// </summary>
		protected override IEnumerable<INodeInfo> GetNodeInfos(string filename)
		{
			// See if we know this file an can load from cache.
			if (!FNodeInfoCache.ContainsKey(filename))
			{
				var nodeInfos = new List<INodeInfo>();
				
				// We can't handle dynamic plugins
				if (!IsDynamicAssembly(filename))
					LoadNodeInfosFromFile(filename, ref nodeInfos);
				
				// Cache the result
				FNodeInfoCache[filename] = nodeInfos;
			}
			
			return FNodeInfoCache[filename];
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
						nodeInfo.Executable = new DotNetExecutable(null, new Lazy<Assembly>(() => Assembly.LoadFrom(filename)));
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
							nodeInfo.Executable = new DotNetExecutable(null, assembly);
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
		
		/// <summary>
		/// Called by AbstractFileFactory to extract one node info out of nodeInfos which matches given arguments.
		/// </summary>
		protected override INodeInfo GetNodeInfo(string filename, string arguments)
		{
			var nodeInfos = GetNodeInfos(filename);
			
			// This is easy in case of V1 plugins. Arguments are already set. We just need to
			// look it up.
			foreach (var nodeInfo in nodeInfos)
			{
				if (nodeInfo.Arguments != null && nodeInfo.Arguments == arguments)
					return nodeInfo;
			}
			
			// In case of V2 plugins it's harder. Arguments are not set, because MEF doesn't provide
			// this information.
			foreach (var part in FCatalogCache[filename].Parts)
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
							var fullName = exportedPluginType.FullName;
							
							// We found the demaned type -> lookup associated and already created node info.
							if (fullName == arguments)
							{
								var pluginInfoAttribute = Attribute.GetCustomAttribute(exportedPluginType, typeof(PluginInfoAttribute)) as PluginInfoAttribute;
								
								foreach (var nodeInfo in nodeInfos)
								{
									if (nodeInfo.Systemname == pluginInfoAttribute.Systemname)
										return nodeInfo;
								}
							}
						}
					}
				}
			}
			
			return base.GetNodeInfo(filename, arguments);
		}
		
		#endregion
		
		private bool IsValidNodeInfo(INodeInfo nodeInfo)
		{
			var registeredInfo = FHost.GetNodeInfo(nodeInfo.Systemname);
			return registeredInfo == null || registeredInfo.Type == NodeType.Dynamic;
		}
		
		public IPluginBase InstantiateV2Plugin(string systemName)
		{
			try
			{
				var lifetimeContext = FMEFPlugins[systemName].CreateExport();
				var plugin = lifetimeContext.Value;
				
				FPluginLifetimeContexts[plugin] = lifetimeContext;
				
				return plugin;
			}
			catch (KeyNotFoundException e)
			{
				throw new KeyNotFoundException(systemName, e);
			}
		}
		
		public void DisposeV2Plugin(IPluginBase plugin)
		{
			if (FPluginLifetimeContexts.ContainsKey(plugin))
			{
				FPluginLifetimeContexts[plugin].Dispose();
				FPluginLifetimeContexts.Remove(plugin);
			}
		}
		
		#region Helper functions
		
		protected IEnumerable<INodeInfo> ExtractNodeInfosFromCatalog(ComposablePartCatalog catalog)
		{
//			FCatalog.Catalogs.Clear();
//			FCatalog.Catalogs.Add(catalog);
//			FContainer.ComposeParts(this);
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
				if (FMEFPlugins.ContainsKey(info.Systemname))
					FMEFPlugins.Remove(info.Systemname);
				
				FMEFPlugins.Add(info.Systemname, pluginExport);
				
				yield return info;
			}
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