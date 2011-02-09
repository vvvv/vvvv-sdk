using System;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
	public class ConfigAttribute : PinAttribute
	{
		public ConfigAttribute(string name)
			:base(name)
		{
		}
	}
}
