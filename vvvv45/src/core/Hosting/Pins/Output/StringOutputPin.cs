using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Output
{
	public class StringOutputPin : Pin<string>
	{
		protected IStringOut FStringOut;
		
		public StringOutputPin(IPluginHost host, OutputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateStringOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FStringOut);
			FStringOut.SetSubType2(attribute.DefaultString, attribute.MaxChars, attribute.FileMask, (TStringType)attribute.StringType);
			
			base.InitializeInternalPin(FStringOut);
		}
		
		public override void Update()
		{
			base.Update();
			
			if (FAttribute.SliceMode != SliceMode.Single)
				FStringOut.SliceCount = FSliceCount;
			
			for (int i = 0; i < FSliceCount; i++)
			{
				FStringOut.SetString(i, FBuffer[i]);
			}
		}
	}
}
