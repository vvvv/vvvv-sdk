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
	
	/// <summary>
	/// Optional interface to be implemented on a plugin that deals with Configuration pins
	/// </summary>
	[Guid("7D8F77BF-3CF3-487B-89B7-9D203E58A59E"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginConfigurate
	{
		/// <summary>
		/// Called by the PluginHost before the Evaluate function every frame for every ConfigurationPin that has changed. 
		/// The ConfigurationPin is handed over as the functions input parameter. This is where a plugin would typically 
		/// create/delete pins as reaction to the changed value of a ConfigurationPin that specifies the number of pins of a specific type.
		/// </summary>
		/// <param name="input">Interface to the ConfigurationPin for which the function is called.</param>
		void Configurate(IPluginConfig input);
	}
	#endregion basic interfaces
}
