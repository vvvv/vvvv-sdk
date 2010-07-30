using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Output
{
	public class StringOutputPin : Pin<string>
	{
		protected IStringOut FStringOut;
		
		public StringOutputPin(IPluginHost host, OutputAttribute attribute)
		{
			host.CreateStringOutput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FStringOut);
			FStringOut.SetSubType(attribute.DefaultString, attribute.IsFilename);
		}
		
		public override int SliceCount 
		{
			get 
			{
				throw new NotImplementedException();
			}
			set 
			{
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
