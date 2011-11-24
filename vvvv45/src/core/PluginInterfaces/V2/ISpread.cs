using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using VVVV.Utils.Streams;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// Common non-generic interface to the underlying input/output/config pins.
	/// Set/Get, Read/Write methods are only implemented when it makes sense.
	/// </summary>
	[ComVisible(false)]
	public interface ISpread : IEnumerable
	{
		/// <summary>
		/// Provides read/write access to the actual data.
		/// </summary>
		object this[int index]
		{
			get;
			set;
		}
		
		/// <summary>
		/// Get/Set the size of this spread.
		/// </summary>
		int SliceCount
		{
			get;
			set;
		}
	}
	
	/// <summary>
	/// Common interface to the underlying input/output/config pins.
	/// Set/Get, Read/Write methods are only implemented when it makes sense.
	/// </summary>
	[ComVisible(false)]
	public interface ISpread<T> : IEnumerable<T>, ISpread
	{
		/// <summary>
		/// Provides read/write access to the actual data.
		/// </summary>
		new T this[int index]
		{
			get;
			set;
		}
		
		/// <summary>
		/// Get/Set the size of this spread.
		/// </summary>
		new int SliceCount
		{
			get;
			set;
		}
		
		IIOStream<T> GetStream();
	}

	[ComVisible(false)]
	public static class SpreadExtensions
	{
		/// <summary>
		/// Converts a <see cref="ISpread{T}"/> to a <see cref="string"/> of comma seperated values.
		/// </summary>
		/// <param name="spread">The <see cref="ISpread{T}"/> to convert to a <see cref="string"/>.</param>
		/// <returns>A comma seperated <see cref="string"/> of <see cref="ISpread{T}"/>.</returns>
		public static string ToString<T>(this ISpread<T> spread)
		{
			return spread.AsString();
		}
		
		/// <summary>
		/// Converts a <see cref="ISpread{T}"/> to a <see cref="string"/> of comma seperated values.
		/// </summary>
		/// <param name="spread">The <see cref="ISpread{T}"/> to convert to a <see cref="string"/>.</param>
		/// <returns>A comma seperated <see cref="string"/> of <see cref="ISpread{T}"/>.</returns>
		public static string AsString<T>(this ISpread<T> spread)
		{
			var sb = new StringBuilder();
			
			for (int i = 0; i < spread.SliceCount - 1; i++)
				sb.AppendFormat("{0}, ", spread[i].ToString());

			if (spread.SliceCount > 0)
				sb.Append(spread[spread.SliceCount - 1].ToString());
			
			return sb.ToString();
		}
		
		/// <summary>
		/// Used to calculate the slice count if two spreads need to be
		/// iterated.
		/// </summary>
		/// <param name="c1">Slice count 1.</param>
		/// <param name="c2">Slice count 2.</param>
		/// <returns>
		/// Maximum of c1 and c2 if c1 greater 0 and c2 greater 0;
		/// otherwise, 0.
		/// </returns>
		public static int CombineSpreads(this int c1, int c2)
		{
			if (c1 == 0 || c2 == 0)
				return 0;
			else
				return Math.Max(c1, c2);
		}
		
		/// <summary>
		/// <see cref="CombineSpreads"/>
		/// </summary>
		public static int CombineWith<V>(this int c1, ISpread<V> spread2)
		{
			return CombineSpreads(c1, spread2.SliceCount);
		}
		
		/// <summary>
		/// <see cref="CombineSpreads"/>
		/// </summary>
		public static int CombineWith<U, V>(this ISpread<U> spread1, ISpread<V> spread2)
		{
			return CombineSpreads(spread1.SliceCount, spread2.SliceCount);
		}
		
		/// <summary>
		/// Copy all values from <see cref="IEnumerable{T}"/> to this <see cref="ISpread{T}"/>.
		/// </summary>
		/// <param name="spread">The <see cref="ISpread{T}"/> to copy to.</param>
		/// <param name="enumerable">The IEnumerable{T} to copy from.</param>
		public static void AssignFrom<T>(this ISpread<T> spread, IEnumerable<T> enumerable)
		{
			spread.SliceCount = enumerable.Count();
			
			int i = 0;
			foreach	(var entry in enumerable)
				spread[i++] = entry;
		}
		
		/// <summary>
		/// Copy all values from <see cref="IList{T}"/> to this <see cref="ISpread{T}"/>.
		/// </summary>
		/// <param name="spread">The <see cref="ISpread{T}"/> to copy to.</param>
		/// <param name="list">The IList{T} to copy from.</param>
		public static void AssignFrom<T>(this ISpread<T> spread, IList<T> list)
		{
			spread.SliceCount = list.Count;
			
			for (int i = 0; i < list.Count; i++)
				spread[i] = list[i];
		}
		
		/// <summary>
		/// Create a copy of the <see cref="ISpread{T}"/>.
		/// </summary>
		/// <param name="spread">The <see cref="ISpread{T}"/> to copy.</param>
		/// <returns>A new copy of <see cref="ISpread{T}"/>.</returns>
		public static ISpread<T> Clone<T>(this ISpread<T> spread)
		{
			return new Spread<T>(spread);
		}
		
		/// <summary>
		/// Adds an item to the <see cref="ISpread{T}"/>.
		/// </summary>
		/// <param name="spread">The <see cref="ISpread{T}"/> to add the object to.</param>
		/// <param name="item">The object to add.</param>
		/// <remarks>This is operation has a runtime of O(1).</remarks>
		public static void Add<T>(this ISpread<T> spread, T item)
		{
			spread.SliceCount++;
			spread[spread.SliceCount - 1] = item;
		}
		
		/// <summary>
		/// Adds the elements of the specified enumerable to the end of the <see cref="ISpread{T}"/>.
		/// </summary>
		/// <param name="spread">The <see cref="ISpread{T}"/> to add the elements to.</param>
		/// <param name="enumerable">The elements to be added.</param>
		public static void AddRange<T>(this ISpread<T> spread, IEnumerable<T> enumerable)
		{
			foreach (var item in enumerable)
			{
				spread.Add(item);
			}
		}
		
		/// <summary>
		/// Adds the elements of the specified list to the end of the <see cref="ISpread{T}"/>.
		/// </summary>
		/// <param name="spread">The <see cref="ISpread{T}"/> to add the elements to.</param>
		/// <param name="list">The elements to be added.</param>
		public static void AddRange<T>(this ISpread<T> spread, IList<T> list)
		{
			int offset = spread.SliceCount;
			spread.SliceCount += list.Count;
			for (int i = offset, j = 0; i < spread.SliceCount; i++, j++)
			{
				spread[i] = list[j];
			}
		}
		
		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="ISpread{T}"/>.
		/// </summary>
		/// <param name="spread">The <see cref="ISpread{T}"/> to remove the object from.</param>
		/// <param name="item">The object to remove.</param>
		/// <returns>
		/// true if item was successfully removed from the <see cref="ISpread{T}"/>; otherwise, false.
		/// This method also returns false if item is not found in the original <see cref="ISpread{T}"/>.
		/// </returns>
		/// <remarks>This is operation has a runtime of O(n).</remarks>
		public static bool Remove<T>(this ISpread<T> spread, T item)
		{
			for (int i = 0; i < spread.SliceCount; i++)
			{
				if (Comparer<T>.Default.Compare(spread[i], item) == 0)
				{
					spread.RemoveAt(i);
					return true;
				}
			}
			
			return false;
		}
		
		/// <summary>
		/// Removes the element at the specified index of the <see cref="ISpread{T}"/>.
		/// </summary>
		/// <param name="spread">The <see cref="ISpread{T}"/> to remove the element from.</param>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <remarks><paramref name="index"/> can be negative or greater SliceCount.</remarks>
		public static void RemoveAt<T>(this ISpread<T> spread, int index)
		{
			index = VMath.Zmod(index, spread.SliceCount);
			for (int j = index + 1; j < spread.SliceCount; j++)
			{
				spread[j - 1] = spread[j];
			}
			
			spread.SliceCount--;
		}
		
		/// <summary>
		/// Removes all the elements that match the conditions defined by the specified predicate.
		/// </summary>
		/// <param name="spread">The <see cref="ISpread{T}"/> to remove the objects from.</param>
		/// <param name="match">The <see cref="Predicate{T}"/> delegate that defines the conditions of the elements to remove.</param>
		/// <returns>The number of elements removed from the <see cref="ISpread{T}"/>.</returns>
		public static int RemoveAll<T>(this ISpread<T> spread, Predicate<T> match)
		{
			int removeCount = 0;
			
			for (int i = 0; i < spread.SliceCount; i++)
			{
				if (match(spread[i]))
				{
					int slicesToRemove = 1;
					for (int j = i + 1; j < spread.SliceCount; j++)
					{
						if (match(spread[j]))
							slicesToRemove++;
						else
							break;
					}
					
					for (int j = i + slicesToRemove; j < spread.SliceCount; j++)
					{
						spread[j - slicesToRemove] = spread[j];
					}
					
					spread.SliceCount -= slicesToRemove;
					removeCount += slicesToRemove;
				}
			}
			
			return removeCount;
		}
		
		/// <summary>
		/// Removes a range of elements from the <see cref="ISpread{T}"/>.
		/// </summary>
		/// <param name="spread">The <see cref="ISpread{T}"/> to remove the elements from.</param>
		/// <param name="index">The zero-based starting index of the range of elements to remove.</param>
		/// <param name="count">The number of elements to remove.</param>
		/// <remarks>The items are removed and all the elements following them in the <see cref="ISpread{T}"/> have their indexes reduced by count.</remarks>
		public static void RemoveRange<T>(this ISpread<T> spread, int index, int count)
		{
			for (int i = 0; i < count; i++)
			{
				spread.RemoveAt(index);
			}
		}
		
		/// <summary>
		/// Creates a shallow copy of a range of elements in the source <see cref="ISpread{T}"/>.
		/// </summary>
		/// <param name="spread">The <see cref="ISpread{T}"/> to get the elements from.</param>
		/// <param name="index">The zero-based <see cref="ISpread{T}"/> index at which the range starts.</param>
		/// <param name="count">The number of elements in the range.</param>
		/// <returns>A shallow copy of a range of elements in the source <see cref="ISpread{T}"/>.</returns>
		public static ISpread<T> GetRange<T>(this ISpread<T> spread, int index, int count)
		{
			var result = new Spread<T>(count);
			
            for (int i = 0; i < count; i++)
			{
                result[i] = spread[index+i];
			}
			
			return result;
		}
		
		/// <summary>
		/// Determines the index of a specific item in the <see cref="ISpread{T}"/>.
		/// </summary>
		/// <param name="spread">The <see cref="ISpread{T}"/> to use to locate the item.</param>
		/// <param name="item">The object to locate in the <see cref="ISpread{T}"/>.</param>
		/// <returns>The index of item if found in the <see cref="ISpread{T}"/>; otherwise, -1.</returns>
		/// <remarks>If an object occurs multiple times in the <see cref="ISpread{T}"/>, the IndexOf method always returns the first instance found.</remarks>
		public static int IndexOf<T>(this ISpread<T> spread, T item)
		{
			for (int i = 0; i < spread.SliceCount; i++)
			{
				if (Comparer<T>.Default.Compare(spread[i], item) == 0)
				{
					return i;
				}
			}
			
			return -1;
		}
		
		/// <summary>
		/// Inserts an item to the <see cref="ISpread{T}"/> at the specified index.
		/// </summary>
		/// <param name="spread">The <see cref="ISpread{T}"/> to insert the item into.</param>
		/// <param name="index">The zero-based index at which item should be inserted.</param>
		/// <param name="item">The object to insert into the <see cref="ISpread{T}"/>.</param>
		/// <remarks>
		/// If index equals the number of items in the <see cref="ISpread{T}"/>, then item is appended to the spread.
		/// </remarks>
		public static void Insert<T>(this ISpread<T> spread, int index, T item)
		{
			if (index == spread.SliceCount)
			{
				spread.Add(item);
			}
			else
			{
				index = VMath.Zmod(index, spread.SliceCount);
				spread.SliceCount++;
				for (int j = spread.SliceCount - 1; j >= index; j--)
				{
					spread[j] = spread[j - 1];
				}
				spread[index] = item;
			}
		}
		
		/// <summary>
		/// Inserts the elements of a collection into the <see cref="ISpread{T}"/> at the specified index.
		/// </summary>
		/// <param name="spread">The <see cref="ISpread{T}"/> to insert the items into.</param>
		/// <param name="index">The zero-based index at which the new elements should be inserted.</param>
		/// <param name="collection">The collection whose elements should be inserted into the <see cref="ISpread{T}"/>.</param>
		public static void InsertRange<T>(this ISpread<T> spread, int index, IEnumerable<T> collection)
		{
			foreach (var item in collection.Reverse())
			{
				spread.Insert(index, item);
			}
		}
		
		public static int GetMaxSliceCount<T>(this ISpread<ISpread<T>> spreads)
		{
			switch (spreads.SliceCount)
			{
				case 0:
					return 0;
				case 1:
					return spreads[0].SliceCount;
				default:
					var stream = spreads.GetStream();
					int result = stream.Read().SliceCount;
					while (!stream.Eof)
					{
						result = result.CombineWith(stream.Read());
					}
//					int result = spreads[0].SliceCount;
//					for (int i = 1; i < spreads.SliceCount; i++)
//					{
//						result = result.CombineWith(spreads[i]);
//					}
					return result;
			}
		}
		
		public static int GetSliceCountSum<T>(this ISpread<ISpread<T>> spreads)
		{
			int result = 0;
			
			foreach (var spread in spreads)
			{
				result += spread.SliceCount;
			}
			
			return result;
		}
		
		public static void SetSliceCountBy<T>(this ISpread<T> outputSpread, ISpread<ISpread<T>> inputSpreads)
		{
			outputSpread.SliceCount = inputSpreads.GetMaxSliceCount() * inputSpreads.SliceCount;
		}
		
		public static void SetSliceCountBy<T>(this ISpread<ISpread<T>> outputSpreads, ISpread<T> inputSpread)
		{
			int outputSpreadCount = outputSpreads.SliceCount;
			int remainder = 0;
			int sliceCountPerSpread = Math.DivRem(inputSpread.SliceCount, outputSpreadCount, out remainder);
			if (remainder > 0) sliceCountPerSpread++;
			
			for (int i = 0; i < outputSpreadCount; i++)
			{
				outputSpreads[i].SliceCount = sliceCountPerSpread;
			}
		}
		
		public static TAccumulate FoldL<TSource, TAccumulate>(
			this ISpread<TSource> source,
			TAccumulate seed,
			Func<TAccumulate, TSource, TAccumulate> func
		)
		{
			var stream = source.GetStream();
			var buffer = stream.CreateReadBuffer();
			
			while (!stream.Eof)
			{
				int n = stream.Read(buffer, 0, buffer.Length);
				
				for (int i = 0; i < n; i++)
				{
					seed = func(seed, buffer[i]);
				}
			}
			
			return seed;
		}
		
		public static void Raise<T>(
			this ISpread<T> reducedSpread,
			ISpread<int> binSizeSpread,
			ISpread<ISpread<T>> expandedSpread
		)
		{
			var expandedStream = expandedSpread.GetStream();
			var binSizeStream = binSizeSpread.GetStream();
			var srcStream = reducedSpread.GetStream();
			
			var binSizeBuffer = binSizeStream.CreateReadBuffer();
			var buffer = srcStream.CreateReadBuffer();
			
			int binSizeSum = 0;
			
			var normBinSize = new Spread<int>();
			while (!binSizeStream.Eof)
			{
				int binSizeCount = binSizeStream.Read(binSizeBuffer, 0, binSizeBuffer.Length);
				for (int i = 0; i < binSizeCount; i++)
				{
					binSizeBuffer[i] = NormalizeBinSize(srcStream.Length, binSizeBuffer[i]);
					binSizeSum += binSizeBuffer[i];
					normBinSize.Add(binSizeBuffer[i]);
				}
//				binSizeStream.CurrentSlice -= binSizeCount;
//				binSizeStream.Write(binSizeBuffer, 0, binSizeCount);
			}

			int binTimes = DivByBinSize(srcStream.Length, binSizeSum);
			binTimes = binTimes > 0 ? binTimes : 1;
			expandedStream.Length = binTimes * binSizeStream.Length;
			
//			binSizeStream.Reset();
			binSizeStream = normBinSize.GetStream();
			
			while (!expandedStream.Eof && srcStream.Length > 0)
			{
				if (binSizeStream.Eof)
					binSizeStream.Reset();
				
				var spread = expandedStream.Read();
				
				var dstStream = spread.GetStream();
				dstStream.Length = binSizeStream.Read();
				
				while (!dstStream.Eof)
				{
					if (srcStream.Eof)
						srcStream.Reset();

					int slicesRead = srcStream.Read(buffer, 0, Math.Min(buffer.Length, dstStream.Length));
					dstStream.Write(buffer, 0, slicesRead);
				}
			}
		}
		
		public static void Flatten<T>(
			this ISpread<ISpread<T>> expandedSpread,
			ISpread<T> reducedSpread,
			ISpread<int> binSizeSpread
		)
		{
			var binSizeStream = binSizeSpread.GetStream();
			var dstStream = reducedSpread.GetStream();
			var expandedStream = expandedSpread.GetStream();
			
			var buffer = dstStream.CreateWriteBuffer();
			
			binSizeStream.Length = expandedStream.Length;
			
			int dstStreamLength = 0;
			while (!expandedStream.Eof)
			{
				dstStreamLength += expandedStream.Read().SliceCount;
			}
			dstStream.Length = dstStreamLength;
			
			expandedStream.Reset();
			
			while (!expandedStream.Eof)
			{
				var spread = expandedStream.Read();
				var srcStream = spread.GetStream();
				while (!srcStream.Eof)
				{
					int dataCount = srcStream.Read(buffer, 0, buffer.Length);
					dstStream.Write(buffer, 0, dataCount);
				}
				
				binSizeStream.Write(srcStream.Length);
			}
		}
		
		private static int NormalizeBinSize(int sliceCount, int binSize)
		{
			if (binSize < 0)
			{
				return DivByBinSize(sliceCount, Math.Abs(binSize));
			}
			
			return binSize;
		}
		
		private static int DivByBinSize(int sliceCount, int binSize)
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