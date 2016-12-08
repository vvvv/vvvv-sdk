using SlimDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace VVVV.Nodes.Texture.HTML
{
    using System.Diagnostics;
    using System.Threading;
    using Texture = SlimDX.Direct3D9.Texture;

    class DoubleBufferedTexture : IDisposable
    {
        public readonly Device Device;
        public readonly Size Size;
        public readonly bool IsDegraded;
        public readonly Texture LastCompleteTexture;
        private readonly Texture IncompleteTexture;
        private bool IsDirty;

        public DoubleBufferedTexture(Device device, Size size)
            : this(device, size, false, CreateTexture(device, size, Pool.SystemMemory), CreateTexture(device, size, Pool.Default))
        {
        }

        private DoubleBufferedTexture(Device device, Size size, bool isDegraded, Texture incompleteTexture, Texture lastCompleteTexture)
        {
            Device = device;
            Size = size;
            IsDegraded = isDegraded;
            IsDirty = true;
            IncompleteTexture = incompleteTexture;
            LastCompleteTexture = lastCompleteTexture;
        }

        private static Size GetDeviceSize(Device device, Size size)
        {
            return new Size(
                Math.Min(size.Width, device.Capabilities.MaxTextureWidth), 
                Math.Min(size.Height, device.Capabilities.MaxTextureHeight));
        }

        private static Texture CreateTexture(Device device, Size size, Pool pool)
        {
            var deviceSize = GetDeviceSize(device, size);
            var usage = Usage.None & ~Usage.AutoGenerateMipMap;
            return new Texture(device, deviceSize.Width, deviceSize.Height, 1, usage, Format.A8R8G8B8, pool);
        }

        public void Write(Rectangle rect, IntPtr buffer, int stride)
        {
            // Rect needs to be inside of Width/Height
            var size = GetDeviceSize(Device, Size);
            rect = Rectangle.Intersect(new Rectangle(0, 0, size.Width, size.Height), rect);
            if (rect == Rectangle.Empty) return;
            var dataRect = IncompleteTexture.LockRectangle(0, rect, LockFlags.Discard | LockFlags.NoSystemLock);
            try
            {
                var dataStream = dataRect.Data;
                if (rect.Width == size.Width && rect.Height == size.Height && dataRect.Pitch == stride)
                {
                    dataStream.WriteRange(buffer, size.Height * dataRect.Pitch);
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
                IncompleteTexture.UnlockRectangle(0);
            }
            IsDirty = true;
        }

        private void UpdateTexture()
        {
            if (!IsDegraded && IsDirty)
            {
                Device.UpdateTexture(IncompleteTexture, LastCompleteTexture);
                IsDirty = false;
            }
        }

        public DoubleBufferedTexture Update(Size newSize)
        {
            // Should the size be invalid stick with the old one
            if (newSize.Width <= 0 || newSize.Height <= 0)
            {
                UpdateTexture();
                return this;
            }
            // Should the size change switch to the degraded state
            if (newSize != Size)
            {
                IncompleteTexture.Dispose();
                return new DoubleBufferedTexture(Device, newSize, true, CreateTexture(Device, newSize, Pool.SystemMemory), LastCompleteTexture);
            }
            // Size is the same
            if (IsDegraded)
            {
                // We're degraded (sizes of textures differ)
                if (IsDirty)
                {
                    // We can switch to the non-degraded state
                    LastCompleteTexture.Dispose();
                    var result = new DoubleBufferedTexture(Device, Size, false, IncompleteTexture, CreateTexture(Device, Size, Pool.Default));
                    // And since we already have a result upload the texture immediately
                    result.UpdateTexture();
                    return result;
                }
                else
                    // Write hasn't been called yet - stay in the degraded state
                    return this;
            }
            else
            {
                // Juhu, we're not degraded
                UpdateTexture();
                return this;
            }
        }

        public bool IsValid
        {
            get { return !IsDirty && !IsDegraded; }
        }

        public void Dispose()
        {
            IncompleteTexture.Dispose();
            LastCompleteTexture.Dispose();
        }
    }
}
