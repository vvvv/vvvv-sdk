using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Config
{
	public class EnumConfigPin<T> : ConfigPin<T>
	{
		protected IEnumConfig FEnumConfigPin;
		protected Type FEnumType;
		
		public EnumConfigPin(IPluginHost host, ConfigAttribute attribute)
			: base(host, attribute)
		{
			FEnumType = typeof(T);
			
			var entrys = Enum.GetNames(FEnumType);
			var defEntry = !string.IsNullOrEmpty(attribute.DefaultEnumEntry) ? attribute.DefaultEnumEntry : entrys[0];
			host.UpdateEnum(FEnumType.FullName, defEntry, entrys);

			host.CreateEnumConfig(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FEnumConfigPin);
			FEnumConfigPin.SetSubType(FEnumType.FullName);
			
			base.Initialize(FEnumConfigPin);
		}
		
		public override T this[int index]
		{
			get
			{
				string entry;
				FEnumConfigPin.GetString(index, out entry);

				return (T)Enum.Parse(FEnumType, entry);
			}
			set
			{
				FEnumConfigPin.SetString(index, value.ToString());
			}
		}
	}
}
