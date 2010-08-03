using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Config
{
	public class DynamicEnumConfigPin : ConfigPin<EnumEntry>
	{
		protected IEnumConfig FEnumConfigPin;
		protected string FEnumName;
		
		public DynamicEnumConfigPin(IPluginHost host, ConfigAttribute attribute)
		{
			FEnumName = attribute.EnumName;
			
			host.CreateEnumConfig(attribute.Name, attribute.SliceMode, attribute.Visibility, out FEnumConfigPin);
			FEnumConfigPin.SetSubType(FEnumName);
		}
		
		protected override IPluginConfig PluginConfig 
		{
			get 
			{
				return FEnumConfigPin;
			}
		}
		
		public override EnumEntry this[int index]
		{
			get
			{
				int ord;
				FEnumConfigPin.GetOrd(index, out ord);

				return new EnumEntry(FEnumName, ord);
			}
			set
			{
				FEnumConfigPin.SetOrd(index, value.Index);
			}
		}
	}
}
