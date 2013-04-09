using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;

using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
using VVVV.Core.Model;

using VVVV.PluginInterfaces.V1;

/// <summary>
/// Version 2 of the VVVV PluginInterface.
///
/// To convert this to a typelib make sure AssemblyInfo.cs states: ComVisible(true).
/// Then on a commandline type:
/// <c>regasm _PluginInterfaces.dll /tlb</c>
/// This generates and registers the typelib which can then be imported e.g. via Delphi:Components:Import Component:Import Typelib
/// </summary>
namespace VVVV.PluginInterfaces.V2
{
	#region basic interfaces
	/// <summary>
	/// Basic interface to provide a plugin with an Evaluate function.
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
	
	/// <summary>
	/// Optional interface to provide a plugin with the possibility to prohibit its own deletion.
	/// </summary>
	[Guid("E66B6358-4850-41D3-B4F1-1C09F0557DD0"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IQueryDelete
	{
		/// <summary>
		/// Called by the PluginHost when the plugin is about to be deleted. The plugin can return FALSE to prohibit the deletion. 
		/// </summary>
		bool DeleteMe();
	}

    /// <summary>
    /// Implement this interface on your plugin if the Mouse (System Window)
    /// or Keyboard (System Window) nodes should output data for it.
    /// </summary>
    [Guid("E8C47417-6146-472B-BCE5-A9550AA30C3A"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IUserInputWindow
    {
        IntPtr InputWindowHandle { get; }
    }
	#endregion basic interfaces
}
