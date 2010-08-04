using System;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// Creates a set of Pins of a specific type
	/// </summary>
	public interface ISpreadList<T>
	{
		ISpread<T>[] Spreads { get; }
	}
}
