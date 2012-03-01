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
    class IOFactory : IIOFactory, IDisposable
    {
        private readonly IInternalPluginHost FPluginHost;
        private readonly IORegistry FIORegistry;
        private readonly List<IIOContainer> FCreatedContainers = new List<IIOContainer>();
        private readonly Dictionary<IPluginIO, int> FPluginIORefCount = new Dictionary<IPluginIO, int>();
        // used by internal class PluginContainer
        internal readonly List<IIOContainer> FSyncContainers = new List<IIOContainer>();
        internal readonly List<IIOContainer> FFlushContainers = new List<IIOContainer>();
        internal readonly List<IIOContainer> FConfigContainers = new List<IIOContainer>();
        
        public IOFactory(IInternalPluginHost pluginHost, IORegistry streamRegistry)
        {
            FPluginHost = pluginHost;
            FIORegistry = streamRegistry;
        }
        
        public void Dispose()
        {
            OnDisposing(EventArgs.Empty);
        }
        
        public IPluginHost2 PluginHost
        {
            get
            {
                return FPluginHost;
            }
        }
        
        public IIOContainer CreateIOContainer(IOBuildContext context)
        {
            var io = FIORegistry.CreateIOContainer(this, context);
            if (io == null)
            {
                throw new NotSupportedException(string.Format("Can't create container for build context '{1}'.", context));
            }
            return io;
        }

        public bool CanCreateIOContainer(IOBuildContext context)
        {
            if (!FIORegistry.CanCreate(context))
            {
                var type = context.IOType;
                if (type.IsGenericType)
                {
                    var openGenericType = type.GetGenericTypeDefinition();
                    return FIORegistry.CanCreate(context.ReplaceIOType(openGenericType));
                }
                
                return false;
            }
            
            return true;
        }
        
        public event EventHandler Synchronizing;
        
        internal void OnSynchronizing(EventArgs e)
        {
            if (Synchronizing != null) 
            {
                Synchronizing(this, e);
            }
        }
        
        public event EventHandler Flushing;
        
        internal void OnFlushing(EventArgs e)
        {
            if (Flushing != null) 
            {
                Flushing(this, e);
            }
        }
        
        public event EventHandler<ConfigEventArgs> Configuring;
        
        internal void OnConfiguring(ConfigEventArgs e)
        {
            if (Configuring != null) 
            {
                Configuring(this, e);
            }
        }
        
        public event EventHandler Disposing;
        
        protected virtual void OnDisposing(EventArgs e)
        {
            if (Disposing != null) 
            {
                Disposing(this, e);
            }
        }
    }
}
