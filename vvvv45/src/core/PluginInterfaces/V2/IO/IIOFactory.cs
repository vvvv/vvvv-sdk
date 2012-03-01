using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
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
	}
	
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
	
	public static class IOFactoryExtensions
	{
		public static IIOContainer<T> CreateIOContainer<T>(this IIOFactory factory, IOAttribute attribute, bool subscribe = true)
			where T : class
		{
		    var context = IOBuildContext.Create(typeof(T), attribute, subscribe);
			return (IIOContainer<T>) factory.CreateIOContainer(context);
		}
		
		public static object CreateIO(this IIOFactory factory, Type type, IOAttribute attribute, bool subscribe = true)
		{
		    var context = IOBuildContext.Create(type, attribute, subscribe);
			return factory.CreateIOContainer(context).RawIOObject;
		}
		
		public static T CreateIO<T>(this IIOFactory factory, IOAttribute attribute, bool subscribe = true)
			where T : class
		{
			return (T) factory.CreateIO(typeof(T), attribute, subscribe);
		}
	}
}
