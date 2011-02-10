using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// Common interface to the underlying input/output/config pins.
	/// Set/Get, Read/Write methods are only implemented when it makes sense.
	/// </summary>
	[ComVisible(false)]
	public interface ISpread<T> : IEnumerable<T>
	{
		/// <summary>
		/// Provides read/write access to the actual data.
		/// </summary>
		T this[int index]
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
	}
}