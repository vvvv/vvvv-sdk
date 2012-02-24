using System;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.Streams;

namespace VVVV.PluginInterfaces.V2
{
    [Flags]
    public enum IOAction
    {
        None = 0,
        Sync = 1,
        Flush = 2,
        Config = 4 
    }
    
    /// <summary>
    /// IO containers are used by the IO registry and IO factory to create and
    /// destroy all different types of in- and output classes like for example
    /// spreads and streams. Each of those classes is wrapped by an IO container
    /// in order to have one common interface to deal with in the registration
    /// process and to relieve those classes from additional dependencies to
    /// the plugin interface.
    /// </summary>
    public interface IIOContainer : IDisposable, ISynchronizable, IFlushable
    {
        /// <summary>
        /// Gets the io object this container holds on to.
        /// </summary>
        object IOObject { get; }
               
        /// <summary>
        /// Gets the native interface which was used to create the io object
        /// this container holds on to. Some containers might return null here
        /// as their containing io object might not even use a native interface
        /// to do its work.
        /// </summary>
        IPluginIO PluginIO { get; }
        
        /// <summary>
        /// Gets the action flags, used to determine which actions need to
        /// get called on this container. For example a spread needs to
        /// be synced and flushed, therefor a container holding on to a spread
        /// would return IOAction.Sync | IOAction.Flush here.
        /// </summary>
        IOAction ActionFlags { get; }
        
        /// <summary>
        /// The configurate action gets called when a user in vvvv changes
        /// the value of a config pin and the action flags of this container
        /// contain the IOAction.Config flag.
        /// </summary>
        void Configurate();
    }
    
    public interface IIOContainer<out T> : IIOContainer
    {
        /// <summary>
        /// Gets the io object this container holds on to.
        /// </summary>
        new T IOObject
        {
            get;
        }
    }
}
