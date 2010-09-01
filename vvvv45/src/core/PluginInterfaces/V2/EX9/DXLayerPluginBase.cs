using System;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using SlimDX.Direct3D9;

namespace VVVV.PluginInterfaces.V2.EX9
{
	/// <summary>
	/// Base class for plugins with layer out.
	/// </summary>
	public abstract class DXLayerPluginBase<T> : DXResourcePluginBase<T>, IPluginDXLayer where T : DeviceData
	{
		protected IDXRenderStateIn FRenderStatePin;
		protected IDXSamplerStateIn FSamplerStatePin;
		protected IDXLayerIO FLayerOut;
		
		public DXLayerPluginBase(IPluginHost host, bool createRenderState, bool createSamplerState)
		{
			if(createRenderState)
			{
				host.CreateRenderStateInput(TSliceMode.Dynamic, TPinVisibility.True, out FRenderStatePin);
				FRenderStatePin.Order = -2;
			}
			
			if(createSamplerState)
			{
				host.CreateSamplerStateInput(TSliceMode.Dynamic, TPinVisibility.True, out FSamplerStatePin);
				FSamplerStatePin.Order = -1;
			}
			
			host.CreateLayerOutput("Layer", TPinVisibility.True, out FLayerOut);
		}
		
		protected abstract void Render(Device device, T deviceData);
		
		public abstract void SetStates();
		
		public void Render(IDXLayerIO ForPin, IPluginDXDevice DXDevice)
		{
			Render(Device.FromPointer(new IntPtr(DXDevice.DevicePointer())), FDeviceData[DXDevice.DevicePointer()]);
		}
	}
}
