using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Utils.Win32
{
    public static class Extensions
    {
        /// <summary>
        /// Extracts lower 16-bit word from an 32-bit int.
        /// </summary>
        public static short LoWord(this int number)
        {
            unchecked
            {
                return (short)number;
            }
        }

        /// <summary>
        /// Extracts higher 16-bit word from an 32-bit int.
        /// </summary>
        public static short HiWord(this int number)
        {
            unchecked
            {
                return (short)(number >> 16);
            }
        }
    }
}
