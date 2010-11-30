using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Input
{
	public class EnumInputPin<T> : DiffPin<T>
	{
		protected IEnumIn FEnumInputPin;
		protected Type FEnumType;
		
		public EnumInputPin(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
			FEnumType = typeof(T);
			
			var entrys = Enum.GetNames(FEnumType);
			var defEntry = (attribute.DefaultEnumEntry != "") ? attribute.DefaultEnumEntry : entrys[0];
			host.UpdateEnum(FEnumType.Name, defEntry, entrys);
			
			host.CreateEnumInput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FEnumInputPin);
			FEnumInputPin.SetSubType(FEnumType.Name);
			
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
					string entry;
					FEnumInputPin.GetString(i, out entry);
					FData[i] = (T)Enum.Parse(FEnumType, entry);
				}
			}
			
			base.Update();
		}
	}
}
