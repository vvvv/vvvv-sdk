using System;
using System.Collections;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace Hoster
{
	public class TTexturePin: TBasePin, IDXTextureOut, IPluginConfig
	{
		public TTexturePin(IPluginHost Parent, string PinName, TPinDirection PinDirection, TSliceMode SliceMode, TPinVisibility Visibility)
		: base(Parent, PinName, 1, TPinDirection.Output, null, SliceMode, Visibility)
		{
		}

		override protected void ChangeSliceCount()
		{
		}
				
		public void MarkPinAsChanged()
		{
		}

		override protected string AsString(int index)
		{
			return "TexturePin";
		}
		
		override public void SetSpreadAsString(string Spread)
		{
		}
	}	
}
