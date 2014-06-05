using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace VVVV.Utils
{
    public static class Memory
    {
#if X86
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false), SuppressUnmanagedCodeSecurity]
        public static unsafe extern void* Copy(void* dst, void* src, uint lengthInBytes);

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false), SuppressUnmanagedCodeSecurity]
        public static extern IntPtr Copy(IntPtr dst, IntPtr src, uint lengthInBytes);
#else
        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static unsafe extern void Copy(void* dst, void* src, uint lengthInBytes);

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern void Copy(IntPtr dst, IntPtr src, uint lengthInBytes);
#endif
    }
}
