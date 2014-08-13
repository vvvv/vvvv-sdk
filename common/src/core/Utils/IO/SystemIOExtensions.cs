 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using VVVV.Utils;
using VVVV.Utils.Streams;

namespace System.IO
{
    public static class SystemIOExtensions
    {
        public static bool StreamEquals(this Stream stream1, Stream stream2)
        {
            if (stream1 == null && stream2 != null)
                return false;
            if (stream1 != null && stream2 == null)
                return false;
            if (stream1.Equals(stream2))
                return true;

            stream1.Position = 0;
            stream2.Position = 0;
            try
            {
                using (var buffer1 = MemoryPool<byte>.GetBuffer())
                using (var buffer2 = MemoryPool<byte>.GetBuffer())
                {
                    var bufferSize = buffer1.Length;
                    while (true)
                    {
                        var count1 = stream1.Read(buffer1, 0, bufferSize);
                        // Check the case where stream1 and stream2 use the same
                        // underlying stream.
                        if (stream1.Position == stream2.Position)
	                        return true;
                        var count2 = stream2.Read(buffer2, 0, bufferSize);
                        if (count1 != count2)
                            return false;
                        if (count1 == 0)
                            return true;
                        if (count1 != bufferSize)
                        {
                            for (int i = 0; i < count1; i++)
                                if (buffer1[i] != buffer2[i])
                                    return false;
                        }
                        else
                        {
                            if (!ArrayExtensions.ContentEquals(buffer1, buffer2))
                                return false;
                        }
                    }
                }
            }
            finally
            {
                stream1.Position = 0;
                stream2.Position = 0;
            }
        }

        public static byte[] ReadValue(this BinaryReader reader, int fieldSize, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return reader.ReadBytes(fieldSize);
            else
            {
                byte[] bytes = new byte[fieldSize];
                for (int i = fieldSize - 1; i > -1; i--)
                    bytes[i] = reader.ReadByte();
                return bytes;
            }
        }

        public static int ReadValues(this BinaryReader reader, byte[] buffer, int fieldSize, ByteOrder byteOrder)
        {
            var bytesRead = reader.Read(buffer, 0, buffer.Length);
            if (byteOrder == ByteOrder.BigEndian && fieldSize > 1)
            {
                for (int i = 0; i < bytesRead; i += fieldSize)
                    for (int j = 0; j < fieldSize / 2; j++)
                          buffer.Swap(i + j, i + fieldSize - j - 1);
            }
            return bytesRead;
        }

        public static void WriteValue(this BinaryWriter writer, byte[] value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
            {
                for (int i = value.Length - 1; i > -1; i--)
                    writer.Write(value[i]);
            }
        }

        public static void WriteValues(this BinaryWriter writer, byte[] values, int fieldSize, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(values);
            else
            {
                for (int i = 0; i < values.Length; i += fieldSize)
                  for (int j = i + fieldSize - 1; j >= i; j--)
                      writer.Write(values[j]);
            }
        }

        public static Boolean ReadBoolean(this BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return (Boolean)reader.ReadBoolean();
            else
                return BitConverter.ToBoolean(reader.ReadValue(sizeof(Boolean), ByteOrder.BigEndian), 0);
        }

        public static int Read(this BinaryReader reader, Boolean[] buffer, int offset, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(Boolean)];
            var itemsRead = reader.ReadValues(byteBuffer, sizeof(Boolean), byteOrder);
            Buffer.BlockCopy(byteBuffer, 0, buffer, offset, itemsRead);
            return itemsRead / sizeof(Boolean);
        }

