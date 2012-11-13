using System;

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
    }

}
