using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using VVVV.Hosting.Factories;
using VVVV.Hosting.Interfaces;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.IO
{
    public class PluginContainer : IPlugin, IDisposable
    {
        [Export(typeof(IIOFactory))]
        [Export(typeof(IOFactory))]
        private readonly IOFactory FIOFactory;
        private readonly CompositionContainer FContainer;
        private readonly IPluginEvaluate FPlugin;
        private readonly bool FAutoEvaluate;

        [Import(typeof(IPluginBase))]
        public IPluginBase PluginBase
        {
            get;
            private set;
        }

        public PluginContainer(
            IInternalPluginHost pluginHost,
            IIORegistry ioRegistry,
            CompositionContainer parentContainer,
            INodeInfoFactory nodeInfoFactory,
            DotNetPluginFactory pluginFactory,
            Type pluginType,
            INodeInfo nodeInfo
           )
        {
            FIOFactory = new IOFactory(pluginHost, ioRegistry, parentContainer, nodeInfoFactory, pluginFactory);

            var catalog = new TypeCatalog(pluginType);
            var ioExportProvider = new IOExportProvider(FIOFactory);
            var hostExportProvider = new HostExportProvider() { PluginHost = pluginHost };
            var exportProviders = new ExportProvider[] { hostExportProvider, ioExportProvider, parentContainer };
            FContainer = new CompositionContainer(catalog, exportProviders);
            FContainer.ComposeParts(this);
            FPlugin = PluginBase as IPluginEvaluate;

            FAutoEvaluate = nodeInfo.AutoEvaluate;
            FIOFactory.OnCreated(EventArgs.Empty);
        }

        public void Dispose()
        {
            FContainer.Dispose();
            FIOFactory.Dispose();
        }

        void IPlugin.SetPluginHost(IPluginHost Host)
        {
            throw new NotImplementedException();
        }

        void IPlugin.Configurate(IPluginConfig configPin)
        {
            FIOFactory.OnConfiguring(new ConfigEventArgs(configPin));
        }

        public void Evaluate(int spreadMax)
        {
            FIOFactory.OnSynchronizing(EventArgs.Empty);

            // HACK: Can we remove this? Maybe by seperating...
            if (FPlugin != null)
            {
                FPlugin.Evaluate(spreadMax);
            }

            FIOFactory.OnFlushing(EventArgs.Empty);
        }

        bool IPlugin.AutoEvaluate
        {
            get
            {
                return FAutoEvaluate;
            }
        }
    }
}
