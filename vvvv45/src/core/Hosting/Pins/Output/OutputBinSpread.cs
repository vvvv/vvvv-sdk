using System;
using System.Diagnostics;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
	public class OutputBinSpread<T> : BinSpread<T>, IDisposable
	{
		protected Pin<int> FBinSize;
		protected Pin<T> FSpreadPin;
		protected bool FSpreadsBuilt;
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
			FBinSize = new IntOutputPin(host, att);
			FBinSize.Updated += AnyUpdated;
		}
		
		public virtual void Dispose()
		{
		    FSpreadPin.Updated -= AnyUpdated;
		    FBinSize.Updated -= AnyUpdated;
		}
		
		void AnyUpdated(object sender, EventArgs args)
		{
			if (FUpdateCount == 0)
				BuildSpreads();
			
			FUpdateCount++;

			if (FUpdateCount >= 2)
				FUpdateCount = 0;
		}

		void BuildSpreads()
		{
			FBinSize.SliceCount = SliceCount;
			
			int count = 0;
			for(int i = 0; i < SliceCount; i++)
			{
				var c = this[i].SliceCount;
				count += c;
				FBinSize[i] = c;
			}
			
			FSpreadPin.SliceCount = count;
			
			var outputBuffer = FSpreadPin.Buffer;
			int offset = 0;
			for(int i = 0; i < SliceCount; i++)
			{
				var spread = this[i];
				
				for(int j = 0; j < spread.SliceCount; j++)
					outputBuffer[offset + j] = spread[j];
				
				offset += spread.SliceCount;
			}
		}
	}
}
