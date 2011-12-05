using System;

namespace VVVV.PluginInterfaces.V2
{
	public interface IIOFactory : IDisposable
	{
		IPluginHost2 PluginHost
		{
			get;
		}
		IIOHandler CreateIOHandler(Type type, IOAttribute attribute);
		void DestroyIOHandler(IIOHandler ioHandler);
		bool CanCreateIOHandler(Type type, IOAttribute attribute);
	}
	
	public static class IOFactoryExtensions
	{
		public static IIOHandler<T> CreateIOHandler<T>(this IIOFactory factory, IOAttribute attribute)
			where T : class
		{
			return (IIOHandler<T>) factory.CreateIOHandler(typeof(T), attribute);
		}
		
		public static object CreateIO(this IIOFactory factory, Type type, IOAttribute attribute)
		{
			return factory.CreateIOHandler(type, attribute).RawIOObject;
		}
		
		public static T CreateIO<T>(this IIOFactory factory, IOAttribute attribute)
			where T : class
		{
			return (T) factory.CreateIO(typeof(T), attribute);
		}
	}
}
