using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
	public sealed class OutputAttribute : PinAttribute
	{
	  public static readonly string DefaultBinName = " Bin Size";
	  
		public OutputAttribute(string name)
			:base(name)
		{
		  	BinName = DefaultBinName;
			BinVisibility = PinVisibility.True;
			BinOrder = 0;
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
		
		/// <summary>
		/// The position of the bin size used in ISpread&lt;ISpread&lt;T&gt;&gt; implementations.
		/// </summary>
		public int BinOrder
		{
			get;
			set;
		}
	}
}
