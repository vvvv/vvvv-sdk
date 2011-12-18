using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
	public sealed class InputAttribute : PinAttribute
	{
		public static readonly int DefaultBinSize = -1;
		public static readonly string DefaultBinName = " Bin Size";
		
		public InputAttribute(string name)
			:base(name)
		{
			BinSize = DefaultBinSize;
			BinName = DefaultBinName;
			BinVisibility = PinVisibility.True;
		}
		
		/// <summary>
		/// The bin size used in ISpread&lt;ISpread&lt;T&gt;&gt; implementations.
		/// </summary>
		public int BinSize
		{
			get;
			set;
		}
		
		/// <summary>
		/// The bin name used in ISpread&lt;ISpread&lt;T&gt;&gt; implementations.
		/// </summary>
		public string BinName
		{
			get;
			set;
		}
		
		/// <summary>
		/// The visibility of the bin size pin in the patch and inspektor.
		/// </summary>
		public PinVisibility BinVisibility
		{
			get;
			set;
		}
	}
}
