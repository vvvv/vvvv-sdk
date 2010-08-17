using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Config
{
	public class StringConfigPin : ConfigPin<string>
	{
		protected IStringConfig FStringConfig;
		
		public StringConfigPin(IPluginHost host, ConfigAttribute attribute)
		{
			host.CreateStringConfig(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FStringConfig);
			FStringConfig.SetSubType2(attribute.DefaultString, attribute.MaxChar, attribute.FileMask, (TStringType)attribute.StringType);

		}
		
		protected override IPluginConfig PluginConfig 
		{
			get 
			{
				return FStringConfig;
			}
		}
		
		public override string this[int index] 
		{
			get 
			{
				string value;
				FStringConfig.GetString(index, out value);
				return value;
			}
			set 
			{
				FStringConfig.SetString(index, value);
			}
		}
	}
}
