using System;
using System.Runtime.InteropServices;
using SlimDX.Direct3D9;

namespace VVVV.PluginInterfaces.V2.EX9
{
	/// <summary>
	/// Base Class for custom data per graphics device.
	/// </summary>
	[ComVisible(false)]
	public class DeviceData
	{
		/// <summary>
		/// Update the device data this frame?
		/// </summary>
		public bool Update { get; set; }
		
		/// <summary>
		/// Recreate the device data this frame?
		/// </summary>
		public bool Reinitialize { get; set; }
		
		/// <summary>
		/// Create a DeviceData instance with 'Update = true' and 'Reinitialize = false'.
		/// </summary>
		public DeviceData()
		{
			Update = true;
			Reinitialize = false;
		}
	}
	
	//generic spreaded device data
	[ComVisible(false)]
	public class GenericDeviceData<T> : DeviceData
	{
		//texture for this device
		public T Data { get; set; }
		
		public GenericDeviceData(T data)
			: base()
		{
			Data = data;
		}
	}
	
	//texture data per graphics device
	[ComVisible(false)]
	public class TextureDeviceData : GenericDeviceData<ISpread<Texture>>
	{
		public TextureDeviceData(ISpread<Texture> texture)
			: base(texture)
		{
		}
	}
	
	//mesh data per graphics device
	[ComVisible(false)]
	public class MeshDeviceData : GenericDeviceData<Mesh>
	{
		public MeshDeviceData(Mesh mesh)
			: base(mesh)
		{
		}
	}
	
}
