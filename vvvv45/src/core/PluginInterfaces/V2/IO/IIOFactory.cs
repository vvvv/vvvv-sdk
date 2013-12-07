using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.Streams;

namespace VVVV.PluginInterfaces.V2
{
    /// <summary>
    /// An io factory provides methods to create io containers.
    /// </summary>
    [ComVisible(false)]
	public interface IIOFactory : IDisposable
	{
	    /// <summary>
	    /// The plugin host used to create all native interfaces.
	    /// </summary>
		IPluginHost2 PluginHost
		{
			get;
		}
		
		/// <summary>
		/// Creates a new IO container as described by the build context.
		/// </summary>
		IIOContainer CreateIOContainer(IOBuildContext context);
		
		/// <summary>
		/// Whether or not an IO container can be created for the given build context.
		/// </summary>
		bool CanCreateIOContainer(IOBuildContext context);
		
		/// <summary>
		/// The Synchronizing event takes place before the plugin gets evaluated.
		/// </summary>
		event EventHandler Synchronizing;
		
		/// <summary>
		/// The Flushing event takes place after the plugin has been evaluated.
		/// </summary>
		event EventHandler Flushing;
		
		/// <summary>
		/// The Configuring event takes place after a user changed a config pin.
		/// </summary>
		event EventHandler<ConfigEventArgs> Configuring;
		
		/// <summary>
		/// The Connected event takes place after a pin has been connected.
		/// </summary>
		event EventHandler<ConnectionEventArgs> Connected;
		
		/// <summary>
		/// The Disconnected event takes place after a pin has been disconnected.
		/// </summary>
		event EventHandler<ConnectionEventArgs> Disconnected;
		
		/// <summary>
		/// The Created event takes place after the plugin has been created.
		/// </summary>
		event EventHandler Created;
		
		/// <summary>
		/// The Disposing event takes place before the plugin is being disposed.
		/// </summary>
		event EventHandler Disposing;
	}
	
	[ComVisible(false)]
	public class ConfigEventArgs : EventArgs
	{
	    public ConfigEventArgs(IPluginConfig pluginConfig)
	    {
	        PluginConfig = pluginConfig;
	    }
	    
	    public IPluginIO PluginConfig
	    {
	        get;
	        private set;
	    }
	}
	
	[ComVisible(false)]
	public class ConnectionEventArgs : EventArgs
	{
	    public ConnectionEventArgs(IPluginIO pluginIO, IPin otherPin)
	    {
	        PluginIO = pluginIO;
	        OtherPin = otherPin;
	    }
	    
	    public IPluginIO PluginIO
	    {
	        get;
	        private set;
	    }
	    
	    public IPin OtherPin
	    {
	        get;
	        private set;
	    }
	}
	
	[ComVisible(false)]
	public static class IOFactoryExtensions
	{
		public static IIOContainer<T> CreateIOContainer<T>(this IIOFactory factory, IOAttribute attribute, bool subscribe = true)
			where T : class
		{
		    return (IIOContainer<T>) factory.CreateIOContainer(typeof(T), attribute, subscribe);
		}
		
		public static IIOContainer CreateIOContainer(this IIOFactory factory, Type ioType, IOAttribute attribute, bool subscribe = true)
		{
		    var context = IOBuildContext.Create(ioType, attribute, subscribe);
			return factory.CreateIOContainer(context);
		}
		
		public static object CreateIO(this IIOFactory factory, IOBuildContext context)
		{
			return factory.CreateIOContainer(context).RawIOObject;
		}
		
		public static object CreateIO(this IIOFactory factory, Type type, IOAttribute attribute, bool subscribe = true)
		{
		    var context = IOBuildContext.Create(type, attribute, subscribe);
			return factory.CreateIO(context);
		}
		
		public static T CreateIO<T>(this IIOFactory factory, IOAttribute attribute, bool subscribe = true)
		{
			return (T) factory.CreateIO(typeof(T), attribute, subscribe);
		}

        public static Pin<T> CreatePin<T>(this IIOFactory factory, IOAttribute attribute, bool subscribe = true)
        {
            return (Pin<T>)factory.CreateIO(typeof(Pin<T>), attribute, subscribe);
        }

        public static ISpread<T> CreateSpread<T>(this IIOFactory factory, IOAttribute attribute, bool subscribe = true)
        {
            return (ISpread<T>)factory.CreateIO(typeof(ISpread<T>), attribute, subscribe);
        }

        public static IDiffSpread<T> CreateDiffSpread<T>(this IIOFactory factory, IOAttribute attribute, bool subscribe = true)
        {
            return (IDiffSpread<T>)factory.CreateIO(typeof(IDiffSpread<T>), attribute, subscribe);
        }

        public static IIOContainer<IInStream<int>> CreateBinSizeInput(this IIOFactory factory, InputAttribute attribute)
        {
            return factory.CreateIOContainer<IInStream<int>>(attribute, false);
        }

        public static IIOContainer<IOutStream<int>> CreateBinSizeOutput(this IIOFactory factory, OutputAttribute attribute)
        {
            return factory.CreateIOContainer<IOutStream<int>>(attribute, false);
        }

        public static ISpread<ISpread<T>> CreateBinSizeSpread<T>(this IIOContainer<IInStream<int>> binSizePin, InputAttribute attribute, bool subscribe = true)
        {
            var factory = binSizePin.Factory;
            var context = IOBuildContext.Create(typeof(ISpread<ISpread<T>>), attribute, subscribe);
            context.BinSizeIOContainer = binSizePin;
            return factory.CreateIO(context) as ISpread<ISpread<T>>;
        }

        public static ISpread<ISpread<T>> CreateBinSizeSpread<T>(this IIOContainer<IOutStream<int>> binSizePin, OutputAttribute attribute, bool subscribe = true)
        {
            var factory = binSizePin.Factory;
            var context = IOBuildContext.Create(typeof(ISpread<ISpread<T>>), attribute, subscribe);
            context.BinSizeIOContainer = binSizePin;
            return factory.CreateIO(context) as ISpread<ISpread<T>>;
        }
	}
}
