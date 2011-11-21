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
		
		public void SetRenderState(RenderState State, int Value)
		{
		}
		
		public void SetSamplerState(int Sampler, SamplerState State, int Value)
		{
		}
		
		public void SetTextureStageState(int Stage, TextureStage State, int Value)
		{
		}
		
		public void SetSliceStates(int Slice)
		{
		}
	}	
}
