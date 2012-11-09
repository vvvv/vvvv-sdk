using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using System.IO;
using VVVV.Utils.Streams;
using System.Runtime.InteropServices;

namespace VVVV.Nodes.Raw
{
    [PluginInfo(Name = "AsValue", Category = "Raw", Help = "Interprets a sequence of bytes as a value.")]
    public class AsValueNode : IPluginEvaluate
    {
#pragma warning disable 0649
        [Input("Input")]
        IInStream<Stream> FInputStreams;

        [Input("Format", DefaultEnumEntry = "Double")]
        IInStream<IInStream<ValueTypeFormat>> FFormats;

        [Input("Byte Order", Visibility = PinVisibility.OnlyInspector)]
        IInStream<ByteOrder> FByteOrder;

        [Output("Output")]
        IOutStream<IInStream<double>> FOutputs; 
#pragma warning restore

        public void Evaluate(int spreadMax)
        {
            FOutputs.Length = StreamUtils.GetSpreadMax(FInputStreams, FFormats, FByteOrder);
            var buffer = MemoryPool<double>.GetArray();
            try
            {
                using (var reader = FInputStreams.GetCyclicReader())
                using (var formatReader = FFormats.GetCyclicReader())
                using (var byteOrderReader = FByteOrder.GetCyclicReader())
                using (var writer = FOutputs.GetWriter())
                {
                    while (!writer.Eos)
                    {
                        var inputStream = reader.Read();
                        var formatStream = formatReader.Read();
                        var byteOrder = byteOrderReader.Read();
                        BufferedIOStream<double> outputStream = null;

                        if (formatStream.Length == 1)
                        {
                            var format = formatStream.Single();
                            outputStream = ConvertAllAtOnce(inputStream, buffer, format, byteOrder);
                        }
                        else
                        {
                            outputStream = ConvertOneByOne(inputStream, formatStream, byteOrder).ToStream();
                        }
                        writer.Write(outputStream);
                    }
                }
            }
            finally
            {
                MemoryPool<double>.PutArray(buffer);
            }
        }

