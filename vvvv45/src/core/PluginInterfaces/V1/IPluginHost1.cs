using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;

using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
using VVVV.Core.Model;

namespace VVVV.PluginInterfaces.V1
{
    #region IAddonHost
    /// <summary>
    /// The base interface of all addon hosts
    /// </summary>
    [Guid("3184506E-67F5-4D17-A2D0-7886548A2FBF"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAddonHost
    {}
    #endregion IAddonHost
    
	#region IPluginHost
	/// <summary>
	/// The interface to be implemented by a program to host IPlugins.
	/// </summary>
	[Guid("E72C5CF0-4738-4F20-948E-83E96D4E7843"), 
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginHost: IAddonHost
	{
		/// <summary>
		/// Creates a ConfigurationPin of type Value.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="Dimension">The pins dimension count. Valid values: 1, 2, 3 or 4</param>
		/// <param name="DimensionNames">Optional. An individual suffix to the pins Dimensions.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IValueConfig interface.</param>
		void CreateValueConfig(string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueConfig Pin);
		/// <summary>
		/// Creates an InputPin of type Value. Use this as opposed to <see cref="CreateValueFastInput(string, int, string[], TSliceMode, TPinVisibility, out IValueFastIn)">CreateValueFastInput</see>
		/// if you need to be able to ask for <see cref="VVVV.PluginInterfaces.V1.IPluginIn.PinIsChanged">IPluginIn.PinIsChanged</see>. May be slow with large SpreadCounts.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="Dimension">The pins dimension count. Valid values: 1, 2, 3 or 4</param>
		/// <param name="DimensionNames">Optional. An individual suffix to the pins Dimensions.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IValueIn interface.</param>
		void CreateValueInput(string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueIn Pin);
		/// <summary>
		/// Creates an InputPin of type Value that does not implement <see cref="VVVV.PluginInterfaces.V1.IPluginIn.PinIsChanged">IPluginIn.PinIsChanged</see> and is therefore faster with large SpreadCounts.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="Dimension">The pins dimension count. Valid values: 1, 2, 3 or 4</param>
		/// <param name="DimensionNames">Optional. An individual suffix to the pins Dimensions.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IValueFastIn interface.</param>
		void CreateValueFastInput(string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueFastIn Pin);
		/// <summary>
		/// Creates an OutputPin of type Value.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="Dimension">The pins dimension count. Valid values: 1, 2, 3 or 4</param>
		/// <param name="DimensionNames">Optional. An individual suffix to the pins Dimensions.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IValueOut interface.</param>
		void CreateValueOutput(string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueOut Pin);
		/// <summary>
		/// Creates a ConfigurationPin of type String.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IStringConfig interface.</param>
		void CreateStringConfig(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IStringConfig Pin);
		/// <summary>
		/// Creates an InputPin of type String.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IStringIn interface.</param>
		void CreateStringInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IStringIn Pin);
		/// <summary>
		/// Creates an OutputPin of type String.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IStringIn interface.</param>
		void CreateStringOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IStringOut Pin);
		/// <summary>
		/// Creates a ConfigurationPin of type Color.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IColorConfig interface.</param>
		void CreateColorConfig(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IColorConfig Pin);
		/// <summary>
		/// Creates an InputPin of type Color.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IColorIn interface.</param>
		void CreateColorInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IColorIn Pin);
		/// <summary>
		/// Creates an OutputPin of type Color.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IColorOut interface.</param>
		void CreateColorOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IColorOut Pin);
		/// <summary>
		/// Creates a ConfigurationPin of type Enum.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IEnumConfig interface.</param>
		void CreateEnumConfig(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IEnumConfig Pin);
		/// <summary>
		/// Creates a InputPin of type Enum.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IEnumIn interface.</param>
		void CreateEnumInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IEnumIn Pin);
		/// <summary>
		/// Creates a OutputPin of type Enum.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IEnumOut interface.</param>
		void CreateEnumOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IEnumOut Pin);
		/// <summary>
		/// Creates an InputPin of type Transform.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created ITransformIn interface.</param>
		void CreateTransformInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out ITransformIn Pin);
		/// <summary>
		/// Creates an OutputPin of type Transform.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created ITransformOut interface.</param>
		void CreateTransformOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out ITransformOut Pin);
        /// <summary>
        /// Creates an InputPin of type Raw.
        /// </summary>
        /// <param name="Name">The pins name.</param>
        /// <param name="SliceMode">The pins SliceMode.</param>
        /// <param name="Visibility">The pins initial visibility.</param>
        /// <param name="Pin">Pointer to the created IRawIn interface.</param>
        void CreateRawInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IRawIn Pin);
        /// <summary>
        /// Creates an OutputPin of type Raw.
        /// </summary>
        /// <param name="Name">The pins name.</param>
        /// <param name="SliceMode">The pins SliceMode.</param>
        /// <param name="Visibility">The pins initial visibility.</param>
        /// <param name="Pin">Pointer to the created IRawOut interface.</param>
        void CreateRawOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IRawOut Pin);
		/// <summary>
		/// Creates an InputPin of the generic node type.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created INodeIn interface.</param>
		void CreateNodeInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out INodeIn Pin);
		/// <summary>
		/// Creates an OutputPin of the generic node type.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created INodeIn interface.</param>
		void CreateNodeOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out INodeOut Pin);
		/// <summary>
		/// Creates an OutputPin of type DirectX Mesh.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IDXMeshIO interface.</param>
		void CreateMeshOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IDXMeshOut Pin);
		/// <summary>
		/// Creates an OutputPin of type DirectX Texture.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IDXTextureOut interface.</param>
		void CreateTextureOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IDXTextureOut Pin);
		/// <summary>
		/// Creates an OutputPin of type DirectX Layer.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IDXLayerIO interface.</param>
		void CreateLayerOutput(string Name, TPinVisibility Visibility, out IDXLayerIO Pin);
		/// <summary>
		/// Creates an InputPin of type DirectX RenderState.
		/// </summary>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IDXRenderStateIO interface.</param>
		void CreateRenderStateInput(TSliceMode SliceMode, TPinVisibility Visibility, out IDXRenderStateIn Pin);
		/// <summary>
		/// Creates an InputPin of type DirectX SamplerState.
		/// </summary>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IDXRenderStateIO interface.</param>
		void CreateSamplerStateInput(TSliceMode SliceMode, TPinVisibility Visibility, out IDXSamplerStateIn Pin);
		/// <summary>
		/// Deletes the given pin from the plugin
		/// </summary>
		/// <param name="Pin">The pin to be deleted</param>
		void DeletePin(IPluginIO Pin);
		/// <summary>
		/// Returns the current time which the plugin should use if it does timebased calculations.
		/// </summary>
		/// <param name="CurrentTime">The hosts current time.</param>
		void GetCurrentTime(out double CurrentTime);
		/// <summary>
		/// Returns the absolut file path to the plugins host.
		/// </summary>
		/// <param name="Path">Absolut file path to the plugins host (i.e path to the patch the plugin is placed in, in vvvv).</param>
		void GetHostPath(out string Path);
		/// <summary>
		/// Returns a slash-separated path of node IDs that uniquely identifies this node in the vvvv graph.
		/// </summary>
		/// <param name="UseDescriptiveNames">If TRUE descriptive node names are used where available instead of the node ID.</param>
		/// <param name="Path">Slash-separated path of node IDs that uniquely identifies this node in the vvvv graph.</param>
		void GetNodePath(bool UseDescriptiveNames, out string Path);
		/// <summary>
		/// Allows a plugin to write messages to a console on the host (ie. Renderer (TTY) in vvvv).
		/// </summary>
		/// <param name="Type">The type of message. Depending on the setting of this parameter the PluginHost can handle messages differently.</param>
		/// <param name="Message">The message to be logged.</param>
		void Log(TLogType Type, string Message);
		/// <summary>
		/// Allows a plugin to create/update an Enum with vvvv
		/// </summary>
		/// <param name="EnumName">The Enums name.</param>
		/// <param name="Default">The Enums default value.</param>
		/// <param name="EnumEntries">An array of strings that specify the enums entries.</param>
		void UpdateEnum(string EnumName, string Default, string[] EnumEntries);
		/// <summary>
		/// Returns the number of entries for a given Enum.
		/// </summary>
		/// <param name="EnumName">The name of the Enum to get the EntryCount of.</param>
		/// <param name="EntryCount">Number of entries in the Enum.</param>
		void GetEnumEntryCount(string EnumName, out int EntryCount);
		/// <summary>
		/// Returns the name of a given EnumEntry of a given Enum.
		/// </summary>
		/// <param name="EnumName">The name of the Enum to get the EntryName of.</param>
		/// <param name="Index">Index of the EnumEntry.</param>
		/// <param name="EntryName">String representation of the EnumEntry.</param>
		void GetEnumEntry(string EnumName, int Index, out string EntryName);
        /// <summary>
        /// Triggers Evaluate() of the plugin - if not evaluated yet in that frame. 
        /// </summary>
        void Evaluate();
        /// <summary>
        /// Registers a type.
        /// </summary>
        void RegisterType(Guid guid, string friendlyName);
	}
	#endregion host
}
