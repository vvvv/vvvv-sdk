using System;
using System.IO;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Config
{
	public class StringConfigPin : ConfigPin<string>
	{
		protected IStringConfig FStringConfig;
		protected bool FIsPath;
		
		public StringConfigPin(IPluginHost host, ConfigAttribute attribute)
			: base(host, attribute)
		{
			host.CreateStringConfig(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FStringConfig);
			FStringConfig.SetSubType2(attribute.DefaultString, attribute.MaxChars, attribute.FileMask, (TStringType)attribute.StringType);

			FIsPath = (attribute.StringType == StringType.Directory) || (attribute.StringType == StringType.Filename);
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
				var s = value == null ? "" : value;
				return FIsPath ? GetFullPath(s) : s;
			}
			set 
			{
				FStringConfig.SetString(index, value);
			}
		}
		
		protected string GetFullPath(string path)
		{
			string patchPath;
			FHost.GetHostPath(out patchPath);
			
			try 
			{
				path = Path.GetFullPath(Path.Combine(patchPath, path));	
			} 
			catch (Exception e)
			{
				FLogger.Log(LogType.Error, e.Message);
			}
			
			return path;
		}
	}
}
