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
    abstract class FrameDecoder
    {
        class BitmapFrameDecoder : FrameDecoder
        {
            private readonly BitmapSource FBitmapSource;
            
            public BitmapFrameDecoder(TexturePool texturePool, MemoryPool memoryPool, Stream stream)
                : base(texturePool, memoryPool, stream)
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
                var texture = FTexturePool.GetTexture(
                    device,
                    FBitmapSource.PixelWidth,
                    FBitmapSource.PixelHeight,
                    1,
                    Usage.Dynamic & ~Usage.AutoGenerateMipMap,
                    Format.A8R8G8B8,
                    Pool.Default);
                
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
            
            public Direct3D9FrameDecoder(TexturePool texturePool, MemoryPool memoryPool, Stream stream)
                : base(texturePool, memoryPool, stream)
            {
                // This is stupid...but FromStream will trigger GC far too often.
                var buffer = FMemoryPool.GetMemory((int) stream.Length);
                stream.Read(buffer, 0, buffer.Length);
                stream.Position = 0;
                FMemoryPool.PutMemory(buffer);
                FImageInformation = ImageInformation.FromMemory(buffer);
            }
            
            public override Texture Decode(Device device)
            {
                var format = FImageInformation.Format;
                var usage = Usage.Dynamic & ~Usage.AutoGenerateMipMap;
                
                Texture texture = null;
                
                try 
                {
                    texture = FTexturePool.GetTexture(
                        device,
                        FImageInformation.Width,
                        FImageInformation.Height,
                        FImageInformation.MipLevels,
                        usage,
                        format,
                        Pool.Default);
                } 
                catch (SlimDXException) 
                {
                    // Try with different parameters
                    texture = FTexturePool.GetTexture(
                        device,
                        FImageInformation.Width,
                        FImageInformation.Height,
                        FImageInformation.MipLevels,
                        usage,
                        Format.A8R8G8B8,
                        Pool.Default);
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
        
        private static Dictionary<string, Func<TexturePool, MemoryPool, Stream, FrameDecoder>> FDecoderFactories = new Dictionary<string, Func<TexturePool, MemoryPool, Stream, FrameDecoder>>();
        
        static FrameDecoder()
        {
            Register(
                Direct3D9FrameDecoder.SupportedFileExtensions, 
                (texturePool, memoryPool, stream) => new Direct3D9FrameDecoder(texturePool, memoryPool, stream)
               );
            Register(
                BitmapFrameDecoder.SupportedFileExtensions, 
                (texturePool, memoryPool, stream) => new BitmapFrameDecoder(texturePool, memoryPool, stream)
               );
        }
        
        public static FrameDecoder Create(string filename, TexturePool texturePool, MemoryPool memoryPool, Stream stream)
        {
            var extension = Path.GetExtension(filename);
            
            Func<TexturePool, MemoryPool, Stream, FrameDecoder> decoderFactory = null;
            if (!FDecoderFactories.TryGetValue(extension, out decoderFactory))
            {
                return new BitmapFrameDecoder(texturePool, memoryPool, stream);
            }
            return decoderFactory(texturePool, memoryPool, stream);
        }
        
        public static void Register(IEnumerable<string> extensions, Func<TexturePool, MemoryPool, Stream, FrameDecoder> decoderFactory)
        {
            foreach (var extension in extensions)
            {
                FDecoderFactories[extension] = decoderFactory;
            }
        }
        
        protected readonly TexturePool FTexturePool;
        protected readonly MemoryPool FMemoryPool;
        protected readonly Stream FStream;
        
        public FrameDecoder(TexturePool texturePool, MemoryPool memoryPool, Stream stream)
        {
            FTexturePool = texturePool;
            FMemoryPool = memoryPool;
            FStream = stream;
        }
        
        public abstract Texture Decode(Device device);
    }
}
