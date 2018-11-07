using System;
using System.Collections.Generic;

namespace System.Linq
{
	public static class EnumerableExtensions
	{
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
		{
            return new HashSet<T>(source);
		}
	}
}
