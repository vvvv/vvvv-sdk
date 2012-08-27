using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VVVV.Utils.Streams
{
    [ComVisible(false)]
	public static class MemoryPool<T>
	{
		private static readonly Dictionary<int, Stack<T[]>> FPool = new Dictionary<int, Stack<T[]>>();
		
		public static T[] GetArray(int length = StreamUtils.BUFFER_SIZE)
		{
			lock (FPool)
			{
				Stack<T[]> stack = null;
				if (!FPool.TryGetValue(length, out stack))
				{
					stack = new Stack<T[]>();
					FPool[length] = stack;
				}
				
				if (stack.Count == 0)
				{
					return new T[length];
				}
				else
				{
					return stack.Pop();
				}
			}
		}
		
		public static void PutArray(T[] array)
		{
			lock (FPool)
			{
				Stack<T[]> stack = null;
				if (!FPool.TryGetValue(array.Length, out stack))
				{
					stack = new Stack<T[]>();
					FPool[array.Length] = stack;
				}
				stack.Push(array);
			}
		}
	}
}
