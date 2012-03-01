using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.IO
{
    [ComVisible(false)]
    static class IOContainer
    {
        public static IIOContainer Create<TIOObject>(
            IOBuildContext context,
            TIOObject iOObject,
            IIOContainer baseIOContainer,
            Action<TIOObject> preEvalAction = null,
            Action<TIOObject> postEvalAction = null,
            Action<TIOObject> configAction = null
           )
        {
            // We need to construct with runtime type here, as compiletime type might be too simple (cast exceptions later).
            var ioHandlerType = typeof(IOContainer<>).MakeGenericType(iOObject.GetType());
            return (IIOContainer) Activator.CreateInstance(ioHandlerType, context, iOObject, baseIOContainer, preEvalAction, postEvalAction, configAction);
//			return new IOHandler<TIOObject>(iOObject, metadata, preEvalAction, postEvalAction, configAction);
        }
    }
    
    [ComVisible(false)]
    class IOContainer<TIOObject> : IOContainerBase, IIOContainer<TIOObject>
    {
        private readonly Action<TIOObject> SyncAction;
        private readonly Action<TIOObject> FlushAction;
        private readonly Action<TIOObject> ConfigAction;
        private readonly IIOFactory FFactory;
        public TIOObject IOObject { get; private set; }
        
        public IOContainer(
            IOBuildContext context,
            TIOObject iOObject,
            IIOContainer baseIOContainer,
            Action<TIOObject> syncAction,
            Action<TIOObject> flushAction,
            Action<TIOObject> configAction)
            : base(baseIOContainer, iOObject)
        {
            FFactory = context.Factory;
            IOObject = iOObject;
            SyncAction = syncAction;
            FlushAction = flushAction;
            ConfigAction = configAction;
            
            if (context.SubscribeToIOEvents)
            {
                if (SyncAction != null)
                {
                    FFactory.Synchronizing += HandleSynchronizing;
                }
                if (FlushAction != null)
                {
                    FFactory.Flushing += HandleFlushing;
                }
                if (ConfigAction != null)
                {
                    FFactory.Configuring += HandleConfiguring;
                }
            }
        }
        
        protected override void OnDisposed(EventArgs e)
        {
            if (SyncAction != null)
            {
                FFactory.Synchronizing -= HandleSynchronizing;
            }
            if (FlushAction != null)
            {
                FFactory.Flushing -= HandleFlushing;
            }
            if (ConfigAction != null)
            {
                FFactory.Configuring -= HandleConfiguring;
            }
            
            base.OnDisposed(e);
        }
        
        private void HandleSynchronizing(object sender, EventArgs args)
        {
            SyncAction(IOObject);
        }
        
        private void HandleFlushing(object sender, EventArgs args)
        {
            FlushAction(IOObject);
        }
        
        private void HandleConfiguring(object sender, ConfigEventArgs args)
        {
            if (args.PluginConfig == PluginIO)
            {
                ConfigAction(IOObject);
            }
        }
    }
    
    [ComVisible(false)]
    abstract class IOContainerBase : IIOContainer
    {
        public IOContainerBase(IIOContainer baseContainer, object rawIOObject)
        {
            BaseContainer = baseContainer;
            RawIOObject = rawIOObject;
        }
        
        public IOContainerBase(object rawIOObject)
            : this(null, rawIOObject)
        {
            
        }
        
        public event EventHandler Disposed;
        
        protected virtual void OnDisposed(EventArgs e)
        {
            if (Disposed != null)
            {
                Disposed(this, e);
            }
        }
        
        public object RawIOObject
        {
            get;
            private set;
        }
        
        public IIOContainer BaseContainer
        {
            get;
            private set;
        }
        
        public void Dispose()
        {
            OnDisposed(EventArgs.Empty);
            if (BaseContainer != null)
            {
                BaseContainer.Dispose();
            }
        }
    }
}
