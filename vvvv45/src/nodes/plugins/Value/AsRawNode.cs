using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using System.IO;
using System.ComponentModel.Composition;

namespace VVVV.Nodes.Value
{
    [PluginInfo(Name = "AsRaw", Category = "Value", Help = "Returns a value as a sequence of bytes.")]
    public class AsRawNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
#pragma warning disable 0649
        [Input("Input")]
        IInStream<IInStream<double>> FInputStreams;

        [Input("Format", DefaultEnumEntry = "Double")]
        IInStream<IInStream<ValueTypeFormat>> FFormats;

        [Input("Byte Order", Visibility = PinVisibility.OnlyInspector)]
        IInStream<ByteOrder> FByteOrder;

        [Output("Output", AutoFlush = false)]
        IIOStream<Stream> FOutputStreams; 
#pragma warning restore

        public void OnImportsSatisfied()
        {
            FOutputStreams.Length = 0;
        }

        public unsafe void Evaluate(int spreadMax)
        {
            spreadMax = StreamUtils.GetSpreadMax(FInputStreams, FFormats, FByteOrder);
            FOutputStreams.ResizeAndDispose(spreadMax, () => new MemoryComStream());
            var inputBuffer = MemoryPool<double>.GetArray();
            try
            {
                using (var inputReader = FInputStreams.GetCyclicReader())
                using (var formatReader = FFormats.GetCyclicReader())
                using (var byteOrderReader = FByteOrder.GetCyclicReader())
                {
                    foreach (MemoryStream outputStream in FOutputStreams)
                    {
                        var inputStream = inputReader.Read();
                        var formatStream = formatReader.Read();
                        var byteOrder = byteOrderReader.Read();
                        if (formatStream.Length == 1)
                        {
                            var format = formatStream.Single();
                            ConvertAllAtOnce(inputStream, outputStream, inputBuffer, format, byteOrder);
                        }
                        else
                            ConvertOneByOne(inputStream, outputStream, formatStream, byteOrder);
                    }
                }
                FOutputStreams.Flush(true);
            }
            finally
            {
                MemoryPool<double>.PutArray(inputBuffer);
            }
        }