        static BufferedIOStream<double> ConvertAllAtOnce(Stream srcStream, double[] buffer, ValueTypeFormat format, ByteOrder byteOrder)
        {
            var binaryReader = new BinaryReader(srcStream);
            var sizeOfT = ValueTypeFormatUtils.SizeOf(format);
            var dstStreamLength = (int)srcStream.Length / sizeOfT;
            var dstStream = new BufferedIOStream<double>(dstStreamLength);
            dstStream.Length = dstStreamLength;
            using (var dstStreamWriter = dstStream.GetWriter())
            {
                while (!dstStreamWriter.Eos)
                {
                    switch (format)
                    {
                        case ValueTypeFormat.Boolean:
                            var booleanBuffer = binaryReader.ReadBooleans(buffer.Length, byteOrder);
                            for (int i = 0; i < booleanBuffer.Length; i++)
                                buffer[i] = (double)Convert.ChangeType(booleanBuffer[i], TypeCode.Double);
                            dstStreamWriter.Write(buffer, 0, booleanBuffer.Length);
                            break;
                        case ValueTypeFormat.SByte:
                            var sbyteBuffer = binaryReader.ReadBytes(buffer.Length);
                            for (int i = 0; i < sbyteBuffer.Length; i++)
                                buffer[i] = (double)(SByte)sbyteBuffer[i];
                            dstStreamWriter.Write(buffer, 0, sbyteBuffer.Length);
                            break;
                        case ValueTypeFormat.Byte:
                            var byteBuffer = binaryReader.ReadBytes(buffer.Length);
                            for (int i = 0; i < byteBuffer.Length; i++)
                                buffer[i] = (double)byteBuffer[i];
                            dstStreamWriter.Write(buffer, 0, byteBuffer.Length);
                            break;
                        case ValueTypeFormat.Int16:
                            var int16Buffer = binaryReader.ReadInt16s(buffer.Length, byteOrder);
                            for (int i = 0; i < int16Buffer.Length; i++)
                                buffer[i] = (double)int16Buffer[i];
                            dstStreamWriter.Write(buffer, 0, int16Buffer.Length);
                            break;
                        case ValueTypeFormat.UInt16:
                            var uint16Buffer = binaryReader.ReadUInt16s(buffer.Length, byteOrder);
                            for (int i = 0; i < uint16Buffer.Length; i++)
                                buffer[i] = (double)uint16Buffer[i];
                            dstStreamWriter.Write(buffer, 0, uint16Buffer.Length);
                            break;
                        case ValueTypeFormat.Int32:
                            var int32Buffer = binaryReader.ReadInt32s(buffer.Length, byteOrder);
                            for (int i = 0; i < int32Buffer.Length; i++)
                                buffer[i] = (double)int32Buffer[i];
                            dstStreamWriter.Write(buffer, 0, int32Buffer.Length);
                            break;
                        case ValueTypeFormat.UInt32:
                            var uint32Buffer = binaryReader.ReadUInt32s(buffer.Length, byteOrder);
                            for (int i = 0; i < uint32Buffer.Length; i++)
                                buffer[i] = (double)uint32Buffer[i];
                            dstStreamWriter.Write(buffer, 0, uint32Buffer.Length);
                            break;
                        case ValueTypeFormat.Int64:
                            var int64Buffer = binaryReader.ReadInt64s(buffer.Length, byteOrder);
                            for (int i = 0; i < int64Buffer.Length; i++)
                                buffer[i] = (double)int64Buffer[i];
                            dstStreamWriter.Write(buffer, 0, int64Buffer.Length);
                            break;
                        case ValueTypeFormat.UInt64:
                            var uint64Buffer = binaryReader.ReadUInt64s(buffer.Length, byteOrder);
                            for (int i = 0; i < uint64Buffer.Length; i++)
                                buffer[i] = (double)uint64Buffer[i];
                            dstStreamWriter.Write(buffer, 0, uint64Buffer.Length);
                            break;
                        case ValueTypeFormat.Single:
                            var singleBuffer = binaryReader.ReadSingles(buffer.Length, byteOrder);
                            for (int i = 0; i < singleBuffer.Length; i++)
                                buffer[i] = (double)singleBuffer[i];
                            dstStreamWriter.Write(buffer, 0, singleBuffer.Length);
                            break;
                        case ValueTypeFormat.Double:
                            var itemRead = binaryReader.Read(buffer, 0, buffer.Length, byteOrder);
                            dstStreamWriter.Write(buffer, 0, itemRead);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            return dstStream;
        }

        static IEnumerable<double> ConvertOneByOne(Stream srcStream, IInStream<ValueTypeFormat> formatStream, ByteOrder byteOrder)
        {
            var binaryReader = new BinaryReader(srcStream);
            using (var formatStreamReader = formatStream.GetCyclicReader())
            {
                while (srcStream.Position < srcStream.Length)
                {
                    var format = formatStreamReader.Read();
                    double result;
                    switch (format)
                    {
                        case ValueTypeFormat.Boolean:
                            result = (Double)Convert.ChangeType(binaryReader.ReadBoolean(byteOrder), typeof(Double));
                            break;
                        case ValueTypeFormat.SByte:
                            result = (Double)binaryReader.ReadSByte();
                            break;
                        case ValueTypeFormat.Byte:
                            result = (Double)binaryReader.ReadByte();
                            break;
                        case ValueTypeFormat.Int16:
                            result = (Double)binaryReader.ReadInt16(byteOrder);
                            break;
                        case ValueTypeFormat.UInt16:
                            result = (Double)binaryReader.ReadUInt16(byteOrder);
                            break;
                        case ValueTypeFormat.Int32:
                            result = (Double)binaryReader.ReadInt32(byteOrder);
                            break;
                        case ValueTypeFormat.UInt32:
                            result = (Double)binaryReader.ReadUInt32(byteOrder);
                            break;
                        case ValueTypeFormat.Int64:
                            result = (Double)binaryReader.ReadInt64(byteOrder);
                            break;
                        case ValueTypeFormat.UInt64:
                            result = (Double)binaryReader.ReadUInt64(byteOrder);
                            break;
                        case ValueTypeFormat.Single:
                            result = (Double)binaryReader.ReadSingle(byteOrder);
                            break;
                        case ValueTypeFormat.Double:
                            result = binaryReader.ReadDouble(byteOrder);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    yield return result;
                }
            }
        }
    }
}
