using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class EnumInputPin<T> : ObservablePin<T>
	{
		protected IEnumIn FEnumConfigPin;
		protected Type FEnumType;
		
		public EnumInputPin(IPluginHost host, InputAttribute attribute)
		{
			FEnumType = typeof(T);
			
			var entrys = Enum.GetNames(FEnumType);
			var defEntry = (attribute.DefaultEnumEntry != "") ? attribute.DefaultEnumEntry : entrys[0];	
			host.UpdateEnum(FEnumType.Name, defEntry, entrys);
			
			host.CreateEnumInput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FEnumConfigPin);
			FEnumConfigPin.SetSubType(FEnumType.Name);
		}

		public override int SliceCount 
		{
			get
			{
				return FEnumConfigPin.SliceCount;
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
				return FEnumConfigPin.PinIsChanged;
			}
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
				throw new NotSupportedException();
			}
		}
	}
}
