using System;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
	public sealed class ConfigAttribute : PinAttribute
	{
		public ConfigAttribute(string name)
			:base(name)
		{
		}
		
		public override object Clone()
		{
			return base.Clone(new ConfigAttribute(Name));
		}
	}
}
