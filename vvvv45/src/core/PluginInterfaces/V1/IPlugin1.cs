using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

using SlimDX.Direct3D9;
using VVVV.Core.Model;
using VVVV.PluginInterfaces.InteropServices.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

/// <summary>
/// Version 1 of the VVVV PluginInterface.
///
/// To convert this to a typelib make sure AssemblyInfo.cs states: ComVisible(true).
/// Then on a commandline type:
/// <c>regasm _PluginInterfaces.dll /tlb</c>
/// This generates and registers the typelib which can then be imported e.g. via Delphi:Components:Import Component:Import Typelib
/// </summary>
namespace VVVV.PluginInterfaces.V1
{
	#region plugin
	/// <summary>
	/// The one single interface a plugin has to implement
	/// </summary>
	[Guid("084BB2C9-E8B4-4575-8611-C262399B2A95"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginBase
	{}
	
	/// <summary>
	/// The one single interface a plugin has to implement
	/// </summary>
	[Guid("7F813C89-4EDE-4087-A626-4320BE41C87F"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPlugin: IPluginBase
	{
	    /// <summary>
		/// Called by the PluginHost to hand itself over to the plugin. This is where the plugin creates its initial pins.
		/// </summary>
		/// <param name="Host">Interface to the PluginHost.</param>
		void SetPluginHost(IPluginHost Host);
		/// <summary>
		/// Called by the PluginHost before the Evaluate function every frame for every ConfigurationPin that has changed. 
		/// The ConfigurationPin is handed over as the functions input parameter. This is where a plugin would typically 
		/// create/delete pins as reaction to the changed value of a ConfigurationPin that specifies the number of pins of a specific type.
		/// </summary>
		/// <param name="input">Interface to the ConfigurationPin for which the function is called.</param>
		void Configurate(IPluginConfig input);
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
	
	/// <summary>
	/// Optional interface to be implemented on a plugin that needs to know when one of its pins is connected or disconnected
	/// </summary>
	[Guid("B77C459E-E561-424B-AB3A-572C9BB6CD93"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginConnections
	{
		/// <summary>
		/// Called by the PluginHost for every input or output that is being connected. This is typically useful for 
		/// NodeIO Inputs that can cache a reference to the upstream interface at this place instead of getting the reference
		/// every frame in Evaluate.
		/// </summary>
		/// <param name="pin">Interface to the pin for which the function is called.</param>
		void ConnectPin(IPluginIO pin);
		/// <summary>
		/// Called by the PluginHost for every input or output that is being disconnected. This is typically useful for 
		/// NodeIO Inputs that can set a cached reference to the upstream interface to null at this place.
		/// </summary>
		/// <param name="pin">Interface to the pin for which the function is called.</param>
		void DisconnectPin(IPluginIO pin);
	}

    /// <summary>
    /// Optional interface to be implemented on a plugin that wants to allow feedback loops.
    /// </summary>
    [Guid("E4CCB6C4-A875-47DF-BA4F-625457A067FD"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPluginFeedbackLoop
    {
        /// <summary>
        /// Called by the PluginHost for every input/output pair to decide whether or not the
        /// input needs to be evaluated before the output can be validated.
        /// </summary>
        /// <param name="inputPin">The input pin.</param>
        /// <param name="outputPin">The output pin.</param>
        bool OutputRequiresInputEvaluation(IPluginIO inputPin, IPluginIO outputPin);
    }

    /// <summary>
    /// Implement that interface to make your plugin aware of evaluate getting turned on or off. 
    /// The vvvv user turns a node on or off by using the Evaluate pin on the node or on one of its parent patches.
    /// </summary>
    [Guid("BAFE0468-E61A-4455-87F1-8E573717E12F"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPluginAwareOfEvaluation
    {
        /// <summary>
        /// Node will get evaluated this frame and the coming frames.
        /// </summary>
        void TurnOn();

        /// <summary>
        /// Node will not get evaluated this frame and the coming frames.
        /// </summary>
        void TurnOff();
    }

    #endregion plugin

    #region PluginDXInterfaces
    /// <summary>
    /// Optional interface to be implemented on a plugin that deals with DirectX resources like Meshes, Textures, Layers...
    /// </summary>
    [Guid("1BDD5442-8113-4EF4-9951-906633170D8C"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginDXResource
	{
		/// <summary>
		/// Called by the PluginHost every frame for every device. Therefore a plugin should only do 
		/// device specific operations here and still keep node specific calculations in the Evaluate call. 
		/// </summary>
		/// <param name="ForPin">Interface to the pin for which the function is called.</param>
		/// <param name="OnDevice">Pointer to the device on which the resources is to be updated.</param>
		void UpdateResource(IPluginOut ForPin, [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(DeviceMarshaler))] Device OnDevice);
		/// <summary>
		/// Called by the PluginHost whenever a resource for a specific pin needs to be destroyed on a specific device. 
		/// This is also called when the plugin is destroyed, so don't dispose dxresources in the plugins destructor/Dispose()
		/// </summary>
		/// <param name="ForPin">Interface to the pin for which the function is called.</param>
		/// <param name="OnDevice">Pointer to the device on which the resources is to be destroyed.</param>
		/// <param name="OnlyUnManaged">If True only unmanaged DirectX resources need to be destroyed.</param>
		void DestroyResource(IPluginOut ForPin, [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(DeviceMarshaler))] Device OnDevice, bool OnlyUnManaged);
	}
	
	/// <summary>
	/// Optional interface to be implemented on a plugin that deals with DirectX Meshes
	/// </summary>
	[Guid("E0DF9FCE-327E-4492-9C03-BA513CF93FC4"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginDXMesh: IPluginDXResource
	{
		/// <summary>
		/// Called by the PluginHost everytime a mesh is accessed via a pin on the plugin.
		/// This is called from the PluginHost from within DirectX BeginScene/EndScene,
		/// therefore the plugin shouldn't be doing much here other than handing back the right mesh.
		/// </summary>
		/// <param name="ForPin">Interface to the pin via which the mesh is accessed.</param>
		/// <param name="OnDevice">The device for which the mesh is accessed.</param>
		[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MeshMarshaler))]
		Mesh GetMesh(
		    IDXMeshOut ForPin, 
		    [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(DeviceMarshaler))] Device OnDevice);
	}
	
	/// <summary>
	/// Optional interface to be implemented on a plugin that deals with DirectX Textures
	/// </summary>
	[Guid("A679DDC2-3740-4FDE-9CCF-5EB290A3433B"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginDXTexture: IPluginDXResource
	{
		/// <summary>
		/// Called by the PluginHost everytime a texture is accessed via a pin on the plugin.
		/// This is called from the PluginHost from within DirectX BeginScene/EndScene,
		/// therefore the plugin shouldn't be doing much here other than handing back the right texture.
		/// </summary>
		/// <param name="ForPin">Interface to the pin via which the texture is accessed.</param>
		/// <param name="OnDevice">The device for which the texture is accessed.</param>
		[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(TextureMarshaler))]
		Texture GetTexture(
		    IDXTextureOut ForPin, 
		    [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(DeviceMarshaler))] Device OnDevice);
	}
	
	/// <summary>
	/// Same as IPluginDXTexture but with additional parameter to allow for spreadable outputs
	/// </summary>
	[Guid("848007F9-9180-4A28-9CAD-F8E0968D88AD"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginDXTexture2: IPluginDXResource
	{
		/// <summary>
		/// Called by the PluginHost everytime a texture is accessed via a pin on the plugin.
		/// This is called from the PluginHost from within DirectX BeginScene/EndScene,
		/// therefore the plugin shouldn't be doing much here other than handing back the right texture.
		/// </summary>
		/// <param name="ForPin">Interface to the pin via which the texture is accessed.</param>
		/// <param name="OnDevice">The device for which the texture is accessed.</param>
		/// <param name="Slice">Slice Index of the texture to be accessed.</param>
		[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(TextureMarshaler))]
		Texture GetTexture(
		    IDXTextureOut ForPin, 
		    [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(DeviceMarshaler))] Device OnDevice, 
		    int Slice);
	}

	/// <summary>
	/// Optional interface to be implemented on a plugin that deals with DirectX Layers
	/// </summary>
	[Guid("14F2AA87-EF8B-4A93-8F67-7CCA3F5E3522"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginDXLayer: IPluginDXResource
	{
		/// <summary>
		/// Called by the PluginHost everytime it needs to update its StateBlock. Here the plugin
		/// must specify all States it will set during <see cref="VVVV.PluginInterfaces.V1.IPluginDXLayer.Render(IDXLayerIO, Device)">IPluginDXLayer.Render</see>
		/// via calls to <see cref="VVVV.PluginInterfaces.V1.IDXRenderStateIn">IDXRenderStateIn</see>'s and <see cref="VVVV.PluginInterfaces.V1.IDXSamplerStateIn">IDXSamplerStateIn</see>'s functions.
		/// </summary>
		void SetStates();
		/// <summary>
		/// Called by the PluginHost everytime the plugin is supposed to render itself.
		/// This is called from the PluginHost from within DirectX BeginScene/EndScene,
		/// therefore the plugin shouldn't be doing much here other than some drawing calls.
		/// </summary>
		/// <param name="ForPin">Interface to the pin for which the function is called.</param>
		/// <param name="DXDevice">Device on which the plugin is supposed to render.</param>
		void Render(IDXLayerIO ForPin, [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(DeviceMarshaler))] Device DXDevice);
	}
	#endregion PluginDXInterfaces
}
