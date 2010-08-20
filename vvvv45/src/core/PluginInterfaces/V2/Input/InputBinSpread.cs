using System;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class InputBinSpread<T> : BinSpread<T>
	{
		protected DiffPin<int> FBinSize;
		protected DiffInputWrapperPin<T> FSpreadPin;
		protected ISpread<ISpread<T>> FSpreads;
		protected bool FSpreadsBuilt;
		protected int FUpdateCount;
		
		public InputBinSpread(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
			//data pin
			FSpreadPin = new DiffInputWrapperPin<T>(host, attribute);
			FSpreadPin.Pin.Updated += new PinUpdatedEventHandler<T>(FSpreadPin_Updated);
			
			//bin size pin
			var att = new InputAttribute(attribute.Name + " Bin Size");
			att.DefaultValue = -1;
			FBinSize = new DiffIntInputPin(host, att);
			FBinSize.Updated += new PinUpdatedEventHandler<int>(FBinSize_Updated);
			
			FSpreads = new Spread<ISpread<T>>(0);
		}

		void FBinSize_Updated(Pin<int> pin)
		{
			FUpdateCount++;
			if (FUpdateCount > 1)
			{
				FUpdateCount = 0;
				if (FBinSize.IsChanged || FSpreadPin.IsChanged)
					BuildSpreads();
				
			}
		}

		void FSpreadPin_Updated(Pin<T> pin)
		{
			FUpdateCount++;
			if (FUpdateCount > 1)
			{
				FUpdateCount = 0;
				if (FBinSize.IsChanged || FSpreadPin.IsChanged)
					BuildSpreads();
			}
		}

		void BuildSpreads()
		{
			var binCount = FBinSize.SliceCount;

			if (binCount == 0)
			{
				FSpreads.SliceCount = 0;
			}
			else
			{
				var firstSlice = FBinSize[0];
				
				if (binCount == 1 || firstSlice < 0)
				{
					if (firstSlice < 0)
					{
						DivideByNegConst(firstSlice);
					}
					else if (firstSlice > 0)
					{
						DivideByConst(firstSlice);
					}
					else
					{
						FSpreads.SliceCount = 0;
					}	
				}
				else
				{
					DivideByBins(FBinSize);
				}
			}
		}
		
		protected void DivideByConst(int size)
		{
			int slices;
			
			if (FSpreadPin.SliceCount % size == 0)
			{
				slices = FSpreadPin.SliceCount / size;
			}
			else
			{
				slices = FSpreadPin.SliceCount / size + 1;
			}
			
			FSpreads.SliceCount = slices;
			
			for (int i = 0; i<slices; i++) 
			{
				var s = new Spread<T>(size);
				
				for (int j = 0; j<size; j++) 
				{
					s[j] = FSpreadPin[i*size + j];
				}
				
				FSpreads[i] = s;
			}
		}
		
		protected void DivideByNegConst(int size)
		{
			size = (int)VMath.Abs(size);
			
			int slices;

			if (FSpreadPin.SliceCount % size == 0)
			{
				slices = FSpreadPin.SliceCount / size;
			}
			else
			{
				slices = FSpreadPin.SliceCount / size + 1;
			} 
			
			FSpreads.SliceCount = size;
			
			for (int i = 0; i<size; i++) 
			{
				var s = new Spread<T>(slices);
				
				for (int j = 0; j<slices; j++) 
				{
					s[j] = FSpreadPin[i*slices + j];
				}
				
				FSpreads[i] = s;
			}

		}
		
		protected void DivideByBins(ISpread<int> bins)
		{
			var binSum = 0;
			for (int i = 0; i < bins.SliceCount; i++) 
			{
				binSum += bins[i];
			}
			
			int binTimes = 1;
			if(FSpreadPin.SliceCount % binSum == 0)
				binTimes = FSpreadPin.SliceCount / binSum;
			else
				binTimes = FSpreadPin.SliceCount / binSum + 1;
				
			var slices = binTimes * bins.SliceCount;
			
			FSpreads.SliceCount = slices;
			
			var indexSum = 0;
			
			for (int i = 0; i<slices; i++)
			{
				var size = bins[i];
				var s = new Spread<T>(size);
				
				for (int j = 0; j<size; j++) 
				{
					s[j] = FSpreadPin[indexSum + j];
				}
				
				indexSum += size;
				FSpreads[i] = s;
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
			}
		}
	}
}
