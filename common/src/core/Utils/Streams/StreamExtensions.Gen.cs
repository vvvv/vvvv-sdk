using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Utils.Streams
{
    public static partial class StreamExtensions
    {
        public static short Sum(this IInStream<short> stream)
        {
            short result = 0;
            using (var buffer = MemoryPool<short>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                        result += buffer[i];
                }
            }
            return result;
        }

        public static ushort Sum(this IInStream<ushort> stream)
        {
            ushort result = 0;
            using (var buffer = MemoryPool<ushort>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                        result += buffer[i];
                }
            }
            return result;
        }

        public static int Sum(this IInStream<int> stream)
        {
            int result = 0;
            using (var buffer = MemoryPool<int>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                        result += buffer[i];
                }
            }
            return result;
        }

        public static uint Sum(this IInStream<uint> stream)
        {
            uint result = 0;
            using (var buffer = MemoryPool<uint>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                        result += buffer[i];
                }
            }
            return result;
        }

        public static long Sum(this IInStream<long> stream)
        {
            long result = 0;
            using (var buffer = MemoryPool<long>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                        result += buffer[i];
                }
            }
            return result;
        }

        public static ulong Sum(this IInStream<ulong> stream)
        {
            ulong result = 0;
            using (var buffer = MemoryPool<ulong>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                        result += buffer[i];
                }
            }
            return result;
        }

        public static float Sum(this IInStream<float> stream)
        {
            float result = 0;
            using (var buffer = MemoryPool<float>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                        result += buffer[i];
                }
            }
            return result;
        }

        public static double Sum(this IInStream<double> stream)
        {
            double result = 0;
            using (var buffer = MemoryPool<double>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                        result += buffer[i];
                }
            }
            return result;
        }


        public static short Max(this IInStream<short> stream)
        {
            var result = short.MinValue;
            using (var buffer = MemoryPool<short>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                    {
                        var item = buffer[i];
                        if (item > result)
                            result = item;
                    }
                }
            }
            return result;
        }

        public static ushort Max(this IInStream<ushort> stream)
        {
            var result = ushort.MinValue;
            using (var buffer = MemoryPool<ushort>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                    {
                        var item = buffer[i];
                        if (item > result)
                            result = item;
                    }
                }
            }
            return result;
        }

        public static int Max(this IInStream<int> stream)
        {
            var result = int.MinValue;
            using (var buffer = MemoryPool<int>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                    {
                        var item = buffer[i];
                        if (item > result)
                            result = item;
                    }
                }
            }
            return result;
        }

        public static uint Max(this IInStream<uint> stream)
        {
            var result = uint.MinValue;
            using (var buffer = MemoryPool<uint>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                    {
                        var item = buffer[i];
                        if (item > result)
                            result = item;
                    }
                }
            }
            return result;
        }

        public static long Max(this IInStream<long> stream)
        {
            var result = long.MinValue;
            using (var buffer = MemoryPool<long>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                    {
                        var item = buffer[i];
                        if (item > result)
                            result = item;
                    }
                }
            }
            return result;
        }

        public static ulong Max(this IInStream<ulong> stream)
        {
            var result = ulong.MinValue;
            using (var buffer = MemoryPool<ulong>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                    {
                        var item = buffer[i];
                        if (item > result)
                            result = item;
                    }
                }
            }
            return result;
        }

        public static float Max(this IInStream<float> stream)
        {
            var result = float.MinValue;
            using (var buffer = MemoryPool<float>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                    {
                        var item = buffer[i];
                        if (item > result)
                            result = item;
                    }
                }
            }
            return result;
        }

        public static double Max(this IInStream<double> stream)
        {
            var result = double.MinValue;
            using (var buffer = MemoryPool<double>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                    {
                        var item = buffer[i];
                        if (item > result)
                            result = item;
                    }
                }
            }
            return result;
        }


        public static short Min(this IInStream<short> stream)
        {
            var result = short.MaxValue;
            using (var buffer = MemoryPool<short>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                    {
                        var item = buffer[i];
                        if (item < result)
                            result = item;
                    }
                }
            }
            return result;
        }

        public static ushort Min(this IInStream<ushort> stream)
        {
            var result = ushort.MaxValue;
            using (var buffer = MemoryPool<ushort>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                    {
                        var item = buffer[i];
                        if (item < result)
                            result = item;
                    }
                }
            }
            return result;
        }

        public static int Min(this IInStream<int> stream)
        {
            var result = int.MaxValue;
            using (var buffer = MemoryPool<int>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                    {
                        var item = buffer[i];
                        if (item < result)
                            result = item;
                    }
                }
            }
            return result;
        }

        public static uint Min(this IInStream<uint> stream)
        {
            var result = uint.MaxValue;
            using (var buffer = MemoryPool<uint>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                    {
                        var item = buffer[i];
                        if (item < result)
                            result = item;
                    }
                }
            }
            return result;
        }

        public static long Min(this IInStream<long> stream)
        {
            var result = long.MaxValue;
            using (var buffer = MemoryPool<long>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                    {
                        var item = buffer[i];
                        if (item < result)
                            result = item;
                    }
                }
            }
            return result;
        }

        public static ulong Min(this IInStream<ulong> stream)
        {
            var result = ulong.MaxValue;
            using (var buffer = MemoryPool<ulong>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                    {
                        var item = buffer[i];
                        if (item < result)
                            result = item;
                    }
                }
            }
            return result;
        }

        public static float Min(this IInStream<float> stream)
        {
            var result = float.MaxValue;
            using (var buffer = MemoryPool<float>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                    {
                        var item = buffer[i];
                        if (item < result)
                            result = item;
                    }
                }
            }
            return result;
        }

        public static double Min(this IInStream<double> stream)
        {
            var result = double.MaxValue;
            using (var buffer = MemoryPool<double>.GetBuffer())
            using (var reader = stream.GetReader())
            {
                while (!reader.Eos)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < itemsRead; i++)
                    {
                        var item = buffer[i];
                        if (item < result)
                            result = item;
                    }
                }
            }
            return result;
        }

    }
}