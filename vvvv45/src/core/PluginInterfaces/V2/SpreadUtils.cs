using System;
using System.Collections.Generic;
using System.Diagnostics;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace VVVV.PluginInterfaces.V2
{
	public static class SpreadUtils
	{
		public static int NormalizeBinSize(int sliceCount, int binSize)
		{
            if (sliceCount == 0) return 0;
			if (binSize < 0)
			{
				return DivByBinSize(sliceCount, Math.Abs(binSize));
			}
			return binSize;
		}
		
		public static int DivByBinSize(int sliceCount, int binSize)
		{
			Debug.Assert(binSize >= 0);
			
			if (binSize > 0)
			{
				int remainder = 0;
				int result = Math.DivRem(sliceCount, binSize, out remainder);
				if (remainder > 0)
					result++;
				return result;
			}
			return binSize;
		}

        public static bool AnyChanged(params IDiffSpread[] spreads)
        {
            foreach (IDiffSpread spread in spreads)
            {
                if (spread.IsChanged) { return true; }
            }

            return false;
        }

        public static bool AllChanged(params IDiffSpread[] spreads)
        {
            foreach (IDiffSpread spread in spreads)
            {
                if (!spread.IsChanged) { return false; }
            }

            return true;
        }

        public static int SpreadMax(params ISpread[] spreads)
        {
            var max = 0;
            foreach (var spread in spreads)
            {
                var sliceCount = spread.SliceCount;
                if (sliceCount == 0)
                    return 0;
                else
                    max = Math.Max(sliceCount, max);
            }
            return max;
        }

        public static int SpreadMax(IEnumerable<ISpread> spreads)
        {
            var max = 0;
            foreach (var spread in spreads)
            {
                var sliceCount = spread.SliceCount;
                if (sliceCount == 0)
                    return 0;
                else
                    max = Math.Max(sliceCount, max);
            }
            return max;
        }
	}
}
