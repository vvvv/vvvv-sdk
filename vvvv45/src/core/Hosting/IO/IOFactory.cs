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
        private readonly List<IIOHandler> FCreatedHandlers = new List<IIOHandler>();
        private readonly Dictionary<IPluginIO, int> FPluginIORefCount = new Dictionary<IPluginIO, int>();
        // used by internal class PluginContainer
        internal readonly List<IIOHandler> FPreHandlers = new List<IIOHandler>();
        internal readonly List<IIOHandler> FPostHandlers = new List<IIOHandler>();
        internal readonly List<IIOHandler> FConfigHandlers = new List<IIOHandler>();
        
        public IOFactory(IInternalPluginHost pluginHost, IORegistry streamRegistry)
        {
            FPluginHost = pluginHost;
            FIORegistry = streamRegistry;
        }
        
        public void Dispose()
        {
            foreach (var ioHandler in FCreatedHandlers.ToArray())
            {
                DestroyIOHandler(ioHandler);
            }
        }
        
        public IPluginHost2 PluginHost
        {
            get
            {
                return FPluginHost;
            }
        }
        
        public IIOHandler CreateIOHandler(Type type, IOAttribute attribute, bool hookHandlers = true)
        {
            var io = FIORegistry.CreateIOHandler(
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
            var pluginIO = io.Metadata as IPluginIO;
            if (pluginIO != null)
            {
                if (FPluginIORefCount.ContainsKey(pluginIO))
                    FPluginIORefCount[pluginIO]++;
                else
                    FPluginIORefCount[pluginIO] = 1;
            }
            
            if (hookHandlers)
            {
                if (io.NeedsAction(IOActionType.Evaluating))
                {
                    FPreHandlers.Add(io);
                }
                if (io.NeedsAction(IOActionType.Evaluated))
                {
                    FPostHandlers.Add(io);
                }
                if (io.NeedsAction(IOActionType.Configuring))
                {
                    FConfigHandlers.Add(io);
                }
            }
            
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
        public void DestroyIOHandler(IIOHandler io)
        {
            if (io.NeedsAction(IOActionType.Evaluating))
            {
                FPreHandlers.Remove(io);
            }
            if (io.NeedsAction(IOActionType.Evaluated))
            {
                FPostHandlers.Remove(io);
            }
            if (io.NeedsAction(IOActionType.Configuring))
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
            var pluginIO = io.Metadata as IPluginIO;
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
