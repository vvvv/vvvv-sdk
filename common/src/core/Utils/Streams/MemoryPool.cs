using System;
using System.Collections.Generic;

namespace VVVV.Utils.Streams
{
	public static class MemoryPool<T>
	{
		private static readonly Dictionary<int, Stack<Array>> FPool = new Dictionary<int, Stack<Array>>();
		
		public static Memory<T> GetMemory(int length)
		{
			if (!StreamUtils.IsPowerOfTwo(length))
			{
				throw new ArgumentException("Paremeter length must be a power of two.");
			}
			
			lock (FPool)
			{
				Stack<Array> stack = null;
				if (!FPool.TryGetValue(length, out stack))
				{
					stack = new Stack<Array>();
					FPool[length] = stack;
				}
				
				if (stack.Count == 0)
				{
					return new Memory<T>(stack, new T[length]);
				}
				else
				{
					return new Memory<T>(stack, stack.Pop() as T[]);
				}
			}
		}
	}
	
	public class Memory<T> : IDisposable
	{
		private readonly Stack<Array> FStack;
		
		internal Memory(Stack<Array> stack, T[] array)
		{
			FStack = stack;
			Array = array;
		}
		
		public T[] Array
		{
			get;
			private set;
		}
		
		public void Dispose()
		{
			FStack.Push(Array);
		}
	}
}
