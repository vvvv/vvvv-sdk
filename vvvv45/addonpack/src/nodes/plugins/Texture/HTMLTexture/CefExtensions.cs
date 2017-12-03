using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xilium.CefGlue;

namespace VVVV.Nodes.Texture
{
    static class CefExtensions
    {
        public static void SetFrameIdentifier(this CefProcessMessage message, long identifier, int index = 0)
        {
            int low, high;
            MakeInt(identifier, out low, out high);
            var arguments = message.Arguments;
            arguments.SetInt(index, low);
            arguments.SetInt(index + 1, high);
        }

        public static long GetFrameIdentifier(this CefProcessMessage message, int index = 0)
        {
            var arguments = message.Arguments;
            var low = arguments.GetInt(index);
            var high = arguments.GetInt(index + 1);
            return MakeLong(low, high);
        }

        private static long MakeLong(int low, int high)
        {
            return (long)(high << 32) | (long)(low);
        }

        private static void MakeInt(long value, out int low, out int high)
        {
            low = (int)(value);
            high = (int)(value >> 32);
        }
    }
}
