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
		}
		
		public override IPluginIO PluginIO 
		{
			get
			{
				return FStringOut;
			}
		}
		
		public override int SliceCount 
		{
			get 
			{
				throw new NotImplementedException();
			}
			set 
			{
				if (FAttribute.SliceMode != SliceMode.Single)
					FStringOut.SliceCount = value;
			}
		}
		
		public override string this[int index] 
		{
			get 
			{
				throw new NotImplementedException();
			}
			set 
			{
				
				FStringOut.SetString(index, value);
			}
		}
	}
}
