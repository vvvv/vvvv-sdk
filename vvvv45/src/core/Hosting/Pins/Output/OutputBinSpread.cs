using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
	public class OutputBinSpread<T> : BinSpread<T>
	{
		protected Pin<int> FBinSize;
		protected OutputWrapperPin<T> FSpreadPin;
		protected ISpread<ISpread<T>> FSpreads;
		protected bool FSpreadsBuilt;
		protected int FUpdateCount;
		
		public OutputBinSpread(IPluginHost host, OutputAttribute attribute)
			: base(host, attribute)
		{
			//data pin
			FSpreadPin = new OutputWrapperPin<T>(host, attribute);
			FSpreadPin.Pin.Updated += new PinUpdatedEventHandler<T>(FSpreadPin_Updated);
			
			//bin size pin
			var att = new OutputAttribute(attribute.Name + " Bin Size");
			att.DefaultValue = 1;
			FBinSize = new IntOutputPin(host, att);
			FBinSize.Updated += new PinUpdatedEventHandler<int>(FBinSize_Updated);
			
			FSpreads = new Spread<ISpread<T>>(1);
			FSpreads[0] = new Spread<T>(1);
		}

		void Any_Updated()
		{
			if (FUpdateCount == 0)			
				BuildSpreads();
			
			FUpdateCount++;

			if (FUpdateCount >= 2)
				FUpdateCount = 0;			
		}

		void FBinSize_Updated(Pin<int> pin)
		{
			Any_Updated();
		}

		void FSpreadPin_Updated(Pin<T> pin)
		{
			Any_Updated();
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
				if (FSpreads.SliceCount != value)
				{
					FSpreads.SliceCount = value;
					
					for (int i = 0; i<FSpreads.SliceCount; i++) 
					{
						FSpreads[i] = new Spread<T>(0);
					}
				}
			}
		}
	}
}
