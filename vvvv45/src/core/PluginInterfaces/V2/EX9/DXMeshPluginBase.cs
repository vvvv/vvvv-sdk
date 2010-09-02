using System;
using SlimDX.Direct3D9;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.EX9
{	
	public abstract class DXMeshOutPluginBase : DXResourcePluginBase<MeshDeviceData>, IPluginDXMesh
	{
		protected IDXMeshOut FMeshOut;
		
		public DXMeshOutPluginBase(IPluginHost host)
		{
			host.CreateMeshOutput("Mesh", TSliceMode.Dynamic, TPinVisibility.True, out FMeshOut);
		}
		
		protected abstract Mesh CreateMesh(Device device);
		protected abstract void UpdateMesh(Mesh mesh);
		
		public void GetMesh(IDXMeshOut ForPin, int OnDevice, out int Mesh)
		{
			Mesh = 0;
			if(FDeviceData.ContainsKey(OnDevice))
				Mesh = FDeviceData[OnDevice].Data.ComPointer.ToInt32();
		}
		
		protected override MeshDeviceData CreateDeviceData(Device device)
		{
			return new MeshDeviceData(CreateMesh(device));
		}
		
		protected override void UpdateDeviceData(MeshDeviceData deviceData)
		{
			UpdateMesh(deviceData.Data);
		}
		
		protected override void DestroyDeviceData(MeshDeviceData deviceData, bool OnlyUnManaged)
		{
			deviceData.Data.Dispose();
		}
	}
}
