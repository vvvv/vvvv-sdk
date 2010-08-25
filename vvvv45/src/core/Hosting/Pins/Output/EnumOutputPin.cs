using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Output
{
	public class EnumOutputPin<T> : Pin<T>
	{
		protected IEnumOut FEnumOutputPin;
		protected Type FEnumType;
		
		public EnumOutputPin(IPluginHost host, OutputAttribute attribute)
			: base(host, attribute)
		{
			FEnumType = typeof(T);
			
			var entrys = Enum.GetNames(FEnumType);
			var defEntry = (attribute.DefaultEnumEntry != "") ? attribute.DefaultEnumEntry : entrys[0];	
			host.UpdateEnum(FEnumType.Name, defEntry, entrys);
			
			host.CreateEnumOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FEnumOutputPin);
			FEnumOutputPin.SetSubType(FEnumType.Name);
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

		public override T this[int index]
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				FEnumOutputPin.SetString(index, value.ToString());
			}
		}
	}
}
