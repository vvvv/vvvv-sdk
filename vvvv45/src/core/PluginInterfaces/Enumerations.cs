namespace VVVV.PluginInterfaces.V1
{
   #region enums
	/// <summary>
	/// Used in the pin creating functions of <see cref="VVVV.PluginInterfaces.V1.IPluginHost">IPluginHost</see> to specifiy possible SliceCounts.
	/// </summary>
	public enum TSliceMode {
		/// <summary>
		/// The pin can only have one slice.
		/// </summary>
		Single,
		/// <summary>
		/// The pin can have any number of slices.
		/// </summary>
		Dynamic};
	
	/// <summary>
	/// Used to set the <see cref="VVVV.PluginInterfaces.V1.PluginInfo.InitialComponentMode">InitialComponentMode</see>
	/// in <see cref="VVVV.PluginInterfaces.V1.PluginInfo">IPluginInfo</see> which specifies the ComponentMode
	/// for a plugin when it is being created.
	/// </summary>
	public enum TComponentMode {
		/// <summary>
		/// The plugins GUI will initially be hidden, only its node is visible.
		/// </summary>
		Hidden,
		/// <summary>
		/// The plugins GUI will initially be showing in a box in the patch.
		/// </summary>
		InABox,
		/// <summary>
		/// The plugins GUI will initially be showing in its own window.
		/// </summary>
		InAWindow};
	
	/// <summary>
	/// Used in the pin creating functions of <see cref="VVVV.PluginInterfaces.V1.IPluginHost">IPluginHost</see> to specifiy the initial visibility of the pin.
	/// If this is not set to FALSE then the option can be changed by the user via the Inspektor.
	/// </summary>
	public enum TPinVisibility {
		/// <summary>
		/// The pin is not visible at all.
		/// </summary>
		False,
		/// <summary>
		/// The pin is visible only in the Inspektor
		/// </summary>
		OnlyInspector,
		/// <summary>
		/// The pin is not visible on the node, but space is reserved for it and it appears on mouseover.
		/// </summary>
		Hidden,
		/// <summary>
		/// Default. The pin is visible on the node.
		/// </summary>
		True};
	
	/// <summary>
	/// Used to specifiy a pins Direction.
	/// </summary>
	public enum TPinDirection {
		/// <summary>
		/// The pin is a ConfigurationPin and as such only accessible via the Inspektor.
		/// </summary>
		Configuration,
		/// <summary>
		/// The pin is an input to the node.
		/// </summary>
		Input,
		/// <summary>
		/// The pin is an output from the node.
		/// </summary>
		Output};
	
	/// <summary>
	/// Used in the <see cref="VVVV.PluginInterfaces.V1.IPluginHost.Log()">IPluginHost.Log</see> function to specify the type of the log message.
	/// </summary>
	public enum TLogType {
		/// <summary>
		/// Specifies a debug message.
		/// </summary>
		Debug,
		/// <summary>
		/// Specifies an ordinary message.
		/// </summary>
		Message,
		/// <summary>
		/// Specifies a warning message.
		/// </summary>
		Warning,
		/// <summary>
		/// Specifies an errormessage.
		/// </summary>
		/// 
		Error};
	
	/// <summary>
	/// Used in <see cref="VVVV.PluginInterfaces.V1.INodeInfo">INodeInfo</see> to specify the type of the provided node.
	/// </summary>
	public enum TNodeType {
		/// <summary>
		/// Specifies a native node.
		/// </summary>
		Native,
		/// <summary>
		/// Specifies a patch node that may be a module or not.
		/// </summary>
		Patch,
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
		Dynamic};
	
	/// <summary>
	/// Used in the pin creating functions of <see cref="VVVV.PluginInterfaces.V1.IPluginHost">IPluginHost</see> to specifiy possible SliceCounts.
	/// </summary>
	public enum TWindowType {
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
		HDE};
	
	#endregion enums
}