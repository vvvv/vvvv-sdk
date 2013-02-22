
using System;
using System.Collections.Generic;

/// <summary>
/// Utils to work with Linq.
/// </summary>
namespace VVVV.Utils.Linq
{
	public static class LinqExtensions
	{
		public static IEnumerable<T> AsDepthFirstEnumerable<T>(this T parent) where T: IEnumerable<T>
		{
			yield return parent;
			foreach (var subTree in parent)
			{
				foreach (var child in subTree.AsDepthFirstEnumerable())
				{
					yield return child;
				}
			}
		}
		
		public static IEnumerable<T> AsDepthFirstEnumerable<T>(this T parent, Func<T, IEnumerable<T>> childSelector)
		{
			yield return parent;
			foreach (var subTree in childSelector(parent))
			{
				foreach (var child in subTree.AsDepthFirstEnumerable(childSelector))
				{
					yield return child;
				}
			}
		}

        public static IEnumerable<T> Intersperse<T>(this IEnumerable<T> source, T element)
        {
            var first = true;
            foreach (var item in source)
            {
                if (first)
                {
                    first = false;
                    yield return item;
                }
                else
                {
                    yield return element;
                    yield return item;
                }
            }
        }
	}
}
