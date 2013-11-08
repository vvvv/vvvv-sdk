using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace VVVV.Nodes
{
    class ByteConvert
    {
        
        public static object ToStruct(byte[] data, Type structType)
        {
        
            GCHandle MyGC = GCHandle.Alloc(data, GCHandleType.Pinned);
            object structure = Marshal.PtrToStructure(MyGC.AddrOfPinnedObject(), structType);

            MyGC.Free();

            return structure;

        }

        public static byte[] ToBytes(object structure, Type structType)
        {

            IntPtr Ptr = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
            byte[] bytes = new byte[Marshal.SizeOf(structure)];

            Marshal.StructureToPtr(structure, Ptr, false);
            Marshal.Copy(Ptr, bytes, 0, Marshal.SizeOf(structure));

            Marshal.FreeHGlobal(Ptr);

            return bytes;

        }

    }
}
