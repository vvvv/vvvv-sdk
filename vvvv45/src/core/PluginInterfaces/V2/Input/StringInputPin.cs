using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	public class StringInputPin : InputPin<string>
	{
		protected IStringIn FStringIn;
		
		public StringInputPin(IPluginHost host, InputAttribute attribute)
			:base(attribute)
		{
			host.CreateStringInput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FStringIn);
			FStringIn.SetSubType(attribute.DefaultString, attribute.IsFilename);
		}
		
		public override IPluginIn PluginIn
		{
			get
			{
				return FStringIn;
			}
		}
		
		public override string this[int index] 
		{
			get 
			{
				string value;
				FStringIn.GetString(index, out value);
				return value;
			}
			set 
			{
				throw new NotImplementedException();
			}
		}
	}
}
