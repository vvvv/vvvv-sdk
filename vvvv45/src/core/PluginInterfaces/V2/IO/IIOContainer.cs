using System;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.Streams;

namespace VVVV.PluginInterfaces.V2
{
    /// <summary>
    /// IO containers are used by the IO registry and IO factory to create and
    /// destroy all different types of in- and output classes like for example
    /// spreads and streams. Each of those classes is wrapped by an IO container
    /// in order to have one common interface to deal with in the registration
    /// process and to relieve those classes from additional dependencies to
    /// the plugin interface.
    /// </summary>
    public interface IIOContainer : IDisposable
    {
        /// <summary>
        /// Gets the io object this container holds on to.
        /// </summary>
        object RawIOObject { get; }
               
        /// <summary>
        /// Gets the native interface which was used to create the io object
        /// this container holds on to. Some containers might return null here
        /// as their containing io object might not even use a native interface
        /// to do its work.
        /// </summary>
        IIOContainer BaseContainer { get; }
        
        event EventHandler Disposed;
    }
    
    public interface IIOContainer<out T> : IIOContainer
    {
        /// <summary>
        /// Gets the io object this container holds on to.
        /// </summary>
//        T IOObject
//        {
//            get;
//        }
    }
}
