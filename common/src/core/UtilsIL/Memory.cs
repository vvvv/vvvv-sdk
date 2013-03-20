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
#else
        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static unsafe extern void Copy(void* dst, void* src, uint lengthInBytes);
#endif
    }
}
