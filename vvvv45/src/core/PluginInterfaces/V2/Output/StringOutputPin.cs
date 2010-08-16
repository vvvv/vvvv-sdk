using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Output
{
	public class StringOutputPin : Pin<string>
	{
		protected IStringOut FStringOut;
		
		public StringOutputPin(IPluginHost host, OutputAttribute attribute)
		{
			host.CreateStringOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FStringOut);
			FStringOut.SetSubType(attribute.DefaultString, attribute.StringType == StringType.Filename);
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
