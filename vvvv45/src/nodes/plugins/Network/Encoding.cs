using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using System.Collections.Generic;
using System.IO;

namespace VVVV.Nodes.Network
{
    // Compatible with System.TypeCode
    public enum Format
    {
        Empty = 0,
        Boolean = 3,
        SByte = 5,
        Byte = 6,
        Int16 = 7,
        UInt16 = 8,
        Int32 = 9,
        UInt32 = 10,
        Int64 = 11,
        UInt64 = 12,
        Single = 13,
        Double = 14
    }

    public abstract class EncodingNode : IPluginEvaluate
    {
        [Input("Encoding", EnumName = "Encoding", Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<EnumEntry> FEncodingIn;

        [Input("Byte Order", Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<ByteOrder> FByteOrder;

        static bool registered;
        static EncodingNode()
        {
            if (!registered)
            {
                registered = true;

                var encodings = Encoding.GetEncodings()
                    .Select(info => info.Name)
                    .OrderBy(n => n)
                    .ToArray();
                EnumManager.UpdateEnum("Encoding", "Windows-1252", encodings);
            }
        }

        public abstract void Evaluate(int spreadMax);
    }

    [PluginInfo(Name = "Encode", Category = "Network", Help = "Basic template with one string in/out", Tags = "")]
    public class ValueEncodeNode : EncodingNode
    {
        [Input("Input")]
        ISpread<ISpread<double>> FInput;

        [Input("Format")]
        ISpread<ISpread<Format>> FFormat;

        [Output("Output")]
        ISpread<string> FOutput;

        [Output("Offset")]
        ISpread<ISpread<int>> FOffset;

        //called when data for any output pin is requested
        public override void Evaluate(int spreadMax)
        {
            FOutput.SliceCount = FOffset.SliceCount = FInput.CombineWith(FFormat).CombineWith(FEncodingIn);

            for (int i = 0; i < FOutput.SliceCount; i++)
            {
                spreadMax = FInput[i].CombineWith(FFormat[i]);
                FOffset[i].SliceCount = spreadMax;

                var encoding = Encoding.GetEncoding(FEncodingIn[i].Name);
                var input = FInput[i];
                var format = FFormat[i];
                var resultStream = new MemoryStream();
                var binaryWriter = new BinaryWriter(resultStream);
                for (int j = 0; j < spreadMax; j++)
                {
                    FOffset[i][j] = (int)resultStream.Position;
                    var f = format[j];
                    if (f == Format.Empty) continue;
                    var convertedValue = Convert.ChangeType(input[j], (TypeCode)f);
                    switch (f)
                    {
                        case Format.Empty:
                            break;
                        case Format.Boolean:
                            binaryWriter.Write((Boolean)convertedValue);
                            break;
                        case Format.SByte:
                            binaryWriter.Write((SByte)convertedValue);
                            break;
                        case Format.Byte:
                            binaryWriter.Write((Byte)convertedValue);
                            break;
                        case Format.Int16:
                            binaryWriter.Write((Int16)convertedValue, FByteOrder[i]);
                            break;
                        case Format.UInt16:
                            binaryWriter.Write((UInt16)convertedValue, FByteOrder[i]);
                            break;
                        case Format.Int32:
                            binaryWriter.Write((Int32)convertedValue, FByteOrder[i]);
                            break;
                        case Format.UInt32:
                            binaryWriter.Write((UInt32)convertedValue, FByteOrder[i]);
                            break;
                        case Format.Int64:
                            binaryWriter.Write((Int64)convertedValue, FByteOrder[i]);
                            break;
                        case Format.UInt64:
                            binaryWriter.Write((UInt64)convertedValue, FByteOrder[i]);
                            break;
                        case Format.Single:
                            binaryWriter.Write((Single)convertedValue, FByteOrder[i]);
                            break;
                        case Format.Double:
                            binaryWriter.Write((Double)convertedValue, FByteOrder[i]);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                FOutput[i] = encoding.GetString(resultStream.GetBuffer(), 0, (int)resultStream.Length);
            }
        }
    }

    [PluginInfo(Name = "Decode", Category = "Network", Help = "Basic template with one string in/out", Tags = "")]
    public class ValueDecodeNode : EncodingNode
    {
        [Input("Input")]
        ISpread<string> FInput;

        [Input("Offset")]
        ISpread<ISpread<int>> FOffset;

        [Input("Format")]
        ISpread<ISpread<Format>> FFormat;

        [Output("Output")]
        ISpread<ISpread<double>> FOutput;

        //called when data for any output pin is requested
        public unsafe override void Evaluate(int spreadMax)
        {
            FOutput.SliceCount = FInput.CombineWith(FOffset).CombineWith(FFormat).CombineWith(FEncodingIn);

            for (int i = 0; i < FOutput.SliceCount; i++)
            {
                var encoding = Encoding.GetEncoding(FEncodingIn[i].Name);
                var inputStream = new MemoryStream(encoding.GetBytes(FInput[i]), false);
                var offset = FOffset[i];
                var format = FFormat[i];
                var output = FOutput[i];
                spreadMax = offset.CombineWith(format);
                output.SliceCount = spreadMax;

                var binaryReader = new BinaryReader(inputStream);
                for (int j = 0; j < spreadMax; j++)
                {
                    var f = format[j];
                    if (f == Format.Empty) continue;
                    inputStream.Position = offset[j];
                    object value = null;
                    switch (f)
                    {
                        case Format.Empty:
                            break;
                        case Format.Boolean:
                            value = binaryReader.ReadBoolean();
                            break;
                        case Format.SByte:
                            value = binaryReader.ReadSByte();
                            break;
                        case Format.Byte:
                            value = binaryReader.ReadByte();
                            break;
                        case Format.Int16:
                            value = binaryReader.ReadInt16(FByteOrder[i]);
                            break;
                        case Format.UInt16:
                            value = binaryReader.ReadUInt16(FByteOrder[i]);
                            break;
                        case Format.Int32:
                            value = binaryReader.ReadInt32(FByteOrder[i]);
                            break;
                        case Format.UInt32:
                            value = binaryReader.ReadUInt32(FByteOrder[i]);
                            break;
                        case Format.Int64:
                            value = binaryReader.ReadInt64(FByteOrder[i]);
                            break;
                        case Format.UInt64:
                            value = binaryReader.ReadUInt64(FByteOrder[i]);
                            break;
                        case Format.Single:
                            value = binaryReader.ReadSingle(FByteOrder[i]);
                            break;
                        case Format.Double:
                            value = binaryReader.ReadDouble(FByteOrder[i]);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    output[j] = (double)Convert.ChangeType(value, TypeCode.Double);
                }
            }
        }
    }
}
