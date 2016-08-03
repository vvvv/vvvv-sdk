using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;

// Source: https://github.com/eried/PrimeComm/blob/7cbce9f16a0f647c43a6b69e7a77a820a0f80df4/PrimeLib/GdipEffect.cs

namespace VVVV.Utils.Imaging
{
    public enum DitherType
    {
        None = 0,
        Solid = 1,
        Ordered4x4 = 2,
        Ordered8x8 = 3,
        Ordered16x16 = 4,
        Spiral4x4 = 5,
        Spiral8x8 = 6,
        DualSpiral4x4 = 7,
        DualSpiral8x8 = 8,
        ErrorDiffusion = 9,
        Max = 10
    }
    public enum PaletteType
    {
        Custom = 0,
        Optimal = 1,
        FixedBW = 2,
        FixedHalftone8 = 3,
        FixedHalftone27 = 4,
        FixedHalftone64 = 5,
        FixedHalftone125 = 6,
        FixedHalftone216 = 7,
        FixedHalftone252 = 8,
        FixedHalftone256 = 9
    }

    public static class GDIPlus
    {
        [DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GdipInitializePalette(ref GdiPalette Pal, int palettetype, int optimalColors,
            int useTransparentColor, IntPtr bitmap);

        [DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GdipInitializePalette(int[] Pal, PaletteType palettetype, int optimalColors,
            int useTransparentColor, IntPtr bitmap);

        [DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GdipBitmapConvertFormat(IntPtr bitmap, int pixelFormat, DitherType dithertype,
            PaletteType palettetype, ref GdiPalette Pal, float alphaThresholdPercent);

        [DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GdipBitmapConvertFormat(IntPtr bitmap, int pixelFormat, DitherType dithertype,
            PaletteType palettetype, int[] Pal, float alphaThresholdPercent);

        [DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GdipBitmapConvertFormat(IntPtr bitmap, int pixelFormat, DitherType dithertype,
            PaletteType palettetype, IntPtr Pal, float alphaThresholdPercent);

        public static int ChangeTo8bppIndexed(this Bitmap Bmp, PaletteType palettetype = PaletteType.Optimal,
            DitherType ditherType = DitherType.ErrorDiffusion, int optimalColors = 256)
        {
            int Entries;
            // http://msdn.microsoft.com/en-us/library/ms534159(v=vs.85).aspx
            switch (palettetype)
            {
                case PaletteType.FixedBW:
                    Entries = 2;
                    break;
                case PaletteType.FixedHalftone8:
                    Entries = 16;
                    break;
                case PaletteType.FixedHalftone27:
                    Entries = 36;
                    break;
                case PaletteType.FixedHalftone64:
                    Entries = 73;
                    break;
                case PaletteType.FixedHalftone125:
                    Entries = 134;
                    break;
                case PaletteType.FixedHalftone216:
                    Entries = 225;
                    break;
                case PaletteType.FixedHalftone252:
                    Entries = 253;
                    break;
                case PaletteType.FixedHalftone256:
                    Entries = 256;
                    break;
                case PaletteType.Optimal:
                    Entries = Math.Min(Math.Max(optimalColors, 1), 256);
                    break;
                default:
                    throw new ArgumentException("Error");
            }
            var Pal = new int[2 + Entries];
            Pal[0] = (int)PaletteFlags.GrayScale; // Flag
            Pal[1] = Entries; // Count
            if (palettetype == PaletteType.Optimal)
                GdipInitializePalette(Pal, palettetype, Entries, 0, Bmp.NativeHandle());
            else
                GdipInitializePalette(Pal, palettetype, Entries, 0, IntPtr.Zero);

            if (palettetype == PaletteType.Optimal)
                if (ditherType != DitherType.None && ditherType != DitherType.Solid &&
                    ditherType != DitherType.ErrorDiffusion)
                    return 2;

            return GdipBitmapConvertFormat(Bmp.NativeHandle(), Convert.ToInt32(PixelFormat.Format8bppIndexed), ditherType, palettetype, Pal, 0);
        }

        public static int ChangeToSpecialIndexed(this Bitmap Bmp, PaletteType palettetype = PaletteType.Optimal,
           DitherType ditherType = DitherType.ErrorDiffusion, int optimalColors = 256)
        {
            int Entries;
            // http://msdn.microsoft.com/en-us/library/ms534159(v=vs.85).aspx
            switch (palettetype)
            {
                case PaletteType.FixedBW:
                    Entries = 2;
                    break;
                case PaletteType.FixedHalftone8:
                    Entries = 16;
                    break;
                case PaletteType.FixedHalftone27:
                    Entries = 36;
                    break;
                case PaletteType.FixedHalftone64:
                    Entries = 73;
                    break;
                case PaletteType.FixedHalftone125:
                    Entries = 134;
                    break;
                case PaletteType.FixedHalftone216:
                    Entries = 225;
                    break;
                case PaletteType.FixedHalftone252:
                    Entries = 253;
                    break;
                case PaletteType.FixedHalftone256:
                    Entries = 256;
                    break;
                case PaletteType.Optimal:
                    Entries = Math.Min(Math.Max(optimalColors, 1), 256);
                    break;
                default:
                    throw new ArgumentException("Error");
            }
            var Pal = new int[2 + Entries];
            Pal[0] = (int)PaletteFlags.GrayScale; // Flag
            Pal[1] = Entries; // Count
            if (palettetype == PaletteType.Optimal)
                GdipInitializePalette(Pal, palettetype, Entries, 0, Bmp.NativeHandle());
            else
                GdipInitializePalette(Pal, palettetype, Entries, 0, IntPtr.Zero);
            if (palettetype == PaletteType.Optimal)
                if (ditherType != DitherType.None && ditherType != DitherType.Solid &&
                    ditherType != DitherType.ErrorDiffusion)
                    return 2;

            return GdipBitmapConvertFormat(Bmp.NativeHandle(), Convert.ToInt32(PixelFormat.Format16bppArgb1555), ditherType,
                palettetype, Pal, 50f);
        }

        public static int ChangeTo4bppIndexed(this Bitmap Bmp, PaletteType palettetype = PaletteType.Optimal,
            DitherType ditherType = DitherType.ErrorDiffusion, int optimalColors = 16)
        {
            int Entries;
            // http://msdn.microsoft.com/en-us/library/ms534159(v=vs.85).aspx
            switch (palettetype)
            {
                case PaletteType.FixedBW:
                    Entries = 2;
                    break;
                case PaletteType.FixedHalftone8:
                    Entries = 16;
                    break;
                case PaletteType.Optimal:
                    if (optimalColors <= 0 || optimalColors > 16)
                        throw new ArgumentOutOfRangeException("Colors should be between 0 (inclusive) and 16 (exclusive)");
                    Entries = optimalColors;
                    break;
                default:
                    throw new ArgumentException("Error");
            }
            var Pal = new int[2 + Entries];
            Pal[0] = (int)PaletteFlags.GrayScale; // Flag
            Pal[1] = Entries; // Count
            if (palettetype == PaletteType.Optimal)
                GdipInitializePalette(Pal, palettetype, Entries, 0, Bmp.NativeHandle());
            else
                GdipInitializePalette(Pal, palettetype, Entries, 0, IntPtr.Zero);
            if (palettetype == PaletteType.Optimal)
                if (ditherType != DitherType.None && ditherType != DitherType.Solid &&
                    ditherType != DitherType.ErrorDiffusion)
                    return 2;

            return GdipBitmapConvertFormat(Bmp.NativeHandle(), Convert.ToInt32(PixelFormat.Format4bppIndexed), ditherType,
                palettetype, Pal, 50f);
        }

        public static int ChangeTo1bppIndexed(this Bitmap Bmp,
            DitherType ditherType = DitherType.ErrorDiffusion)
        {
            if (ditherType != DitherType.Solid && ditherType != DitherType.ErrorDiffusion)
                return 2;

            var Pal = new int[4];
            Pal[0] = (int)PaletteFlags.GrayScale; // Flag
            Pal[1] = 2; // Count
            GdipInitializePalette(Pal, PaletteType.FixedBW, 2, 0, IntPtr.Zero);

            return GdipBitmapConvertFormat(Bmp.NativeHandle(), Convert.ToInt32(PixelFormat.Format1bppIndexed), ditherType,
                PaletteType.FixedBW, Pal, 50f);
        }

        public static int ChangeTo16bppRgb555(this Bitmap Bmp,
            DitherType ditherType = DitherType.ErrorDiffusion)
        {
            return GdipBitmapConvertFormat(Bmp.NativeHandle(), Convert.ToInt32(PixelFormat.Format16bppRgb555), ditherType,
                PaletteType.Custom, IntPtr.Zero, 50f);
        }

        public static int ChangeTo24bppRgb(this Bitmap Bmp)
        {
            return GdipBitmapConvertFormat(Bmp.NativeHandle(), Convert.ToInt32(PixelFormat.Format24bppRgb),
                DitherType.None, PaletteType.Custom, IntPtr.Zero, 50f);
        }

        public static int ChangeTo32bppARGB(this Bitmap Bmp)
        {
            return GdipBitmapConvertFormat(Bmp.NativeHandle(), Convert.ToInt32(PixelFormat.Format32bppArgb),
                DitherType.None, PaletteType.Custom, IntPtr.Zero, 50f);
        }

        internal static TResult GetPrivateField<TResult>(this object obj, string fieldName)
        {
            if (obj == null) return default(TResult);
            Type ltType = obj.GetType();
            FieldInfo lfiFieldInfo = ltType.GetField(fieldName,
                BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic);
            if (lfiFieldInfo != null)
                return (TResult)lfiFieldInfo.GetValue(obj);
            throw new InvalidOperationException(
                string.Format("Instance field '{0}' could not be located in object of type '{1}'.", fieldName,
                    obj.GetType().FullName));
        }

        public static IntPtr NativeHandle(this Bitmap Bmp)
        {
            return Bmp.GetPrivateField<IntPtr>("nativeImage");
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GdiPalette
        {
            internal readonly PaletteFlags Flag;
            internal readonly int Count;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
            internal readonly byte[] Entries;
        }
    }
}