        public static Boolean[] ReadBooleans(this BinaryReader reader, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(Boolean)];
            var itemsRead = reader.ReadValues(byteBuffer, sizeof(Boolean), byteOrder);
            var resultBuffer = new Boolean[itemsRead / sizeof(Boolean)];
            Buffer.BlockCopy(byteBuffer, 0, resultBuffer, 0, resultBuffer.Length * sizeof(Boolean));
            return resultBuffer;
        }

        public static void Write(this BinaryWriter writer, Boolean value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
                writer.WriteValue(BitConverter.GetBytes(value), byteOrder);
        }

        public static void Write(this BinaryWriter writer, Boolean[] buffer, int offset, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(Boolean)];
            Buffer.BlockCopy(buffer, offset, byteBuffer, 0, byteBuffer.Length);
            writer.WriteValues(byteBuffer, sizeof(Boolean), byteOrder);
        }

        public static Int16 ReadInt16(this BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return (Int16)reader.ReadInt16();
            else
                return BitConverter.ToInt16(reader.ReadValue(sizeof(Int16), ByteOrder.BigEndian), 0);
        }

        public static int Read(this BinaryReader reader, Int16[] buffer, int offset, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(Int16)];
            var itemsRead = reader.ReadValues(byteBuffer, sizeof(Int16), byteOrder);
            Buffer.BlockCopy(byteBuffer, 0, buffer, offset, itemsRead);
            return itemsRead / sizeof(Int16);
        }

        public static Int16[] ReadInt16s(this BinaryReader reader, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(Int16)];
            var itemsRead = reader.ReadValues(byteBuffer, sizeof(Int16), byteOrder);
            var resultBuffer = new Int16[itemsRead / sizeof(Int16)];
            Buffer.BlockCopy(byteBuffer, 0, resultBuffer, 0, resultBuffer.Length * sizeof(Int16));
            return resultBuffer;
        }

        public static void Write(this BinaryWriter writer, Int16 value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
                writer.WriteValue(BitConverter.GetBytes(value), byteOrder);
        }

        public static void Write(this BinaryWriter writer, Int16[] buffer, int offset, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(Int16)];
            Buffer.BlockCopy(buffer, offset, byteBuffer, 0, byteBuffer.Length);
            writer.WriteValues(byteBuffer, sizeof(Int16), byteOrder);
        }

        public static UInt16 ReadUInt16(this BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return (UInt16)reader.ReadUInt16();
            else
                return BitConverter.ToUInt16(reader.ReadValue(sizeof(UInt16), ByteOrder.BigEndian), 0);
        }

        public static int Read(this BinaryReader reader, UInt16[] buffer, int offset, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(UInt16)];
            var itemsRead = reader.ReadValues(byteBuffer, sizeof(UInt16), byteOrder);
            Buffer.BlockCopy(byteBuffer, 0, buffer, offset, itemsRead);
            return itemsRead / sizeof(UInt16);
        }

        public static UInt16[] ReadUInt16s(this BinaryReader reader, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(UInt16)];
            var itemsRead = reader.ReadValues(byteBuffer, sizeof(UInt16), byteOrder);
            var resultBuffer = new UInt16[itemsRead / sizeof(UInt16)];
            Buffer.BlockCopy(byteBuffer, 0, resultBuffer, 0, resultBuffer.Length * sizeof(UInt16));
            return resultBuffer;
        }

        public static void Write(this BinaryWriter writer, UInt16 value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
                writer.WriteValue(BitConverter.GetBytes(value), byteOrder);
        }

        public static void Write(this BinaryWriter writer, UInt16[] buffer, int offset, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(UInt16)];
            Buffer.BlockCopy(buffer, offset, byteBuffer, 0, byteBuffer.Length);
            writer.WriteValues(byteBuffer, sizeof(UInt16), byteOrder);
        }

        public static Int32 ReadInt32(this BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return (Int32)reader.ReadInt32();
            else
                return BitConverter.ToInt32(reader.ReadValue(sizeof(Int32), ByteOrder.BigEndian), 0);
        }

        public static int Read(this BinaryReader reader, Int32[] buffer, int offset, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(Int32)];
            var itemsRead = reader.ReadValues(byteBuffer, sizeof(Int32), byteOrder);
            Buffer.BlockCopy(byteBuffer, 0, buffer, offset, itemsRead);
            return itemsRead / sizeof(Int32);
        }

        public static Int32[] ReadInt32s(this BinaryReader reader, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(Int32)];
            var itemsRead = reader.ReadValues(byteBuffer, sizeof(Int32), byteOrder);
            var resultBuffer = new Int32[itemsRead / sizeof(Int32)];
            Buffer.BlockCopy(byteBuffer, 0, resultBuffer, 0, resultBuffer.Length * sizeof(Int32));
            return resultBuffer;
        }

        public static void Write(this BinaryWriter writer, Int32 value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
                writer.WriteValue(BitConverter.GetBytes(value), byteOrder);
        }

        public static void Write(this BinaryWriter writer, Int32[] buffer, int offset, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(Int32)];
            Buffer.BlockCopy(buffer, offset, byteBuffer, 0, byteBuffer.Length);
            writer.WriteValues(byteBuffer, sizeof(Int32), byteOrder);
        }

        public static UInt32 ReadUInt32(this BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return (UInt32)reader.ReadUInt32();
            else
                return BitConverter.ToUInt32(reader.ReadValue(sizeof(UInt32), ByteOrder.BigEndian), 0);
        }

        public static int Read(this BinaryReader reader, UInt32[] buffer, int offset, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(UInt32)];
            var itemsRead = reader.ReadValues(byteBuffer, sizeof(UInt32), byteOrder);
            Buffer.BlockCopy(byteBuffer, 0, buffer, offset, itemsRead);
            return itemsRead / sizeof(UInt32);
        }

        public static UInt32[] ReadUInt32s(this BinaryReader reader, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(UInt32)];
            var itemsRead = reader.ReadValues(byteBuffer, sizeof(UInt32), byteOrder);
            var resultBuffer = new UInt32[itemsRead / sizeof(UInt32)];
            Buffer.BlockCopy(byteBuffer, 0, resultBuffer, 0, resultBuffer.Length * sizeof(UInt32));
            return resultBuffer;
        }

        public static void Write(this BinaryWriter writer, UInt32 value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
                writer.WriteValue(BitConverter.GetBytes(value), byteOrder);
        }

        public static void Write(this BinaryWriter writer, UInt32[] buffer, int offset, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(UInt32)];
            Buffer.BlockCopy(buffer, offset, byteBuffer, 0, byteBuffer.Length);
            writer.WriteValues(byteBuffer, sizeof(UInt32), byteOrder);
        }

        public static Int64 ReadInt64(this BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return (Int64)reader.ReadInt64();
            else
                return BitConverter.ToInt64(reader.ReadValue(sizeof(Int64), ByteOrder.BigEndian), 0);
        }

        public static int Read(this BinaryReader reader, Int64[] buffer, int offset, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(Int64)];
            var itemsRead = reader.ReadValues(byteBuffer, sizeof(Int64), byteOrder);
            Buffer.BlockCopy(byteBuffer, 0, buffer, offset, itemsRead);
            return itemsRead / sizeof(Int64);
        }

        public static Int64[] ReadInt64s(this BinaryReader reader, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(Int64)];
            var itemsRead = reader.ReadValues(byteBuffer, sizeof(Int64), byteOrder);
            var resultBuffer = new Int64[itemsRead / sizeof(Int64)];
            Buffer.BlockCopy(byteBuffer, 0, resultBuffer, 0, resultBuffer.Length * sizeof(Int64));
            return resultBuffer;
        }

        public static void Write(this BinaryWriter writer, Int64 value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
                writer.WriteValue(BitConverter.GetBytes(value), byteOrder);
        }

        public static void Write(this BinaryWriter writer, Int64[] buffer, int offset, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(Int64)];
            Buffer.BlockCopy(buffer, offset, byteBuffer, 0, byteBuffer.Length);
            writer.WriteValues(byteBuffer, sizeof(Int64), byteOrder);
        }

        public static UInt64 ReadUInt64(this BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return (UInt64)reader.ReadUInt64();
            else
                return BitConverter.ToUInt64(reader.ReadValue(sizeof(UInt64), ByteOrder.BigEndian), 0);
        }

        public static int Read(this BinaryReader reader, UInt64[] buffer, int offset, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(UInt64)];
            var itemsRead = reader.ReadValues(byteBuffer, sizeof(UInt64), byteOrder);
            Buffer.BlockCopy(byteBuffer, 0, buffer, offset, itemsRead);
            return itemsRead / sizeof(UInt64);
        }

        public static UInt64[] ReadUInt64s(this BinaryReader reader, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(UInt64)];
            var itemsRead = reader.ReadValues(byteBuffer, sizeof(UInt64), byteOrder);
            var resultBuffer = new UInt64[itemsRead / sizeof(UInt64)];
            Buffer.BlockCopy(byteBuffer, 0, resultBuffer, 0, resultBuffer.Length * sizeof(UInt64));
            return resultBuffer;
        }

        public static void Write(this BinaryWriter writer, UInt64 value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
                writer.WriteValue(BitConverter.GetBytes(value), byteOrder);
        }

        public static void Write(this BinaryWriter writer, UInt64[] buffer, int offset, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(UInt64)];
            Buffer.BlockCopy(buffer, offset, byteBuffer, 0, byteBuffer.Length);
            writer.WriteValues(byteBuffer, sizeof(UInt64), byteOrder);
        }

        public static Single ReadSingle(this BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return (Single)reader.ReadSingle();
            else
                return BitConverter.ToSingle(reader.ReadValue(sizeof(Single), ByteOrder.BigEndian), 0);
        }

        public static int Read(this BinaryReader reader, Single[] buffer, int offset, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(Single)];
            var itemsRead = reader.ReadValues(byteBuffer, sizeof(Single), byteOrder);
            Buffer.BlockCopy(byteBuffer, 0, buffer, offset, itemsRead);
            return itemsRead / sizeof(Single);
        }

        public static Single[] ReadSingles(this BinaryReader reader, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(Single)];
            var itemsRead = reader.ReadValues(byteBuffer, sizeof(Single), byteOrder);
            var resultBuffer = new Single[itemsRead / sizeof(Single)];
            Buffer.BlockCopy(byteBuffer, 0, resultBuffer, 0, resultBuffer.Length * sizeof(Single));
            return resultBuffer;
        }

        public static void Write(this BinaryWriter writer, Single value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
                writer.WriteValue(BitConverter.GetBytes(value), byteOrder);
        }

        public static void Write(this BinaryWriter writer, Single[] buffer, int offset, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(Single)];
            Buffer.BlockCopy(buffer, offset, byteBuffer, 0, byteBuffer.Length);
            writer.WriteValues(byteBuffer, sizeof(Single), byteOrder);
        }

        public static Double ReadDouble(this BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return (Double)reader.ReadDouble();
            else
                return BitConverter.ToDouble(reader.ReadValue(sizeof(Double), ByteOrder.BigEndian), 0);
        }

        public static int Read(this BinaryReader reader, Double[] buffer, int offset, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(Double)];
            var itemsRead = reader.ReadValues(byteBuffer, sizeof(Double), byteOrder);
            Buffer.BlockCopy(byteBuffer, 0, buffer, offset, itemsRead);
            return itemsRead / sizeof(Double);
        }

        public static Double[] ReadDoubles(this BinaryReader reader, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(Double)];
            var itemsRead = reader.ReadValues(byteBuffer, sizeof(Double), byteOrder);
            var resultBuffer = new Double[itemsRead / sizeof(Double)];
            Buffer.BlockCopy(byteBuffer, 0, resultBuffer, 0, resultBuffer.Length * sizeof(Double));
            return resultBuffer;
        }

        public static void Write(this BinaryWriter writer, Double value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
                writer.WriteValue(BitConverter.GetBytes(value), byteOrder);
        }

        public static void Write(this BinaryWriter writer, Double[] buffer, int offset, int count, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var byteBuffer = new byte[count * sizeof(Double)];
            Buffer.BlockCopy(buffer, offset, byteBuffer, 0, byteBuffer.Length);
            writer.WriteValues(byteBuffer, sizeof(Double), byteOrder);
        }

    }
}
 
