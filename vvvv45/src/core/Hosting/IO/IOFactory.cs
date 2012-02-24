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
        private readonly List<IIOContainer> FCreatedHandlers = new List<IIOContainer>();
        private readonly Dictionary<IPluginIO, int> FPluginIORefCount = new Dictionary<IPluginIO, int>();
        // used by internal class PluginContainer
        internal readonly List<IIOContainer> FPreHandlers = new List<IIOContainer>();
        internal readonly List<IIOContainer> FPostHandlers = new List<IIOContainer>();
        internal readonly List<IIOContainer> FConfigHandlers = new List<IIOContainer>();
        
        public IOFactory(IInternalPluginHost pluginHost, IORegistry streamRegistry)
        {
            FPluginHost = pluginHost;
            FIORegistry = streamRegistry;
        }
        
        public void Dispose()
        {
            foreach (var ioHandler in FCreatedHandlers.ToArray())
            {
                DestroyIOContainer(ioHandler);
            }
        }
        
        public IPluginHost2 PluginHost
        {
            get
            {
                return FPluginHost;
            }
        }
        
        public IIOContainer CreateIOContainer(Type type, IOAttribute attribute, bool hookHandlers = true)
        {
            var io = FIORegistry.CreateIOContainer(
                type,
                this,
                attribute
               );
            
            if (io == null)
            {
                throw new NotSupportedException(string.Format("Can't create {0} of type '{1}'.", attribute, type));
            }
            
            FCreatedHandlers.Add(io);
            
            // Would be much nicer if we could simply call pluginIO.AddRef()
            var pluginIO = io.PluginIO as IPluginIO;
            if (pluginIO != null)
            {
                if (FPluginIORefCount.ContainsKey(pluginIO))
                    FPluginIORefCount[pluginIO]++;
                else
                    FPluginIORefCount[pluginIO] = 1;
            }
            
            if (hookHandlers)
            {
                if (io.NeedsAction(IOAction.Sync))
                {
                    FPreHandlers.Add(io);
                }
                if (io.NeedsAction(IOAction.Flush))
                {
                    FPostHandlers.Add(io);
                }
                if (io.NeedsAction(IOAction.Config))
                {
                    FConfigHandlers.Add(io);
                }
            }
            
            return io;
        }
        
        public bool CanCreateIOContainer(Type type, IOAttribute attribute)
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
        public void DestroyIOContainer(IIOContainer io)
        {
            if (io.NeedsAction(IOAction.Sync))
            {
                FPreHandlers.Remove(io);
            }
            if (io.NeedsAction(IOAction.Flush))
            {
                FPostHandlers.Remove(io);
            }
            if (io.NeedsAction(IOAction.Config))
            {
                FConfigHandlers.Remove(io);
            }
            
            FCreatedHandlers.Remove(io);
            
            var disposableIO = io as IDisposable;
            if (disposableIO != null)
            {
                disposableIO.Dispose();
            }
            
            // Would be much nicer if we could simply call pluginIO.Release()
            var pluginIO = io.PluginIO as IPluginIO;
            if (pluginIO != null)
            {
                FPluginIORefCount[pluginIO]--;
                if (FPluginIORefCount[pluginIO] == 0)
                {
                    FPluginIORefCount.Remove(pluginIO);
                    FPluginHost.DeletePin(pluginIO);
                }
            }
        }
    }
}
