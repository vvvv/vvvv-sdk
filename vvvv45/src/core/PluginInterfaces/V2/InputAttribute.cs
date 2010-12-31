using System;
using System.ComponentModel.Composition;

namespace VVVV.PluginInterfaces.V2
{
	public class InputAttribute : PinAttribute
	{
		public static readonly int DefaultBinSize = -1;
		
		public InputAttribute(string name)
			:base(name)
		{
			BinSize = DefaultBinSize;
		}
		
		/// <summary>
		/// The bin size used in ISpread&lt;ISpread&lt;T&gt;&gt; implementations.
		/// </summary>
		public int BinSize
		{
			get;
			set;
		}
	}
}
