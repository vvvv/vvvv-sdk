using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;

/// <summary>
/// Version 1 of the VVVV PluginInterface
/// </summary>
/// <remarks>
/// To convert this to a typelib make sure AssemblyInfo.cs states: ComVisible(true) 
/// Then on a commandline type:
/// <c>regasm _PluginInterfaces.dll /tlb</c>
/// This generates and registers the typelib which can then be imported e.g. via Delphi:Components:Import Component:Import Typelib
/// </remarks>
namespace VVVV.PluginInterfaces.V1
{
	#region enums
	/// <summary>
	/// Used in the Pin creating functions of <see cref="T:VVVV.PluginInterfaces.V1.IPluginHost">IPluginHost</see> to specifiy possible SliceCounts.
	/// </summary>
	public enum TSliceMode {
		/// <summary>
		/// The Pin can only have one Slice.
		/// </summary>
		Single, 
		/// <summary>
		/// The Pin can have any number of Slices.
		/// </summary>
		Dynamic};
	
	/// <summary>
	/// Used to set the <see cref="P:VVVV.PluginInterfaces.V1.PluginInfo.InitialComponentMode">InitialComponentMode</see> 
	/// in <see cref="T:VVVV.PluginInterfaces.V1.PluginInfo">IPluginInfo</see> which specifies the ComponentMode 
	/// for a plugin when it is being created.
	/// </summary>
	public enum TComponentMode {
		/// <summary>
		/// The plugins GUI will initially be hidden, only its Node is visible.
		/// </summary>
		Hidden, 
		/// <summary>
		/// The plugins GUI will initially be showing in a box in the Patch.
		/// </summary>
		InABox, 
		/// <summary>
		/// The plugins GUI will initially be showing in its own window.
		/// </summary>
		InAWindow};
	
	/// <summary>
	/// Used in the Pin creating functions of <see cref="T:VVVV.PluginInterfaces.V1.IPluginHost">IPluginHost</see> to specifiy the initial visibility of the Pin.
	/// If this is not set to FALSE then the option can be changed by the user via the Inspektor.
	/// </summary>
	public enum TPinVisibility {
		/// <summary>
		/// The Pin is not visible at all.
		/// </summary>
		False, 
		/// <summary>
		/// The Pin is visible only in the Inspektor
		/// </summary>
		OnlyInspector, 
		/// <summary>
		/// The Pin is not visible on the Node, but space is reserved for it and it appears on mouseover.
		/// </summary>
		Hidden, 
		/// <summary>
		/// Default. The Pin is visible on the Node.
		/// </summary>
		True};
	
	/// <summary>
	/// Used to specifiy a Pins Direction.
	/// </summary>
	public enum TPinDirection {
		/// <summary>
		/// The Pin is a ConfigurationPin and as such only accessible via the Inspektor.
		/// </summary>
		Configuration, 
		/// <summary>
		/// The Pin is an Input to the Node.
		/// </summary>
		Input, 
		/// <summary>
		/// The Pin is an Output from the Node.
		/// </summary>
		Output};
	
	/// <summary>
	/// Used in the <see cref="M:VVVV.PluginInterfaces.V1.IPluginHost.Log(VVVV.PluginInterfaces.V1.TLogType,System.String)">IPluginHost.Log</see> function to specify the type of the log message.
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
	#endregion enums
	
	#region basic pins
	
