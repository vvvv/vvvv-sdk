using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	public class DynamicEnumInputPin : DiffPin<EnumEntry>
	{
		protected IEnumIn FEnumInputPin;
		protected string FEnumName;
		
		public DynamicEnumInputPin(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
			FEnumName = attribute.EnumName;
			
			host.CreateEnumInput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FEnumInputPin);
			FEnumInputPin.SetSubType(FEnumName);
			
			base.InitializeInternalPin(FEnumInputPin);
		}
		
		protected override bool IsInternalPinChanged
		{
			get
			{
				return FEnumInputPin.PinIsChanged;
			}
		}
		
		unsafe protected override void DoUpdate()
		{
			SliceCount = FEnumInputPin.SliceCount;
			
			for (int i = 0; i < FSliceCount; i++)
			{
				int ord;
				FEnumInputPin.GetOrd(i, out ord);
				FBuffer[i] = new EnumEntry(FEnumName, ord);
			}
		}
	}
}
