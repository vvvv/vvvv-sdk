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
            foreach (var ioContainer in FCreatedContainers.ToArray())
            {
                ioContainer.Dispose();
            }
            FCreatedContainers.Clear();
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
            context.Factory = this;
            var io = FIORegistry.CreateIOContainer(context);
            if (io == null)
            {
                throw new NotSupportedException(string.Format("Can't create {0} of type '{1}'.", attribute, type));
            }
            
            FCreatedContainers.Add(io);
            io.Disposed += HandleDisposed;            
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
                    return FIORegistry.CanCreate(
                        new IOBuildContext()
                        {
                            DataType = context.DataType,
                            Factory = context.Factory,
                            IOAttribute = context.IOAttribute,
                            IOType = openGenericType,
                            SubscribeToIOEvents = context.SubscribeToIOEvents
                        }
                       );
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
        
        void HandleDisposed(object sender, EventArgs e)
        {
            var io = (IIOContainer) sender;
            FCreatedContainers.Remove(io);
            io.Disposed -= HandleDisposed;
        }
    }
}
