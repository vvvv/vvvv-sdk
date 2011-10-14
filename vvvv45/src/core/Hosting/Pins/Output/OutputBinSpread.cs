using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
	public class OutputBinSpread<T> : BinSpread<T>, IDisposable
	{
		protected Pin<int> FBinSize;
		protected Pin<T> FSpreadPin;
		protected int FUpdateCount;
		
		public OutputBinSpread(IPluginHost host, OutputAttribute attribute)
			: base(attribute)
		{
			//data pin
			FSpreadPin = PinFactory.CreatePin<T>(host, attribute);
			FSpreadPin.Updated += AnyUpdated;
			
			//bin size pin
			var att = new OutputAttribute(attribute.Name + " Bin Size");
			att.DefaultValue = 1;
			FBinSize = PinFactory.CreatePin<int>(host, att);
			FBinSize.Updated += AnyUpdated;
		}
		
		public virtual void Dispose()
		{
		    FSpreadPin.Updated -= AnyUpdated;
		    FBinSize.Updated -= AnyUpdated;
		    FSpreadPin.Dispose();
		    FBinSize.Dispose();
		}

		void AnyUpdated(object sender, EventArgs args)
		{
			if (FUpdateCount == 0)
			    this.Flatten(FSpreadPin, FBinSize);
			
			FUpdateCount++;

			if (FUpdateCount >= 2)
				FUpdateCount = 0;
		}
	}
}
