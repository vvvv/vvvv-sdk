using System;
using SlimDX.Direct3D9;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.EX9
{
	public abstract class DXTextureOutPluginBase : DXResourcePluginBase<TextureDeviceData>, IPluginDXTexture
	{
		protected IDXTextureOut FTextureOut;
		
		public DXTextureOutPluginBase(IPluginHost host)
		{
			host.CreateTextureOutput("Texture Out", TSliceMode.Dynamic, TPinVisibility.True, out FTextureOut);
		}
		
		protected abstract Texture CreateTexture(Device device);
		protected abstract void UpdateTexture(Texture texture);
		
		public void GetTexture(IDXTextureOut ForPin, int OnDevice, out int Texture)
		{
			Texture = 0;
			if(FDeviceData.ContainsKey(OnDevice)) 
				Texture = FDeviceData[OnDevice].Data.ComPointer.ToInt32();
		}
		
		protected override TextureDeviceData CreateDeviceData(Device device)
		{
			return new TextureDeviceData(CreateTexture(device));
		}
		
		protected override void UpdateDeviceData(TextureDeviceData deviceData)
		{
			UpdateTexture(deviceData.Data);
		}
		
		protected override void DestroyDeviceData(TextureDeviceData deviceData, bool OnlyUnManaged)
		{
			deviceData.Data.Dispose();
		}
	}
}
