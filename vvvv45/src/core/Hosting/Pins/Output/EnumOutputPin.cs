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
			
			base.Initialize(FEnumOutputPin);
		}
		
		public override void Update()
		{
			if (FAttribute.SliceMode != SliceMode.Single)
				FEnumOutputPin.SliceCount = FSliceCount;
			
			for (int i = 0; i < FSliceCount; i++)
			{
				FEnumOutputPin.SetString(i, FData[i].ToString());
			}
			
			base.Update();
		}
	}
}
