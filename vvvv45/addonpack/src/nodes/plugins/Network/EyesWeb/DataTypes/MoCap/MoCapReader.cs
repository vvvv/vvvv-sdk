using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace vvvv.Utils
{
    public class MoCapReader
    {
        public static MoCapDataList ReadMessage(byte[] b)
        {
            IntPtr pt = Marshal.AllocCoTaskMem(b.Length);
            Marshal.Copy(b, 0, pt, b.Length);

            MoCapDataList res = new MoCapDataList();


            unsafe
            {
                int total = 0;
                sbyte* offset = (sbyte*)pt;
                uint nbitems = *(uint*)offset;

                //puint = p;
                //GetValue<uint>(b, 0, 4);
                offset += 4;
                total += 4;

                for (int i = 0; i < nbitems; i++)
                {
                    uint labellen = *(uint*)offset;
                    int ill = Convert.ToInt32(labellen);
                    offset += 4;
                    total += 4;
                    string sname = new string(offset, 0, ill);
                    offset += labellen;
                    total += ill;

                    uint desclen = *(uint*)offset;
                    ill = Convert.ToInt32(desclen);
                    offset += 4;
                    total += 4;
                    string sdesc = new string(offset, 0, ill);
                    offset += ill;
                    total += ill;




                    MOCAP_ITEM item = *(MOCAP_ITEM*)offset;
                    offset += Marshal.SizeOf(item);
                    total += Marshal.SizeOf(item);

                    MoCapDataItem di = new MoCapDataItem();
                    di.Description = sdesc;
                    di.Name = sname;
                    di.Item = item;

                    res.Add(sname, di);
                }

                return res;
            }

        }
    }
}
