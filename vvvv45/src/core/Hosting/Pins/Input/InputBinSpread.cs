using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
	public class InputBinSpread<T> : BinSpread<T>, IDisposable
	{
		protected DiffPin<int> FBinSizePin;
		protected Pin<T> FSpreadPin;
		protected bool FSpreadsBuilt;
		protected int FUpdateCount;
		protected int FBinSizeSum;
		protected int FBinSize;
		
		private bool[] FCache = new bool[0];
		private int[] FOffset = new int[0];
		private Spread<int> FNormalizedBinSize = new Spread<int>(0);
		private int FOldSliceCount;
		
		public InputBinSpread(IPluginHost host, InputAttribute attribute)
			: base(attribute)
		{
			FBinSize = attribute.BinSize;
			
			//data pin
			CreateDataPin(host, attribute);
			FSpreadPin.Updated += AnyPin_Updated;
			
			//bin size pin
			var att = new InputAttribute(attribute.Name + " Bin Size");
			att.DefaultValue = FBinSize;
			FBinSizePin = new DiffIntInputPin(host, att);
			FBinSizePin.Updated += AnyPin_Updated;
		}
		
		public virtual void Dispose()
		{
		    FSpreadPin.Updated -= AnyPin_Updated;
		    FBinSizePin.Updated -= AnyPin_Updated;
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
		
		protected override void BufferDecreased(ISpread<T>[] oldBuffer, ISpread<T>[] newBuffer)
		{
			base.BufferDecreased(oldBuffer, newBuffer);
			
			if (FLazy)
			{
				var oldCache = FCache;
				FCache = new bool[newBuffer.Length];
				Array.Copy(oldCache, FCache, FCache.Length);
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
			// Normalize bin sizes and calculate offsets for lazy loading.
			if (FOldSliceCount != FSpreadPin.SliceCount || FBinSizePin.IsChanged)
			{
				FNormalizedBinSize.SliceCount = FBinSizePin.SliceCount;
				FBinSizeSum = 0;
				
				var binSizeBuffer = FBinSizePin.Buffer;
				var normalizedBinSizeBuffer = FNormalizedBinSize.Buffer;
				for (int i = 0; i < FBinSizePin.SliceCount; i++)
				{
					normalizedBinSizeBuffer[i] = NormalizeBinSize(binSizeBuffer[i], FSpreadPin.SliceCount);
					FBinSizeSum += normalizedBinSizeBuffer[i];
				}
				
				if (FLazy)
				{
					if (FBinSizePin.SliceCount > FOffset.Length)
						FOffset = new int[FBinSizePin.SliceCount];
					
					int offset = 0;
					for (int i = 0; i < FBinSizePin.SliceCount; i++)
					{
						FOffset[i] = offset;
						offset += normalizedBinSizeBuffer[i];
					}
				}
			}
			
			FOldSliceCount = FSpreadPin.SliceCount;
			
			if (FBinSizeSum > 0)
			{
				int remainder = 0;
				int binTimes = Math.DivRem(FSpreadPin.SliceCount, FBinSizeSum, out remainder);
				if (remainder > 0)
					binTimes = FSpreadPin.SliceCount / FBinSizeSum + 1;
				
				SliceCount = binTimes * FBinSizePin.SliceCount;
				
				CopyToBuffer(FBuffer, FSpreadPin, FNormalizedBinSize);
			}
			else
				SliceCount = 0;
		}
		
		// SliceCount is already set.
		protected virtual void CopyToBuffer(ISpread<T>[] buffer, Pin<T> source, ISpread<int> binSize)
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
						int sliceCount = FNormalizedBinSize[index];
						
						int offsetIndex = 0;
						int quotient = Math.DivRem(index, FBinSizePin.SliceCount, out offsetIndex);
						int offset = FOffset[offsetIndex] + (quotient * FBinSizeSum);
						
						spread.SliceCount = sliceCount;
						FSpreadPin.Load(offset, sliceCount);
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
			{
				float bs = Math.Abs(binSize);
				float sc = sliceCount;
				binSize = (int) Math.Ceiling(sc / bs);
			}
			
			return binSize;
		}
	}
}
