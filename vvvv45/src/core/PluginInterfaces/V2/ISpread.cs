using System;
using System.Collections.Generic;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// Common interface to the underlaying input/output/config pins.
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
            var s = "";
            
           	foreach (var slice in spread)
           		s += String.Format("{0}, ", slice.ToString());
            	
            return s;	
        }
     }
}