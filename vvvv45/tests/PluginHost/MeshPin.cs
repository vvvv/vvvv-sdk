using System;
using System.Collections;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace Hoster
{
	public class TMeshPin: TBasePin, IDXMeshOut, IPluginConfig
	{
		public TMeshPin(IPluginHost Parent, string PinName, TPinDirection PinDirection, TSliceMode SliceMode, TPinVisibility Visibility)
		: base(Parent, PinName, 1, TPinDirection.Output, null, SliceMode, Visibility)
		{
			base.Initialize();
		}

		override protected void ChangeSliceCount()
		{
		}
				
		public void MarkPinAsChanged()
		{
		}

		override protected string AsString(int index)
		{
			return "MeshPin";
		}
		
		override public void SetSpreadAsString(string Spread)
		{
		}
	}	
}
