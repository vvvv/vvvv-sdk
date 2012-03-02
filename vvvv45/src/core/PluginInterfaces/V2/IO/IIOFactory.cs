using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
	public interface IIOFactory : IDisposable
	{
		IPluginHost2 PluginHost
		{
			get;
		}
		IIOContainer CreateIOContainer(IOBuildContext context);
		bool CanCreateIOContainer(IOBuildContext context);
		
		/// <summary>
		/// The Synchronizing event takes place before a node gets evaluated.
		/// </summary>
		event EventHandler Synchronizing;
		
		/// <summary>
		/// The Flushing event takes place after a node has been evaluated.
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
		/// The Disposing event takes place when calling the Dispose method.
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
			where T : class
		{
			return (T) factory.CreateIO(typeof(T), attribute, subscribe);
		}
	}
}
