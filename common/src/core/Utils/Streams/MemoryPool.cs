using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VVVV.Utils.Streams
{
    [ComVisible(false)]
	public static class MemoryPool<T>
	{
        public struct Buffer : IDisposable
        {
            private readonly T[] Array;

            public Buffer(T[] array)
            {
                this.Array = array;
            }

            public int Length 
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get { return Array.Length; } 
            }

            public T this[int i]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get { return Array[i]; }
            }

            public static implicit operator T[](Buffer buffer)
            {
                return buffer.Array;
            }
            
            public void Dispose()
            {
                MemoryPool<T>.PutArray(this.Array);
            }
        }
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
            // Clear the array before putting it back (break references)
            if (!typeof(T).IsValueType)
            {
                Array.Clear(array, 0, array.Length);
            }
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

        public static Buffer GetBuffer(int length = StreamUtils.BUFFER_SIZE)
        {
            var array = GetArray(length);
            return new Buffer(array);
        }
	}
}
