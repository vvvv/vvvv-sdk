using System;

namespace VVVV.PluginInterfaces.V2
{
	public interface IIOFactory : IDisposable
	{
		IPluginHost2 PluginHost
		{
			get;
		}
		IIOHandler CreateIOHandler(Type type, IOAttribute attribute, bool hookHandlers = true);
		void DestroyIOHandler(IIOHandler ioHandler);
		bool CanCreateIOHandler(Type type, IOAttribute attribute);
	}
	
	public static class IOFactoryExtensions
	{
		public static IIOHandler<T> CreateIOHandler<T>(this IIOFactory factory, IOAttribute attribute, bool hookHandlers = true)
			where T : class
		{
			return (IIOHandler<T>) factory.CreateIOHandler(typeof(T), attribute, hookHandlers);
		}
		
		public static object CreateIO(this IIOFactory factory, Type type, IOAttribute attribute, bool hookHandlers = true)
		{
			return factory.CreateIOHandler(type, attribute, hookHandlers).RawIOObject;
		}
		
		public static T CreateIO<T>(this IIOFactory factory, IOAttribute attribute, bool hookHandlers = true)
			where T : class
		{
			return (T) factory.CreateIO(typeof(T), attribute, hookHandlers);
		}
	}
}
