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

            private readonly DataPointer FBuffer;
            private readonly int FWidth;
            private readonly int FHeight;
            private readonly int FStride;
            private readonly int FLength;
            
            public BitmapFrameDecoder(Func<Device, int, int, int, Format, Texture> textureFactory, MemoryPool memoryPool, Stream stream)
                : base(textureFactory, memoryPool, stream)
            {
                var dataStream = stream as SharpDX.DataStream;
                using (var wicStream = new WICStream(Factory, new SharpDX.DataPointer(dataStream.DataPointer, (int)dataStream.Length)))
                using (var decoder = new BitmapDecoder(Factory, wicStream, DecodeOptions.CacheOnLoad))
                {
                    using (var frame = decoder.GetFrame(0))
                    {
                        var dstPixelFormat = PixelFormat.Format32bppBGRA;
                            
                        FWidth = frame.Size.Width;
                        FHeight = frame.Size.Height;
                        FStride = PixelFormat.GetStride(dstPixelFormat, FWidth);
                        FLength = FStride * FHeight;

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
            }

            public override void Dispose()
            {
                if (FBuffer != null)
                {
                    FMemoryPool.UnmanagedPool.PutMemory(FBuffer);
                }
                base.Dispose();
            }
            
            public override Texture Decode(Device device)
            {
                var texture = FTextureFactory(device, FWidth, FHeight, 1, Format.A8R8G8B8);
                var dataRectangle = texture.LockRectangle(0, LockFlags.Discard);

                try
                {
                    if (dataRectangle.Pitch == FStride)
                    {
                        Utilities.CopyMemory(dataRectangle.DataPointer, FBuffer.Pointer, FBuffer.Size);
                    }
                    else
                    {
                        for (int y = 0; y < FHeight; y++)
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
            
            public Direct3D9FrameDecoder(Func<Device, int, int, int, Format, Texture> textureFactory, MemoryPool memoryPool, Stream stream)
                : base(textureFactory, memoryPool, stream)
            {
                FImageInformation = ImageInformation.FromStream(stream);
            }
            
            public override Texture Decode(Device device)
            {
                var format = FImageInformation.Format;  
                Texture texture = null;
                
                try 
                {
                    texture = FTextureFactory(
                        device,
                        FImageInformation.Width,
                        FImageInformation.Height,
                        FImageInformation.MipLevels,
                        format);
                } 
                catch (SharpDXException) 
                {
                    // Try with different parameters
                    texture = FTextureFactory(
                        device,
                        FImageInformation.Width,
                        FImageInformation.Height,
                        FImageInformation.MipLevels,
                        Format.A8R8G8B8);
                }
                
                var surface = texture.GetSurfaceLevel(0);
                
                Surface.FromFileInStream(surface, FStream, Filter.None, 0);

                return texture;
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

        class EmptyFrameDecoder : FrameDecoder
        {
            public EmptyFrameDecoder(Func<Device, int, int, int, Format, Texture> textureFactory, MemoryPool memoryPool, Stream stream)
                : base(textureFactory, memoryPool, stream)
            {

            }

            public override Texture Decode(Device device)
            {
                var texture = FTextureFactory(device, 1, 1, 1, Format.A8R8G8B8);
                var dataRectangle = texture.LockRectangle(0, LockFlags.Discard);
                try
                {
                    var white = 0xFFFFFFFF;
                    Utilities.Write(dataRectangle.DataPointer, ref white);
                }
                finally
                {
                    texture.UnlockRectangle(0);
                }
                return texture;
            }
        }
        
        private static Dictionary<string, Func<Func<Device, int, int, int, Format, Texture>, MemoryPool, Stream, FrameDecoder>> FDecoderFactories = new Dictionary<string, Func<Func<Device, int, int, int, Format, Texture>, MemoryPool, Stream, FrameDecoder>>();
        
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
        
        public static FrameDecoder Create(string filename, Func<Device, int, int, int, Format, Texture> textureFactory, MemoryPool memoryPool, Stream stream)
        {
            try
            {
                var extension = Path.GetExtension(filename);

                Func<Func<Device, int, int, int, Format, Texture>, MemoryPool, Stream, FrameDecoder> decoderFactory = null;
                if (!FDecoderFactories.TryGetValue(extension, out decoderFactory))
                {
                    return new BitmapFrameDecoder(textureFactory, memoryPool, stream);
                }
                return decoderFactory(textureFactory, memoryPool, stream);
            }
            catch
            {
                return new EmptyFrameDecoder(textureFactory, memoryPool, stream);
            }
        }
        
        public static void Register(IEnumerable<string> extensions, Func<Func<Device, int, int, int, Format, Texture>, MemoryPool, Stream, FrameDecoder> decoderFactory)
        {
            foreach (var extension in extensions)
            {
                FDecoderFactories[extension] = decoderFactory;
            }
        }
        
        protected Func<Device, int, int, int, Format, Texture> FTextureFactory;
        protected MemoryPool FMemoryPool;
        protected Stream FStream;
        
        public FrameDecoder(Func<Device, int, int, int, Format, Texture> textureFactory, MemoryPool memoryPool, Stream stream)
        {
            FTextureFactory = textureFactory;
            FMemoryPool = memoryPool;
            FStream = stream;
        }
        
        public virtual void Dispose()
        {
            FMemoryPool.StreamPool.PutStream(FStream);
            FTextureFactory = null;
            FMemoryPool = null;
            FStream = null;
        }
        
        public abstract Texture Decode(Device device);
    }
}
