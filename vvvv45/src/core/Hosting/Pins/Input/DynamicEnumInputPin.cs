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
		}
		
		public override IPluginIO PluginIO 
		{
			get
			{
				return FEnumInputPin;
			}
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
