using System;
using System.Collections;
using System.Runtime.InteropServices;
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
using VVVV.Utils.Collections;

namespace VVVV.Hosting.Factories
{
    [ComVisible(false)]
    public delegate void PluginCreatedDelegate(IPluginBase plugin, IPluginHost2 host);
    
    [ComVisible(false)]
    public delegate void PluginDeletedDelegate(IPluginBase plugin);

    enum PluginVersion
    {
        V1,
        V2
    }
    
    /// <summary>
    /// DotNetPluginFactory for V1 and V2 style plugins.
    /// V1 style plugins need to be loaded manually
    /// V2 style plugins are loaded via MEF
    /// </summary>
    [Export(typeof(IAddonFactory))]
    [Export(typeof(DotNetPluginFactory))]
    [ComVisible(false)]
    public class DotNetPluginFactory : AbstractFileFactory<IInternalPluginHost>
    {
        class PluginImporter
        {
            [Import(typeof(IPluginBase), AllowRecomposition=true)]
            public ExportFactory<IPluginBase> PluginExportFactory { get; set; }
        }
        
        [Import]
        protected IHDEHost FHost;

        [Import]
        private StartableRegistry FStartableRegistry;
        
        private PluginImporter FPluginImporter = new PluginImporter();
        private readonly Dictionary<IPluginBase, ExportLifetimeContext<IPluginBase>> FPluginLifetimeContexts;
        private readonly Dictionary<IPluginBase, HostExportProvider> FPinExportProviders;
        private readonly CompositionContainer FParentContainer;
        private readonly Type FReflectionOnlyPluginBaseType;
        protected Regex FDynamicRegExp = new Regex(@"(.*)\._dynamic_\.[0-9]+\.dll$");



        public Dictionary<string, IPluginBase> FNodesPath = new Dictionary<string, IPluginBase>();
        public Dictionary<IPluginBase, IPluginHost2> FNodes = new Dictionary<IPluginBase, IPluginHost2>();
        
        [ImportingConstructor]
        public DotNetPluginFactory(CompositionContainer parentContainer)
            : this(parentContainer, ".dll")
        {

        }
        
        protected DotNetPluginFactory(CompositionContainer parentContainer, string fileExtension)
            : base(fileExtension)
        {
            FParentContainer = parentContainer;
            FPinExportProviders = new Dictionary<IPluginBase, HostExportProvider>();
            FPluginLifetimeContexts = new Dictionary<IPluginBase, ExportLifetimeContext<IPluginBase>>();
            
            var pluginInterfacesAssemblyName = typeof(IPluginBase).Assembly.FullName;
            var pluginInterfacesAssembly = Assembly.ReflectionOnlyLoad(pluginInterfacesAssemblyName);
            FReflectionOnlyPluginBaseType = pluginInterfacesAssembly.GetExportedTypes().Where(t => t.Name == typeof(IPluginBase).Name).First();
        }

        public event PluginCreatedDelegate PluginCreated;
        public event PluginDeletedDelegate PluginDeleted;

        public override string JobStdSubPath
        {
            get
            {
                return "plugins";
            }
        }
        
        protected override void AddSubDir(string dir, bool recursive)
        {
            // Ignore obj directories used by C# IDEs and ignore dynamic bin directories
            if (dir.EndsWith(@"\obj\x86") || dir.EndsWith(@"\obj\x64") || dir.EndsWith(@"\bin\Dynamic")) return;
            
            base.AddSubDir(dir, recursive);
        }
        
        protected override bool CreateNode(INodeInfo nodeInfo, IInternalPluginHost pluginHost)
        {
            var plugin = pluginHost.Plugin;
            
            //make the host mark all its pins for possible deletion
            pluginHost.Plugin = null;
            
            //dispose previous plugin
            if (plugin != null) DisposePlugin(plugin);
            
            //create the new plugin
            plugin = CreatePlugin(nodeInfo, pluginHost as IPluginHost2);
            
            pluginHost.Plugin = plugin;
            
            return true;
        }
        
        protected override bool DeleteNode(INodeInfo nodeInfo, IInternalPluginHost pluginHost)
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
        protected override IEnumerable<INodeInfo> LoadNodeInfos(string filename)
        {
            var nodeInfos = new List<INodeInfo>();
            
            // We can't handle dynamic plugins
            if (!IsDynamicAssembly(filename))
                LoadNodeInfosFromFile(filename, filename, ref nodeInfos, true);
            
            return nodeInfos;
        }
        
