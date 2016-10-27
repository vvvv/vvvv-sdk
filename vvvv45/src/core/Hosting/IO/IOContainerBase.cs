using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.IO
{
    [ComVisible(false)]
    public static class IOContainer
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
        }
    }
    
    [ComVisible(false)]
    public class IOContainer<TIOObject> : IOContainerBase, IIOContainer<TIOObject>
    {
        private readonly Action<TIOObject> SyncAction;
        private readonly Action<TIOObject> FlushAction;
        private readonly Action<TIOObject> ConfigAction;
        public TIOObject IOObject { get; private set; }
        
        public IOContainer(
            IOBuildContext context,
            TIOObject iOObject,
            IIOContainer baseIOContainer,
            Action<TIOObject> syncAction,
            Action<TIOObject> flushAction,
            Action<TIOObject> configAction)
            : base(context, baseIOContainer, iOObject)
        {
            IOObject = iOObject;
            SyncAction = syncAction;
            FlushAction = flushAction;
            ConfigAction = configAction;
            
            if (context.SubscribeToIOEvents)
            {
                if (SyncAction != null)
                {
                    Factory.Synchronizing += HandleSynchronizing;
                }
                if (FlushAction != null)
                {
                    Factory.Flushing += HandleFlushing;
                }
                if (ConfigAction != null)
                {
                    Factory.Configuring += HandleConfiguring;
                    Factory.Created += HandleCreated;
                }
            }
        }
        
        public override void Dispose()
        {
            if (SyncAction != null)
            {
                Factory.Synchronizing -= HandleSynchronizing;
            }
            if (FlushAction != null)
            {
                Factory.Flushing -= HandleFlushing;
            }
            if (ConfigAction != null)
            {
                Factory.Configuring -= HandleConfiguring;
                Factory.Created -= HandleCreated;
            }
            
            var disposableIOObject = RawIOObject as IDisposable;
            if (disposableIOObject != null)
            {
                disposableIOObject.Dispose();
            }
            
            base.Dispose();
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
            var pluginIO = this.GetPluginIO();
            if (args.PluginConfig == pluginIO)
            {
                ConfigAction(IOObject);
            }
        }
        
        private void HandleCreated(object sender, EventArgs args)
        {
            ConfigAction(IOObject);
        }
    }
    
    [ComVisible(false)]
    public abstract class IOContainerBase : IIOContainer
    {
        public IOContainerBase(IOBuildContext context, IIOFactory factory, IIOContainer baseContainer, object rawIOObject, IIOContainer[] associatedContainers)
        {
            Factory = factory;
            BaseContainer = baseContainer;
            RawIOObject = rawIOObject;
            AssociatedContainers = associatedContainers;
            if (context.SubscribeToIOEvents)
            {
                Factory.Disposing += HandleFactoryDisposing;
            }
        }

        public IOContainerBase(IOBuildContext context, IIOContainer baseContainer, object rawIOObject)
            : this(context, baseContainer.Factory, baseContainer, rawIOObject, null)
        {
            
        }
        
        public IOContainerBase(IOBuildContext context, IIOFactory factory, object rawIOObject)
            : this(context, factory, null, rawIOObject, null)
        {
            
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
        
        public IIOFactory Factory
        {
            get;
            private set;
        }

        public IIOContainer[] AssociatedContainers
        {
            get;
            private set;
        }
        
        public virtual void Dispose()
        {
            Factory.Disposing -= HandleFactoryDisposing;
            /*
            AdditionalContainers are null except for GenericIOContainers
            on multi pin types, which handle all their pin disposing internally already
            so only call BaseContainer.Dispose on single pin types
            */
            if (BaseContainer != null && AssociatedContainers == null)
            {
                BaseContainer.Dispose();
            }
        }
        
        void HandleFactoryDisposing(object sender, EventArgs e)
        {
            Dispose();
        }
    }
}
