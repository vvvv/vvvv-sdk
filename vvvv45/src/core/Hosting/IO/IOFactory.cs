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
        // used by internal class PluginContainer
        internal readonly List<IOHandler> FPreHandlers = new List<IOHandler>();
        internal readonly List<IOHandler> FPostHandlers = new List<IOHandler>();
        internal readonly List<IOHandler> FConfigHandlers = new List<IOHandler>();
        
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
        
        public IIOHandler CreateIOHandler(Type type, IOAttribute attribute)
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
                FPreHandlers.Add(io);
            }
            if (io.NeedsPostEvaluation)
            {
                FPostHandlers.Add(io);
            }
            if (io.NeedsConfiguration)
            {
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
    }
}
