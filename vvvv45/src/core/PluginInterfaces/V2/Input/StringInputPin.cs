using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class StringInputPin : ObservablePin<string>
	{
		protected IStringIn FStringIn;
		
		public StringInputPin(IPluginHost host, InputAttribute attribute)
		{
			host.CreateStringInput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FStringIn);
			FStringIn.SetSubType(attribute.DefaultString, attribute.StringType == StringType.Filename);
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
				return value == null ? "" : value;
			}
			set 
			{
				throw new NotImplementedException();
			}
		}
	}
}
