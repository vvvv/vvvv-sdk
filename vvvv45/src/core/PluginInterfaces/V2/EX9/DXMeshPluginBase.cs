using System;
using SlimDX.Direct3D9;
using VVVV.PluginInterfaces.V1;
using System.Runtime.InteropServices;

namespace VVVV.PluginInterfaces.V2.EX9
{	
    [ComVisible(false)]
	public abstract class DXMeshOutPluginBase : DXResourcePluginBase<MeshDeviceData>, IPluginDXMesh
	{
		protected IDXMeshOut FMeshOut;
		
		public DXMeshOutPluginBase(IPluginHost host)
		{
			host.CreateMeshOutput("Mesh", TSliceMode.Dynamic, TPinVisibility.True, out FMeshOut);
		}
		
		protected abstract Mesh CreateMesh(Device device);
		protected abstract void UpdateMesh(Mesh mesh);
		
		public Mesh GetMesh(IDXMeshOut ForPin, Device OnDevice)
		{
			if(FDeviceData.ContainsKey(OnDevice))
				return FDeviceData[OnDevice].Data;
			return null;
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
		
		protected override void SetResourcePinsChanged()
		{
			FMeshOut.MarkPinAsChanged();
		}
	}
}
