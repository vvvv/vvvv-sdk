using System;
using System.Collections;

using VVVV.PluginInterfaces.V1;
using SlimDX.Direct3D9;

namespace Hoster
{	
	public class TStatePin: TBasePin, IDXRenderStateIn, IDXSamplerStateIn, IPluginConfig
	{
		public TStatePin(IPluginHost Parent, TPinDirection PinDirection, TSliceMode SliceMode, TPinVisibility Visibility)
		: base(Parent, "State", 1, PinDirection, null, SliceMode, Visibility)
		{
			base.Initialize();
		}
		
		override protected void ChangeSliceCount()
		{
		}

		override protected string AsString(int index)
		{
			return "StatePin";
		}
		
		override public void SetSpreadAsString(string Spread)
		{
		}
		
		public void SetRenderState<T>(RenderState State, T Value)
		{
		}
		
		public void SetSamplerState<T>(int Sampler, SamplerState State, T Value)
		{
		}
		
		public void SetTextureStageState<T>(int Stage, TextureStage State, T Value)
		{
		}
		
		public void SetSliceStates(int Slice)
		{
		}
	}	
}
