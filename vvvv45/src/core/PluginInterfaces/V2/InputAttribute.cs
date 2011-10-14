using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
	public sealed class InputAttribute : PinAttribute
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
		
		public override object Clone()
		{
			var clonedInstance = new InputAttribute(Name);
			clonedInstance.BinSize = BinSize;
			return base.Clone(clonedInstance);
		}
	}
}
