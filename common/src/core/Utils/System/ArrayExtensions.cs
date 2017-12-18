using System;
using System.Runtime.InteropServices;

namespace VVVV.Utils
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Initializes the array with the defaultValue.
        /// </summary>
        /// <param name="array">The array to initialize.</param>
        /// <param name="defaultVaue">The value to initialize the array with.</param>
        public static void Init<T>(this T[] array, T defaultVaue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = defaultVaue;
            }
        }
        
        /// <summary>
        /// Fills the array with the defaultValue from index to index + length.
        /// </summary>
        /// <param name="array">The array to fill.</param>
        /// <param name="index">The index into the array where the fill operation starts.</param>
        /// <param name="length">The fill operation ends at index + length.</param>
        /// <param name="defaultVaue">The value to fill the array with.</param>
        public static void Fill<T>(this T[] array, int index, int length, T defaultVaue)
        {
            for (int i = index; i < index + length; i++)
            {
                array[i] = defaultVaue;
            }
        }
        
        /// <summary>
        /// Replicates the values between startIndex and endIndex.
        /// </summary>
        /// <param name="array">The array to do the replication on.</param>
        /// <param name="startIndex">Defines from where to begin the replication.</param>
        /// <param name="endIndex">Defines where to end the replication.</param>
        /// <param name="times">Defines the number of replications of the values between startIndex and endIndex.</param>
        public static void Replicate<T>(this T[] array, int startIndex, int endIndex, int times)
        {
            int dst = endIndex;
            for (int i = 0; i < times; i++)
            {
                for (int j = startIndex; j < endIndex; j++)
                {
                    array[dst++] = array[j];
                }
            }
        }

        /// <summary>
        /// Swaps the elements at positions i and j.
        /// </summary>
        /// <param name="array">The array to do the swap on.</param>
        /// <param name="i">The i'th element.</param>
        /// <param name="j">The j'th element.</param>
        public static void Swap<T>(this T[] array, int i, int j)
        {
            var item = array[i];
            array[i] = array[j];
            array[j] = item;
        }

        // See VVVV.Utils.dll.config for mapping under Mono
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int memcmp(byte[] b1, byte[] b2, UIntPtr count);

        /// <summary>
        /// Compares the content of the two arrays for equality.
        /// </summary>
        /// <param name="a">The first array to compare.</param>
        /// <param name="b">The second array to compare.</param>
        /// <returns>True if a and b contain the same data otherwise false.</returns>
        public static bool ContentEquals(this byte[] a, byte[] b)
        {
            return a.Length == b.Length && memcmp(a, b, new UIntPtr((uint)a.Length)) == 0;
        }
    }

}
