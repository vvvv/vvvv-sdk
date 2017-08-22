using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.WIC;
using System.IO;

namespace VVVV.Nodes.ImagePlayer
{
    partial class FrameDecoder
    {
        class BitmapFrameDecoder : FrameDecoder
        {
            private static ImagingFactory Factory = new ImagingFactory();
            private static Dictionary<Guid, Format> FPixelToTextureFormatMap = new Dictionary<Guid, Format>();

            static BitmapFrameDecoder()
            {
                foreach (Format format in Enum.GetValues(typeof(Format)))
                {
                    try
                    {
                        var pixelFormat = TextureToPixelFormat(format);
                        FPixelToTextureFormatMap.Add(pixelFormat, format);
                    }
                    catch (NotSupportedException)
                    {
                        // Ignore
                    }
                }
            }

            private DataPointer FBuffer;
            private int FStride;

            public BitmapFrameDecoder(Func<Device, int, int, int, Format, Usage, Texture> textureFactory, MemoryPool memoryPool, Stream stream, Format preferedFormat)
                : base(textureFactory, memoryPool, preferedFormat)
            {
                var dataStream = stream as SharpDX.DataStream;
                using (var wicStream = new WICStream(Factory, new SharpDX.DataPointer(dataStream.DataPointer, (int)dataStream.Length)))
                using (var decoder = new BitmapDecoder(Factory, wicStream, DecodeOptions.CacheOnLoad))
                using (var frame = decoder.GetFrame(0))
                {
                    // Choose pixel format from file
                    if (preferedFormat == Format.Unknown)
                    {
                        try
                        {
                            FChosenFormat = PixelToTextureFormat(frame.PixelFormat);
                        }
                        catch (NotSupportedException)
                        {
                            // The format as given by the file is not supported by DirectX
                            FChosenFormat = Format.A8R8G8B8;
                        }
                    }

                    FChosenFormat = GetFallbackFormatIfPreferedFormatIsNotSupported(FChosenFormat);
                    var chosenPixelFormat = TextureToPixelFormat(FChosenFormat);
                    Width = frame.Size.Width;
                    Height = frame.Size.Height;
                    FStride = PixelFormat.GetStride(chosenPixelFormat, Width);
                    var length = FStride * Height;

                    FBuffer = memoryPool.UnmanagedPool.GetMemory(length);

                    if (frame.PixelFormat != chosenPixelFormat)
                    {
                        using (var converter = new FormatConverter(Factory))
                        {
                            converter.Initialize(frame, chosenPixelFormat);
                            converter.CopyPixels(FStride, FBuffer);
                        }
                    }
                    else
                    {
                        frame.CopyPixels(FStride, FBuffer);
                    }
                }
                memoryPool.StreamPool.PutStream(stream);
            }

            public override void Dispose()
            {
                if (FBuffer != null)
                {
                    FMemoryPool.UnmanagedPool.PutMemory(FBuffer);
                    FBuffer = DataPointer.Zero;
                }
                base.Dispose();
            }

            protected override Texture Decode(Device device, Format format)
            {
                if (format != FChosenFormat)
                {
                    // Ouch - need to convert once more on render thread
                    var chosenPixelFormat = TextureToPixelFormat(FChosenFormat);
                    // TODO: Remember this for the future
                    using (var frame = new Bitmap(Factory, Width, Height, chosenPixelFormat, new DataRectangle(FBuffer.Pointer, FStride), FBuffer.Size))
                    {
                        var pixelFormat = TextureToPixelFormat(format);
                        FStride = PixelFormat.GetStride(pixelFormat, Width);
                        var length = FStride * Height;
                        var newBuffer = FMemoryPool.UnmanagedPool.GetMemory(length);
                        using (var converter = new FormatConverter(Factory))
                        {
                            converter.Initialize(frame, pixelFormat);
                            converter.CopyPixels(FStride, newBuffer);
                        }
                        FMemoryPool.UnmanagedPool.PutMemory(FBuffer);
                        FBuffer = newBuffer;
                    }
                    FChosenFormat = format;
                }

                var usage = Usage.Dynamic & ~Usage.AutoGenerateMipMap;
                var texture = FTextureFactory(device, Width, Height, 1, format, usage);
                var dataRectangle = texture.LockRectangle(0, LockFlags.Discard);
                try
                {
                    if (dataRectangle.Pitch == FStride)
                    {
                        Utilities.CopyMemory(dataRectangle.DataPointer, FBuffer.Pointer, FBuffer.Size);
                    }
                    else
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            var src = FBuffer.Pointer + y * FStride;
                            var dst = dataRectangle.DataPointer + y * dataRectangle.Pitch;
                            Utilities.CopyMemory(dst, src, FStride);
                        }
                    }
                }
                finally
                {
                    texture.UnlockRectangle(0);
                }
                return texture;
            }

            public static IEnumerable<string> SupportedFileExtensions
            {
                get
                {
                    yield return ".bmp";
                    yield return ".gif";
                    yield return ".ico";
                    yield return ".jpg";
                    yield return ".jpeg";
                    yield return ".png";
                    yield return ".tif";
                    yield return ".tiff";
                    yield return ".wmp";
                    yield break;
                }
            }

