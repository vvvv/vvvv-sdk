using System;

namespace VVVV.Core
{
    public interface IConverter
    {
        /// <summary>
        /// Converts an object of type TFrom to an object of type TTo.
        /// </summary>
        bool Convert<TFrom, TTo>(TFrom fromItem, out TTo toItem);
    }
}
