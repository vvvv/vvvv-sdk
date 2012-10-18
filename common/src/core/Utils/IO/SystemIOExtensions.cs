 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{
    public static class SystemIOExtensions
    {
        public static byte[] ReadBytes(this BinaryReader reader, int fieldSize, ByteOrder byteOrder)
        {
            byte[] bytes = new byte[fieldSize];
            if (byteOrder == ByteOrder.LittleEndian)
                return reader.ReadBytes(fieldSize);
            else
            {
                for (int i = fieldSize - 1; i > -1; i--)
                    bytes[i] = reader.ReadByte();
                return bytes;
            }
        }

        public static void Write(this BinaryWriter writer, byte[] value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
            {
                for (int i = value.Length - 1; i > -1; i--)
                    writer.Write(value[i]);
            }
        }

        public static Int16 ReadInt16(this BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return (Int16)reader.ReadInt16();
            else
                return BitConverter.ToInt16(ReadBytes(reader, sizeof(Int16), ByteOrder.BigEndian), 0);
        }

        public static void Write(this BinaryWriter writer, Int16 value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
                writer.Write(BitConverter.GetBytes(value), byteOrder);
        }

        public static UInt16 ReadUInt16(this BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return (UInt16)reader.ReadUInt16();
            else
                return BitConverter.ToUInt16(ReadBytes(reader, sizeof(UInt16), ByteOrder.BigEndian), 0);
        }

        public static void Write(this BinaryWriter writer, UInt16 value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
                writer.Write(BitConverter.GetBytes(value), byteOrder);
        }

        public static Int32 ReadInt32(this BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return (Int32)reader.ReadInt32();
            else
                return BitConverter.ToInt32(ReadBytes(reader, sizeof(Int32), ByteOrder.BigEndian), 0);
        }

        public static void Write(this BinaryWriter writer, Int32 value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
                writer.Write(BitConverter.GetBytes(value), byteOrder);
        }

        public static UInt32 ReadUInt32(this BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return (UInt32)reader.ReadUInt32();
            else
                return BitConverter.ToUInt32(ReadBytes(reader, sizeof(UInt32), ByteOrder.BigEndian), 0);
        }

        public static void Write(this BinaryWriter writer, UInt32 value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
                writer.Write(BitConverter.GetBytes(value), byteOrder);
        }

        public static Int64 ReadInt64(this BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return (Int64)reader.ReadInt64();
            else
                return BitConverter.ToInt64(ReadBytes(reader, sizeof(Int64), ByteOrder.BigEndian), 0);
        }

        public static void Write(this BinaryWriter writer, Int64 value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
                writer.Write(BitConverter.GetBytes(value), byteOrder);
        }

        public static UInt64 ReadUInt64(this BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return (UInt64)reader.ReadUInt64();
            else
                return BitConverter.ToUInt64(ReadBytes(reader, sizeof(UInt64), ByteOrder.BigEndian), 0);
        }

        public static void Write(this BinaryWriter writer, UInt64 value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
                writer.Write(BitConverter.GetBytes(value), byteOrder);
        }

        public static Single ReadSingle(this BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return (Single)reader.ReadSingle();
            else
                return BitConverter.ToSingle(ReadBytes(reader, sizeof(Single), ByteOrder.BigEndian), 0);
        }

        public static void Write(this BinaryWriter writer, Single value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
                writer.Write(BitConverter.GetBytes(value), byteOrder);
        }

        public static Double ReadDouble(this BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return (Double)reader.ReadDouble();
            else
                return BitConverter.ToDouble(ReadBytes(reader, sizeof(Double), ByteOrder.BigEndian), 0);
        }

        public static void Write(this BinaryWriter writer, Double value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                writer.Write(value);
            else
                writer.Write(BitConverter.GetBytes(value), byteOrder);
        }

    }
}
 