            static Guid TextureToPixelFormat(Format format)
            {
                switch (format)
                {
                    case Format.A1:
                        return PixelFormat.FormatBlackWhite;
                    case Format.A16B16G16R16:
                        return PixelFormat.Format64bppRGBA;
                    case Format.A16B16G16R16F:
                        return PixelFormat.Format64bppRGBAHalf;
                    case Format.A1R5G5B5:
                        return PixelFormat.Format16bppBGRA5551;
                    case Format.A2B10G10R10:
                        return PixelFormat.Format32bppRGBA1010102;
                    case Format.A2R10G10B10:
                        throw new NotSupportedException();
                    case Format.A2W10V10U10:
                        throw new NotSupportedException();
                    case Format.A32B32G32R32F:
                        return PixelFormat.Format128bppRGBAFloat;
                    case Format.A4L4:
                        throw new NotSupportedException();
                    case Format.A4R4G4B4:
                        throw new NotSupportedException();
                    case Format.A8:
                        return PixelFormat.Format8bppAlpha;
                    case Format.A8B8G8R8:
                        return PixelFormat.Format32bppRGBA;
                    case Format.A8L8:
                        throw new NotSupportedException();
                    case Format.A8P8:
                        throw new NotSupportedException();
                    case Format.A8R3G3B2:
                        throw new NotSupportedException();
                    case Format.A8R8G8B8:
                        return PixelFormat.Format32bppBGRA;
                    case Format.BinaryBuffer:
                        throw new NotSupportedException();
                    case Format.D15S1:
                        throw new NotSupportedException();
                    case Format.D16:
                        throw new NotSupportedException();
                    case Format.D16Lockable:
                        throw new NotSupportedException();
                    case Format.D24S8:
                        throw new NotSupportedException();
                    case Format.D24SingleS8:
                        throw new NotSupportedException();
                    case Format.D24X4S4:
                        throw new NotSupportedException();
                    case Format.D24X8:
                        throw new NotSupportedException();
                    case Format.D32:
                        throw new NotSupportedException();
                    case Format.D32Lockable:
                        throw new NotSupportedException();
                    case Format.D32SingleLockable:
                        throw new NotSupportedException();
                    case Format.Dxt1:
                        throw new NotSupportedException();
                    case Format.Dxt2:
                        throw new NotSupportedException();
                    case Format.Dxt3:
                        throw new NotSupportedException();
                    case Format.Dxt4:
                        throw new NotSupportedException();
                    case Format.Dxt5:
                        throw new NotSupportedException();
                    case Format.G16R16:
                        throw new NotSupportedException();
                    case Format.G16R16F:
                        throw new NotSupportedException();
                    case Format.G32R32F:
                        throw new NotSupportedException();
                    case Format.G8R8_G8B8:
                        throw new NotSupportedException();
                    case Format.Index16:
                        throw new NotSupportedException();
                    case Format.Index32:
                        throw new NotSupportedException();
                    case Format.L16:
                        return PixelFormat.Format16bppGray;
                    case Format.L6V5U5:
                        throw new NotSupportedException();
                    case Format.L8:
                        return PixelFormat.Format8bppGray;
                    case Format.MtA2B10G10R10XrBias:
                        throw new NotSupportedException();
                    case Format.MtCxV8U8:
                        throw new NotSupportedException();
                    case Format.Multi2Argb8:
                        throw new NotSupportedException();
                    case Format.P8:
                        return PixelFormat.Format8bppIndexed;
                    case Format.Q16W16V16U16:
                        throw new NotSupportedException();
                    case Format.Q8W8V8U8:
                        throw new NotSupportedException();
                    case Format.R16F:
                        return PixelFormat.Format16bppGrayHalf;
                    case Format.R32F:
                        return PixelFormat.Format32bppGrayFloat;
                    case Format.R3G3B2:
                        throw new NotSupportedException();
                    case Format.R5G6B5:
                        return PixelFormat.Format16bppBGR565;
                    case Format.R8G8B8:
                        return PixelFormat.Format24bppBGR;
                    case Format.R8G8_B8G8:
                        throw new NotSupportedException();
                    case Format.S8Lockable:
                        throw new NotSupportedException();
                    case Format.Unknown:
                        return PixelFormat.FormatDontCare;
                    case Format.Uyvy:
                        throw new NotSupportedException();
                    case Format.V16U16:
                        throw new NotSupportedException();
                    case Format.V8U8:
                        throw new NotSupportedException();
                    case Format.VertexData:
                        throw new NotSupportedException();
                    case Format.X1R5G5B5:
                        return PixelFormat.Format16bppBGR555;
                    case Format.X4R4G4B4:
                        throw new NotSupportedException();
                    case Format.X8B8G8R8:
                        throw new NotSupportedException();
                    case Format.X8L8V8U8:
                        throw new NotSupportedException();
                    case Format.X8R8G8B8:
                        return PixelFormat.Format32bppBGR;
                    case Format.Yuy2:
                        throw new NotSupportedException();
                    default:
                        throw new NotImplementedException();
                }
            }

            static Format PixelToTextureFormat(Guid pixelFormat)
            {
                return FPixelToTextureFormatMap[pixelFormat];
            }
        }
    }
}
