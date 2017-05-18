using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// Used in <see cref="VVVV.PluginInterfaces.V2.INodeInfo">INodeInfo</see> to specify the type of the provided node.
	/// </summary>
	/// 
	public enum NodeType 
	{
		/// <summary>
		/// Specifies a native node.
		/// </summary>
		Native,
		/// <summary>
		/// Specifies a patch node.
		/// </summary>
		Patch,
		/// <summary>
		/// Specifies a module node.
		/// </summary>
		Module,
		/// <summary>
		/// Specifies a freeframe node.
		/// </summary>
		Freeframe,
		/// <summary>
		/// Specifies a VST node.
		/// </summary>
		VST,
		/// <summary>
		/// Specifies an effect node.
		/// </summary>
		Effect,
		/// <summary>
		/// Specifies a static plugin node.
		/// </summary>
		Plugin,
		/// <summary>
		/// Specifies a dynamic plugin node.
		/// </summary>
		Dynamic,
        /// <summary>
        /// Specifies a VL node.
        /// </summary>
        VL,
		/// <summary>
		/// Specifies a node with some text in it (like source code for example).
		/// </summary>
		Text,
		/// <summary>
		/// Specifies a node with an unknown type.
		/// </summary>
		Unknown		    
	};
	
	public enum PinDirection
	{
		Configuration,
		Input,
		Output
	}
	
	public enum BoundsType
	{
		Node,
		Box,
		Window
	}
	
	/// <summary>
	/// Used in the pin creating functions of <see cref="VVVV.PluginInterfaces.V1.IPluginHost">IPluginHost</see> to specifiy possible SliceCounts.
	/// </summary>
	public enum WindowType 
	{
		/// <summary>
		/// A patch editor window.
		/// </summary>
		Patch,
		/// <summary>
		/// A modules window.
		/// </summary>
		Module,
		/// <summary>
		/// A code editor window.
		/// </summary>
		Editor,
	    /// <summary>
		/// A renderer window.
		/// </summary>
		Renderer,
	    /// <summary>
		/// A plugins window.
		/// </summary>
		Plugin
	};
	
	/// <summary>
	/// Used to set the <see cref="VVVV.PluginInterfaces.V1.PluginInfo.InitialComponentMode">InitialComponentMode</see>
	/// in <see cref="VVVV.PluginInterfaces.V1.PluginInfo">IPluginInfo</see> which specifies the ComponentMode
	/// for a plugin when it is being created.
	/// </summary>
	public enum ComponentMode {
		/// <summary>
		/// The plugins GUI will be hidden, only its node is visible.
		/// </summary>
		Hidden,
		/// <summary>
		/// The plugins GUI will be showing in a box in the patch.
		/// </summary>
		InABox,
		/// <summary>
		/// The plugins GUI will be showing in its own window.
		/// </summary>
		InAWindow,
		/// <summary>
		/// The plugins GUI will be showing fullscreen.
		/// </summary>
	    Fullscreen
	};
	
	/// <summary>
	/// Used to define a specific string type for pin creation
	/// </summary>
	public enum StringType
	{
		/// <summary>
		/// Default string pin type
		/// </summary>
		String = TStringType.String,
		/// <summary>
		/// Filename pin type, used with the FileMask property
		/// </summary>
		Filename = TStringType.Filename,
		/// <summary>
		/// Directory pin type
		/// </summary>
		Directory = TStringType.Directory,
		/// <summary>
		/// URL pin type
		/// </summary>
		URL = TStringType.URL,
		/// <summary>
		/// IP string type
		/// </summary>
		IP = TStringType.IP
	};
	
	/// <summary>
	/// Used in the pin creating functions of <see cref="VVVV.PluginInterfaces.V1.IPluginHost">IPluginHost</see> to specifiy possible SliceCounts.
	/// </summary>
	public enum SliceMode 
	{
		/// <summary>
		/// The pin can only have one slice.
		/// </summary>
		Single = TSliceMode.Single,
		/// <summary>
		/// The pin can have any number of slices.
		/// </summary>
		Dynamic = TSliceMode.Dynamic
	};
	
	/// <summary>
	/// Used in the pin creating functions of <see cref="VVVV.PluginInterfaces.V1.IPluginHost">IPluginHost</see> to specifiy the initial visibility of the pin.
	/// If this is not set to FALSE then the option can be changed by the user via the Inspektor.
	/// </summary>
	public enum PinVisibility 
	{
		/// <summary>
		/// The pin is not visible at all.
		/// </summary>
		False = TPinVisibility.False,
		/// <summary>
		/// The pin is visible only in the Inspektor
		/// </summary>
		OnlyInspector = TPinVisibility.OnlyInspector,
		/// <summary>
		/// The pin is not visible on the node, but space is reserved for it and it appears on mouseover.
		/// </summary>
		Hidden = TPinVisibility.Hidden,
		/// <summary>
		/// Default. The pin is visible on the node.
		/// </summary>
		True = TPinVisibility.True
	};
	
	/// <summary>
	/// The MouseButtons
	/// </summary>
	public enum Mouse_Buttons 
	{
		/// <summary>
		/// The Left MouseButton
		/// </summary>
		Left,
		/// <summary>
		/// The Middle MouseButton
		/// </summary>
		Middle,
		/// <summary>
		/// The Right MouseButton
		/// </summary>
		Right
	};
	
	/// <summary>
	/// The ModifierKeys Alt, Control and Shift
	/// </summary>
	public enum Modifier_Keys 
	{
		/// <summary>
		/// The Alt Key
		/// </summary>
		Alt = 1,
		/// <summary>
		/// The Control Key
		/// </summary>
		Control = 2,
		/// <summary>
		/// The Shift MouseButton
		/// </summary>
		Shift = 4
	};
	
	[Flags]
	public enum StatusCode
	{
		None = 0,
		IsMissing = 1,
		IsBoygrouped = 2,
		IsConnected = 4,
		HasInvalidData = 8,
		HasRuntimeError = 16,
		IsExposed = 32
	}
}