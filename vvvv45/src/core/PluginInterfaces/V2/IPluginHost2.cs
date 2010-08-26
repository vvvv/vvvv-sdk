using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
    [Guid("21230B31-1929-44F8-B8C0-03E5C2AA42EF"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginHost2 : IPluginHost
	{
		IPluginBase Plugin
		{
			get;
			set;
		}
	}
	
	#region IEffectHost
    [Guid("5A7B81D4-3548-4E4C-B3F5-44A50E3C8E1B"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEffectHost : IAddonHost
	{
		void SetEffect(string filename, string code);
	    string GetParameterDescription();
		string GetErrors();	    
	}
	#endregion IEffectHost
}
