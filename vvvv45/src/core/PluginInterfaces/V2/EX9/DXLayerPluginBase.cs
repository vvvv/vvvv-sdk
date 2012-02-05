using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using SlimDX.Direct3D9;

namespace VVVV.PluginInterfaces.V2.EX9
{
	/// <summary>
	/// Base class for plugins with layer out.
	/// </summary>
	[ComVisible(false)]
	public abstract class DXLayerOutPluginBase<T> : DXResourcePluginBase<T>, IPluginDXLayer where T : DeviceData
	{
		protected IDXRenderStateIn FRenderStatePin;
		protected IDXSamplerStateIn FSamplerStatePin;
		protected IDXLayerIO FLayerOut;
		
		/// <summary>
		/// Constructor to create the DX pins.
		/// </summary>
		/// <param name="host">The plugin host to create the pins on.</param>
		/// <param name="createRenderState">Create a render state pin?</param>
		/// <param name="createSamplerState">Create a sampler state pin?</param>
		public DXLayerOutPluginBase(IPluginHost host, bool createRenderState, bool createSamplerState)
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
		public virtual void SetStates(){}
		
		public void Render(IDXLayerIO ForPin, Device DXDevice)
		{
			Render(DXDevice, FDeviceData[DXDevice]);
		}
		
		protected override void SetResourcePinsChanged()
		{
			//nothing to do
		}
	}
}
