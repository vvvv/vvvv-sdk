using System;
using System.Collections;

using VVVV.PluginInterfaces.V1;

namespace Hoster
{	
	public class TLayerPin: TBasePin, IDXLayerIO, IPluginConfig
	{
		public TLayerPin(IPluginHost Parent, string PinName, TPinDirection PinDirection, TSliceMode SliceMode, TPinVisibility Visibility)
		: base(Parent, PinName, 1, TPinDirection.Output, null, SliceMode, Visibility)
		{
		}
		
		override protected void ChangeSliceCount()
		{
		}

		override protected string AsString(int index)
		{
			return "LayerPin";
		}
		
		override public void SetSpreadAsString(string Spread)
		{
		}
	}	
}
