using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
	public sealed class InputAttribute : PinAttribute
	{
		public InputAttribute(string name)
			:base(name)
		{
			BinSize = -1;
			AutoValidate = true;
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
		/// Whether the pin is being validated on Evaluate or not.
		/// Validation triggers upstream node evaluation if upstream node was not
		/// evaluated yet in this frame.
		/// </summary>
		public bool AutoValidate
		{
			get;
			set;
		}
		
		/// <summary>
		/// Whether or not vvvv should check if the data changed from previous frame.
		/// This is by default disabled for numeric data as their spread counts are
		/// typically high and doing this check might have a negative impact on 
		/// performance.
		/// </summary>
		public bool CheckIfChanged
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
