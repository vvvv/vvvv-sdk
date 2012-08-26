using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VVVV.Utils.Streams
{
    [ComVisible(false)]
	public static class MemoryPool<T>
	{
		private static readonly Stack<T[]> FStack = new Stack<T[]>();
		
		public static T[] GetArray()
		{
			lock (FStack)
			{
				if (FStack.Count == 0)
				{
					return new T[StreamUtils.BUFFER_SIZE];
				}
				else
				{
					return FStack.Pop();
				}
			}
		}
		
		public static void PutArray(T[] array)
		{
            // Clear the array before putting it back (break references)
            if (!typeof(T).IsPrimitive)
            {
                Array.Clear(array, 0, array.Length);
            }
			lock (FStack)
			{
				FStack.Push(array);
			}
		}
	}
}
