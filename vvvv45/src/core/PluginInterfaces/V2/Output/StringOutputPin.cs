using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Output
{
	public class StringOutputPin : OutputPin<string>
	{
		protected IStringOut FStringOut;
		
		public StringOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(attribute)
		{
			host.CreateStringOutput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FStringOut);
			FStringOut.SetSubType(attribute.DefaultString, attribute.IsFilename);
		}
		
		public override IPluginOut PluginOut
		{
			get
			{
				return FStringOut;
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
