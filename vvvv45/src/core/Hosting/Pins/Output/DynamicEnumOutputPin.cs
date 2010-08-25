using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Output
{
	public class DynamicEnumOutputPin : Pin<EnumEntry>
	{
		protected IEnumOut FEnumOutputPin;
		
		public DynamicEnumOutputPin(IPluginHost host, OutputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateEnumOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FEnumOutputPin);
			FEnumOutputPin.SetSubType(attribute.EnumName);
		}
		
		public override IPluginIO PluginIO 
		{
			get
			{
				return FEnumOutputPin;
			}
		}

		public override int SliceCount
		{
			get
			{
				return FEnumOutputPin.SliceCount;
			}
			set
			{
				if (FAttribute.SliceMode != SliceMode.Single)
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
