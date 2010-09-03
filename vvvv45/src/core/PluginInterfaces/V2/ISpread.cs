using System;
using System.Collections.Generic;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// Common interface to the underlying input/output/config pins.
	/// Set/Get, Read/Write methods are only implemented when it makes sense.
	/// </summary>
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

    public static class SpreadExtensions
    {        
        public static string ToString<T>(this ISpread<T> spread)
        {
        	return spread.AsString();
        }
        
        public static string AsString<T>(this ISpread<T> spread)
        {
            var s = "";
                        	
           	for (int i = 0; i < spread.SliceCount-1; i++) 
           		s += String.Format("{0}, ", spread[i].ToString());

           	if (spread.SliceCount > 0)
           		s += spread[spread.SliceCount-1].ToString();
           		
//           	foreach (int i; islice in spread)
//           		s += String.Format("{0}, ", slice.ToString());
            	
            return s;	
        }
        
        public static int CombineSpreads(this int c1, int c2)
        {        	
        	if (c1 == 0 || c2 == 0)
        		return 0;
        	else
        		return Math.Max(c1, c2);
        }
        
        public static int CombineWith<V>(this int c1, ISpread<V> spread2)
        {
        	return CombineSpreads(c1, spread2.SliceCount);
		}
        
        public static int CombineWith<U, V>(this ISpread<U> spread1, ISpread<V> spread2)
        {
        	return CombineSpreads(spread1.SliceCount, spread2.SliceCount);
        }
	}
}