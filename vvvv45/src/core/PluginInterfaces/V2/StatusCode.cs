using System;

namespace VVVV.PluginInterfaces.V2
{
	[Flags]
	public enum StatusCode
	{
		None = 0,
		IsMissing = 1,
		IsBoygrouped = 2,
		IsConnected = 4,
		HasInvalidData = 8,
		HasRuntimeError = 16
	}
}

