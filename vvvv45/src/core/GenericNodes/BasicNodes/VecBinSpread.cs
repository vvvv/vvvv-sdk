#region usings
using System;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
#endregion usings

namespace VVVV.Nodes.Generic
{
    public class VecBinSpread<T>
    {
        readonly List<T[]> FBuffer = new List<T[]>();
        int FVectorSize;
        int FItemCount;
        int FNonEmptyBinCount;

        public int Count { get { return FBuffer.Count; } }

        public int VectorSize { get { return FVectorSize; } }

        public T[] this[int index] { get { return FBuffer[index]; } }

        public int ItemCount { get { return FItemCount; } }

        public int NonEmptyBinCount { get { return FNonEmptyBinCount; } }

        public void Sync(IInStream<T> input, int vectorSize, IInStream<int> bin, int spreadMax = 0)
        {
            var itemCount = 0;
            var buffer = FBuffer;
            var nonEmptyBinCount = 0;
            if (bin.Length > 0)
            {
                using (var binReader = bin.GetCyclicReader())
                using (var dataReader = input.GetCyclicReader())
                {
                    var sliceCount = (int)Math.Ceiling(input.Length / (double)vectorSize);
                    var incr = 0;
                    var i = 0;
                    while (incr < dataReader.Length || (binReader.Position % binReader.Length) != 0 || spreadMax > 0)
                    {
                        int curBin = SpreadUtils.NormalizeBinSize(sliceCount, binReader.Read()) * vectorSize;
                        itemCount += curBin;
                        // Re-use existing arrays if possible
                        T[] data;
                        if (i < buffer.Count)
                        {
                            data = buffer[i];
                            if (data.Length != curBin)
                                data = buffer[i] = Allocate(curBin);
                        }
                        else
                        {
                            data = Allocate(curBin);
                            buffer.Add(data);
                        }
                        i++;
                        if (curBin > 0)
                        {
                            dataReader.Read(data, 0, curBin);
                            nonEmptyBinCount++;
                        }

                        spreadMax--;
                        incr += curBin;
                        if (incr == 0)
                            if (binReader.Position == 0)
                                incr += dataReader.Length;
                    }
                    if (i < buffer.Count)
                        buffer.RemoveRange(i, buffer.Count - i);
                }
            }
            else
            {
                buffer.Clear();
            }

            FVectorSize = vectorSize;
            FItemCount = itemCount;
            FNonEmptyBinCount = nonEmptyBinCount;
        }

        private static T[] Allocate(int length) => length > 0 ? new T[length] : Array.Empty<T>();

        public List<T> GetBinColumn(int index, int column)
        {
            if (FVectorSize == 1)
            {
                return new List<T>(this[index]);
            }
            else
            {
                List<T> col = new List<T>(0);
                for (int i = column; i < this[index].Length; i += FVectorSize)
                    if (this[index].Length > i)
                        col.Add(this[index][i]);
                return col;
            }
        }

        public List<T> GetBinRow(int index, int row)
        {
            return new List<T>(this[index]).GetRange(row * FVectorSize, FVectorSize);
        }
    }
}
