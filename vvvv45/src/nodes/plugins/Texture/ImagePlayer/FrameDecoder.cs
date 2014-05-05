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
    /// <summary>
    /// Decodes a frame (image of some sort) to a texture with the prefered texture format.
    /// If the prefered texture format is not supported by the hardware a fallback format
    /// will be chosen.
    /// Implementations should do all the hard work in their constructor, as it is called
    /// from a background thread. The Decode method is called on the UI thread and should
    /// therefor avoid heavy computations.
    /// If the prefered format is set to unknown, a subclass must set the FChosenFormat
    /// field in its constructor based on the info of the data stream.
    /// Note that the final chosen texture format in the call to Decode is not necessarily
    /// the one written to FChosenFormat.
    /// </summary>
    abstract partial class FrameDecoder : IDisposable
    {
        private static Dictionary<string, Func<Func<Device, int, int, int, Format, Usage, Texture>, MemoryPool, Stream, Format, FrameDecoder>> FDecoderFactories = new Dictionary<string, Func<Func<Device, int, int, int, Format, Usage, Texture>, MemoryPool, Stream, Format, FrameDecoder>>();
        private static Dictionary<Format, Format> FFallbackFormats = new Dictionary<Format, Format>();
        
        static FrameDecoder()
        {
            Register(
                Direct3D9FrameDecoder.SupportedFileExtensions,
                (textureFactory, memoryPool, stream, preferedFormat) => new Direct3D9FrameDecoder(textureFactory, memoryPool, stream, preferedFormat)
               );
            Register(
                BitmapFrameDecoder.SupportedFileExtensions,
                (textureFactory, memoryPool, stream, preferedFormat) => new BitmapFrameDecoder(textureFactory, memoryPool, stream, preferedFormat)
               );
        }

        public static FrameDecoder Create(string filename, Func<Device, int, int, int, Format, Usage, Texture> textureFactory, MemoryPool memoryPool, Stream stream, Format preferedFormat)
        {
            var extension = Path.GetExtension(filename).ToLowerInvariant();
            // In case the prefered format is not supported on one device, fall back to one which works.
            var format = GetFallbackFormatIfPreferedFormatIsNotSupported(preferedFormat);
            Func<Func<Device, int, int, int, Format, Usage, Texture>, MemoryPool, Stream, Format, FrameDecoder> decoderFactory = null;
            if (!FDecoderFactories.TryGetValue(extension, out decoderFactory))
            {
                return new BitmapFrameDecoder(textureFactory, memoryPool, stream, format);
            }
            return decoderFactory(textureFactory, memoryPool, stream, format);
        }

        static Format GetFallbackFormatIfPreferedFormatIsNotSupported(Format preferedFormat)
        {
            Format result;
            if (FFallbackFormats.TryGetValue(preferedFormat, out result))
                return result;
            return preferedFormat;
        }
        
        public static void Register(IEnumerable<string> extensions, Func<Func<Device, int, int, int, Format, Usage, Texture>, MemoryPool, Stream, Format, FrameDecoder> decoderFactory)
        {
            foreach (var extension in extensions)
            {
                FDecoderFactories[extension] = decoderFactory;
            }
        }
        
        protected Func<Device, int, int, int, Format, Usage, Texture> FTextureFactory;
        protected MemoryPool FMemoryPool;
        protected Format FChosenFormat;
        
        public FrameDecoder(Func<Device, int, int, int, Format, Usage, Texture> textureFactory, MemoryPool memoryPool, Format preferedFormat)
        {
            FTextureFactory = textureFactory;
            FMemoryPool = memoryPool;
            FChosenFormat = preferedFormat;
        }
        
        public virtual void Dispose()
        {
            FTextureFactory = null;
            FMemoryPool = null;
        }

        public Texture Decode(Device device)
        {
            // Assertions
            System.Diagnostics.Debug.Assert(FChosenFormat != Format.Unknown);

            Format format;
            if (!FFallbackFormats.TryGetValue(FChosenFormat, out format))
            {
                // Check if the texture format is supported on this device
                using (var direct3D = device.Direct3D)
                {
                    var adapter = 0;
                    var displayMode = direct3D.GetAdapterDisplayMode(adapter);
                    if (!direct3D.CheckDeviceFormat(adapter, DeviceType.Hardware, direct3D.GetAdapterDisplayMode(0).Format, Usage.None, ResourceType.Texture, FChosenFormat))
                    {
                        // Ouch - the chosen format is not supported on this device
                        // Add this format to the list of unsupported formats so we 
                        // can avoid this for future frames.
                        var fallbackFormat = Format.A8R8G8B8;
                        FFallbackFormats.Add(FChosenFormat, fallbackFormat);
                        format = fallbackFormat;
                    }
                    else
                    {
                        format = FChosenFormat;
                    }
                }
            }
            return Decode(device, format);
        }

        protected abstract Texture Decode(Device device, Format format);

        public int Width { get; set; }
        public int Height { get; set; }
    }
}
