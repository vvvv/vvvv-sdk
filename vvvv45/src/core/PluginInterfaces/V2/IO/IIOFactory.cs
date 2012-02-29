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
		IIOView CreateIOContainer(Type type, IOAttribute attribute);
		bool CanCreateIOContainer(Type type, IOAttribute attribute);
		
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
//		public static IIOView<T> CreateIOContainer<T>(this IIOFactory factory, IOAttribute attribute, bool hookHandlers = true)
//			where T : class
//		{
//			return (IIOContainer<T>) factory.CreateIOContainer(typeof(T), attribute, hookHandlers);
//		}
		
		public static object CreateIO(this IIOFactory factory, Type type, IOAttribute attribute, bool hookHandlers = true)
		{
			return factory.CreateIOContainer(type, attribute, hookHandlers).RawIOObject;
		}
		
		public static T CreateIO<T>(this IIOFactory factory, IOAttribute attribute, bool hookHandlers = true)
			where T : class
		{
			return (T) factory.CreateIO(typeof(T), attribute, hookHandlers);
		}
	}
}
