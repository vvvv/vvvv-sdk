using System;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Generic
{
    /// <summary>
    /// Copies values or spread of values. Used in nodes which need to copy data.
    /// </summary>
    /// <typeparam name="T">The type of value this copier is able to copy.</typeparam>
    public abstract class Copier<T>
    {
        class ImmutableTypeCopier : Copier<T>
        {
            public override T Copy(T value)
            {
                return value;
            }
        }

        /// <summary>
        /// Returns a copier for immutable types.
        /// </summary>
        public static readonly Copier<T> Immutable = new ImmutableTypeCopier();

        /// <summary>
        /// Creates a copy of the given value.
        /// </summary>
        /// <param name="value">The value to copy.</param>
        /// <returns>The copied value.</returns>
        public abstract T Copy(T value);

        /// <summary>
        /// Creates a copy of the given spread.
        /// </summary>
        /// <param name="spread">The spread to copy.</param>
        /// <returns>The copied spread.</returns>
        public virtual ISpread<T> CopySpread(ISpread<T> spread)
        {
            var result = new Spread<T>(spread.SliceCount);
            var resultBuffer = result.Stream.Buffer;
            var buffer = spread.Stream.Buffer;
            for (int i = 0; i < spread.SliceCount; i++)
                resultBuffer[i] = Copy(buffer[i]);
            return result;
        }
    }
}
