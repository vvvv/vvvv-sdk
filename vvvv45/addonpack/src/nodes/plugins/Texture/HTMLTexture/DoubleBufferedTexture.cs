using SlimDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace VVVV.Nodes.Texture.HTML
{
    using Texture = SlimDX.Direct3D9.Texture;

    class DoubleBufferedTexture : IDisposable
    {
        public readonly Device Device;
        public readonly Size Size;
        private readonly Texture CpuTexture;
        private readonly Texture GpuTexture;

        public DoubleBufferedTexture(Device device, Size size)
        {
            Device = device;
            Size = new Size(Math.Min(size.Width, device.Capabilities.MaxTextureWidth), Math.Min(size.Height, device.Capabilities.MaxTextureHeight));

            var usage = Usage.None & ~Usage.AutoGenerateMipMap;
            CpuTexture = new Texture(device, Size.Width, Size.Height, 1, usage, Format.A8R8G8B8, Pool.SystemMemory);
            GpuTexture = new Texture(device, Size.Width, Size.Height, 1, usage, Format.A8R8G8B8, Pool.Default);
        }

        public void Write(Rectangle rect, IntPtr buffer, int stride)
        {
            // Rect needs to be inside of Width/Height
            rect = Rectangle.Intersect(new Rectangle(0, 0, Size.Width, Size.Height), rect);
            if (rect == Rectangle.Empty) return;
            var dataRect = CpuTexture.LockRectangle(0, rect, LockFlags.None);
            try
            {
                var dataStream = dataRect.Data;
                if (rect.Width == Size.Width && rect.Height == Size.Height && dataRect.Pitch == stride)
                {
                    dataStream.WriteRange(buffer, Size.Height * dataRect.Pitch);
                }
                else
                {
                    var offset = stride * rect.Y + 4 * rect.X;
                    var source = buffer + offset;
                    for (int y = 0; y < rect.Height; y++)
                    {
                        dataStream.Position = y * dataRect.Pitch;
                        dataStream.WriteRange(source + y * stride, rect.Width * 4);
                    }
                }
            }
            finally
            {
                CpuTexture.UnlockRectangle(0);
            }
        }

        public Texture Swap()
        {
            Device.UpdateTexture(CpuTexture, GpuTexture);
            return GpuTexture;
        }

        public void Dispose()
        {
            CpuTexture.Dispose();
            GpuTexture.Dispose();
        }
    }
}
