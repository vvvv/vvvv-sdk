using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.IO
{
    class PluginIOContainer : IOContainerBase, IIOContainer<IPluginIO>
    {
        private readonly IPluginHost FPluginHost;
        
        public PluginIOContainer(IOBuildContext context, IIOFactory factory, IPluginIO pluginIO)
            : base(context, factory, pluginIO)
        {
            FPluginHost = factory.PluginHost;
            IOObject = pluginIO;
        }
        
        public IPluginIO IOObject 
        {
            get;
            private set;
        }
        
        public override void Dispose()
        {
            FPluginHost.DeletePin(IOObject);
            base.Dispose();
        }
    }
}
