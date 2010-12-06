using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// The spread data type
	/// </summary>
	public class Spread<T> : ISpread<T>
	{
		protected T[] FBuffer;
		protected int FSliceCount;
		private int FUpperThreshold;
		private int FLowerThreshold;
		private int FDecreaseRequests;
		
		public Spread(int size)
		{
			FBuffer = new T[Math.Max(1, size)];
			FSliceCount = size;
			FUpperThreshold = FBuffer.Length;
			FLowerThreshold = 0;
		}
		
		public Spread(IList<T> original)
			: this(original.Count)
		{
			original.CopyTo(FBuffer, 0);
		}
		
		public Spread(ISpread<T> original)
			: this(original.SliceCount)
		{
			for (int i = 0; i < SliceCount; i++)
				FBuffer[i] = original[i];
		}
		
		/// <summary>
		/// Gives direct access to the internal data buffer.
		/// Use with caution. Size of internal buffer != SliceCount.
		/// </summary>
		public T[] Buffer
		{
			get
			{
				return FBuffer;
			}
		}
		
		protected virtual void BufferIncreased(T[] oldBuffer, T[] newBuffer)
		{
			for (int i = 0; i < newBuffer.Length; i++)
				newBuffer[i] = oldBuffer[i % oldBuffer.Length];
		}
		
		public virtual T this[int index]
		{
			get
			{
				return FBuffer[VMath.Zmod(index, FSliceCount)];
			}
			set
			{
				FBuffer[VMath.Zmod(index, FSliceCount)] = value;
			}
		}
		
		public virtual int SliceCount
		{
			get
			{
				return FSliceCount;
			}
			set
			{
				if (value < 0)
					value = 0;
				
				FSliceCount = value;
				
				if (FSliceCount > FUpperThreshold)
				{
					FUpperThreshold = FSliceCount * 2;
					FLowerThreshold = FUpperThreshold / 4;
					FDecreaseRequests = FUpperThreshold;
					
//					Debug.WriteLine(string.Format("Up: {0} Low: {1}", FUpperThreshold, FLowerThreshold));
					
					var oldBuffer = FBuffer;
					FBuffer = new T[FUpperThreshold];
					
					BufferIncreased(oldBuffer, FBuffer);
				}
				else if (FSliceCount < FLowerThreshold)
				{
					FDecreaseRequests--;
					
					if (FDecreaseRequests == 0)
					{
						FUpperThreshold = FUpperThreshold / 2;
						FLowerThreshold = FUpperThreshold / 4;
						
//						Debug.WriteLine(string.Format("Up: {0} Low: {1}", FUpperThreshold, FLowerThreshold));
						
						var oldBuffer = FBuffer;
						FBuffer = new T[FUpperThreshold];
						Array.Copy(oldBuffer, FBuffer, FBuffer.Length);
					}
				}
				else
					FDecreaseRequests = FUpperThreshold;
			}
		}
		
		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < FSliceCount; i++)
				yield return this[i];
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public static class SpreadExtensions_
	{
		public static ISpread<T> ToSpread<T>(this List<T> list)
		{
			return new Spread<T>(list);
		}
	}
}