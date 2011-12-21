using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
	public class DynamicEnumOutputPin : Pin<EnumEntry>
	{
		protected IEnumOut FEnumOutputPin;
		
		public DynamicEnumOutputPin(IPluginHost host, OutputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateEnumOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FEnumOutputPin);
			FEnumOutputPin.SetSubType(attribute.EnumName);
			
			base.InitializeInternalPin(FEnumOutputPin);
		}
		
		public override void Update()
		{
			base.Update();
			
			if (FAttribute.SliceMode != SliceMode.Single)
				FEnumOutputPin.SliceCount = FSliceCount;
			
			for (int i = 0; i < FSliceCount; i++)
			{
				FEnumOutputPin.SetOrd(i, FBuffer[i].Index);
			}
		}
	}
}
