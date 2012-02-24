using System;

namespace VVVV.PluginInterfaces.V2
{
	public interface IIOFactory : IDisposable
	{
		IPluginHost2 PluginHost
		{
			get;
		}
		IIOContainer CreateIOContainer(Type type, IOAttribute attribute, bool hookHandlers = true);
		void DestroyIOContainer(IIOContainer ioHandler);
		bool CanCreateIOContainer(Type type, IOAttribute attribute);
	}
	
	public static class IOFactoryExtensions
	{
		public static IIOContainer<T> CreateIOContainer<T>(this IIOFactory factory, IOAttribute attribute, bool hookHandlers = true)
			where T : class
		{
			return (IIOContainer<T>) factory.CreateIOContainer(typeof(T), attribute, hookHandlers);
		}
		
		public static object CreateIO(this IIOFactory factory, Type type, IOAttribute attribute, bool hookHandlers = true)
		{
			return factory.CreateIOContainer(type, attribute, hookHandlers).IOObject;
		}
		
		public static T CreateIO<T>(this IIOFactory factory, IOAttribute attribute, bool hookHandlers = true)
			where T : class
		{
			return (T) factory.CreateIO(typeof(T), attribute, hookHandlers);
		}
	}
}
