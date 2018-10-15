using System;
using System.Runtime.CompilerServices;

namespace VVVV.Utils
{
    public static unsafe class Memory
    {
        public static void Copy(void* dst, void* src, uint lengthInBytes) => Unsafe.CopyBlock(dst, src, lengthInBytes);

        public static void Copy(IntPtr dst, IntPtr src, uint lengthInBytes) => Unsafe.CopyBlock(dst.ToPointer(), src.ToPointer(), lengthInBytes);
    }
}
