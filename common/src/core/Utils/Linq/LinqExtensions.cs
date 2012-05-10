
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static bool None<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            // none means: tell me if a certain assumption is false for all elements
            return enumerable.All(t => !predicate(t));
        }
	}
}
