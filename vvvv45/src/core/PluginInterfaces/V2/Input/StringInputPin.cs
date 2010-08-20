using System;
using System.IO;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class StringInputPin : DiffPin<string>
	{
		protected IStringIn FStringIn;
		protected bool FIsPath;
		
		public StringInputPin(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateStringInput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FStringIn);
			FStringIn.SetSubType2(attribute.DefaultString, attribute.MaxChars, attribute.FileMask, (TStringType)attribute.StringType);
			
			FIsPath = (attribute.StringType == StringType.Directory) || (attribute.StringType == StringType.Filename);
		}
		
		public override IPluginIO PluginIO
		{
			get
			{
				return FStringIn;
			}
		}
		
		public override bool IsChanged 
		{
			get 
			{
				return FStringIn.PinIsChanged;
			}
		}
		
		public override int SliceCount 
		{
			get 
			{
				return FStringIn.SliceCount;
			}
			set 
			{
				throw new NotImplementedException();
			}
		}
		
		public override string this[int index] 
		{
			get 
			{
				string value;
				FStringIn.GetString(index, out value);
				var s = value == null ? "" : value;
				return FIsPath ? GetFullPath(s) : s;
			}
			set 
			{
				throw new NotImplementedException();
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
