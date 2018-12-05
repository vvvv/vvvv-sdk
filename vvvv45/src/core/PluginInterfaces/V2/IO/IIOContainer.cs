using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.Streams;

namespace VVVV.PluginInterfaces.V2
{
    /// <summary>
    /// IO containers are used by the IO registry and IO factory to create and
    /// destroy all different types of in- and output classes like for example
    /// spreads and streams. Each of those classes are wrapped by an IO container
    /// in order to have one common interface to deal with in the registration
    /// process and to relieve those classes from additional dependencies to
    /// the plugin interface.
    /// </summary>
    [ComVisible(false)]
    public interface IIOContainer : IDisposable
    {
        /// <summary>
        /// Gets the io object this container holds on to.
        /// </summary>
        object RawIOObject { get; }
               
        /// <summary>
        /// Gets the base io container, whose io object is used by the
        /// io object this container holds on to.
        /// </summary>
        IIOContainer BaseContainer { get; }
        
        /// <summary>
        /// Gets the io factory which was used to create this container.
        /// </summary>
        IIOFactory Factory { get; }

        IIOContainer[] AssociatedContainers { get; }
    }
    
    [ComVisible(false)]
    public interface IIOContainer<out T> : IIOContainer
    {
        /// <summary>
        /// Gets the io object this container holds on to.
        /// </summary>
        T IOObject
        {
            get;
        }
    }

    /// <summary>
    /// Implemented by spreads and streams which either use a bin size pin or are a pin group.
    /// </summary>
    [ComVisible(false)]
    public interface IIOMultiPin
    {
        IIOContainer BaseContainer { get; }
        IIOContainer[] AssociatedContainers { get; }
    }

    [ComVisible(false)]
    public static class IOContainerExtensions
    {
        /// <summary>
        /// Returns the native plugin io interface if any.
        /// </summary>
        public static IPluginIO GetPluginIO(this IIOContainer container)
        {
            IPluginIO pluginIO = null;
            while (container != null)
            {
                pluginIO = container.RawIOObject as IPluginIO;
                if (pluginIO != null) break;
                container = container.BaseContainer;
            }
            return pluginIO;
        }

        /// <summary>
        /// Returns the native plugin io interfaces if any.
        /// </summary>
        public static IEnumerable<IPluginIO> GetPluginIOs(this IIOContainer container)
        {
            var pluginIO = container.RawIOObject as IPluginIO;
            if (pluginIO != null)
                yield return pluginIO;
            var baseContainer = container.BaseContainer;
            if (baseContainer != null)
            {
                foreach (var p in baseContainer.GetPluginIOs())
                    yield return p;
            }
            else
            {
                var containers = container.AssociatedContainers;
                if (containers != null)
                    foreach (var c in containers)
                        foreach (var p in c.GetPluginIOs())
                            yield return p;
            }
        }
    }
}
