using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Input
{
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
			
			base.Initialize(FEnumInputPin);
		}
		
		public override bool IsChanged
		{
			get
			{
				return FEnumInputPin.PinIsChanged;
			}
		}
		
		public override void Update()
		{
			if (IsChanged)
			{
				SliceCount = FEnumInputPin.SliceCount;
				
				for (int i = 0; i < FSliceCount; i++)
				{
					int ord;
					FEnumInputPin.GetOrd(i, out ord);
					FData[i] = new EnumEntry(FEnumName, ord);
				}
			}
			
			base.Update();
		}
	}
}
