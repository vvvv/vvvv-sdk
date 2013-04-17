using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using VVVV.Core.Logging;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.WIC;

namespace VVVV.Nodes.ImagePlayer
{
    abstract class FrameDecoder : IDisposable
    {
        class BitmapFrameDecoder : FrameDecoder
        {
            private static ImagingFactory Factory = new ImagingFactory();

            private DataPointer FBuffer;
            private readonly int FStride;
            private readonly int FLength;
            
            public BitmapFrameDecoder(Func<Device, int, int, int, Format, Usage, Texture> textureFactory, MemoryPool memoryPool, Stream stream)
                : base(textureFactory, memoryPool)
            {
                var dataStream = stream as SharpDX.DataStream;
                using (var wicStream = new WICStream(Factory, new SharpDX.DataPointer(dataStream.DataPointer, (int)dataStream.Length)))
                using (var decoder = new BitmapDecoder(Factory, wicStream, DecodeOptions.CacheOnLoad))
                {
                    using (var frame = decoder.GetFrame(0))
                    {
                        var dstPixelFormat = PixelFormat.Format32bppBGRA;
                            
                        Width = frame.Size.Width;
                        Height = frame.Size.Height;
                        FStride = PixelFormat.GetStride(dstPixelFormat, Width);
                        FLength = FStride * Height;

                        FBuffer = memoryPool.UnmanagedPool.GetMemory(FLength);

                        if (frame.PixelFormat != dstPixelFormat)
                        {
                            using (var converter = new FormatConverter(Factory))
                            {
                                converter.Initialize(frame, dstPixelFormat);
                                converter.CopyPixels(FStride, FBuffer);
                            }
                        }
                        else
                        {
                            frame.CopyPixels(FStride, FBuffer);
                        }
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
            
            public override Texture Decode(Device device)
            {
                var usage = Usage.Dynamic & ~Usage.AutoGenerateMipMap;
                var texture = FTextureFactory(device, Width, Height, 1, Format.A8R8G8B8, usage);
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
                            var src = FBuffer.Pointer +  y * FStride;
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
        }
        
        class Direct3D9FrameDecoder : FrameDecoder
        {
            private readonly ImageInformation FImageInformation;
            private Stream FStream;
            
            public Direct3D9FrameDecoder(Func<Device, int, int, int, Format, Usage, Texture> textureFactory, MemoryPool memoryPool, Stream stream)
                : base(textureFactory, memoryPool)
            {
                FStream = stream;
                FImageInformation = ImageInformation.FromStream(stream);
                Width = FImageInformation.Width;
                Height = FImageInformation.Height;
            }
            
            public override Texture Decode(Device device)
            {
                Format format;
                switch (FImageInformation.Format)
                {
                    case Format.Dxt1:
                    case Format.Dxt2:
                    case Format.Dxt3:
                    case Format.Dxt4:
                    case Format.Dxt5:
                        format = FImageInformation.Format;
                        break;
                    default:
                        format = Format.A8R8G8B8;
                        break;
                }

                var usage = Usage.None & ~Usage.AutoGenerateMipMap;
                var texture = FTextureFactory(
                    device,
                    FImageInformation.Width,
                    FImageInformation.Height,
                    FImageInformation.MipLevels,
                    format,
                    usage);

                using (var surface = texture.GetSurfaceLevel(0))
                {
                    FStream.Position = 0;
                    Surface.FromFileInStream(surface, FStream, Filter.None, 0);
                }
                return texture;
            }

            public override void Dispose()
            {
                if (FStream != null)
                {
                    FMemoryPool.StreamPool.PutStream(FStream);
                    FStream = null;
                }
                base.Dispose();
            }
            
            public static IEnumerable<string> SupportedFileExtensions
            {
                get
                {
                    return 
                        from extension in Enum.GetNames(typeof(ImageFileFormat))
                        select string.Format(".{0}", extension.ToLower());
                }
            }
        }
        
        private static Dictionary<string, Func<Func<Device, int, int, int, Format, Usage, Texture>, MemoryPool, Stream, FrameDecoder>> FDecoderFactories = new Dictionary<string, Func<Func<Device, int, int, int, Format, Usage, Texture>, MemoryPool, Stream, FrameDecoder>>();
        
        static FrameDecoder()
        {
            Register(
                Direct3D9FrameDecoder.SupportedFileExtensions, 
                (textureFactory, memoryPool, stream) => new Direct3D9FrameDecoder(textureFactory, memoryPool, stream)
               );
            Register(
                BitmapFrameDecoder.SupportedFileExtensions, 
                (textureFactory, memoryPool, stream) => new BitmapFrameDecoder(textureFactory, memoryPool, stream)
               );
        }
        
        public static FrameDecoder Create(string filename, Func<Device, int, int, int, Format, Usage, Texture> textureFactory, MemoryPool memoryPool, Stream stream)
        {
            var extension = Path.GetExtension(filename);

            Func<Func<Device, int, int, int, Format, Usage, Texture>, MemoryPool, Stream, FrameDecoder> decoderFactory = null;
            if (!FDecoderFactories.TryGetValue(extension, out decoderFactory))
            {
                return new BitmapFrameDecoder(textureFactory, memoryPool, stream);
            }
            return decoderFactory(textureFactory, memoryPool, stream);
        }
        
        public static void Register(IEnumerable<string> extensions, Func<Func<Device, int, int, int, Format, Usage, Texture>, MemoryPool, Stream, FrameDecoder> decoderFactory)
        {
            foreach (var extension in extensions)
            {
                FDecoderFactories[extension] = decoderFactory;
            }
        }
        
        protected Func<Device, int, int, int, Format, Usage, Texture> FTextureFactory;
        protected MemoryPool FMemoryPool;
        
        public FrameDecoder(Func<Device, int, int, int, Format, Usage, Texture> textureFactory, MemoryPool memoryPool)
        {
            FTextureFactory = textureFactory;
            FMemoryPool = memoryPool;
        }
        
        public virtual void Dispose()
        {
            FTextureFactory = null;
            FMemoryPool = null;
        }
        
        public abstract Texture Decode(Device device);

        public int Width { get; set; }
        public int Height { get; set; }
    }
}
