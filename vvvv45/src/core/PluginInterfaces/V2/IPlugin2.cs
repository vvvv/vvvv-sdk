using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;

using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
using VVVV.Core.Model;

using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	#region basic interfaces
	/// <summary>
	/// Interface that identifies HDE plugins 
	/// </summary>
	[Guid("69BD2770-3E93-4088-8622-0D39DE2DB013"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginHDE: IPluginBase
	{}
	
	/// <summary>
	/// Basic interface to provide a plugin with an Evaluate function
	/// </summary>
	[Guid("5BDEF445-5734-427C-BD9C-A69809277799"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginEvaluate: IPluginBase
	{
		/// <summary>
		/// Called by the PluginHost once per frame. This is where the plugin calculates and sets the SliceCounts and Values
		/// of its outputs depending on the values of its current inputs.
		/// </summary>
		/// <param name="SpreadMax">The maximum SliceCount of all of the plugins inputs, which would typically be used
		/// to adjust the SliceCounts of all outputs accordingly.</param>
		void Evaluate(int SpreadMax);
	}
	#endregion basic interfaces
}
