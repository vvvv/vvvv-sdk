#region usings
using System;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
#endregion usings

namespace VVVV.Nodes
{
	public class VecBinSpread<T>		
	{	
		List<T[]> buffer;
		public int Count { get { return buffer.Count; } }
		
		int vecSize;
		public int VectorSize { get { return vecSize; } }
		
		public T[] this[int index] { get { return buffer[index]; } }
		
		int itemCount;
		public int ItemCount { get { return itemCount; } }
		
		public VecBinSpread(IInStream<T> input, int vectorSize, IInStream<int> bin, int spreadMax = 0)
		{
			vecSize = vectorSize;
			buffer = new List<T[]>();
			itemCount = 0;

			if (bin.Length > 0)
			{
				int sliceCount = (int)Math.Ceiling(input.Length/(double)vecSize);
				int incr = 0;
				using (var binReader = bin.GetCyclicReader())
				{
					using (var dataReader = input.GetCyclicReader())
					{
						while (incr<dataReader.Length || (binReader.Position % binReader.Length)!=0 || spreadMax > 0)
						{
							int curBin = SpreadUtils.NormalizeBinSize(sliceCount, binReader.Read())*vecSize;
							itemCount += curBin;
							T[] data = new T[curBin];
							if (curBin>0)
								dataReader.Read(data,0,curBin);
							buffer.Add(data);
							
							spreadMax--;
							incr += curBin;
							if (incr==0)
								if (binReader.Position == 0)
									incr += dataReader.Length;						
						}
					}
				}
			}
		}
		
		public List<T> GetBinColumn(int index, int column)
		{
			if (vecSize == 1)
			{
				return new List<T>(this[index]);
			}
			else
			{
				List<T> col = new List<T>(0);
				for (int i = column; i < this[index].Length; i+=vecSize)
					col.Add(buffer[index][i]);
				return col;
			}
		}
		
		public List<T> GetBinRow(int index, int row)
		{
			return new List<T>(this[index]).GetRange(row*vecSize,vecSize);
		}
	}
}
