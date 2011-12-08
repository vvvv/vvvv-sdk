using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Windows.Forms;
using VVVV.Hosting.Interfaces;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.IO
{
    class IOFactory : IIOFactory, IPlugin, IPluginConnections, IDisposable
    {
        private readonly IInternalPluginHost FPluginHost;
        private readonly IORegistry FIORegistry;
        private readonly List<IIOHandler> FCreatedHandlers = new List<IIOHandler>();
        private readonly List<IOHandler> FPreHandlers = new List<IOHandler>();
        private readonly List<IOHandler> FPostHandlers = new List<IOHandler>();
        private readonly List<IOHandler> FConfigHandlers = new List<IOHandler>();
        private readonly CompositionContainer FContainer;
        private readonly IPluginEvaluate FPlugin;
        private readonly bool FAutoEvaluate;
        
        [Import(typeof(IPluginBase))]
        public IPluginBase PluginBase
        {
            get;
            private set;
        }
        
        public IOFactory(
            IInternalPluginHost internalPluginHost,
            IORegistry streamRegistry,
            CompositionContainer parentContainer,
            Type pluginType,
            INodeInfo nodeInfo
           )
        {
            FPluginHost = internalPluginHost;
            FIORegistry = streamRegistry;
            
            var catalog = new TypeCatalog(pluginType);
            var ioExportProvider = new IOExportProvider(this);
            var hostExportProvider = new HostExportProvider() { PluginHost = internalPluginHost };
            var exportProviders = new ExportProvider[] { hostExportProvider, ioExportProvider, parentContainer };
            FContainer = new CompositionContainer(catalog, exportProviders);
            FContainer.ComposeParts(this);
            FPlugin = PluginBase as IPluginEvaluate;
            FAutoEvaluate = nodeInfo.AutoEvaluate;
            
            // HACK: FPluginHost is null in case of WindowSwitcher and friends
            if (FPluginHost != null)
            {
                var win32Window = PluginBase as IWin32Window;
                if (win32Window != null)
                {
                    FPluginHost.Win32Window = win32Window;
                }
            }
        }
        
        public void Dispose()
        {
            foreach (var ioHandler in FCreatedHandlers.ToArray())
            {
                DestroyIOHandler(ioHandler);
            }
            FContainer.Dispose();
        }
        
        IPluginHost2 IIOFactory.PluginHost
        {
            get
            {
                return FPluginHost;
            }
        }
        
        IIOHandler IIOFactory.CreateIOHandler(Type type, IOAttribute attribute)
        {
            var io = FIORegistry.CreateIOHandler(
                type.IsGenericType ? type.GetGenericTypeDefinition() : type,
                type,
                this,
                attribute
               );
            
            if (io == null)
            {
                throw new NotSupportedException(string.Format("Can't create {0} of type '{1}'.", attribute, type));
            }
            
            if (io.NeedsPreEvaluation)
            {
//				HookPluginNode();
                FPreHandlers.Add(io);
            }
            if (io.NeedsPostEvaluation)
            {
//				HookPluginNode();
                FPostHandlers.Add(io);
            }
            if (io.NeedsConfiguration)
            {
//				HookPluginNode();
                FConfigHandlers.Add(io);
            }
            
            FCreatedHandlers.Add(io);
            
            return io;
        }
        
        public bool CanCreateIOHandler(Type type, IOAttribute attribute)
        {
            if (!FIORegistry.CanCreate(type, attribute))
            {
                if (type.IsGenericType)
                {
                    var openGenericType = type.GetGenericTypeDefinition();
                    return FIORegistry.CanCreate(openGenericType, attribute);
                }
                
                return false;
            }
            
            return true;
        }
        
        // HACK: IIOHandler should implement IDisposable
        public void DestroyIOHandler(IIOHandler io_interface)
        {
            // HACK: Remove this cast
            var io = (IOHandler) io_interface;
            
            if (io.NeedsPreEvaluation)
            {
                FPreHandlers.Remove(io);
            }
            if (io.NeedsPostEvaluation)
            {
                FPostHandlers.Remove(io);
            }
            if (io.NeedsConfiguration)
            {
                FConfigHandlers.Remove(io);
            }
            
            FCreatedHandlers.Remove(io);
            
            var disposableIO = io as IDisposable;
            if (disposableIO != null)
            {
                disposableIO.Dispose();
            }
            
            var pluginIO = io.Metadata as IPluginIO;
            if (pluginIO != null)
            {
                FPluginHost.DeletePin(pluginIO);
            }
        }
        
        void IPlugin.SetPluginHost(IPluginHost Host)
        {
            throw new NotImplementedException();
        }
        
        void IPlugin.Configurate(IPluginConfig configPin)
        {
            foreach (var config in FConfigHandlers)
            {
                if (config.Metadata == configPin)
                {
                    config.Configurate();
                    break;
                }
            }
        }
        
        void IPlugin.Evaluate(int SpreadMax)
        {
            foreach (var input in FPreHandlers)
            {
                input.PreEvaluate();
            }
            
            // HACK: Can we remove this? Maybe by seperating...
            if (FPlugin != null)
            {
                FPlugin.Evaluate(SpreadMax);
            }
            
            foreach (var output in FPostHandlers)
            {
                output.PostEvaluate();
            }
        }
        
        bool IPlugin.AutoEvaluate
        {
            get
            {
                return FAutoEvaluate;
            }
        }
        
        void IPluginConnections.ConnectPin(IPluginIO pin)
        {
            // TODO: Implement this
        }
        
        void IPluginConnections.DisconnectPin(IPluginIO pin)
        {
            // TODO: Implement this
        }
    }
}
