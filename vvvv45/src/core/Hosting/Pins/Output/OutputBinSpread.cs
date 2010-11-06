using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
	public class OutputBinSpread<T> : BinSpread<T>
	{
		protected Pin<int> FBinSize;
		protected Pin<T> FSpreadPin;
		protected ISpread<ISpread<T>> FSpreads;
		protected bool FSpreadsBuilt;
		protected int FUpdateCount;
		
		public OutputBinSpread(IPluginHost host, OutputAttribute attribute)
			: base(host, attribute)
		{
			//data pin
			FSpreadPin = PinFactory.CreatePin<T>(host, attribute);
			FSpreadPin.Updated += FSpreadPin_Updated;
			
			//bin size pin
			var att = new OutputAttribute(attribute.Name + " Bin Size");
			att.DefaultValue = 1;
			FBinSize = new IntOutputPin(host, att);
			FBinSize.Updated += FBinSize_Updated;
			
			FSpreads = new Spread<ISpread<T>>(1);
			FSpreads[0] = new Spread<T>(1);
		}

		void FBinSize_Updated(object sender, EventArgs args)
		{
			AnyUpdated();
		}

		void FSpreadPin_Updated(object sender, EventArgs args)
		{
			AnyUpdated();
		}
		
		private void AnyUpdated()
		{
			if (FUpdateCount == 0)			
				BuildSpreads();
			
			FUpdateCount++;

			if (FUpdateCount >= 2)
				FUpdateCount = 0;			
		}

		void BuildSpreads()
		{
			FBinSize.SliceCount = FSpreads.SliceCount;
			
			int count = 0;
        	for(int i = 0; i < FSpreads.SliceCount; i++)
        	{
        		var c = FSpreads[i].SliceCount;
        	    count += c;
        	    FBinSize[i] = c;
        	}
        	
        	FSpreadPin.SliceCount = count;
        	
        	count = 0;
        	for(int i = 0; i < FSpreads.SliceCount; i++)
        	{
        	    for(int j = 0; j < FSpreads[i].SliceCount; j++)
        	    {
        	        FSpreadPin[count + j] = FSpreads[i][j];
        	    }
        	    
        	    count += FSpreads[i].SliceCount;
        	}
		}
		
		public override ISpread<T> this[int index]
		{
			get
			{
				return FSpreads[index];
			}
			set
			{
				FSpreads[index] = value;
			}
		}
		
		public override int SliceCount
		{
			get
			{
				return FSpreads.SliceCount;
			}
			set
			{
				FSpreads.SliceCount = value;
			}
		}
	}
}
