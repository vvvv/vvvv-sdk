using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Core.Model;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using com = System.Runtime.InteropServices.ComTypes;

/// <summary>
/// Version 1 of the VVVV PluginInterface.
/// DirectX/SlimDX related parts are in a separate file: PluginDXInterface1.cs
///
/// To convert this to a typelib make sure AssemblyInfo.cs states: ComVisible(true).
/// Then on a commandline type:
/// <c>regasm _PluginInterfaces.dll /tlb</c>
/// This generates and registers the typelib which can then be imported e.g. via Delphi:Components:Import Component:Import Typelib
/// </summary>
namespace VVVV.PluginInterfaces.V1
{
	#region basic pins
	[Guid("19D25C40-AE80-4960-9847-4FECF661522B"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IConnectionHandler
	{
		bool Accepts([In, MarshalAs(UnmanagedType.IUnknown)] object source, [In, MarshalAs(UnmanagedType.IUnknown)] object sink);
        [Obsolete("Not used anymore. Now handled by IInternalHDEHost.GetInfoString.")]
		string GetFriendlyNameForSink([In, MarshalAs(UnmanagedType.IUnknown)] object sink);
        [Obsolete("Not used anymore. Now handled by IInternalHDEHost.GetInfoString.")]
        string GetFriendlyNameForSource([In, MarshalAs(UnmanagedType.IUnknown)] object source);
	}
	
	/// <summary>
	/// Base interface of all pin interfaces. Never used directly.
	/// </summary>
	[Guid("D3C5CB5C-C054-4AB6-AC04-6BDB34692B25"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown), 
	 SuppressUnmanagedCodeSecurity]
	public interface IPluginIO
	{
		/// <summary>
		/// The pins name.
		/// </summary>
		string Name{get; set;}
		/// <summary>
		/// The order property helps the node to arrange its pins visually. The higher the order, the more right the pin appears on the node.
		/// </summary>
		int Order{get; set;}
		/// <summary>
		/// Specifies whether the pin is connected in the patch or not.
		/// </summary>
		bool IsConnected{get;}
		/// <summary>
		/// Gets the plugin host which created this plugin io.
		/// </summary>
		IPluginHost PluginHost{get;}
	}
	
	/// <summary>
	/// Base interface of all ConfigurationPin interfaces. Never used directly.
	/// </summary>
	[Guid("11FDCEBD-FFC0-415D-90D5-DA4DBBDB5B67"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginConfig: IPluginIO
	{
		/// <summary>
		/// The pins SliceCount specifies the number of Values (2D Vector, String...) it carries. This is like the length of an array or list.
		/// </summary>
		int SliceCount{get; set;}
		/// <summary>
		/// Returns a String of the pins concatenated Values. Typcally used internally only to save a pins state to disk.
		/// </summary>
		string SpreadAsString{get;}
		/// <summary>
		/// Returns whether any slice of this pin has been changed in the current frame. This information is typically used to determine if
		/// further processing is needed or can be ommited.
		/// </summary>
		bool PinIsChanged{get;}
	}
	
	/// <summary>
	/// Base interface of all InputPin interfaces. Never used directly.
	/// </summary>
	[Guid("68C6F37B-1D45-4683-9FC2-BC2580187D44"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginIn: IPluginIO
	{
		/// <summary>
		/// The pins SliceCount specifies the number of Values (2D Vector, String...) it carries. This is like the length of an array or list.
		/// </summary>
		int SliceCount{get;}
		/// <summary>
		/// Returns a String of the pins concatenated Values. Typcally used internally only to save a pins state to disk.
		/// </summary>
		string SpreadAsString{get;}
		/// <summary>
		/// Returns whether any slice of this pin has been changed in the current frame. This information is typically used to determine if
		/// further processing is needed or can be ommited.
		/// </summary>
		bool PinIsChanged{get;}
        /// <summary>
        /// Validates the upstream pin for this frame. Normally this leads to the evaluation of the upstream node.
        /// </summary>
        /// <returns>Whether or not the data changed.</returns>
		bool Validate();
        /// <summary>
        /// Gets or sets whether the upstream pin gets validated automatically before calling evaluate on the plugin.
        /// </summary>
		bool AutoValidate{get;set;}
	}
	
	/// <summary>
	/// Base interface of all fast InputPin interfaces. Never used directly.
	/// </summary>
	[Guid("9AFAD289-7C11-4296-B232-8B33FAC3E27D"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown), 
	 SuppressUnmanagedCodeSecurity]
	public interface IPluginFastIn: IPluginIO
	{
		/// <summary>
		/// The pins SliceCount specifies the number of Values (2D Vector, String...) it carries. This is like the length of an array or list.
		/// </summary>
		int SliceCount{get;}
		/// <summary>
		/// Returns a String of the pins concatenated Values. Typcally used internally only to save a pins state to disk.
		/// </summary>
		string SpreadAsString{get;}
        /// <summary>
        /// Validates the upstream pin for this frame. Normally this leads to the evaluation of the upstream node.
        /// </summary>
        /// <returns>Whether or not the data changed.</returns>
		bool Validate();
        /// <summary>
        /// Gets or sets whether the upstream pin gets validated automatically before calling evaluate on the plugin.
        /// </summary>
		bool AutoValidate{get;set;}
	}
	
	/// <summary>
	/// Base interface of all OutputPin interfaces. Never used directly.
	/// </summary>
	[Guid("67FB9F25-0579-495C-8535-28CC15F54C55"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginOut: IPluginIO
	{
		/// <summary>
		/// The pins SliceCount specifies the number of Values (2D Vector, String...) it carries. This is like the length of an array or list.
		/// </summary>
		int SliceCount{get; set;}
		/// <summary>
		/// Returns a String of the pins concatenated Values. Typcally used internally only to save a pins state to disk.
		/// </summary>
		string SpreadAsString{set;}
        /// <summary>
        /// Whether or not feedback loops are allowed on this pin.
        /// </summary>
        bool AllowFeedback { get; set; }
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
		/// <summary>
		/// Used to write a Value to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the Value to.</param>
		/// <param name="value">The Value to write.</param>
		void SetValue(int index, double value);
		/// <summary>
		/// Used to write a 2D Vector to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the 2D Vector to.</param>
		/// <param name="value1">The Value to write to the 1st dimension.</param>
		/// <param name="value2">The Value to write to the 2nd dimension.</param>
		void SetValue2D(int index, double value1, double value2);
		/// <summary>
		/// Used to write a 3D Vector to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the 3D Vector to.</param>
		/// <param name="value1">The Value to write to the 1st dimension.</param>
		/// <param name="value2">The Value to write to the 2nd dimension.</param>
		/// <param name="value3">The Value to write to the 3rd dimension.</param>
		void SetValue3D(int index, double value1, double value2, double value3);
		/// <summary>
		/// Used to write a 4D Vector to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the 4D Vector to.</param>
		/// <param name="value1">The Value to write to the 1st dimension.</param>
		/// <param name="value2">The Value to write to the 2nd dimension.</param>
		/// <param name="value3">The Value to write to the 3rd dimension.</param>
		/// <param name="value4">The Value to write to the 4th dimension.</param>
		void SetValue4D(int index, double value1, double value2, double value3, double value4);
		/// <summary>
		/// Used to write a Matrix to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the Matrix to.</param>
		/// <param name="value">The Matrix to write.</param>
		void SetMatrix(int index, Matrix4x4 value);
		
		/// <summary>
		/// Used to retrieve a Value from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the Value from.</param>
		/// <param name="value">The retrieved Value.</param>
		void GetValue(int index, out double value);
		/// <summary>
		/// Used to retrieve a 2D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the 2D Vector from.</param>
		/// <param name="value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="value2">The retrieved 2nd dimension of the Vector.</param>
		void GetValue2D(int index, out double value1, out double value2);
		/// <summary>
		/// Used to retrieve a 3D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the 3D Vector from.</param>
		/// <param name="value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="value2">The retrieved 2nd dimension of the Vector.</param>
		/// <param name="value3">The retrieved 3rd dimension of the Vector.</param>
		void GetValue3D(int index, out double value1, out double value2, out double value3);
		/// <summary>
		/// Used to retrieve a 4D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the 4D Vector from.</param>
		/// <param name="value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="value2">The retrieved 2nd dimension of the Vector.</param>
		/// <param name="value3">The retrieved 3rd dimension of the Vector.</param>
		/// <param name="value4">The retrieved 4th dimension of the Vector.</param>
		void GetValue4D(int index, out double value1, out double value2, out double value3, out double value4);
		/// <summary>
		/// Used to retrieve a 4x4 Matrix from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the 4x4 Matrix from.</param>
		/// <param name="value">The retrieved 4x4 Matrix.</param>
		void GetMatrix(int index, out Matrix4x4 value);
		/// <summary>
		/// Used to retrieve a Pointer to the Values of the pin.
		/// </summary>
		/// <param name="sliceCount">The pins current SliceCount, specifying the number of values accessible via the Pointer.</param>
		/// <param name="value">A Pointer to the pins first Value.</param>
		void GetValuePointer(out int sliceCount, out double* value);
		void GetValuePointer(out int* length, out double** dataPointer);

		/// <summary>
		/// Used to set the SubType of a Value pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="min">Minimum of the Values range.</param>
		/// <param name="max">Maximum of the Values range.</param>
		/// <param name="stepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="default">The Value the pin is initialized with and can be reset to at any time.</param>
		/// <param name="isBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="isToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="isInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType(double min, double max, double stepSize, double @default, bool isBang, bool isToggle, bool isInteger);
		/// <summary>
		/// Used to set the SubType of a 2D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="min">Minimum of the Values range.</param>
		/// <param name="max">Maximum of the Values range.</param>
		/// <param name="stepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="isBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="isToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="isInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType2D(double min, double max, double stepSize, double default1, double default2, bool isBang, bool isToggle, bool isInteger);
		/// <summary>
		/// Used to set the SubType of a 3D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="min">Minimum of the Values range.</param>
		/// <param name="max">Maximum of the Values range.</param>
		/// <param name="stepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="isBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="isToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="isInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType3D(double min, double max, double stepSize, double default1, double default2, double default3, bool isBang, bool isToggle, bool isInteger);
		/// <summary>
		/// Used to set the SubType of a 4D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="min">Minimum of the Values range.</param>
		/// <param name="max">Maximum of the Values range.</param>
		/// <param name="stepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default4">The Value the pins 4th dimension is initialized with and can be reset to at any time.</param>
		/// <param name="isBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="isToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="isInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType4D(double min, double max, double stepSize, double default1, double default2, double default3, double default4, bool isBang, bool isToggle, bool isInteger);
	}
	
	/// <summary>
	/// Interface to an InputPin of type Value.
	/// </summary>
	[Guid("40137258-9CDE-49F4-93BA-DE7D91007809"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IValueIn: IPluginIn				//value input pin
	{
		/// <summary>
		/// Used to retrieve a Value from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the Value from.</param>
		/// <param name="value">The retrieved Value.</param>
		void GetValue(int index, out double value);
		/// <summary>
		/// Used to retrieve a 2D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the 2D Vector from.</param>
		/// <param name="value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="value2">The retrieved 2nd dimension of the Vector.</param>
		void GetValue2D(int index, out double value1, out double value2);
		/// <summary>
		/// Used to retrieve a 3D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the 3D Vector from.</param>
		/// <param name="value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="value2">The retrieved 2nd dimension of the Vector.</param>
		/// <param name="value3">The retrieved 3rd dimension of the Vector.</param>
		void GetValue3D(int index, out double value1, out double value2, out double value3);
		/// <summary>
		/// Used to retrieve a 4D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the 4D Vector from.</param>
		/// <param name="value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="value2">The retrieved 2nd dimension of the Vector.</param>
		/// <param name="value3">The retrieved 3rd dimension of the Vector.</param>
		/// <param name="value4">The retrieved 4th dimension of the Vector.</param>
		void GetValue4D(int index, out double value1, out double value2, out double value3, out double value4);
		/// <summary>
		/// Used to retrieve a 4x4 Matrix from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the 4x4 Matrix from.</param>
		/// <param name="value">The retrieved 4x4 Matrix.</param>
		void GetMatrix(int index, out Matrix4x4 value);
		/// <summary>
		/// Used to retrieve a Pointer to the Values of the pin, which can be used to retrive large Spreads of Values more efficiently.
		/// Attention: Don't use this Pointer to write Values to the pin!
		/// </summary>
		/// <param name="sliceCount">The pins current SliceCount, specifying the number of values accessible via the Pointer.</param>
		/// <param name="value">A Pointer to the pins first Value.</param>
		void GetValuePointer(out int sliceCount, out double* value);
		void GetValuePointer(out int* length, out double** dataPointer);
		
		/// <summary>
		/// Used to set the SubType of a Value pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="min">Minimum of the Values range.</param>
		/// <param name="max">Maximum of the Values range.</param>
		/// <param name="stepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="default">The Value the pin is initialized with and can be reset to at any time.</param>
		/// <param name="isBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="isToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="isInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType  (double min, double max, double stepSize, double @default, bool isBang, bool isToggle, bool isInteger);
		/// <summary>
		/// Used to set the SubType of a 2D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="min">Minimum of the Values range.</param>
		/// <param name="max">Maximum of the Values range.</param>
		/// <param name="stepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="isBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="isToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="isInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType2D(double min, double max, double stepSize, double default1, double default2, bool isBang, bool isToggle, bool isInteger);
		/// <summary>
		/// Used to set the SubType of a 3D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="min">Minimum of the Values range.</param>
		/// <param name="max">Maximum of the Values range.</param>
		/// <param name="stepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="isBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="isToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="isInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType3D(double min, double max, double stepSize, double default1, double default2, double default3, bool isBang, bool isToggle, bool isInteger);
		/// <summary>
		/// Used to set the SubType of a 4D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="min">Minimum of the Values range.</param>
		/// <param name="max">Maximum of the Values range.</param>
		/// <param name="stepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default4">The Value the pins 4th dimension is initialized with and can be reset to at any time.</param>
		/// <param name="isBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="isToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="isInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType4D(double min, double max, double stepSize, double default1, double default2, double default3, double default4, bool isBang, bool isToggle, bool isInteger);
	}

	/// <summary>
	/// Interface to a fast InputPin of type Value.
	/// </summary>
	[Guid("095081B7-D929-4459-83C0-18AA809E6635"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown), 
	 SuppressUnmanagedCodeSecurity]
	unsafe public interface IValueFastIn: IPluginFastIn		//fast value input pin
	{
		/// <summary>
		/// Used to retrieve a Value from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the Value from.</param>
		/// <param name="value">The retrieved Value.</param>
		void GetValue(int index, out double value);
		/// <summary>
		/// Used to retrieve a 2D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the 2D Vector from.</param>
		/// <param name="value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="value2">The retrieved 2nd dimension of the Vector.</param>
		void GetValue2D(int index, out double value1, out double value2);
		/// <summary>
		/// Used to retrieve a 3D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the 3D Vector from.</param>
		/// <param name="value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="value2">The retrieved 2nd dimension of the Vector.</param>
		/// <param name="value3">The retrieved 3rd dimension of the Vector.</param>
		void GetValue3D(int index, out double value1, out double value2, out double value3);
		/// <summary>
		/// Used to retrieve a 4D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the 4D Vector from.</param>
		/// <param name="value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="value2">The retrieved 2nd dimension of the Vector.</param>
		/// <param name="value3">The retrieved 3rd dimension of the Vector.</param>
		/// <param name="value4">The retrieved 4th dimension of the Vector.</param>
		void GetValue4D(int index, out double value1, out double value2, out double value3, out double value4);
		/// <summary>
		/// Used to retrieve a 4x4 Matrix from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the 4x4 Matrix from.</param>
		/// <param name="value">The retrieved 4x4 Matrix.</param>
		void GetMatrix(int index, out Matrix4x4 value);
		/// <summary>
		/// Used to retrieve a Pointer to the Values of the pin, which can be used to retrive large Spreads of Values more efficiently.
		/// Attention: Don't use this Pointer to write Values to the pin!
		/// </summary>
		/// <param name="sliceCount">The pins current SliceCount, specifying the number of values accessible via the Pointer.</param>
		/// <param name="value">A Pointer to the pins first Value.</param>
		void GetValuePointer(out int sliceCount, out double* value);
		void GetValuePointer(out int* length, out double** dataPointer);
		
		/// <summary>
		/// Used to set the SubType of a Value pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="min">Minimum of the Values range.</param>
		/// <param name="max">Maximum of the Values range.</param>
		/// <param name="stepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="default">The Value the pin is initialized with and can be reset to at any time.</param>
		/// <param name="isBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="isToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="isInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType  (double min, double max, double stepSize, double @default, bool isBang, bool isToggle, bool isInteger);
		/// <summary>
		/// Used to set the SubType of a 2D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="min">Minimum of the Values range.</param>
		/// <param name="max">Maximum of the Values range.</param>
		/// <param name="stepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="isBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="isToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="isInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType2D(double min, double max, double stepSize, double default1, double default2, bool isBang, bool isToggle, bool isInteger);
		/// <summary>
		/// Used to set the SubType of a 3D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="min">Minimum of the Values range.</param>
		/// <param name="max">Maximum of the Values range.</param>
		/// <param name="stepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="isBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="isToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="isInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType3D(double min, double max, double stepSize, double default1, double default2, double default3, bool isBang, bool isToggle, bool isInteger);
		/// <summary>
		/// Used to set the SubType of a 4D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="min">Minimum of the Values range.</param>
		/// <param name="max">Maximum of the Values range.</param>
		/// <param name="stepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default4">The Value the pins 4th dimension is initialized with and can be reset to at any time.</param>
		/// <param name="isBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="isToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="isInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType4D(double min, double max, double stepSize, double default1, double default2, double default3, double default4, bool isBang, bool isToggle, bool isInteger);
	}
	
	/// <summary>
	/// Interface to an OutputPin of type Value.
	/// </summary>
	[Guid("B55B70E8-9C3D-408D-B9F9-A90CF8288FC7"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown), 
	 SuppressUnmanagedCodeSecurity]
	unsafe public interface IValueOut: IPluginOut			//value output pin
	{
		/// <summary>
		/// Used to write a Value to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the Value to.</param>
		/// <param name="value">The Value to write.</param>
		void SetValue(int index, double value);
		/// <summary>
		/// Used to write a 2D Vector to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the 2D Vector to.</param>
		/// <param name="value1">The Value to write to the 1st dimension.</param>
		/// <param name="value2">The Value to write to the 2nd dimension.</param>
		void SetValue2D(int index, double value1, double value2);
		/// <summary>
		/// Used to write a 3D Vector to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the 3D Vector to.</param>
		/// <param name="value1">The Value to write to the 1st dimension.</param>
		/// <param name="value2">The Value to write to the 2nd dimension.</param>
		/// <param name="value3">The Value to write to the 3rd dimension.</param>
		void SetValue3D(int index, double value1, double value2, double value3);
		/// <summary>
		/// Used to write a 4D Vector to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the 4D Vector to.</param>
		/// <param name="value1">The Value to write to the 1st dimension.</param>
		/// <param name="value2">The Value to write to the 2nd dimension.</param>
		/// <param name="value3">The Value to write to the 3rd dimension.</param>
		/// <param name="value4">The Value to write to the 4th dimension.</param>
		void SetValue4D(int index, double value1, double value2, double value3, double value4);
		/// <summary>
		/// Used to write a Matrix to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the Matrix to.</param>
		/// <param name="value">The Matrix to write.</param>
		void SetMatrix(int index, Matrix4x4 value);
		/// <summary>
		/// Used to retrieve a Pointer to the Values of the pin, which can be used to write large number of values more efficiently.
		/// Note though, that when writing Values to the Pointer the pins dimensions and overall SliceCount have to be taken care of manually.
		/// </summary>
		/// <param name="value">A Pointer to the pins first Value.</param>
		void GetValuePointer(out double* value);
		void GetValuePointer(out double** value);

		/// <summary>
		/// Used to set the SubType of a Value pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="min">Minimum of the Values range.</param>
		/// <param name="max">Maximum of the Values range.</param>
		/// <param name="stepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="default">The Value the pin is initialized with and can be reset to at any time.</param>
		/// <param name="isBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="isToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="isInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType  (double min, double max, double stepSize, double @default, bool isBang, bool isToggle, bool isInteger);
		/// <summary>
		/// Used to set the SubType of a 2D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="min">Minimum of the Values range.</param>
		/// <param name="max">Maximum of the Values range.</param>
		/// <param name="stepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="isBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="isToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="isInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType2D(double min, double max, double stepSize, double default1, double default2, bool isBang, bool isToggle, bool isInteger);
		/// <summary>
		/// Used to set the SubType of a 3D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="min">Minimum of the Values range.</param>
		/// <param name="max">Maximum of the Values range.</param>
		/// <param name="stepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="isBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="isToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="isInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType3D(double min, double max, double stepSize, double default1, double default2, double default3, bool isBang, bool isToggle, bool isInteger);
		/// <summary>
		/// Used to set the SubType of a 4D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="min">Minimum of the Values range.</param>
		/// <param name="max">Maximum of the Values range.</param>
		/// <param name="stepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="default4">The Value the pins 4th dimension is initialized with and can be reset to at any time.</param>
		/// <param name="isBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="isToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="isInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType4D(double min, double max, double stepSize, double default1, double default2, double default3, double default4, bool isBang, bool isToggle, bool isInteger);
	}
	

    [Guid("EB2450D2-2381-44F9-A421-B3E2540FA8B5"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IValueData
    {
        /// <summary>
        /// Used to retrieve a Value from the pin at the specified slice.
        /// </summary>
        /// <param name="index">The index of the slice to retrieve the Value from.</param>
        /// <param name="value">The retrieved Value.</param>
        void GetValue(int index, out double value);
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
		/// <summary>
		/// Used to write a String to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the String to.</param>
		/// <param name="value">The String to write.</param>
		void SetString(int index, string value);
		/// <summary>
		/// Used to retrieve a String from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the String from.</param>
		/// <param name="value">The retrieved String.</param>
		void GetString(int index, out string value);
		/// <summary>
		/// Used to set the SubType of a String pin, which is a more detailed specification of the String, used by the GUI to guide the user to insert correct Strings.
		/// Note though that this does not prevent a user from setting "wrong" Strings on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="default">The String the pin is initialized with and can be reset to at any time.</param>
		/// <param name="isFilename">Hint to the GUI that this String is a filename</param>
		void SetSubType(string @default, bool isFilename);
		/// <summary>
		/// Alternative version to <see cref="SetSubType(string, bool)">IStringConfig.SetSubType()</see> with more options.
		/// </summary>
		/// <param name="default">The String the pin is initialized with and can be reset to at any time</param>
		/// <param name="maxCharacters">Constrains the string to a given number of characters. Use -1 for unlimited characters</param>
		/// <param name="fileMask">Filemask in the form of: Audio File (*.wav, *.mp3)|*.wav;*.mp3</param>
		/// <param name="stringType">Enum specifying the type of string more precisely.</param>
		void SetSubType2(string @default, int maxCharacters, string fileMask, TStringType stringType);
	}
	
	/// <summary>
	/// Interface to an InputPin of type String.
	/// </summary>
	[Guid("E329D418-20DE-4D91-B060-60EF2D73A7A6"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IStringIn: IPluginIn
	{
		/// <summary>
		/// Used to retrieve a String from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the String from.</param>
		/// <param name="value">The retrieved String.</param>
		void GetString(int index, out string value);
		/// <summary>
		/// Used to set the SubType of a String pin, which is a more detailed specification of the String, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" Strings on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="default">The String the pin is initialized with and can be reset to at any time.</param>
		/// <param name="isFilename">Hint to the GUI that this String is a filename</param>
		void SetSubType(string @default, bool isFilename);
		/// <summary>
		/// Alternative version to <see cref="SetSubType(string, bool)">IStringIn.SetSubType()</see> with more options.
		/// </summary>
		/// <param name="default">The String the pin is initialized with and can be reset to at any time</param>
		/// <param name="maxCharacters">Constrains the string to a given number of characters</param>
		/// <param name="fileMask">Filemask in the form of: Audio File (*.wav, *.mp3)|*.wav;*.mp3</param>
		/// <param name="stringType">Enum specifying the type of string more precisely.</param>
		void SetSubType2(string @default, int maxCharacters, string fileMask, TStringType stringType);
	}

	/// <summary>
	/// Interface to an OutputPin of type String.
	/// </summary>
	[Guid("EC32C616-A85F-42AC-B7D1-630E1F739D1D"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IStringOut: IPluginOut
	{
		/// <summary>
		/// Used to write a String to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the String to.</param>
		/// <param name="value">The String to write.</param>
		void SetString(int index, string value);
		/// <summary>
		/// Used to set the SubType of a String pin, which is a more detailed specification of the String, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" Strings on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="default">The String the pin is initialized with and can be reset to at any time.</param>
		/// <param name="isFilename">Hint to the GUI that this String is a filename</param>
		void SetSubType(string @default, bool isFilename);
		/// <summary>
		/// Alternative version to <see cref="SetSubType(string, bool)">IStringOut.SetSubType()</see> with more options.
		/// </summary>
		/// <param name="default">The String the pin is initialized with and can be reset to at any time</param>
		/// <param name="maxCharacters">Constrains the string to a given number of characters</param>
		/// <param name="fileMask">Filemask in the form of: Audio File (*.wav, *.mp3)|*.wav;*.mp3</param>
		/// <param name="stringType">Enum specifying the type of string more precisely.</param>
		void SetSubType2(string @default, int maxCharacters, string fileMask, TStringType stringType);
	}

    [Guid("4221296D-FD9E-4378-92ED-7ADE291E5242"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStringData
    {
        /// <summary>
        /// Used to retrieve a String from the pin at the specified slice.
        /// </summary>
        /// <param name="index">The index of the slice to retrieve the String from.</param>
        /// <param name="value">The retrieved String.</param>
        void GetString(int index, out string value);
    }

	#endregion string pins
	
	#region color pins
	
	/// <summary>
	/// Interface to a ConfigurationPin of type Color.
	/// </summary>
	[Guid("BAA49637-29FA-426A-9188-86906E660D30"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IColorConfig: IPluginConfig
	{
		/// <summary>
		/// Used to write a Color to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the Color to.</param>
		/// <param name="color">The Color to write.</param>
		void SetColor(int index, RGBAColor color);
		/// <summary>
		/// Used to retrieve a Color from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the Color from.</param>
		/// <param name="color">The retrieved Color.</param>
		void GetColor(int index, out RGBAColor color);
		/// <summary>
		/// Used to retrieve a Pointer to the Colors of the pin, which can be used to retrive large Spreads of Colors more efficiently.
		/// Each Color consists of 4 doubles, one for each of Red, Green, Blue and Alpha.
		/// Attention: Don't use this Pointer to write Colors to the pin!
		/// </summary>
		/// <param name="sliceCount">The pins current SliceCount, specifying the number of colors accessible via the Pointer.</param>
		/// <param name="value">A Pointer to the pins first Colors Red channel double.</param>
		
		void GetColorPointer(out int sliceCount, out double* value);
		void GetColorPointer(out int* pLength, out double** ppData);

		/// <summary>
		/// Used to set the SubType of a Color pin, which is a more detailed specification of the Color, used by the GUI to guide the user to insert correct Colors.
		/// Note though that this does not prevent a user from setting "wrong" Colors on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="default">The Color the pin is initialized with and can be reset to at any time.</param>
		/// <param name="hasAlpha">Hint to the GUI that this Color has an alpha channel.</param>
		void SetSubType(RGBAColor @default, bool hasAlpha);
	}
	
	/// <summary>
	/// Interface to an InputPin of type Color.
	/// </summary>
	[Guid("CB6289A8-28BD-4A52-9B7A-BC1092EA2FA5"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IColorIn: IPluginIn
	{
		/// <summary>
		/// Used to retrieve a Color from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the Color from.</param>
		/// <param name="color">The retrieved Color.</param>
		void GetColor(int index, out RGBAColor color);
		/// <summary>
		/// Used to retrieve a Pointer to the Colors of the pin, which can be used to retrive large Spreads of Colors more efficiently.
		/// Each Color consists of 4 doubles, one for each of Red, Green, Blue and Alpha.
		/// Attention: Don't use this Pointer to write Colors to the pin!
		/// </summary>
		/// <param name="sliceCount">The pins current SliceCount, specifying the number of colors accessible via the Pointer.</param>
		/// <param name="value">A Pointer to the pins first Colors Red channel double.</param>
		void GetColorPointer(out int sliceCount, out double* value);
		void GetColorPointer(out int* pLength, out double** ppData);
		/// <summary>
		/// Used to set the SubType of a Color pin, which is a more detailed specification of the Color, used by the GUI to guide the user to insert correct Colors.
		/// Note though that this does not prevent a user from setting "wrong" Colors on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="default">The Color the pin is initialized with and can be reset to at any time.</param>
		/// <param name="hasAlpha">Hint to the GUI that this Color has an alpha channel.</param>
		void SetSubType(RGBAColor @default, bool hasAlpha);
	}
	
	/// <summary>
	/// Interface to an OutputPin of type Color.
	/// </summary>
	[Guid("432CE6BA-6F57-4387-A223-D2DAFA8125F0"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IColorOut: IPluginOut
	{
		/// <summary>
		/// Used to write a Color to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the Color to.</param>
		/// <param name="color">The Color to write.</param>
		void SetColor(int index, RGBAColor color);
		/// <summary>
		/// Used to retrieve a Pointer to the Colors of the pin, which can be used to write large number of Colors more efficiently.
		/// Each Color consists of 4 doubles, one for each of Red, Green, Blue and Alpha.
		/// Note though, that when writing Colors to the Pointer the pins SliceCount has to be taken care of manually.
		/// </summary>
		/// <param name="value">A Pointer to the pins first Colors Red channel double.</param>
		void GetColorPointer(out double* value);
		/// <summary>
		/// Used to set the SubType of a Color pin, which is a more detailed specification of the Color, used by the GUI to guide the user to insert correct Colors.
		/// Note though that this does not prevent a user from setting "wrong" Colors on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="default">The Color the pin is initialized with and can be reset to at any time.</param>
		/// <param name="hasAlpha">Hint to the GUI that this Color has an alpha channel.</param>
		void SetSubType(RGBAColor @default, bool hasAlpha);
		void GetColorPointer(out double** ppDst);
	}

    [Guid("C07687A7-5C92-4D99-9616-F9CFCD9414F1"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IColorData
    {
        /// <summary>
        /// Used to retrieve a Color from the pin at the specified slice.
        /// </summary>
        /// <param name="index">The index of the slice to retrieve the Color from.</param>
        /// <param name="color">The retrieved Color.</param>
        void GetColor(int index, out RGBAColor color);
    }

    #endregion color pins

    #region enum pins
    /// <summary>
    /// Interface to a ConfigurationPin of type Enum.
    /// </summary>
    [Guid("2FE17270-7B4C-4A46-A4EB-E8B56B9AD197"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEnumConfig: IPluginConfig
	{
		/// <summary>
		/// Used to write an Enum given as an ordinal value to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the Enum to.</param>
		/// <param name="value">The ordinal Enum value to write.</param>
		void SetOrd(int index, int value);
		/// <summary>
		/// Used to write an Enum given as an string value to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the Enum to.</param>
		/// <param name="value">The string Enum value to write.</param>
		void SetString(int index, string value);
		/// <summary>
		/// Used to retrieve an Enum in ordinal form from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the Enum from.</param>
		/// <param name="value">The retrieved Enum.</param>
		void GetOrd(int index, out int value);
		/// <summary>
		/// Used to retrieve an Enum in string form from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the Enum from.</param>
		/// <param name="value">The retrieved Enum.</param>
		void GetString(int index, out string value);
		/// <summary>
		/// Used to set the SubType of an Enum pin. Should only be called once immediately 
		/// after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="enumName">Name of the Enum type to set to the pin. If the given name 
		/// is not yet registered with vvvv a new type with this name is created. 
		/// Using <see cref="VVVV.PluginInterfaces.V1.IPluginHost.UpdateEnum">IPluginHost.UpdateEnum</see> 
		/// a newly created Enum can be filled with custom entries.</param>
		void SetSubType(string enumName);
		/// <summary>
		/// This method must be called before SetSubtype, it sets the default entry for this particular enum config pin 
		/// </summary>
		/// <param name="entryName">The exact name of the entry</param>
		void SetDefaultEntry(string entryName);
	}
	
	/// <summary>
	/// Interface to an InputPin of type Enum.
	/// </summary>
	[Guid("DE852C36-FE3A-4767-97F5-7595A9A59D6A"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEnumIn: IPluginIn
	{
		/// <summary>
		/// Used to retrieve an Enum in ordinal form from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the Enum from.</param>
		/// <param name="value">The retrieved Enum.</param>
		void GetOrd(int index, out int value);
		/// <summary>
		/// Used to retrieve an Enum in string form from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the Enum from.</param>
		/// <param name="value">The retrieved Enum.</param>
		void GetString(int index, out string value);
		/// <summary>
		/// Used to set the SubType of an Enum pin. Should only be called once immediately 
		/// after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="enumName">Name of the Enum type to set to the pin. If the given name 
		/// is not yet registered with vvvv a new type with this name is created. 
		/// Using <see cref="VVVV.PluginInterfaces.V1.IPluginHost.UpdateEnum">IPluginHost.UpdateEnum</see> 
		/// a newly created Enum can be filled with custom entries.</param>
		void SetSubType(string enumName);
		/// <summary>
		/// This method must be called before SetSubtype, it sets the default entry for this particular enum pin 
		/// </summary>
		/// <param name="entryName">The exact name of the entry</param>
		void SetDefaultEntry(string entryName);
        /// <summary>
        /// Registers the given listener on enum changes.
        /// </summary>
        /// <param name="listener">The listener to register.</param>
        void SetEnumChangedListener(IEnumChangedListener listener);
	}


    /// <summary>
    /// Listener interface to be informed of enum changes on an enum pin.
    /// </summary>
    [Guid("FC915DF0-643F-4CFC-9132-BE9612575381"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumChangedListener
    {
        /// <summary>
        /// Raised whenever the enum changes.
        /// </summary>
        /// <param name="name">The name of the enum.</param>
        /// <param name="defaultEntry">The new default value of the enum.</param>
        /// <param name="entries">The new entries of the enum.</param>
        void EnumChangedCB(string name, string defaultEntry, string[] entries);
    }

    /// <summary>
    /// Interface to an OutputPin of type Enum.
    /// </summary>
    [Guid("C933059A-C46E-4149-966D-04D03B93A078"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEnumOut: IPluginOut
	{
		/// <summary>
		/// Used to write an Enum given as an ordinal value to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the Enum to.</param>
		/// <param name="value">The ordinal Enum value to write.</param>
		void SetOrd(int index, int value);
		/// <summary>
		/// Used to write an Enum given as an string value to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the Enum to.</param>
		/// <param name="value">The string Enum value to write.</param>
		void SetString(int index, string value);
		/// <summary>
		/// Used to set the SubType of an Enum pin. Should only be called once immediately 
		/// after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="enumName">Name of the Enum type to set to the pin. If the given name 
		/// is not yet registered with vvvv a new type with this name is created. 
		/// Using <see cref="VVVV.PluginInterfaces.V1.IPluginHost.UpdateEnum">IPluginHost.UpdateEnum</see> 
		/// a newly created Enum can be filled with custom entries.</param>
		void SetSubType(string enumName);
	}
	#endregion enum pins
	
	#region node pins
	/// <summary>
	/// Base Interface for NodePin connections
	/// </summary>
	[Guid("AB312E34-8025-40F2-8241-1958793F3D39"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Obsolete("Not needed anymore in beta>26.")]
	public interface INodeIOBase
	{}
	
	/// <summary>
	/// Interface to an InputPin of the generic node type
	/// </summary>
	[Guid("FE6FEBC6-8581-4EB5-9AC8-E428CB9D1A03"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface INodeIn: IPluginIn
	{
		/// <summary>
		/// Used to retrieve the actual slice index this pin has to access on the upstream node. Note that the actual slice
		/// index maybe convoluted by an upstream node like GetSlice (node).
		/// </summary>
		/// <param name="slice">The slice index as seen by this pin.</param>
		/// <param name="upstreamSlice">The actual slice index as probably convoluted via upstream GetSlice (node).</param>
		void GetUpsreamSlice(int slice, out int upstreamSlice);
		/// <summary>
		/// Used to retrieve a reference of an interface offered by the upstream connected node.
		/// </summary>
		/// <param name="upstreamInterface">The retrieved interface.</param>
		[Obsolete("Replaced by GetUpstreamInterface(object UpstreamInterface).")]
		void GetUpstreamInterface(out INodeIOBase upstreamInterface);
		/// <summary>
		/// Used to set the SubType of a node pin, which is a more detailed specification of the node type via a set of Guids that identifiy the interfaces accepted on this pin.
		/// The SubType is used by the GUI to guide the user to make only links between pins that understand the same interfaces.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="guids">An array of Guids (typically only one) that specifies the interfaces that this input accepts.</param>
		/// <param name="friendlyName">A user readable name specifying the type of the node connection.</param>
		void SetSubType(Guid[] guids, string friendlyName);
		
		/// <summary>
		/// Used to set the SubType of a node pin, which is a more detailed specification of the node type via a set of Guids that identifiy the interfaces accepted on this pin.
		/// The SubType is used by the GUI to guide the user to make only links between pins that understand the same interfaces.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="type">The Pins Type.</param>
		/// <param name="guids">An array of Guids (typically only one) that specifies the interfaces that this input accepts.</param>
		/// <param name="friendlyName">A user readable name specifying the type of the node connection.</param>
		void SetSubType2(Type type, Guid[] guids, string friendlyName);
		/// <summary>
		/// Used to retrieve a reference of an interface offered by the upstream connected node.
		/// </summary>
		/// <param name="upstreamInterface">The retrieved interface.</param>
		void GetUpstreamInterface([MarshalAs(UnmanagedType.IUnknown)] out object upstreamInterface);
		void SetConnectionHandler(IConnectionHandler handler, [MarshalAs(UnmanagedType.IUnknown)] object sink);

        /// <summary>
        /// Gives access to the internal Convolution Array. This was added for performance reasons. Basic functionality described at GetUpStreamSlice()
        /// </summary>
        /// <param name="sliceCount">Should always equal the slicecount of the pin.</param>
        /// <param name="slices"></param>
        void GetUpStreamSlices(out int sliceCount, out int* slices);

        /// <summary>
        /// Added for performance reasons. If is false you may skip indexing actual slices. Obviously no GetSlice node is involved.
        /// </summary>
        bool IsConvoluted { get; } 
	}
	
	/// <summary>
	/// Interface to an OutputPin of the generic node type
	/// </summary>
	[Guid("5D4F7524-CC1B-44FA-881F-A88D343D7A21"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface INodeOut: IPluginOut
	{
		/// <summary>
		/// Used to set the interface this
		/// </summary>
		/// <param name="theInterface"></param>
		[Obsolete("Replaced by SetInterface(object TheInterface).")]
		void SetInterface(INodeIOBase theInterface);
		/// <summary>
		/// Used to set the SubType of a node pin, which is a more detailed specification of the node type via a set of Guids that identifiy the interfaces offered on this pin.
		/// The SubType is used by the GUI to guide the user to make only links between pins that understand the same interfaces.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="guids">An array of Guids (typically only one) that specifies the interfaces that this output accepts.</param>
		/// <param name="friendlyName">A user readable name specifying the type of the node connection.</param>
		void SetSubType(Guid[] guids, string friendlyName);
		
		/// <summary>
		/// Used to set the SubType of a node pin, which is a more detailed specification of the node type via a set of Guids that identifiy the interfaces accepted on this pin.
		/// The SubType is used by the GUI to guide the user to make only links between pins that understand the same interfaces.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="type">The Pins Type.</param>
		/// <param name="guids">An array of Guids (typically only one) that specifies the interfaces that this input accepts.</param>
		/// <param name="friendlyName">A user readable name specifying the type of the node connection.</param>
		void SetSubType2(Type type, Guid[] guids, string friendlyName);
		
		/// <summary>
		/// Used to mark this pin as being changed compared to the last frame. 
		/// </summary>
		void MarkPinAsChanged();
		/// <summary>
		/// Used to set the interface this
		/// </summary>
		/// <param name="theInterface"></param>
		void SetInterface([MarshalAs(UnmanagedType.IUnknown)] object theInterface);
		void SetConnectionHandler(IConnectionHandler handler, [MarshalAs(UnmanagedType.IUnknown)] object source);
	}
	
	/// <summary>
	/// Interface to an InputPin of type Transform.
	/// </summary>
	[Guid("605FD0B2-AD68-40B4-92E5-819599544CF2"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface ITransformIn: IPluginIn
	{
		/// <summary>
		/// Used to retrieve a Matrix from the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the Matrix from.</param>
		/// <param name="value">The retrieved Matrix.</param>
		void GetMatrix(int index, out Matrix4x4 value);
		/// <summary>
		/// Used to retrieve a Pointer to the Values of the pin, which can be used to retrive large Spreads of Values more efficiently.
		/// Attention: Don't use this Pointer to write Values to the pin!
		/// </summary>
		/// <param name="sliceCount">The pins current SliceCount, specifying the number of values accessible via the Pointer.</param>
		/// <param name="value">A Pointer to the pins first Value.</param>
		void GetMatrixPointer(out int sliceCount, out float* value);
		void GetMatrixPointer(out int* pLength, out float** ppData);
		/// <summary>
		/// Used to retrieve a World Matrix from the pin at the specified slice. 
		/// You should call this method only from within your Render method when supporting the IPluginDXLayer interface.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the Matrix from.</param>
		/// <param name="value">The retrieved Matrix.</param>
		void GetRenderWorldMatrix(int index, out Matrix4x4 value);
		/// <summary>
		/// Used to initialize rendering by letting vvvv know which transform pin controls spaces. 
		/// This sets view and projection matrices.
		/// </summary>
		void SetRenderSpace();
		/// <summary>
		/// Used to retrieve a World Matrix from the pin at the specified slice. 
		/// You should call this method only from within your Render method when supporting the IPluginDXLayer interface.
		/// </summary>
		/// <param name="index">The index of the slice to retrieve the Matrix from.</param>
		/// <param name="value">The retrieved Matrix.</param>
		void GetRenderWorldMatrix(int index, [Out, MarshalAs(UnmanagedType.Struct)] out Matrix value);
	}
	
	/// <summary>
	/// Interface to an OutputPin of type Transform.
	/// </summary>
	[Guid("AA8D6410-36E5-4EA2-AF70-66CD6321FF36"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface ITransformOut: IPluginOut
	{
		/// <summary>
		/// Used to write a Matrix to the pin at the specified slice.
		/// </summary>
		/// <param name="index">The index of the slice to write the Matrix to.</param>
		/// <param name="value">The Matrix to write.</param>
		void SetMatrix(int index, Matrix4x4 value);
		/// <summary>
		/// Used to retrieve a Pointer to the Values of the pin, which can be used to write large number of values more efficiently.
		/// Note though, that when writing Values to the Pointer the pins dimensions and overall SliceCount have to be taken care of manually.
		/// </summary>
		/// <param name="value">A Pointer to the pins first Value.</param>
		void GetMatrixPointer(out float* value);
		void GetMatrixPointer(out float** ppDst);
	}

    [Guid("d1db8373-78f0-4e75-af23-d344daa06472"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe public interface IRawIn : IPluginIn
    {
        void GetData(int slice, out com.IStream stream);
    }

    [Guid("8943c8e5-4833-4ca2-baea-2e32e627ffcf"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe public interface IRawOut : IPluginOut
    {
        void SetData(int slice, com.IStream stream);
        /// <summary>
        /// Used to mark this pin as being changed compared to the last frame. 
        /// </summary>
        void MarkPinAsChanged();
    }

    [Guid("59631F76-5FBB-435D-B79A-EEDBE5FE1923"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe public interface IRawData : IPluginIn
    {
        void GetData(int slice, out com.IStream stream);
    }    

    #endregion node pins	

    #region DXPins
    /// <summary>
    /// Interface to an OutputPin of type DirectX Mesh.
    /// </summary>
    [Guid("4D7E1619-0342-48EE-8AD0-13245226FD99"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDXMeshOut: IPluginOut
	{
		/// <summary>
		/// Used to mark the mesh as being changed compared to the last frame. 
		/// </summary>
		void MarkPinAsChanged();
	}
	
	[Guid("3E1B832D-FF75-4BC3-A894-47BC84A6199E"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDXTextureOut: IPluginOut
	{
		/// <summary>
		/// Used to mark the texture as being changed compared to the last frame. 
		/// </summary>
		void MarkPinAsChanged();
	}
	
	/// <summary>
	/// Interface to an OutputPin of type DirectX Layer.
	/// </summary>
	[Guid("513190D5-68C5-4623-9BDA-D5C2B8D50172"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDXLayerIO: IPluginOut
	{
		
	}
	
	/// <summary>
	/// Base interface to all InputPins of type DirectX State.
	/// </summary>
	[Guid("9A09094D-4627-4CA4-A65D-D9FC2295FAB8"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDXStateIn: IPluginIn
	{
		/// <summary>
		/// Used to set States connected to this input slicewise during the RenderLoop.
		/// </summary>
		/// <param name="index">The Index of the currently rendered slice</param>
		void SetSliceStates(int index);
	}
	
	/// <summary>
	/// Interface to an InputPin of type DirectX RenderState.
	/// </summary>
	[Guid("18DC7340-1AF3-46A5-9FF7-3348644DAB05"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDXRenderStateIn: IDXStateIn
	{
		/// <summary>
		/// Used to set RenderStates from within <see cref="VVVV.PluginInterfaces.V1.IPluginDXLayer.SetStates()">IPluginDXLayer.SetStates</see>.
		/// </summary>
		/// <param name="state">The RenderState</param>
		/// <param name="value">The RenderStates value</param>
		void SetRenderState([MarshalAs(UnmanagedType.I4)] RenderState state, int value);
	}
	
	/// <summary>
	/// Interface to an InputPin of type DirectX SamplerState.
	/// </summary>
	[Guid("49D69B50-6498-4B19-957E-828D86CD9E45"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDXSamplerStateIn: IDXStateIn
	{
		/// <summary>
		///  Used to set SamplerStates from within <see cref="VVVV.PluginInterfaces.V1.IPluginDXLayer.SetStates()">IPluginDXLayer.SetStates</see>.
		/// </summary>
		/// <param name="sampler">The sampler index to apply the SamplerState to</param>
		/// <param name="state">The SamplerState</param>
		/// <param name="value">The SamplerStates value</param>
		void SetSamplerState(int sampler, [MarshalAs(UnmanagedType.I4)] SamplerState state, int value);
		/// <summary>
		/// Used to set TextureStageStates from within <see cref="VVVV.PluginInterfaces.V1.IPluginDXLayer.SetStates()">IPluginDXLayer.SetStates</see>.
		/// </summary>
		/// <param name="sampler"></param>
		/// <param name="state"></param>
		/// <param name="value"></param>
		void SetTextureStageState(int sampler, [MarshalAs(UnmanagedType.I4)] TextureStage state, int value);
	}
	#endregion DXPins
}
