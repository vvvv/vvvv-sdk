using System;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	public interface IEnumEntry
	{
		string Name { get; }
		int Index { get; }
	}
}
