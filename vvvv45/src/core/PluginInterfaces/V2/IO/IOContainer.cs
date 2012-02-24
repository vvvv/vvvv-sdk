using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
    public abstract class IOContainer
    {
        public static IIOContainer Create<TIOObject>(
            TIOObject iOObject,
            IPluginIO metadata,
            Action<TIOObject> preEvalAction = null,
            Action<TIOObject> postEvalAction = null,
            Action<TIOObject> configAction = null
           )
        {
            // We need to construct with runtime type here, as compiletime type might be too simple (cast exceptions later).
            var ioHandlerType = typeof(IOContainer<>).MakeGenericType(iOObject.GetType());
            return (IIOContainer) Activator.CreateInstance(ioHandlerType, iOObject, metadata, preEvalAction, postEvalAction, configAction);
//			return new IOHandler<TIOObject>(iOObject, metadata, preEvalAction, postEvalAction, configAction);
        }
    }
    
    [ComVisible(false)]
    public class IOContainer<TIOObject> : IOContainer, IIOContainer<TIOObject>
    {
        private readonly Action<TIOObject> PreEvalAction;
        private readonly Action<TIOObject> PostEvalAction;
        private readonly Action<TIOObject> ConfigurateAction;
        public object RawIOObject { get; private set; }
        public IPluginIO PluginIO { get; private set; }
        public TIOObject IOObject { get; private set; }
        
        public IOContainer(
            TIOObject iOObject,
            IPluginIO pluginIO,
            Action<TIOObject> preEvalAction,
            Action<TIOObject> postEvalAction,
            Action<TIOObject> configAction)
        {
            RawIOObject = iOObject;
            PluginIO = pluginIO;
            IOObject = iOObject;
            PreEvalAction = preEvalAction;
            PostEvalAction = postEvalAction;
            ConfigurateAction = configAction;
        }
        
        public virtual void Dispose()
        {
            if (PluginIO != null)
            {
                var pluginHost = PluginIO.PluginHost;
                pluginHost.DeletePin(PluginIO);
            }
        }
        
        public IOAction ActionFlags
        {
            get
            {
                var result = IOAction.None;
                if (PreEvalAction != null)
                    result |= IOAction.Sync;
                if (PostEvalAction != null)
                    result |= IOAction.Flush;
                if (ConfigurateAction != null)
                    result |= IOAction.Config;
                return result;
            }
        }
        
        public bool Sync()
        {
            PreEvalAction(IOObject);
        }
        
        public void Flush()
        {
            PostEvalAction(IOObject);
        }
        
        public void Configurate()
        {
            ConfigurateAction(IOObject);
        }
    }
}