        static void ConvertAllAtOnce(IInStream<double> srcStream, MemoryStream dstStream, double[] buffer, ValueTypeFormat format, ByteOrder byteOrder)
        {
            var sizeOfFormat = ValueTypeFormatUtils.SizeOf(format);
            dstStream.SetLength(srcStream.Length * sizeOfFormat);
            dstStream.Position = 0;
            var binaryWriter = new BinaryWriter(dstStream);
            using (var reader = srcStream.GetReader())
            {
                var itemsToRead = reader.Length;
                while (itemsToRead > 0)
                {
                    var itemsRead = reader.Read(buffer, 0, buffer.Length);
                    switch (format)
                    {
                        case ValueTypeFormat.Boolean:
                            var booleanBuffer = new Boolean[itemsRead];
                            for (int i = 0; i < booleanBuffer.Length; i++)
                                booleanBuffer[i] = (Boolean)Convert.ChangeType(buffer[i], typeof(Boolean));
                            binaryWriter.Write(booleanBuffer, 0, itemsRead, byteOrder);
                            break;
                        case ValueTypeFormat.SByte:
                            var sbyteBuffer = new Byte[itemsRead];
                            unchecked
                            {
                                for (int i = 0; i < sbyteBuffer.Length; i++)
                                    sbyteBuffer[i] = (Byte)(SByte)buffer[i];
                            }
                            binaryWriter.Write(sbyteBuffer, 0, itemsRead);
                            break;
                        case ValueTypeFormat.Byte:
                            var byteBuffer = new Byte[itemsRead];
                            for (int i = 0; i < byteBuffer.Length; i++)
                                byteBuffer[i] = (Byte)buffer[i];
                            binaryWriter.Write(byteBuffer, 0, itemsRead);
                            break;
                        case ValueTypeFormat.Int16:
                            var int16Buffer = new Int16[itemsRead];
                            for (int i = 0; i < int16Buffer.Length; i++)
                                int16Buffer[i] = (Int16)buffer[i];
                            binaryWriter.Write(int16Buffer, 0, itemsRead, byteOrder);
                            break;
                        case ValueTypeFormat.UInt16:
                            var uint16Buffer = new UInt16[itemsRead];
                            for (int i = 0; i < uint16Buffer.Length; i++)
                                uint16Buffer[i] = (UInt16)buffer[i];
                            binaryWriter.Write(uint16Buffer, 0, itemsRead, byteOrder);
                            break;
                        case ValueTypeFormat.Int32:
                            var int32Buffer = new Int32[itemsRead];
                            for (int i = 0; i < int32Buffer.Length; i++)
                                int32Buffer[i] = (Int32)buffer[i];
                            binaryWriter.Write(int32Buffer, 0, itemsRead, byteOrder);
                            break;
                        case ValueTypeFormat.UInt32:
                            var uint32Buffer = new UInt32[itemsRead];
                            for (int i = 0; i < uint32Buffer.Length; i++)
                                uint32Buffer[i] = (UInt32)buffer[i];
                            binaryWriter.Write(uint32Buffer, 0, itemsRead, byteOrder);
                            break;
                        case ValueTypeFormat.Int64:
                            var int64Buffer = new Int64[itemsRead];
                            for (int i = 0; i < int64Buffer.Length; i++)
                                int64Buffer[i] = (Int64)buffer[i];
                            binaryWriter.Write(int64Buffer, 0, itemsRead, byteOrder);
                            break;
                        case ValueTypeFormat.UInt64:
                            var uint64Buffer = new UInt64[itemsRead];
                            for (int i = 0; i < uint64Buffer.Length; i++)
                                uint64Buffer[i] = (UInt64)buffer[i];
                            binaryWriter.Write(uint64Buffer, 0, itemsRead, byteOrder);
                            break;
                        case ValueTypeFormat.Single:
                            var singleBuffer = new Single[itemsRead];
                            for (int i = 0; i < singleBuffer.Length; i++)
                                singleBuffer[i] = (Single)buffer[i];
                            binaryWriter.Write(singleBuffer, 0, itemsRead, byteOrder);
                            break;
                        case ValueTypeFormat.Double:
                            binaryWriter.Write(buffer, 0, itemsRead, byteOrder);
                            break;
                        default:
                            break;
                    }
                    itemsToRead -= itemsRead;
                }
            }
        }

        static void ConvertOneByOne(IInStream<double> srcStream, MemoryStream dstStream, IInStream<ValueTypeFormat> formatStream, ByteOrder byteOrder)
        {
            dstStream.SetLength(srcStream.Length);
            dstStream.Position = 0;
            var binaryWriter = new BinaryWriter(dstStream);
            using (var formatStreamReader = formatStream.GetCyclicReader())
            {
                foreach (var value in srcStream)
                {
                    var format = formatStreamReader.Read();
                    switch (format)
                    {
                        case ValueTypeFormat.Boolean:
                            binaryWriter.Write((Boolean)Convert.ChangeType(value, typeof(Boolean)), byteOrder);
                            break;
                        case ValueTypeFormat.SByte:
                            binaryWriter.Write((SByte)value);
                            break;
                        case ValueTypeFormat.Byte:
                            binaryWriter.Write((Byte)value);
                            break;
                        case ValueTypeFormat.Int16:
                            binaryWriter.Write((Int16)value, byteOrder);
                            break;
                        case ValueTypeFormat.UInt16:
                            binaryWriter.Write((UInt16)value, byteOrder);
                            break;
                        case ValueTypeFormat.Int32:
                            binaryWriter.Write((Int32)value, byteOrder);
                            break;
                        case ValueTypeFormat.UInt32:
                            binaryWriter.Write((UInt32)value, byteOrder);
                            break;
                        case ValueTypeFormat.Int64:
                            binaryWriter.Write((Int64)value, byteOrder);
                            break;
                        case ValueTypeFormat.UInt64:
                            binaryWriter.Write((UInt64)value, byteOrder);
                            break;
                        case ValueTypeFormat.Single:
                            binaryWriter.Write((Single)value, byteOrder);
                            break;
                        case ValueTypeFormat.Double:
                            binaryWriter.Write(value, byteOrder);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }
    }
}
