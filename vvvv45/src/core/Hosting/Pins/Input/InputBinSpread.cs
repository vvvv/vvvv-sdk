using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
	public class InputBinSpread<T> : BinSpread<T>
	{
		protected DiffPin<int> FBinSize;
		protected Pin<T> FSpreadPin;
		protected bool FSpreadsBuilt;
		protected int FUpdateCount;
		protected int FBinSizeSum;
		
		protected bool FLazy;
		private bool[] FCache;
		private int[] FOffset;
		
		public InputBinSpread(IPluginHost host, InputAttribute attribute)
		{
			FLazy = attribute.Lazy;
			
			//data pin
			CreateDataPin(host, attribute);
			FSpreadPin.Updated += AnyPin_Updated;
			
			//bin size pin
			var att = new InputAttribute(attribute.Name + " Bin Size");
			att.DefaultValue = -1;
			FBinSize = new DiffIntInputPin(host, att);
			FBinSize.Updated += AnyPin_Updated;
			
			//lazy loading
			if (FLazy)
			{
				FCache = new bool[0];
				FOffset = new int[0];
				FBinSize.Changed += FBinSize_Changed;
			}
		}
		
		void FBinSize_Changed(IDiffSpread<int> spread)
		{
			if (FBinSize.SliceCount > FOffset.Length)
				FOffset = new int[FBinSize.SliceCount];
			
			int offset = 0;
			for (int i = 0; i < FBinSize.SliceCount; i++)
			{
				FOffset[i] = offset;
				offset += FBinSize[i];
			}
		}
		
		protected override void BufferIncreased(ISpread<T>[] oldBuffer, ISpread<T>[] newBuffer)
		{
			base.BufferIncreased(oldBuffer, newBuffer);
			
			if (FLazy)
			{
				var oldCache = FCache;
				FCache = new bool[newBuffer.Length];
				Array.Copy(oldCache, FCache, oldCache.Length);
			}
		}

		protected virtual bool NeedToBuildSpread()
		{
			return true;
		}
		
		protected virtual void CreateDataPin(IPluginHost host, InputAttribute attribute)
		{
			FSpreadPin = PinFactory.CreatePin<T>(host, attribute);
		}
		
		void AnyPin_Updated(object sender, EventArgs args)
		{
			FUpdateCount++;
			if (FUpdateCount > 1)
			{
				FUpdateCount = 0;
				if (NeedToBuildSpread())
					BuildSpreads();
			}
		}

		void BuildSpreads()
		{
			if (FBinSize.IsChanged)
			{
				FBinSizeSum = 0;
				
				var binSizeBuffer = FBinSize.Buffer;
				for (int i = 0; i < FBinSize.SliceCount; i++)
				{
					binSizeBuffer[i] = NormalizeBinSize(binSizeBuffer[i], FSpreadPin.SliceCount);
					FBinSizeSum += binSizeBuffer[i];
				}
			}
			
			if (FBinSizeSum > 0)
			{
				int binTimes = 1;
				if (FSpreadPin.SliceCount % FBinSizeSum == 0)
					binTimes = FSpreadPin.SliceCount / FBinSizeSum;
				else
					binTimes = FSpreadPin.SliceCount / FBinSizeSum + 1;
				
				SliceCount = binTimes * FBinSize.SliceCount;
				
				CopyToBuffer(FBuffer, FSpreadPin, FBinSize);
			}
			else
				SliceCount = 0;
		}
		
		// SliceCount is already set.
		protected virtual void CopyToBuffer(ISpread<T>[] buffer, Pin<T> source, DiffPin<int> binSize)
		{
			if (FLazy)
			{
				// We're lazy, simply clear the cache
				Array.Clear(FCache, 0, FCache.Length);
			}
			else
			{
				int offset = 0;
				for (int i = 0; i < FSliceCount; i++)
				{
					var spread = buffer[i];
					int size = binSize[i];
					
					spread.SliceCount = size;
					
					for (int j = 0; j < size; j++)
						spread[j] = source[offset + j];
					
					offset += size;
				}
			}
		}
		
		public override ISpread<T> this[int index]
		{
			get
			{
				if (FLazy)
				{
					index = VMath.Zmod(index, FSliceCount);
					if (!FCache[index])
					{
						var spread = FBuffer[index];
						int sliceCount = FBinSize[index];
						int offsetIndex = index % FBinSize.SliceCount;
						int offset = FOffset[offsetIndex] + ((index / FBinSize.SliceCount) * FBinSizeSum);
						
						spread.SliceCount = sliceCount;
						for (int i = 0; i < sliceCount; i++)
							spread[i] = FSpreadPin[offset + i];
						
						FCache[index] = true;
					}
					
					return FBuffer[index];
				}
				else
					return base[index];
			}
			set
			{
				if (FLazy)
				{
					index = VMath.Zmod(index, FSliceCount);
					FBuffer[index] = value;
					FCache[index] = true;
				}
				else
					base[index] = value;
			}
		}
		
		private int NormalizeBinSize(int binSize, int sliceCount)
		{
			if (binSize < 0)
				binSize = sliceCount / Math.Abs(binSize);
			
			return binSize;
		}
	}
}
