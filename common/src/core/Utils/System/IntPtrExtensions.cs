using System;
using System.Runtime.InteropServices;

namespace VVVV.Utils
{
	public static class IntPtrExtensions
	{
		public static IntPtr Move(this IntPtr ptr, int cbSize)
		{
			return new IntPtr(ptr.ToInt64() + cbSize);
		}

		public static IntPtr Move<T>(this IntPtr ptr)
		{
			return ptr.Move(Marshal.SizeOf(typeof(T)));
		}

		public static T ElementAt<T>(this IntPtr ptr, int index)
		{
			var offset = Marshal.SizeOf(typeof(T))*index;
			var offsetPtr = ptr.Move(offset);
			return (T)Marshal.PtrToStructure(offsetPtr, typeof(T));
		}
	}
}