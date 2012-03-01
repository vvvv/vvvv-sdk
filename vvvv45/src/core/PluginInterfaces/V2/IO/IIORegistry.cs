using System;

namespace VVVV.PluginInterfaces.V2
{
	public interface IIORegistry
	{
		void Register(IIORegistry registry);
		bool CanCreate(IOBuildContext context);
		IIOContainer CreateIOContainer(IOBuildContext context);
	}
}