        private static string FCurrentAssemblyDir = string.Empty;
        protected void LoadNodeInfosFromFile(string filename, string sourcefilename, ref List<INodeInfo> nodeInfos, bool commitUpdates)
        {
            // See if it's a .net assembly
            if (!IsDotNetAssembly(filename)) return;

            bool containsV1Plugins = false;
            bool nonLazyStartable = false;
            
            // Remember the current directory for later assembly resolution
            FCurrentAssemblyDir = Path.GetDirectoryName(filename);
            
            var assembly = Assembly.ReflectionOnlyLoadFrom(filename);
            foreach (var type in assembly.GetExportedTypes())
            {
                if (!type.IsAbstract && !type.IsGenericTypeDefinition && FReflectionOnlyPluginBaseType.IsAssignableFrom(type))
                {
                    var attribute = GetPluginInfoAttributeData(type);
                    
                    if (attribute != null)
                    {
                        // V2
                        var nodeInfo = ExtractNodeInfoFromAttributeData(attribute, sourcefilename);
                        nodeInfo.Arguments = type.FullName;
                        nodeInfo.Type = NodeType.Plugin;
                        nodeInfos.Add(nodeInfo);
                    }
                    else
                    {
                        // V1. See below.
                        containsV1Plugins = true;
                    }
                }


                bool nonlazy = FStartableRegistry.ProcessType(type, assembly);

                if (nonlazy)
                {
                    nonLazyStartable = true;
                }
            }
            
            // V1 plugins need to be loaded in LoadFrom context in order to instantiate the
            // static PluginInfo field. Type instantiation is not possible in
            // ReflectionOnly context.
            // TODO: This is very slow. Think about caching.
            if (containsV1Plugins)
            {
                assembly = Assembly.LoadFrom(filename);
                foreach (var type in assembly.GetExportedTypes())
                {
                    if (!type.IsAbstract && !type.IsGenericTypeDefinition && typeof(IPluginBase).IsAssignableFrom(type))
                    {
                        var nodeInfo = ExtractNodeInfoFromType(type, sourcefilename);
                        if (nodeInfo != null)
                        {
                            nodeInfo.Arguments = type.FullName;
                            nodeInfo.Type = NodeType.Plugin;
                            nodeInfos.Add(nodeInfo);
                        }
                    }
                }
            }

            if (nonLazyStartable)
            {
                var assemblyload = Assembly.LoadFrom(filename);
                FStartableRegistry.ProcessAssembly(assemblyload);
            }
            
            
            foreach (var nodeInfo in nodeInfos)
            {
                nodeInfo.Factory = this;
                if (commitUpdates)
                    nodeInfo.CommitUpdate();
            }
        }

        private static CustomAttributeData GetPluginInfoAttributeData(Type type)
        {
            var attributes = CustomAttributeData.GetCustomAttributes(type).Where(ca => ca.Constructor.DeclaringType.FullName == typeof(PluginInfoAttribute).FullName).ToArray();
            return attributes.Length > 0 ? attributes[0] : null;
        }
        
        private INodeInfo ExtractNodeInfoFromAttributeData(CustomAttributeData attribute, string filename)
        {
            var namedArguments = new Dictionary<string, object>();
            foreach (var namedArgument in attribute.NamedArguments)
            {
                namedArguments[namedArgument.MemberInfo.Name] = namedArgument.TypedValue.Value;
            }
            
            var nodeInfo = FNodeInfoFactory.CreateNodeInfo(
                (string) namedArguments.ValueOrDefault("Name"),
                (string) namedArguments.ValueOrDefault("Category"),
                (string) namedArguments.ValueOrDefault("Version"),
                filename,
                true);
            
            namedArguments.Remove("Name");
            namedArguments.Remove("Category");
            namedArguments.Remove("Version");
            
            if (namedArguments.ContainsKey("InitialWindowWidth") && namedArguments.ContainsKey("InitialWindowHeight"))
            {
                nodeInfo.InitialWindowSize = new Size((int) namedArguments["InitialWindowWidth"], (int) namedArguments["InitialWindowHeight"]);
                namedArguments.Remove("InitialWindowWidth");
                namedArguments.Remove("InitialWindowHeight");
            }
            
            if (namedArguments.ContainsKey("InitialBoxWidth") && namedArguments.ContainsKey("InitialBoxHeight"))
            {
                nodeInfo.InitialBoxSize = new Size((int) namedArguments["InitialBoxWidth"], (int) namedArguments["InitialBoxHeight"]);
                namedArguments.Remove("InitialBoxWidth");
                namedArguments.Remove("InitialBoxHeight");
            }
            
            if (namedArguments.ContainsKey("InitialComponentMode"))
            {
                nodeInfo.InitialComponentMode = (TComponentMode) namedArguments["InitialComponentMode"];
                namedArguments.Remove("InitialComponentMode");
            }
            
            foreach (var entry in namedArguments)
            {
                nodeInfo.GetType().InvokeMember((string) entry.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty, Type.DefaultBinder, nodeInfo, new object[] { entry.Value });
            }

            return nodeInfo;
        }
        
