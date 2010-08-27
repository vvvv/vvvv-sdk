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
	public class DotNetPluginFactory : AbstractFileFactory, IPluginFactory
	{
		[ImportMany(typeof(IPluginBase), AllowRecomposition=true)]
		private List<ExportFactory<IPluginBase, INodeInfoStuff>> FNodeInfoExports { get; set; }
		
		[Import]
		private ILogger Logger { get; set; }
		
		private AggregateCatalog FCatalog;
		private CompositionContainer FContainer;
		
		private Dictionary<string, ExportFactory<IPluginBase, INodeInfoStuff>> FMEFPlugins = new Dictionary<string, ExportFactory<IPluginBase, INodeInfoStuff>>();
		private Dictionary<IPluginBase, ExportLifetimeContext<IPluginBase>> FPluginLifetimeContexts = new Dictionary<IPluginBase, ExportLifetimeContext<IPluginBase>>();
		private CompositionContainer FParentContainer;
		private Dictionary<string, INodeInfo> FStaticPlugins;
		
		protected HostExportProvider FHostExportProvider;
		
		#region Constructor
		[ImportingConstructor]
		public DotNetPluginFactory(CompositionContainer parentContainer)
		{
			FStaticPlugins = new Dictionary<string, INodeInfo>();
			FFileExtension = ".dll";

			FParentContainer = parentContainer;
			
			FHostExportProvider = new HostExportProvider();
			var exportProviders = new ExportProvider[] { FParentContainer, FHostExportProvider };
			FCatalog = new AggregateCatalog();
			FContainer = new CompositionContainer(FCatalog, exportProviders);
			
			//foreach (var file in Directory.GetFiles(FDirectory.ConcatPath("hde")))
			//	AddFile(file);
			
			FDirectory = Path.GetFullPath(Path.Combine(FDirectory, @"..\..\plugins"));
		}
		#endregion
		
		#region IPluginFactory
		/// <summary>
		/// Called by ProjectFactory. Registers dynamic plugins.
		/// </summary>
		public bool Register(IExecutable executable)
		{
			if (executable is DotNetExecutable)
			{
				DotNetExecutable dotNetExec = (DotNetExecutable) executable;
				
				var project = dotNetExec.Project;
				var assembly = dotNetExec.Assembly;
				
				try
				{
					bool foundPlugin = false;
					
					//check for V1 style plugin
					foreach (var info in ExtractNodeInfosFromAssembly(assembly))
					{
						info.Executable = executable;
						info.Type = NodeType.Dynamic;
						info.Filename = project.Location.LocalPath;
						
						// Notify listeners about the new extracted node info.
						// Ignore the node info if it is aready registered statically.
						if (!FStaticPlugins.ContainsKey(info.Systemname))
							OnNodeInfoAdded(info);
						
						foundPlugin = true;
					}
					
					//maybe V2 style plugin
					if (!foundPlugin)
					{
						foreach (var info in ExtractNodeInfosFromCatalog(new AssemblyCatalog(assembly)))
						{
							info.Executable = executable;
							info.Type = NodeType.Dynamic;
							info.Filename = project.Location.LocalPath;
							
							// Ignore the node info if it is aready registered statically.
							if (!FStaticPlugins.ContainsKey(info.Systemname))
								OnNodeInfoAdded(info);
							
							foundPlugin = true;
						}
					}
					
					return foundPlugin;
				}
				catch (ReflectionTypeLoadException e)
				{
					foreach (var f in e.LoaderExceptions)
						Logger.Log(f);
					return false;
				}
				catch (Exception e)
				{
					Logger.Log(e);
					return false;
				}
			}
			
			return false;
		}
		
		// TODO: Should be called by ProjectFactory ...
		public bool UnRegister(IExecutable executable)
		{
//			if (executable is DotNetExecutable)
//			{
//				DotNetExecutable dotNetExec = (DotNetExecutable) executable;
//
//				bool foundPlugin = false;
//
//				IProject project = dotNetExec.Project;
//				Assembly assembly = dotNetExec.Assembly;
//
//				try {
//					foreach (Type type in assembly.GetTypes())
//					{
//						// if type implements IPlugin
//						if (typeof(IPluginBase).IsAssignableFrom(type))
//						{
//							INodeInfo info = GetNodeInfo(type);
//							if (info != null)
//							{
//								// Add ourselfs to the NodeInfo.
//								info.Filename = assembly.Location;
//								info.Arguments = type.Namespace + "." + type.Name;
//								info.Executable = executable;
//								info.Class = type.Name;
//								info.Namespace = type.Namespace;
//								info.Type = TNodeType.Plugin;
//
//								// Notify listeners about the to be removed node info.
//								OnNodeInfoRemoved(info);
//
//								foundPlugin = true;
//							}
//						}
//					}
//				} catch (Exception e)
//				{
//					FLogger.Log(e);
//					return false;
//				}
//
//				return foundPlugin;
//			}
			
			return false;
		}
		
		public override bool Create(INodeInfo nodeInfo, IAddonHost host)
		{
			if ((nodeInfo.Type != NodeType.Plugin) && (nodeInfo.Type != NodeType.Dynamic))
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
					Logger.Log(f);
			}
			catch (Exception e)
			{
				Logger.Log(e);
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
		/// Called either by AbstractFileFactory to extract all node infos in given file or
		/// vvvv to get the node info for one specific node.
		/// </summary>
		public override IEnumerable<INodeInfo> ExtractNodeInfos(string systemname)
		{
			IList<INodeInfo> nodeInfos = new List<INodeInfo>();
			
			// systemname is of form FILENAME[|ARGUMENTS], for example:
			// - C:\Path\To\Assembly.dll
			// or
			// - C:\Path\To\Assembly.dll|Namespace.Class
			
			string filename = systemname;
			string arguments = null;
			
			int pipeIndex = systemname.IndexOf('|');
			if (pipeIndex >= 0)
			{
				filename = systemname.Substring(0, pipeIndex);
				arguments = systemname.Substring(pipeIndex + 1);
			}
			
			if (Path.GetExtension(filename) != FFileExtension) return nodeInfos;
			
			// See if it's a .net assembly
			if (!IsDotNetAssembly(filename)) 
			{
				Logger.Log(LogType.Debug, "{0} is not a CLR assembly.", filename);
				return nodeInfos;
			}
			
			try
			{
				// Check for V2 style plugins
				foreach (var nodeInfo in ExtractNodeInfosFromCatalog(new AssemblyCatalog(filename)))
				{
					nodeInfo.Executable = new DotNetExecutable(null, new Lazy<Assembly>(() => Assembly.LoadFrom(filename)));
					nodeInfo.Filename = filename;
					nodeInfo.Type = NodeType.Plugin;
					nodeInfos.Add(nodeInfo);
				}
				
				if (nodeInfos.Count == 0)
				{
					var assembly = Assembly.LoadFrom(filename);
					
					// Check for V1 style plugins
					foreach (var nodeInfo in ExtractNodeInfosFromAssembly(assembly))
					{
						nodeInfo.Executable = new DotNetExecutable(null, assembly);
						nodeInfo.Filename = filename;
						nodeInfo.Type = NodeType.Plugin;
						nodeInfos.Add(nodeInfo);
					}
				}
				
				// Add the node infos to a local dictionary in order to look them up
				// quickly in Register method called later by project factory.
				foreach (var nodeInfo in nodeInfos)
					FStaticPlugins[nodeInfo.Systemname] = nodeInfo;
				
				// If additional arguments are present vvvv is only interested in one specific
				// NodeInfo -> look for it.
				if (arguments != null)
				{
					// This is easy in case of V1 plugins. Arguments are already set. We just need to
					// look it up.
					foreach (var nodeInfo in nodeInfos)
					{
						if (nodeInfo.Arguments != null && nodeInfo.Arguments == arguments)
							return new INodeInfo[] { nodeInfo };
					}
					
					// In case of V2 plugins it's harder. Arguments are not set, because MEF doesn't provide
					// this information.
					foreach (var part in FCatalog.Parts)
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
												return new INodeInfo[] { nodeInfo };
										}
									}
								}
							}
						}
					}
				}
			}
			catch (ReflectionTypeLoadException e)
			{
				Logger.Log(LogType.Error, "Extracting node infos from {0} caused the following expection:", filename);
				foreach (var f in e.LoaderExceptions)
					Logger.Log(f);
			}
			catch (Exception e)
			{
				Logger.Log(LogType.Error, "Extracting node infos from {0} caused the following expection:", filename);
				Logger.Log(e);
			}
			
			return nodeInfos;
		}
		#endregion
		
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
		
		protected IEnumerable<INodeInfo> ExtractNodeInfosFromCatalog(AssemblyCatalog catalog)
		{
			FCatalog.Catalogs.Clear();
			FCatalog.Catalogs.Add(catalog);
			FContainer.ComposeParts(this);
			
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