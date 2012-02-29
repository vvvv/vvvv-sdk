using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.IO
{
    class PluginIOContainer : IOContainerBase, IIOContainer<IPluginIO>
    {
        private readonly IPluginHost FPluginHost;
        
        public PluginIOContainer(IPluginHost pluginHost, IPluginIO pluginIO)
            : base(pluginIO)
        {
            FPluginHost = pluginHost;
            IOObject = pluginIO;
        }
        
        public IPluginIO IOObject 
        {
            get;
            private set;
        }
        
        protected override void OnDisposed(EventArgs e)
        {
            base.OnDisposed(e);
            FPluginHost.DeletePin(IOObject);
        }
    }
}
