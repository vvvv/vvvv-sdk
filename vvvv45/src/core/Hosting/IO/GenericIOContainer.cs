using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.IO
{
    [ComVisible(false)]
    static class GenericIOContainer
    {
        public static IIOContainer Create<T>(
            IOBuildContext context,
            IIOFactory factory,
            T ioObject,
            Action<T> syncAction = null,
            Action<T> flushAction = null
           )
        {
            // We need to construct with runtime type here, as compiletime type might be too simple (cast exceptions later).
            var ioHandlerType = typeof(GenericIOContainer<>).MakeGenericType(ioObject.GetType());
            return (IIOContainer) Activator.CreateInstance(ioHandlerType, context, factory, ioObject, syncAction, flushAction);
        }
    }
    
    [ComVisible(false)]
    class GenericIOContainer<T> : IOContainerBase, IIOContainer<T>
        where T : class
    {
        private readonly Action<T> SyncAction;
        private readonly Action<T> FlushAction;
        
        public GenericIOContainer(
            IOBuildContext context,
            IIOFactory factory,
            T ioObject,
            Action<T> syncAction = null,
            Action<T> flushAction = null
           )
            : base(context, factory, ioObject)
        {
            IOObject = ioObject;
            SyncAction = syncAction;
            FlushAction = flushAction;
            
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
            }
        }
        
        public override void Dispose()
        {
            Factory.Synchronizing -= HandleSynchronizing;
            Factory.Flushing -= HandleFlushing;
            var disposableIOObject = RawIOObject as IDisposable;
            if (disposableIOObject != null)
            {
                disposableIOObject.Dispose();
            }
            base.Dispose();
        }
        
        public T IOObject
        {
            get;
            private set;
        }

        void HandleSynchronizing(object sender, EventArgs e)
        {
            SyncAction(IOObject);
        }
        
        void HandleFlushing(object sender, EventArgs e)
        {
            FlushAction(IOObject);
        }
    }
}
