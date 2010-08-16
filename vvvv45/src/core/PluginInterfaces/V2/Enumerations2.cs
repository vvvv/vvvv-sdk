using VVVV.PluginInterfaces.V1;
namespace VVVV.PluginInterfaces.V2
{
   #region enums
	/// <summary>
	/// Used in <see cref="VVVV.PluginInterfaces.V1.INodeInfo">INodeInfo</see> to specify the type of the provided node.
	/// </summary>
	public enum TNodeType 
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
		Dynamic
	};
	
	/// <summary>
	/// Used in the pin creating functions of <see cref="VVVV.PluginInterfaces.V1.IPluginHost">IPluginHost</see> to specifiy possible SliceCounts.
	/// </summary>
	public enum TWindowType 
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
		Plugin,
	    /// <summary>
		/// A HDE window.
		/// </summary>
		HDE
	};
	
	/// <summary>
	/// Used to define a specific string type for pin creation
	/// </summary>
	public enum StringType
	{
		/// <summary>
		/// Default string pin type
		/// </summary>
		String,
		/// <summary>
		/// Filename pin type, used with the FileMask property
		/// </summary>
		Filename,
		/// <summary>
		/// Directory pin type
		/// </summary>
		Directory,
		/// <summary>
		/// URL pin type
		/// </summary>
		URL,
		/// <summary>
		/// IP string type
		/// </summary>
		IP
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
	
	#endregion enums
}