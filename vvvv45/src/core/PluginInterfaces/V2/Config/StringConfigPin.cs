using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Config
{
	public class StringConfigPin : ConfigPin<string>
	{
		protected IStringConfig FStringConfig;
		
		public StringConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(attribute)
		{
			host.CreateStringConfig(attribute.Name, attribute.SliceMode, attribute.Visibility, out FStringConfig);
			FStringConfig.SetSubType(attribute.DefaultString, attribute.IsFilename);
		}
		
		public override IPluginConfig PluginConfig
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
