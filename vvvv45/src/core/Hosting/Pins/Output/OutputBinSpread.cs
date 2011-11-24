using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using VVVV.Hosting.Streams;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
	public class OutputBinSpread<T> : BinSpread<T>, IDisposable
	{
		protected ISpread<int> FBinSizeSpread;
		protected ISpread<T> FDataSpread;
		protected int FUpdateCount;
		
		public OutputBinSpread(IOFactory ioFactory, OutputAttribute attribute)
			: base(ioFactory, attribute)
		{
			//data pin
			FDataSpread = FIOFactory.CreateIO<ISpread<T>>(attribute);
//			FDataSpread.Updated += AnyUpdated;
			
			//bin size pin
			var att = new OutputAttribute(attribute.Name + " Bin Size");
			att.DefaultValue = 1;
			FBinSizeSpread = FIOFactory.CreateIO<ISpread<int>>(att);
//			FBinSizeSpread.Updated += AnyUpdated;
		}
		
		public virtual void Dispose()
		{
//		    FDataSpread.Updated -= AnyUpdated;
//		    FBinSizeSpread.Updated -= AnyUpdated;
//		    FDataSpread.Dispose();
//		    FBinSizeSpread.Dispose();
		}

		void AnyUpdated(object sender, EventArgs args)
		{
			if (FUpdateCount == 0)
			    this.Flatten(FDataSpread, FBinSizeSpread);
			
			FUpdateCount++;

			if (FUpdateCount >= 2)
				FUpdateCount = 0;
		}
	}
}
