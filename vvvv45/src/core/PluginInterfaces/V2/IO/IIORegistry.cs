using System;

namespace VVVV.PluginInterfaces.V2
{
	public interface IIORegistry
	{
		void Register(IIORegistry registry);
		bool CanCreate(Type ioType, IOAttribute attribute);
		IIOHandler CreateIOHandler(Type ioType, IIOFactory factory, IOAttribute attribute);
	}
}
