using System;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D9;

namespace VVVV.PluginInterfaces.V1
{
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
		/// <param name="Slice">The Index of the currently rendered slice</param>
		void SetSliceStates(int Index);
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
		/// <param name="State">The RenderState</param>
		/// <param name="Value">The RenderStates value</param>
		void SetRenderState<T>(RenderState State, T Value);
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
		/// <param name="Sampler">The sampler index to apply the SamplerState to</param>
		/// <param name="State">The SamplerState</param>
		/// <param name="Value">The SamplerStates value</param>
		void SetSamplerState<T>(int Sampler, SamplerState State, T Value);
		/// <summary>
		/// Used to set TextureStageStates from within <see cref="VVVV.PluginInterfaces.V1.IPluginDXLayer.SetStates()">IPluginDXLayer.SetStates</see>.
		/// </summary>
		/// <param name="Sampler"></param>
		/// <param name="State"></param>
		/// <param name="Value"></param>
		void SetTextureStageState<T>(int Sampler, TextureStage State, T Value);
	}
	#endregion DXPins
	
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
		void UpdateResource(IPluginOut ForPin, int OnDevice);
		/// <summary>
		/// Called by the PluginHost whenever a resource for a specific pin needs to be destroyed on a specific device. 
		/// This is also called when the plugin is destroyed, so don't dispose dxresources in the plugins destructor/Dispose()
		/// </summary>
		/// <param name="ForPin">Interface to the pin for which the function is called.</param>
		/// <param name="OnDevice">Pointer to the device on which the resources is to be destroyed.</param>
		/// <param name="OnlyUnManaged">If True only unmanaged DirectX resources need to be destroyed.</param>
		void DestroyResource(IPluginOut ForPin, int OnDevice, bool OnlyUnManaged);
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
		/// <param name="OnDevice">Pointer to the device for which the mesh is accessed.</param>
		/// <param name="Mesh">The retrieved mesh</param>
		void GetMesh(IDXMeshOut ForPin, int OnDevice, out int Mesh);
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
		/// <param name="OnDevice">Pointer to the device for which the texture is accessed.</param>
		/// <param name="Texture">The retrieved mesh</param>
		void GetTexture(IDXTextureOut ForPin, int OnDevice, out int Texture);
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
		/// must specify all States it will set during <see cref="VVVV.PluginInterfaces.V1.IPluginDXLayer.Render()">IPluginDXLayer.Render</see>
		/// via calls to <see cref="VVVV.PluginInterfaces.V1.IDXRenderStateIn">IDXRenderStateIn</see>'s and <see cref="VVVV.PluginInterfaces.V1.IDXSamplerStateIn">IDXSamplerStateIn</see>'s functions.
		/// </summary>
		void SetStates();
		/// <summary>
		/// Called by the PluginHost everytime the plugin is supposed to render itself.
		/// This is called from the PluginHost from within DirectX BeginScene/EndScene,
		/// therefore the plugin shouldn't be doing much here other than some drawing calls.
		/// </summary>
		/// <param name="ForPin">Interface to the pin for which the function is called.</param>
		/// <param name="OnDevice">Pointer to the device on which the plugin is supposed to render.</param>
		void Render(IDXLayerIO ForPin, IPluginDXDevice DXDevice);
	}  
	
	/// <summary>
	/// Interface to access the hosts current DirectX device. Available as input parameter to any of the IPluginDXResource functions 
	/// </summary>
	[Guid("765B10CB-4CA9-4927-B1DF-A8FB67692267"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginDXDevice
	{
		int DevicePointer();
	}
	#endregion PluginDXInterfaces
}