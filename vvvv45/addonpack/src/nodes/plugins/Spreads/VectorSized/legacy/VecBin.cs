using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes
{
	/// <summary>
	/// Spread with vector size and bin size
	/// </summary>
	public class VecBin
	{
		private int vecSize;
		
		private List<List<double>> bins;
		public int BinCount
		{
			get { return bins.Count; }
		}
		
		public List<double> Slices
		{
			get 
			{ 
				List<double> collect = new List<double>();
				foreach (List<double> l in bins)
				{
					collect.AddRange(l);
				}
				return collect;
			}
		}
		public int SliceCount
		{
			get 
			{ 
				int incr = 0;
				foreach (List<double> l in bins)
				{
					incr+=l.Count;
				}
				return incr;
			}
		}
		
		public VecBin(IValueIn InValues, IValueIn InBins, int VectorSize)
		{
			vecSize = VectorSize;
			int slicecount = (int)Math.Ceiling((double)InValues.SliceCount/(double)vecSize);
			
			bins = new List<List<double>>();
			int incr=0;
			int binIncr = 0;
			
			double tmpBin;
			int curBin;
//			while (incr<slicecount || binIncr<InBins.SliceCount)
//			{
//				bins.Add(new List<double>());
//				
//				InBins.GetValue(binIncr, out tmpBin);
//				curBin = (int)Math.Round(tmpBin);
//				if (curBin<0)
//					curBin = slicecount;
//				
//				double curSlice;
//				for (int i=0; i<curBin*vecSize; i++)
//				{
//					InValues.GetValue((incr*vecSize)+i, out curSlice);
//						bins[binIncr].Add(curSlice);
//				}
//				
//				incr+=curBin;
//				if (curBin==0)
//					incr+=slicecount*vecSize;
//				binIncr++;
//			}
			
			while (incr<slicecount || (binIncr%InBins.SliceCount)!=0)
			{
				int l = 1;
				InBins.GetValue(binIncr, out tmpBin);
				curBin = (int)Math.Round(tmpBin);

				if (curBin<0)
				{
					l = Math.Abs(curBin);
					curBin = (int)Math.Ceiling(slicecount/(double)l);
				}
				
				double curSlice;
				for (int _l=0; _l<l; _l++)
				{
					bins.Add(new List<double>());
					for (int i=0; i<curBin*vecSize; i++)
					{
						InValues.GetValue((incr*vecSize)+i, out curSlice);
						bins[bins.Count-1].Add(curSlice);
					}
					incr+=curBin;
				}
				if (binIncr==0 && curBin==0)
					incr+=slicecount*vecSize;
				binIncr++;
			}
		}
		
		public List<double> GetBin(int index)
		{
			index = (int)(index%bins.Count);
			return bins[index];
		}
		
		public List<double> GetBinVector(int BinIndex, int VectorIndex)
		{
			List<double> bin = GetBin(BinIndex);
			int vecMax = bin.Count/vecSize;
			VectorIndex = (int)(VectorIndex%vecMax);
			if (VectorIndex<0)
				VectorIndex = vecMax+VectorIndex;
			return bin.GetRange(VectorIndex*vecSize,vecSize);
		}
		
		public List<double> GetVector(int index)
		{
			int vecSpreadMax = this.SliceCount/vecSize;
			index = (int)(vecSpreadMax%index);
			int incr=0;
			while (bins[incr].Count/vecSize<index)
			{
				incr++;
				index-=bins[incr].Count/vecSize;
			}
			return bins[incr].GetRange(index*vecSize,vecSize);
		}
	}
}
