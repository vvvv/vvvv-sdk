using System;
using System.Collections.Generic;

namespace VVVV.PluginInterfaces.V2
{
	public interface ISpread<T> : IEnumerable<T>
	{
		T this[int index]
		{
			get;
			set;
		}
		
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