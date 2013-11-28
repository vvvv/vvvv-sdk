#include "defines.il"

.class public abstract sealed VVVV.Utils.Memory
{
#ifndef X86
	.method public static void Copy(void* dst, void* src, unsigned int32 lengthInBytes) cil managed
	{
		.maxstack 3
		
		ldarg.0
		ldarg.1
		ldarg.2
		cpblk
		ret
	}

    .method public static void Copy(native int dst, native int src, unsigned int32 lengthInBytes) cil managed
	{
		.maxstack 3
		
		ldarg.0
		ldarg.1
		ldarg.2
		cpblk
		ret
	}
#endif
}