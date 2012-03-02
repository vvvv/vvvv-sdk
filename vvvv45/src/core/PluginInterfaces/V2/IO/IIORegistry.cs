using System;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
	public interface IIORegistry
	{
		void Register(IIORegistry registry);
		bool CanCreate(IOBuildContext context);
		IIOContainer CreateIOContainer(IIOFactory factory, IOBuildContext context);
	}
}
