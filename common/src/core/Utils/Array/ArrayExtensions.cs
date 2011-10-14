using System;

namespace VVVV.Utils
{
    public static class ArrayExtensions
    {
        public static void Init<T>(this T[] array, T defaultVaue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = defaultVaue;
            }
        }
        
        public static void Fill<T>(this T[] array, int index, int length, T defaultVaue)
        {
            for (int i = index; i < index + length; i++)
            {
                array[i] = defaultVaue;
            }
        }
    }

}
