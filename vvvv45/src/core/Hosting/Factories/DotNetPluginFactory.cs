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
    public delegate void PluginCreatedDelegate(IPluginBase plugin, IPluginHost2 host);
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
    public class DotNetPluginFactory : AbstractFileFactory<IInternalPluginHost>
    {
        class PluginImporter
        {
            [Import(typeof(IPluginBase), AllowRecomposition=true)]
            public ExportFactory<IPluginBase> PluginExportFactory { get; set; }
        }
        
        [Import]
        protected IHDEHost FHost;
        
        private PluginImporter FPluginImporter = new PluginImporter();
        private Dictionary<IPluginBase, ExportLifetimeContext<IPluginBase>> FPluginLifetimeContexts;
        private readonly Dictionary<INodeInfo, PluginVersion> FPluginVersion = new Dictionary<INodeInfo, PluginVersion>();
        protected Regex FDynamicRegExp = new Regex(@"(.*)\._dynamic_\.[0-9]+\.dll$");

        public Dictionary<string, IPluginBase> FNodesPath = new Dictionary<string, IPluginBase>();
        public Dictionary<IPluginBase, IPluginHost2> FNodes = new Dictionary<IPluginBase, IPluginHost2>();
        
        protected HostExportProvider FHostExportProvider;
        public ExportProvider[] ExportProviders
        {
            get;
            private set;
        }
        
        #region Constructor
        [ImportingConstructor]
        public DotNetPluginFactory(CompositionContainer parentContainer)
            : this(parentContainer, ".dll")
        {

        }
        
        protected DotNetPluginFactory(CompositionContainer parentContainer, string fileExtension)
            : base(fileExtension)
        {
            FPluginLifetimeContexts = new Dictionary<IPluginBase, ExportLifetimeContext<IPluginBase>>();
            FHostExportProvider = new HostExportProvider();
            ExportProviders = new ExportProvider[] { FHostExportProvider, parentContainer };
        }
        #endregion

        public event PluginCreatedDelegate PluginCreated;
        public event PluginDeletedDelegate PluginDeleted;
        
        #region IAddonFactory
        
        public override string JobStdSubPath 
        {
            get 
            {
                return "plugins";
            }
        }
        
        protected override void AddSubDir(string dir, bool recursive)
        {
            // Ignore obj directories used by C# IDEs
            if (dir.EndsWith(@"\obj\x86") || dir.EndsWith(@"\obj\x64")) return;
            
            base.AddSubDir(dir, recursive);
        }
        
        protected override bool CreateNode(INodeInfo nodeInfo, IInternalPluginHost pluginHost)
        {
            //dispose previous plugin
            var plugin = pluginHost.Plugin;
            if (plugin != null) DisposePlugin(plugin);
            
            //make the host mark all its pins for possible deletion
            pluginHost.Plugin = null;
            
            //create the plugin
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
        
        protected void LoadNodeInfosFromFile(string filename, string sourcefilename, ref List<INodeInfo> nodeInfos, bool commitUpdates)
        {
            // See if it's a .net assembly
            if (!IsDotNetAssembly(filename)) return;
            
            var assembly = Assembly.LoadFrom(filename);
            foreach (var type in assembly.GetExportedTypes())
            {
                if (!type.IsAbstract && !type.IsGenericTypeDefinition && typeof(IPluginBase).IsAssignableFrom(type))
                {
                    var attributes = type.GetCustomAttributes(typeof(PluginInfoAttribute), true);
                    
                    INodeInfo nodeInfo = null;
                    if (attributes.Length > 0)
                    {
                        // V2
                        var attribute = attributes[0] as PluginInfoAttribute;
                        nodeInfo = ExtractNodeInfoFromAttribute(attribute, sourcefilename);
						if (nodeInfo != null)
                        	FPluginVersion[nodeInfo] = PluginVersion.V2;
                    }
                    else
                    {
                        // V1
                        nodeInfo = ExtractNodeInfoFromType(type, sourcefilename);
						if (nodeInfo != null)
                        	FPluginVersion[nodeInfo] = PluginVersion.V1;
                    }
                    
                    if (nodeInfo != null)
                    {
                        nodeInfo.Arguments = type.FullName;
                        nodeInfo.Type = NodeType.Plugin;
                        nodeInfos.Add(nodeInfo);
                    }
                }
            }
			
            foreach (var nodeInfo in nodeInfos)
            {
                nodeInfo.Factory = this;
                if (commitUpdates)
                    nodeInfo.CommitUpdate();
            }
        }
        
        private INodeInfo ExtractNodeInfoFromAttribute(PluginInfoAttribute attribute, string filename)
        {
            var nodeInfo = FNodeInfoFactory.CreateNodeInfo(attribute.Name, attribute.Category, attribute.Version, filename, true);
            nodeInfo.Shortcut = attribute.Shortcut;
            nodeInfo.Author = attribute.Author;
            nodeInfo.Help = attribute.Help;
            nodeInfo.Tags = attribute.Tags;
            nodeInfo.Bugs = attribute.Bugs;
            nodeInfo.Credits = attribute.Credits;
            nodeInfo.Warnings = attribute.Warnings;
            nodeInfo.InitialWindowSize = new Size(attribute.InitialWindowWidth, attribute.InitialWindowHeight);
            nodeInfo.InitialBoxSize = new Size(attribute.InitialBoxWidth, attribute.InitialBoxHeight);
            nodeInfo.InitialComponentMode = attribute.InitialComponentMode;
            nodeInfo.AutoEvaluate = attribute.AutoEvaluate;
            nodeInfo.Ignore = attribute.Ignore;
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
        
        #endregion
        
        public IPluginBase CreatePlugin(INodeInfo nodeInfo, IPluginHost2 pluginHost)
        {
			var assemblyLocation = GetAssemblyLocation(nodeInfo);
			switch (FPluginVersion[nodeInfo])
			{
				case PluginVersion.V2:
				{
					var assembly = Assembly.LoadFrom (assemblyLocation);
					var type = assembly.GetType (nodeInfo.Arguments);
					var catalog = new TypeCatalog (type);
					var container = new CompositionContainer (catalog, ExportProviders);
					container.ComposeParts (FPluginImporter);
				
					FHostExportProvider.PluginHost = pluginHost;
			
					var lifetimeContext = FPluginImporter.PluginExportFactory.CreateExport ();
					var plugin = lifetimeContext.Value;
			
					FPluginLifetimeContexts [plugin] = lifetimeContext;

                    //Send event
                    if (this.PluginCreated != null) { this.PluginCreated(plugin, pluginHost); }
			
					return plugin;
				}
				case PluginVersion.V1:
				{
					var assembly = Assembly.LoadFrom (assemblyLocation);
					var plugin = (IPlugin)assembly.CreateInstance (nodeInfo.Arguments);
			
					plugin.SetPluginHost (pluginHost);

                    //Send event
                    if (this.PluginCreated != null) { this.PluginCreated(plugin, pluginHost); }

					return plugin;
				}
			}
            
            throw new InvalidOperationException(string.Format("Can't create plugin '{0}'.", nodeInfo.Systemname));
        }
        
        public void DisposePlugin(IPluginBase plugin)
        {
            //Send event before delete
            if (this.PluginDeleted != null) { this.PluginDeleted(plugin); }

            if (FPluginLifetimeContexts.ContainsKey(plugin))
            {
                FPluginLifetimeContexts[plugin].Dispose();
                FPluginLifetimeContexts.Remove(plugin);
            }
            else if (plugin is IDisposable)
            {
                ((IDisposable) plugin).Dispose();
            }
        }
		
		protected virtual string GetAssemblyLocation (INodeInfo nodeInfo)
		{
			return nodeInfo.Filename;
		}
        
        #region Helper functions
        
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
}