using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Hosting.Interfaces
{
	[Guid("3F072669-BECD-4E5B-9F6F-872399EDEE6D"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginNodeListener
	{
		void BeforeEvaluate();
		void AfterEvaluate();
		void Configurate(IPluginConfig configPin);
	}
}
