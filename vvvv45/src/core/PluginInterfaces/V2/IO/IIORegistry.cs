using System;

namespace VVVV.PluginInterfaces.V2
{
	public interface IIORegistry
	{
		void Register(IIORegistry registry);
		bool CanCreate(Type ioType, IOAttribute attribute);
		IIOContainer CreateIOContainer(Type ioType, IIOFactory factory, IOAttribute attribute);
	}
}
