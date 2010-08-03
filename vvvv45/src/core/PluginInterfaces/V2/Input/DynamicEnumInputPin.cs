using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class DynamicEnumInputPin : ObservablePin<EnumEntry>
	{
		protected IEnumIn FEnumInputPin;
		protected string FEnumName;
		
		public DynamicEnumInputPin(IPluginHost host, InputAttribute attribute)
		{
			FEnumName = attribute.EnumName;
			
			host.CreateEnumInput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FEnumInputPin);
			FEnumInputPin.SetSubType(FEnumName);
		}

		public override int SliceCount 
		{
			get
			{
				return FEnumInputPin.SliceCount;
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		
		public override bool IsChanged
		{
			get
			{
				return FEnumInputPin.PinIsChanged;
			}
		}
		
		public override EnumEntry this[int index]
		{
			get
			{
				int ord;
				FEnumInputPin.GetOrd(index, out ord);

				return new EnumEntry(FEnumName, ord);
			}
			set
			{
				throw new NotSupportedException();
			}
		}
	}
}
