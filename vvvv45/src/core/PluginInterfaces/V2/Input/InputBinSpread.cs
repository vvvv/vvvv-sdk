using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class InputBinSpread<T> : BinSpread<T>
	{
		public InputBinSpread(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
		}
		
		public override ISpread<T> this[int index]
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		
		public override int SliceCount
		{
			get;
			set;
		}
	}
}
