using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace vvvv.Utils
{
    public class TTypeConverter
    {
        public static byte[] GetArray(string str)
        {
            Encoding enc = Encoding.GetEncoding("ISO-8859-1");
            return enc.GetBytes(str);
        }

        public static T GetStructure<T>(string str)
        {
            Encoding enc = Encoding.GetEncoding("ISO-8859-1");
            byte[] bytes = enc.GetBytes(str);
            return GetStructure<T>(bytes);
        }

        #region Get Structure
        public static T GetStructure<T>(Byte[] b)
        {
            Type t = typeof(T);
            IntPtr ptr;
            ptr = Marshal.AllocCoTaskMem(b.Length);
            Marshal.Copy(b, 0, ptr, b.Length);
            T res = (T)Marshal.PtrToStructure(ptr, typeof(T));
            return res;
        }
        #endregion
    }
}
