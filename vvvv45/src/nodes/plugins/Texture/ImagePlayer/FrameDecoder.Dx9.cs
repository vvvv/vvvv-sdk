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
        class Direct3D9FrameDecoder : FrameDecoder
        {
            private readonly ImageInformation FImageInformation;
            private Stream FStream;

            public Direct3D9FrameDecoder(Func<Device, int, int, int, Format, Usage, Texture> textureFactory, MemoryPool memoryPool, Stream stream, Format preferedFormat)
                : base(textureFactory, memoryPool, preferedFormat)
            {
                FStream = stream;
                FImageInformation = ImageInformation.FromStream(stream);
                Width = FImageInformation.Width;
                Height = FImageInformation.Height;
                if (preferedFormat == Format.Unknown)
                    FChosenFormat = FImageInformation.Format;
            }

            protected override Texture Decode(Device device, Format format)
            {
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
    }
}
