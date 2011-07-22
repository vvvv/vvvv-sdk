using System;
using System.Runtime.InteropServices;
using System.IO;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Config
{
    [ComVisible(false)]
	public class StringConfigPin : ConfigPin<string>
	{
		protected IStringConfig FStringConfig;
		
		public StringConfigPin(IPluginHost host, ConfigAttribute attribute)
			: base(host, attribute)
		{
			host.CreateStringConfig(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FStringConfig);
			FStringConfig.SetSubType2(attribute.DefaultString, attribute.MaxChars, attribute.FileMask, (TStringType)attribute.StringType);
		
			base.Initialize(FStringConfig);
		}
		
		public override string this[int index] 
		{
			get 
			{
				string value;
				FStringConfig.GetString(index, out value);
				var s = value == null ? "" : value;
				return s;
			}
			set 
			{
				FStringConfig.SetString(index, value);
			}
		}
	}
}
