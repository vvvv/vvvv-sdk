using System;
using System.Diagnostics;

namespace VVVV.PluginInterfaces.V2
{
	public static class SpreadUtils
	{
		public static int NormalizeBinSize(int sliceCount, int binSize)
		{
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
	}
}
