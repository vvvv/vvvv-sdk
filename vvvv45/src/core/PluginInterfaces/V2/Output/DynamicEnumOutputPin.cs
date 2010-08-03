using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Output
{
	public class DynamicEnumOutputPin : Pin<EnumEntry>
	{
		protected IEnumOut FEnumOutputPin;
		
		public DynamicEnumOutputPin(IPluginHost host, OutputAttribute attribute)
		{
			host.CreateEnumOutput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FEnumOutputPin);
			FEnumOutputPin.SetSubType(attribute.EnumName);
		}

		public override int SliceCount
		{
			get
			{
				return FEnumOutputPin.SliceCount;
			}
			set
			{
				FEnumOutputPin.SliceCount = value;
			}
		}

		public override EnumEntry this[int index]
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				FEnumOutputPin.SetOrd(index, value.Index);
			}
		}
	}
}