        private INodeInfo ExtractNodeInfoFromType(Type type, string filename)
        {
            INodeInfo nodeInfo = null;
            
            foreach (PropertyInfo info in type.GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                if (info.PropertyType == typeof(INodeInfo))
                {
                    var pluginNodeInfo = (INodeInfo) info.GetValue(null, null);
                    
                    nodeInfo = FNodeInfoFactory.CreateNodeInfo(
                        pluginNodeInfo.Name,
                        pluginNodeInfo.Category,
                        pluginNodeInfo.Version,
                        filename,
                        true);
                    
                    nodeInfo.UpdateFromNodeInfo(pluginNodeInfo);
                    break;
                }
                // The old interface
                else if (info.PropertyType == typeof(IPluginInfo))
                {
                    var pluginInfo = (IPluginInfo) info.GetValue(null, null);
                    
                    nodeInfo = FNodeInfoFactory.CreateNodeInfo(
                        pluginInfo.Name,
                        pluginInfo.Category,
                        pluginInfo.Version,
                        filename,
                        true);
                    
                    nodeInfo.UpdateFromPluginInfo(pluginInfo);
                    break;
                }
            }
            
            return nodeInfo;
        }
        
        public IPluginBase CreatePlugin(INodeInfo nodeInfo, IPluginHost2 pluginHost)
        {
            IPluginBase plugin = null;
            
            string assemblyLocation = string.Empty;
            var isUpToDate = GetAssemblyLocation(nodeInfo, out assemblyLocation);
            
            // HACK: pluginHost is null in case of WindowSwitcher/NodeBrowser/etc. Fix this.
            if (pluginHost != null)
            {
                // Mark the node if old assembly was loaded and log warning.
                if (!isUpToDate)
                {
                    pluginHost.Status |= StatusCode.HasInvalidData;
                    FLogger.Log(LogType.Warning, string.Format("Plugin of node '{0}' (ID: {1}) is out of date and couldn't be recompiled. Check its source code for errors.", nodeInfo.Username, pluginHost.GetID()));
                }
                else
                {
                    pluginHost.Status &= ~StatusCode.HasInvalidData;
                }
            }
            
            var assembly = Assembly.LoadFrom(assemblyLocation);

            //Check if need to start anything before rest is loaded
            FStartableRegistry.ProcessAssembly(assembly);

            var type = assembly.GetType(nodeInfo.Arguments);
            var attribute = GetPluginInfoAttributeData(type);
            if (attribute != null)
            {
                var catalog = new TypeCatalog(type);
                var pinExportProvider = new HostExportProvider(pluginHost);
                var exportProviders = new ExportProvider[] { pinExportProvider, FParentContainer };
                var container = new CompositionContainer(catalog, exportProviders);
                container.ComposeParts(FPluginImporter);
                
                var lifetimeContext = FPluginImporter.PluginExportFactory.CreateExport();
                
                plugin = lifetimeContext.Value;
                
                FPluginLifetimeContexts[plugin] = lifetimeContext;
                FPinExportProviders[plugin] = pinExportProvider;
            }
            else
            {
                var v1Plugin = (IPlugin)assembly.CreateInstance(nodeInfo.Arguments);
                
                v1Plugin.SetPluginHost(pluginHost);
                
                plugin = v1Plugin;
            }
            
            //Send event
            if (this.PluginCreated != null) { this.PluginCreated(plugin, pluginHost); }
            
            return plugin;
        }
        
        public void DisposePlugin(IPluginBase plugin)
        {
            //Send event before delete
            if (this.PluginDeleted != null) { this.PluginDeleted(plugin); }

            var disposablePlugin = plugin as IDisposable;
            if (FPluginLifetimeContexts.ContainsKey(plugin))
            {
                FPluginLifetimeContexts[plugin].Dispose();
                FPluginLifetimeContexts.Remove(plugin);
                FPinExportProviders[plugin].Dispose();
                FPinExportProviders.Remove(plugin);
            }
            else if (disposablePlugin != null)
            {
                disposablePlugin.Dispose();
            }
        }
        
        protected virtual bool GetAssemblyLocation (INodeInfo nodeInfo, out string assemblyLocation)
        {
            assemblyLocation = nodeInfo.Filename;
            return true;
        }
              
        // From http://www.anastasiosyal.com/archive/2007/04/17/3.aspx
        private static bool IsDotNetAssembly(string fileName)
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
                                    throw new BadImageFormatException("Invalid Image Format");
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
    }
}