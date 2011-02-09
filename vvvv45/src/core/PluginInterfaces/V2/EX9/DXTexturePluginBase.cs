using System;
using System.Runtime.InteropServices;
using SlimDX.Direct3D9;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.EX9
{
    [ComVisible(false)]
	public abstract class DXTextureOutPluginBase : DXResourcePluginBase<TextureDeviceData>, IPluginDXTexture2
	{
		protected IDXTextureOut FTextureOut;
		private int FOldSliceCount;
		
		public DXTextureOutPluginBase(IPluginHost host)
		{
			host.CreateTextureOutput("Texture Out", TSliceMode.Dynamic, TPinVisibility.True, out FTextureOut);
			FOldSliceCount = 1;
		}
		
		protected abstract Texture CreateTexture(int Slice, Device device);
		protected abstract void UpdateTexture(int Slice, Texture texture);
		
		public void GetTexture(IDXTextureOut ForPin, int OnDevice, int Slice, out int Texture)
		{
			Texture = 0;
			if(FDeviceData.ContainsKey(OnDevice)) 
				Texture = FDeviceData[OnDevice].Data[Slice].ComPointer.ToInt32();
		}
		
		protected override TextureDeviceData CreateDeviceData(Device device)
		{
			var count = FTextureOut.SliceCount;
			var s = new Spread<Texture>(count);

			for (int i=0; i<count; i++) 
			{
				s[i] = CreateTexture(i, device);
			}
			
			return new TextureDeviceData(s);
		}
		
		protected override void UpdateDeviceData(TextureDeviceData deviceData)
		{
			var count = FTextureOut.SliceCount;
			for (int i=0; i<count; i++) 
			{
				UpdateTexture(i, deviceData.Data[i]);
			}
		}
		
		protected override void DestroyDeviceData(TextureDeviceData deviceData, bool OnlyUnManaged)
		{
			foreach(var t in deviceData.Data) t.Dispose();
		}
		
		public void Connect()
		{
			
		}
		
		public void Disconnect()
		{
			
		}
		
		public void Dispose()
		{
			
		}
		
		public void SetSliceCount(int SliceCount)
		{
			if(FOldSliceCount != SliceCount)
			{
				FTextureOut.SliceCount = SliceCount;
				Reinitialize();
				FOldSliceCount = SliceCount;
			}
		}
	}
}
