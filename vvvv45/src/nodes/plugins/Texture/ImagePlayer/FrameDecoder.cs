using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Core.Logging;

namespace VVVV.Nodes.ImagePlayer
{
    abstract class FrameDecoder : IDisposable
    {
        class BitmapFrameDecoder : FrameDecoder
        {
            private readonly BitmapSource FBitmapSource;
            
            public BitmapFrameDecoder(Func<Device, int, int, int, Format, Texture> textureFactory, MemoryPool memoryPool, Stream stream)
                : base(textureFactory, memoryPool, stream)
            {
                var decoder = BitmapDecoder.Create(
                    FStream,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.None
                   );
                
                BitmapSource bitmapSource = decoder.Frames[0];
                if (bitmapSource.Format != PixelFormats.Bgra32)
                {
                    bitmapSource = new FormatConvertedBitmap(bitmapSource, PixelFormats.Bgra32, decoder.Palette, 0.0);
                }
                
                FBitmapSource = bitmapSource;
            }
            
            public override Texture Decode(Device device)
            {
                var texture = FTextureFactory(device, FBitmapSource.PixelWidth, FBitmapSource.PixelHeight, 1, Format.A8R8G8B8);
                var dataRectangle = texture.LockRectangle(0, LockFlags.Discard);

                try
                {
                    var data = dataRectangle.Data;
                    var buffer = data.DataPointer;
                    FBitmapSource.CopyPixels(Int32Rect.Empty, buffer, (int) data.Length, dataRectangle.Pitch);
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
            private readonly byte[] FBuffer;
            
            public Direct3D9FrameDecoder(Func<Device, int, int, int, Format, Texture> textureFactory, MemoryPool memoryPool, Stream stream)
                : base(textureFactory, memoryPool, stream)
            {
                // This is stupid...but FromStream will trigger GC far too often.
                FBuffer = FMemoryPool.GetMemory((int) stream.Length);
                stream.Read(FBuffer, 0, FBuffer.Length);
                stream.Position = 0;
                FImageInformation = ImageInformation.FromMemory(FBuffer);
            }
            
            public override void Dispose()
            {
                FMemoryPool.PutMemory(FBuffer);
                base.Dispose();
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
                catch (SlimDXException) 
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
                
                Surface.FromFileInMemory(surface, FBuffer, Filter.None, 0);
                
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
                    var data = dataRectangle.Data;
                    data.WriteRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
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
        
        protected readonly Func<Device, int, int, int, Format, Texture> FTextureFactory;
        protected readonly MemoryPool FMemoryPool;
        protected readonly Stream FStream;
        
        public FrameDecoder(Func<Device, int, int, int, Format, Texture> textureFactory, MemoryPool memoryPool, Stream stream)
        {
            FTextureFactory = textureFactory;
            FMemoryPool = memoryPool;
            FStream = stream;
        }
        
        public virtual void Dispose()
        {
            
        }
        
        public abstract Texture Decode(Device device);
    }
}
