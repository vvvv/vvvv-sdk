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
            FTextureOut.Order = int.MinValue;
			FOldSliceCount = 1;
		}
		
		protected abstract Texture CreateTexture(int Slice, Device device);
		protected abstract void UpdateTexture(int Slice, Texture texture);
		
		public Texture GetTexture(IDXTextureOut ForPin, Device OnDevice, int Slice)
		{
			if(FDeviceData.ContainsKey(OnDevice)) 
				return FDeviceData[OnDevice].Data[Slice];
			return null;
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
		
		public void SetSliceCount(int SliceCount)
		{
			if(FOldSliceCount != SliceCount)
			{
				FTextureOut.SliceCount = SliceCount;
				Reinitialize();
				FOldSliceCount = SliceCount;
			}
		}
		
		protected override void SetResourcePinsChanged()
		{
			FTextureOut.MarkPinAsChanged();
		}
	}
}
