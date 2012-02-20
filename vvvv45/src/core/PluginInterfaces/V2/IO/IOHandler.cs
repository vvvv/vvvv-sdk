using System;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2
{
    public enum IOActionType
    {
        Evaluating,
        Evaluated,
        Configuring
    }
    
    public interface IIOHandler
    {
        object RawIOObject { get; }
        object Metadata { get; }
        bool NeedsAction(IOActionType actionType);
        void Action(IOActionType actionType);
    }
    
    public interface IIOHandler<out T> : IIOHandler
    {
        T IOObject
        {
            get;
        }
    }
    
    [ComVisible(false)]
    public abstract class IOHandler
    {
        public static IIOHandler Create<TIOObject>(
            TIOObject iOObject,
            object metadata,
            Action<TIOObject> preEvalAction = null,
            Action<TIOObject> postEvalAction = null,
            Action<TIOObject> configAction = null
           )
        {
            // We need to construct with runtime type here, as compiletime type might be too simple (cast exceptions later).
            var ioHandlerType = typeof(IOHandler<>).MakeGenericType(iOObject.GetType());
            return (IIOHandler) Activator.CreateInstance(ioHandlerType, iOObject, metadata, preEvalAction, postEvalAction, configAction);
//			return new IOHandler<TIOObject>(iOObject, metadata, preEvalAction, postEvalAction, configAction);
        }
    }
    
    [ComVisible(false)]
    public class IOHandler<TIOObject> : IOHandler, IIOHandler<TIOObject>
    {
        private readonly Action<TIOObject> PreEvalAction;
        private readonly Action<TIOObject> PostEvalAction;
        private readonly Action<TIOObject> ConfigurateAction;
        public object RawIOObject { get; private set; }
        public object Metadata { get; private set; }
        public TIOObject IOObject { get; private set; }
        
        public IOHandler(
            TIOObject iOObject,
            object metadata,
            Action<TIOObject> preEvalAction,
            Action<TIOObject> postEvalAction,
            Action<TIOObject> configAction)
        {
            RawIOObject = iOObject;
            Metadata = metadata;
            IOObject = iOObject;
            PreEvalAction = preEvalAction;
            PostEvalAction = postEvalAction;
            ConfigurateAction = configAction;
        }
        
        public bool NeedsAction(IOActionType actionType)
        {
            switch (actionType)
            {
                case IOActionType.Evaluating:
                    return PreEvalAction != null;
                case IOActionType.Evaluated:
                    return PostEvalAction != null;
                default:
                    return ConfigurateAction != null;
            }
        }
        
        public void Action(IOActionType actionType)
        {
            switch (actionType) 
            {
                case IOActionType.Evaluating:
                    PreEvalAction(IOObject);
                    break;
                case IOActionType.Evaluated:
                    PostEvalAction(IOObject);
                    break;
                case IOActionType.Configuring:
                    ConfigurateAction(IOObject);
                    break;
            }
        }
    }
}