	 //basic pin
	[Guid("D3C5CB5C-C054-4AB6-AC04-6BDB34692B25"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginIO								   
	{
		string Name{get; set;}
		int Order{get; set;}
		bool IsConnected{get;}
	}	
	
	//basic config pin
	[Guid("11FDCEBD-FFC0-415D-90D5-DA4DBBDB5B67"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginConfig: IPluginIO								   
	{
		int SliceCount{get; set;}
		string SpreadAsString{get;}
	}
	
	//basic input pin 
	[Guid("68C6F37B-1D45-4683-9FC2-BC2580187D44"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginIn: IPluginIO					
	{
		int SliceCount{get;}
		string SpreadAsString{get;}
		bool PinIsChanged{get;}
		//bool SliceIsChanged[int index]{get;}
	}
	
	//basic fast input pin 
	[Guid("9AFAD289-7C11-4296-B232-8B33FAC3E27D"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginFastIn: IPluginIO					
	{
		int SliceCount{get;}
		string SpreadAsString{get;}
	}
	
	//basic output pin 
	[Guid("67FB9F25-0579-495C-8535-28CC15F54C55"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginOut: IPluginIO					
	{
		int SliceCount{set;}
		string SpreadAsString{set;}
	}	
	
	#endregion basic pins
	
	#region value pins

	/// <summary>
	/// Interface to a ConfigurationPin of type Value.
	/// </summary>
	[Guid("46154821-A76F-4258-846D-8524957F98D4"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IValueConfig: IPluginConfig
	{
		//double this[int index]{get; set;}
		void SetValue(int Index, double Value);
		void SetValue2D(int Index, double Value1, double Value2);
		void SetValue3D(int Index, double Value1, double Value2, double Value3);
		void SetValue4D(int Index, double Value1, double Value2, double Value3, double Value4);
		void SetMatrix(int Index, Matrix4x4 Value);
		
		void GetValue(int Index, out double Value);
		void GetValue2D(int Index, out double Value1, out double Value2);
		void GetValue3D(int Index, out double Value1, out double Value2, out double Value3);
		void GetValue4D(int Index, out double Value1, out double Value2, out double Value3, out double Value4);
		void GetMatrix(int Index, out Matrix4x4 Value);			
		void GetValuePointer(out int SliceCount, out double* Value);  
		
		void SetSubType  (double Min, double Max, double StepSize, double Default, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType2D(double Min, double Max, double StepSize, double Default1, double Default2, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType3D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType4D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, double Default4, bool IsBang, bool IsToggle, bool IsInteger);
	}
	
	/// <summary>
	/// Interface to an InputPin of type Value.
	/// </summary>
	[Guid("40137258-9CDE-49F4-93BA-DE7D91007809"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IValueIn: IPluginIn				//value input pin
	{	
		void GetValue(int Index, out double Value);
		void GetValue2D(int Index, out double Value1, out double Value2);
		void GetValue3D(int Index, out double Value1, out double Value2, out double Value3);
		void GetValue4D(int Index, out double Value1, out double Value2, out double Value3, out double Value4);
		void GetMatrix(int Index, out Matrix4x4 Value);
		//attention: don't write values into buffer!
		void GetValuePointer(out int SliceCount, out double* Value); 
		
		void SetSubType  (double Min, double Max, double StepSize, double Default, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType2D(double Min, double Max, double StepSize, double Default1, double Default2, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType3D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType4D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, double Default4, bool IsBang, bool IsToggle, bool IsInteger);
	}

	/// <summary>
	/// Interface to a fast InputPin of type Value.
	/// </summary>
	[Guid("095081B7-D929-4459-83C0-18AA809E6635"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IValueFastIn: IPluginFastIn		//fast value input pin
	{	
		void GetValue(int Index, out double Value);
		void GetValue2D(int Index, out double Value1, out double Value2);
		void GetValue3D(int Index, out double Value1, out double Value2, out double Value3);
		void GetValue4D(int Index, out double Value1, out double Value2, out double Value3, out double Value4);
		void GetMatrix(int Index, out Matrix4x4 Value);			
		//attention: pointer goes to output of other node. don't write values into buffer!
		void GetValuePointer(out int SliceCount, out double* Value);
		
		void SetSubType  (double Min, double Max, double StepSize, double Default, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType2D(double Min, double Max, double StepSize, double Default1, double Default2, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType3D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType4D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, double Default4, bool IsBang, bool IsToggle, bool IsInteger);
	}
	
	/// <summary>
	/// Interface to an OutputPin of type Value.
	/// </summary>
	[Guid("B55B70E8-9C3D-408D-B9F9-A90CF8288FC7"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IValueOut: IPluginOut			//value output pin
	{
		//double this[int index]{set;}
		void SetValue(int Index, double Value);		
		void SetValue2D(int Index, double Value1, double Value2);
		void SetValue3D(int Index, double Value1, double Value2, double Value3);
		void SetValue4D(int Index, double Value1, double Value2, double Value3, double Value4);
		void SetMatrix(int Index, Matrix4x4 Value);
		void GetValuePointer(out double* Value);
		
		void SetSubType(double Min, double Max, double StepSize, double Default, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType2D(double Min, double Max, double StepSize, double Default1, double Default2, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType3D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, bool IsBang, bool IsToggle, bool IsInteger);
		void SetSubType4D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, double Default4, bool IsBang, bool IsToggle, bool IsInteger);
	}
	
	#endregion value pins
	
	#region string pins
	
	/// <summary>
	/// Interface to a ConfigurationPin of type String.
	/// </summary>
	[Guid("1FF25AD1-FBAB-4B29-8BAC-82CE53135868"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IStringConfig: IPluginConfig	 
	{
		void SetString(int Index, string Value);
		void GetString(int Index, out string Value);
		void SetSubType(string Default, bool IsFilename);
	}
	
	/// <summary>
	/// Interface to an InputtPin of type String.
	/// </summary>
	[Guid("E329D418-20DE-4D91-B060-60EF2D73A7A6"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IStringIn: IPluginIn			
	{
		//string this[int index]{get;}
		void GetString(int Index, out string Value);
		void SetSubType(string Default, bool IsFilename);
	}

	/// <summary>
	/// Interface to an OutputPin of type String.
	/// </summary>
	[Guid("EC32C616-A85F-42AC-B7D1-630E1F739D1D"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IStringOut: IPluginOut			
	{
		//string this[int index]{set;}
		void SetString(int Index, string Value);
		void SetSubType(string Default, bool IsFilename);
	}
	
	#endregion string pins
	
	#region color pins
	
	/// <summary>
	/// Interface to a ConfigurationPin of type Color.
	/// </summary>
	[Guid("BAA49637-29FA-426A-9188-86906E660D30"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IColorConfig: IPluginConfig				
	{
		void SetColor(int Index, RGBAColor Color);
		void GetColor(int Index, out RGBAColor Color);
		void SetSubType(RGBAColor Default, bool HasAlpha);
	}
	
	/// <summary>
	/// Interface to an InputPin of type Color.
	/// </summary>
	[Guid("CB6289A8-28BD-4A52-9B7A-BC1092EA2FA5"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IColorIn: IPluginIn				    
	{
		void GetColor(int Index, out RGBAColor Color);
		void SetSubType(RGBAColor Default, bool HasAlpha);
	}
	
	/// <summary>
	/// Interface to an OutputPin of type Color.
	/// </summary>
	[Guid("432CE6BA-6F57-4387-A223-D2DAFA8125F0"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IColorOut: IPluginOut		        
	{
		void SetColor(int Index, RGBAColor Color);
		void SetSubType(RGBAColor Default, bool HasAlpha);
	}
	
	#endregion color pins
	
	#region node pins
	
	/// <summary>
	/// Interface to an InputPin of type Transform.
	/// </summary>
	[Guid("605FD0B2-AD68-40B4-92E5-819599544CF2"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ITransformIn: IPluginIn				
	{
		/// <summary>
		/// Used to retrieve a Matrix from the pin at the specified Slice.
		/// </summary>
		/// <param name="Index">The index of the Slice to get the Matrix from.</param>
		/// <param name="Value">The retrieved Matrix.</param>
		void GetMatrix(int Index, out Matrix4x4 Value);
	}
	
	/// <summary>
	/// Interface to an OutputPin of type Transform.
	/// </summary>
	[Guid("AA8D6410-36E5-4EA2-AF70-66CD6321FF36"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ITransformOut: IPluginOut			
	{
		/// <summary>
		/// Used to write a Matrix to the Pin on the specified Slice.
		/// </summary>
		/// <param name="Index">The index of the Slice to set the Matrix to.</param>
		/// <param name="Value">The Matrix to set.</param>
		void SetMatrix(int Index, Matrix4x4 Value);
	}
	
	#endregion node pins
	
	#region host, plugin
	
	/// <summary>
	/// The interface to be implemented by a program to host IPlugins
	/// </summary>
	[Guid("E72C5CF0-4738-4F20-948E-83E96D4E7843"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPluginHost							
    {
    	/// <summary>
    	/// Creates a ConfigurationPin of type Value. 
    	/// </summary>
    	/// <param name="Name">The Pins name.</param>
    	/// <param name="Dimension">The Pins Dimension Count. Valid values: 1, 2, 3 or 4</param>
    	/// <param name="DimensionNames">Optional. An individual suffix to the Pins Dimensions.</param>
    	/// <param name="SliceMode">The Pins SliceMode.</param>
    	/// <param name="Visibility">The Pins initial visibility.</param>
    	/// <param name="Pin">Pointer to the created IValueConfig interface.</param>
        void CreateValueConfig(string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueConfig Pin);
        /// <summary>
        /// Creates an InputPin of type Value. Use this as opposed to <see cref="M:VVVV.PluginInterfaces.V1.IPluginHost.CreateValueFastInput(System.String,System.Int32,System.String[],VVVV.PluginInterfaces.V1.TSliceMode,VVVV.PluginInterfaces.V1.TPinVisibility,VVVV.PluginInterfaces.V1.IValueFastIn@)">CreateValueFastInput</see>
        /// if you need to be able to ask for <see cref="P:VVVV.PluginInterfaces.V1.IPluginIn.PinIsChanged">IPluginIn.PinIsChanged</see>. May be slow with large SpreadCounts.
        /// </summary>
        /// <param name="Name">The Pins name.</param>
    	/// <param name="Dimension">The Pins Dimension Count. Valid values: 1, 2, 3 or 4</param>
    	/// <param name="DimensionNames">Optional. An individual suffix to the Pins Dimensions.</param>
    	/// <param name="SliceMode">The Pins SliceMode.</param>
    	/// <param name="Visibility">The Pins initial visibility.</param>
    	/// <param name="Pin">Pointer to the created IValueIn interface.</param>
        void CreateValueInput(string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueIn Pin);
        /// <summary>
        /// Creates an InputPin of type Value that does not implement <see cref="P:VVVV.PluginInterfaces.V1.IPluginIn.PinIsChanged">IPluginIn.PinIsChanged</see> and is therefore faster with large SpreadCounts.
        /// </summary>
        /// <param name="Name">The Pins name.</param>
    	/// <param name="Dimension">The Pins Dimension Count. Valid values: 1, 2, 3 or 4</param>
    	/// <param name="DimensionNames">Optional. An individual suffix to the Pins Dimensions.</param>
    	/// <param name="SliceMode">The Pins SliceMode.</param>
    	/// <param name="Visibility">The Pins initial visibility.</param>
    	/// <param name="Pin">Pointer to the created IValueFastIn interface.</param>
        void CreateValueFastInput(string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueFastIn Pin);
        /// <summary>
        /// Creates an OutputPin of type Value.
        /// </summary>
        /// <param name="Name">The Pins name.</param>
    	/// <param name="Dimension">The Pins Dimension Count. Valid values: 1, 2, 3 or 4</param>
    	/// <param name="DimensionNames">Optional. An individual suffix to the Pins Dimensions.</param>
    	/// <param name="SliceMode">The Pins SliceMode.</param>
    	/// <param name="Visibility">The Pins initial visibility.</param>
    	/// <param name="Pin">Pointer to the created IValueOut interface.</param>
        void CreateValueOutput(string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueOut Pin);
        /// <summary>
        /// Creates a ConfigurationPin of type String.
        /// </summary>
        /// <param name="Name">The Pins name.</param>
        /// <param name="SliceMode">The Pins SliceMode.</param>
        /// <param name="Visibility">The Pins initial visibility.</param>
        /// <param name="Pin">Pointer to the created IStringConfig interface.</param>
        void CreateStringConfig(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IStringConfig Pin);
        /// <summary>
        /// Creates an InputPin of type String.
        /// </summary>
        /// <param name="Name">The Pins name.</param>
        /// <param name="SliceMode">The Pins SliceMode.</param>
        /// <param name="Visibility">The Pins initial visibility.</param>
        /// <param name="Pin">Pointer to the created IStringIn interface.</param>
        void CreateStringInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IStringIn Pin);
        /// <summary>
        /// Creates an OutputPin of type String.
        /// </summary>
        /// <param name="Name">The Pins name.</param>
        /// <param name="SliceMode">The Pins SliceMode.</param>
        /// <param name="Visibility">The Pins initial visibility.</param>
        /// <param name="Pin">Pointer to the created IStringIn interface.</param>
        void CreateStringOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IStringOut Pin);
        /// <summary>
        /// Creates a ConfigurationPin of type Color.
        /// </summary>
        /// <param name="Name">The Pins name.</param>
        /// <param name="SliceMode">The Pins SliceMode.</param>
        /// <param name="Visibility">The Pins initial Visibility.</param>
        /// <param name="Pin">Pointer to the created IColorConfig interface.</param>
        void CreateColorConfig(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IColorConfig Pin);
        /// <summary>
        /// Creates an InputPin of type Color.
        /// </summary>
        /// <param name="Name">The Pins name.</param>
        /// <param name="SliceMode">The Pins SliceMode.</param>
        /// <param name="Visibility">The Pins initial Visibility.</param>
        /// <param name="Pin">Pointer to the created IColorIn interface.</param>
        void CreateColorInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IColorIn Pin);
        /// <summary>
        /// Creates an OutputPin of type Color.
        /// </summary>
        /// <param name="Name">The Pins name.</param>
        /// <param name="SliceMode">The Pins SliceMode.</param>
        /// <param name="Visibility">The Pins initial Visibility.</param>
        /// <param name="Pin">Pointer to the created IColorOut interface.</param>
        void CreateColorOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IColorOut Pin);
        /// <summary>
        /// Creates an InputPin of type Transform.
        /// </summary>
        /// <param name="Name">The Pins name.</param>
        /// <param name="SliceMode">The Pins SliceMode.</param>
        /// <param name="Visibility">The Pins initial Visibility.</param>
        /// <param name="Pin">Pointer to the created ITransformIn interface.</param>
        void CreateTransformInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out ITransformIn Pin);
        /// <summary>
        /// Creates an OutputPin of type Transform.
        /// </summary>
        /// <param name="Name">The Pins name.</param>
        /// <param name="SliceMode">The Pins SliceMode.</param>
        /// <param name="Visibility">The Pins initial Visibility.</param>
        /// <param name="Pin">Pointer to the created ITransformOut interface.</param>
        void CreateTransformOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out ITransformOut Pin);
        
        /// <summary>
        /// Deletes the given Pin from the plugin
        /// </summary>
        /// <param name="Pin">The Pin to be deleted</param>
        void DeletePin(IPluginIO Pin);
        /// <summary>
        /// Returns the current time which the plugin should use if it does timebased calculations
        /// </summary>
        /// <param name="CurrentTime"></param>
        void GetCurrentTime(out double CurrentTime);
        /// <summary>
        /// Returns the absolut file path to the plugins host
        /// </summary>
        /// <param name="Path">Absolut file path to the plugins host (i.e path to the patch the plugin is placed in, in vvvv)</param>
        void GetHostPath(out string Path);
        /// <summary>
        /// Allows a plugin to write messages to a console on the host (ie. Renderer (TTY) in vvvv) 
        /// </summary>
        /// <param name="Type">The type of message. Depending on the setting of this parameter the PluginHost can handle messages differently</param>
        /// <param name="Message">The message to be logged</param>
        void Log(TLogType Type, string Message);
    }
    
    /// <summary>
    /// The one single interface a Plugin has to implement
    /// </summary>
    [Guid("7F813C89-4EDE-4087-A626-4320BE41C87F"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPlugin								
    {
    	/// <summary>
    	/// Called by the PluginHost to hand itself over to the Plugin. This is where the Plugin creates its initial Pins.
    	/// </summary>
    	/// <param name="Host">Interface to the PluginHost.</param>
        void SetPluginHost(IPluginHost Host);
        /// <summary>
        /// Called by the PluginHost before the Evaluate function every frame for every ConfigurationPin, which is also 
        /// handed over as the functions Input parameter. This is where a plugin would create/delete pins typically as 
        /// reaction to the change of a ConfigurationPin that specifies the number of pins of a specific type.
        /// </summary>
        /// <param name="Input">Interface to the ConfigurationPin for which the function is called.</param>
        void Configurate(IPluginConfig Input);
        /// <summary>
        /// Called by the PluginHost once per frame. This is where the plugin calculates and sets the SliceCounts and Values 
        /// of its outputs depending on the values of its current inputs.
        /// </summary>
        /// <param name="SpreadMax">The maximum SliceCount of all of the plugins inputs, which would typically be used 
        /// to adjust the SliceCounts of all outputs accordingly.</param>
        void Evaluate(int SpreadMax);
        /// <summary>
        /// Called by the PluginHost only once during initialization to find out if this plugin needs to be evaluated 
        /// every frame even if there is not output connected. Typically this can return FALSE as long as the plugin doesn't have
        /// a special reason for doing otherwise.
        /// </summary>
        bool AutoEvaluate {get;}
    }
    
    #endregion host, plugin
    
	#region plugin info
    
    //plugin info interface
    [Guid("16EE5CF9-0D75-4ECF-9440-7D2909E8F7DC"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginInfo
	{
		string Name {get; set;}
		string Category {get; set;}
		string Version {get; set;}
		string Help {get; set;}
		string Tags {get; set;}
		string Author {get; set;}
		string Credits {get; set;}
		string Bugs {get; set;}
		string Warnings {get; set;}
		string Namespace {get; set;}
		string Class {get; set;}
		Size InitialWindowSize {get; set;}
		Size InitialBoxSize {get; set;}
		TComponentMode InitialComponentMode {get; set;}
	}
	
	//plugin info inplementation
	[Guid("FE1216D6-5439-416D-8FB7-16E9A29EF67B")]
	public class PluginInfo: MarshalByRefObject, IPluginInfo
	{
		private string FName = "";
		private string FCategory = "";
		private string FVersion = "";
		private string FAuthor = "";
		private string FHelp = "";
		private string FTags = "";
		private string FBugs = "";
		private string FCredits = "";
		private string FWarnings = "";
		private string FNamespace = "";
		private string FClass = "";
		private Size FInitialWindowSize = new Size(0, 0);
		private Size FInitialBoxSize = new Size(0, 0);
		private TComponentMode FInitialComponentMode = TComponentMode.Hidden;
		
	    public string Name
	    {
	        get {return FName;}
	        set {FName = value;}
		}
	    public string Category
	    {
	        get {return FCategory;}
	        set {FCategory = value;}
		}
	    public string Version
	    {
	        get {return FVersion;}
	        set {FVersion = value;}
		}
	    public string Author
	    {
	        get {return FAuthor;}
	        set {FAuthor = value;}
		}
	    public string Help
	    {
	        get {return FHelp;}
	        set {FHelp = value;}
		}
	    public string Tags
	    {
	        get {return FTags;}
	        set {FTags = value;}
		}
	    public string Bugs
	    {
	        get {return FBugs;}
	        set {FBugs = value;}
		}
	    public string Credits
	    {
	        get {return FCredits;}
	        set {FCredits = value;}
		}
	    public string Warnings
	    {
	        get {return FWarnings;}
	        set {FWarnings = value;}
		}
	    public string Namespace
	    {
	        get {return FNamespace;}
	        set {FNamespace = value;}
		}
	    public string Class
	    {
	        get {return FClass;}
	        set {FClass = value;}
		}
	    public Size InitialWindowSize
	    {
	        get {return FInitialWindowSize;}
	        set {FInitialWindowSize = value;}
		}
	    public Size InitialBoxSize
	    {
	        get {return FInitialBoxSize;}
	        set {FInitialBoxSize = value;}
		}
	    public TComponentMode InitialComponentMode
	    {
	        get {return FInitialComponentMode;}
	        set {FInitialComponentMode = value;}
		}
	}
	
	#endregion plugin info
	
}
