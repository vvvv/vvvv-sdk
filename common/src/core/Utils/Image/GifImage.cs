using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace VVVV.Utils.Imaging
{
    //adapted from:
    //http://stackoverflow.com/questions/1196322/how-to-create-an-animated-gif-in-net
    //gif file format specification:
    //http://www.fileformat.info/format/gif/egff.htm

    /// <summary>
    /// Uses default .net GIF encoding and adds animation headers.
    /// </summary>
    public class Gif : IDisposable, IEnumerable<Image>
    {
        #region Header Constants
        const byte FileTrailer = 0x3b,
            ApplicationBlockSize = 0x0b,
            GraphicControlExtensionBlockSize = 0x04;

        const int ApplicationExtensionBlockIdentifier = 0xff21,
            GraphicControlExtensionBlockIdentifier = 0xf921;

        const long SourceGlobalColorInfoPosition = 10,
            SourceGraphicControlExtensionLength = 8,
            SourceImageBlockHeaderLength = 11,
            SourceColorBlockPosition = 13;

        long SourceColorBlockLength, SourceGraphicControlExtensionPosition, SourceImageBlockPosition;

        const string NetscapeIdentification = "NETSCAPE2.0",
            VVVVIdentification = "vvvv50.....",
            FileType = "GIF",
            FileVersion = "89a";
        #endregion

        class GifFrame : IDisposable
        {
            public Image Image;
            public double Delay;
            public int XOffset, YOffset;

            public GifFrame(Image image, double delay, int xOffset, int yOffset)
            {
                Image = image;
                Delay = delay;
                XOffset = xOffset;
                YOffset = yOffset;
            }

            public void Dispose()
            {
                Image.Dispose();
            }
        }

        List<GifFrame> Frames = new List<GifFrame>();

        public Gif() { DefaultFrameDelay = 1; }

        public Gif(Stream InStream, int Repeat = 0, int Delay = 500)
        {
            using (var Animation = Bitmap.FromStream(InStream))
            {
                int Length = Animation.GetFrameCount(FrameDimension.Time);

                DefaultFrameDelay = Delay;
                this.Repeat = Repeat;

                for (int i = 0; i < Length; ++i)
                {
                    Animation.SelectActiveFrame(FrameDimension.Time, i);

                    var Frame = new Bitmap(Animation.Size.Width, Animation.Size.Height);

                    Graphics.FromImage(Frame).DrawImage(Animation, new Point(0, 0));

                    Frames.Add(new GifFrame(Frame, Delay, 0, 0));
                }
            }
        }

        #region Properties
        public int DefaultWidth { get; set; }

        public int DefaultHeight { get; set; }

        public int Count { get { return Frames.Count; } }

        /// <summary>
        /// Default Delay in Milliseconds
        /// </summary>
        public int DefaultFrameDelay { get; set; }

        public int Repeat { get; private set; }
        #endregion

        /// <summary>
        /// Adds a frame to this animation.
        /// </summary>
        /// <param name="Image">The image to add</param>
        /// <param name="XOffset">The positioning x offset this image should be displayed at.</param>
        /// <param name="YOffset">The positioning y offset this image should be displayed at.</param>
        public void AddFrame(Image Image, double? frameDelay = null, int XOffset = 0, int YOffset = 0)
        {
            Frames.Add(new GifFrame(Image, frameDelay ?? DefaultFrameDelay, XOffset, YOffset));
        }

        public void AddFrame(string FilePath, double? frameDelay = null, int XOffset = 0, int YOffset = 0)
        {
            AddFrame(new Bitmap(FilePath), frameDelay, XOffset, YOffset);
        }

        public void RemoveAt(int Index) { Frames.RemoveAt(Index); }

        public void Clear()
        {
            foreach (var frame in Frames)
                frame.Dispose();
            Frames.Clear();
        }

        public void Save(Stream OutStream, PaletteType paletteType, DitherType ditherType, int colorCount)
        {
            using (var Writer = new BinaryWriter(OutStream))
            {
                //Header
                Writer.Write(FileType.ToCharArray());
                Writer.Write(FileVersion.ToCharArray());

                //Logical Screen Descriptor
                var w = Frames[0].Image.Width;
                var h = Frames[0].Image.Height;
                Writer.Write((short)(DefaultWidth == 0 ? w : DefaultWidth)); // Initial Logical Width
                Writer.Write((short)(DefaultHeight == 0 ? h : DefaultHeight)); // Initial Logical Height
                
                byte gctInfo = 0;
                if (paletteType != PaletteType.Optimal) //use a global colortable
                {
                    using (var gifStream = new MemoryStream())
                    {
                        var bitmap = new Bitmap(Frames[0].Image);
                        bitmap.ChangeTo8bppIndexed(paletteType, ditherType, colorCount);
                        bitmap.Save(gifStream, ImageFormat.Gif);

                        gifStream.Position = SourceGlobalColorInfoPosition;
                        gctInfo = (byte)gifStream.ReadByte();
                        var bv = new BitVector32(gctInfo);
                        var sizeSection = BitVector32.CreateSection(7);
                        SourceColorBlockLength = (1 << (bv[sizeSection] + 1)) * 3;
                        SourceGraphicControlExtensionPosition = SourceColorBlockLength + 13;
                        SourceImageBlockPosition = SourceGraphicControlExtensionPosition;

                        Writer.Write(gctInfo); // Global Color Table Info
                        Writer.Write((byte)0); // Background Color Index
                        Writer.Write((byte)0); // Pixel aspect ratio

                        WriteColorTable(gifStream, Writer);
                    }
                }
                else //local colortables will be used for each frame
                {
                    Writer.Write(gctInfo); // Global Color Table Info
                    Writer.Write((byte)0); // Background Color Index
                    Writer.Write((byte)0); // Pixel aspect ratio
                }
                
                //Netscape extension for looping animations
                unchecked { Writer.Write((short)ApplicationExtensionBlockIdentifier); };
                Writer.Write((byte)ApplicationBlockSize);
                Writer.Write(NetscapeIdentification.ToCharArray());
                Writer.Write((byte)3); // Application block length
                Writer.Write((byte)1);
                Writer.Write((short)Repeat); // Repeat count for images.
                Writer.Write((byte)0); // terminator

                // vvvv Extension Header
                unchecked { Writer.Write((short)ApplicationExtensionBlockIdentifier); };
                Writer.Write((byte)ApplicationBlockSize);
                Writer.Write(VVVVIdentification.ToCharArray());
                Writer.Write((byte)1); // Application block length
                Writer.Write((byte)0); // dummy
                Writer.Write((byte)0); // terminator

                //individual frames
                for (int i = 0; i < Count; ++i)
                {
                    var frame = Frames[i];
                    {
                        using (var gifStream = new MemoryStream())
                        {
                            var bitmap = new Bitmap(frame.Image);
                            if (bitmap.ChangeTo8bppIndexed(paletteType, ditherType, colorCount) == 0)
                                bitmap.Save(gifStream, ImageFormat.Gif);
                            else
                                frame.Image.Save(gifStream, ImageFormat.Gif);

                            WriteGraphicControlBlock(gifStream, Writer, frame.Delay);
                            WriteImageBlock(gifStream, Writer, paletteType == PaletteType.Optimal, frame.XOffset, frame.YOffset, bitmap.Width, bitmap.Height);
                        }
                    }
                }

                // Complete File
                Writer.Write(FileTrailer);
            }
        }

        #region Write
        void WriteColorTable(Stream sourceGif, BinaryWriter Writer)
        {
            sourceGif.Position = SourceColorBlockPosition; // Locating the image color table
            var colorTable = new byte[SourceColorBlockLength]; 
            sourceGif.Read(colorTable, 0, colorTable.Length);
            Writer.Write(colorTable, 0, colorTable.Length);
        }

        void WriteGraphicControlBlock(Stream sourceGif, BinaryWriter Writer, double frameDelay)
        {
            unchecked { Writer.Write((short)GraphicControlExtensionBlockIdentifier); }; // Identifier
            Writer.Write((byte)GraphicControlExtensionBlockSize); // Block Size
            Writer.Write((byte)(0 & 0xf7 | 0x08)); // Setting disposal flag
            Writer.Write((short)(frameDelay)); // Setting frame delay
            Writer.Write((byte)0); // (byte)blockhead[6]); // Transparent color index
            Writer.Write((byte)0); // Terminator
        }

        void WriteImageBlock(Stream sourceGif, BinaryWriter Writer, bool includeColorTable, int x, int y, int w, int h)
        {
            sourceGif.Position = SourceGlobalColorInfoPosition;
            var gctInfo = (byte)sourceGif.ReadByte();
            var bv = new BitVector32(gctInfo);
            var sizeSection = BitVector32.CreateSection(7);
            SourceColorBlockLength = (1 << (bv[sizeSection] + 1)) * 3;
            SourceImageBlockPosition = SourceColorBlockLength + 13;

            sourceGif.Position = SourceImageBlockPosition; // Locating the image block
            var header = new byte[SourceImageBlockHeaderLength];
            sourceGif.Read(header, 0, header.Length);
            Writer.Write((byte)header[0]); // Separator
            Writer.Write((short)x); // Position X
            Writer.Write((short)y); // Position Y
            Writer.Write((short)w); // Width
            Writer.Write((short)h); // Height

            if (includeColorTable)
            {
                Writer.Write((byte)(gctInfo & 0x3f | 0x80)); // Enabling local color table
                WriteColorTable(sourceGif, Writer);
            }
            else
                Writer.Write((byte)0); // (header[9] & 0x07 | 0x07)); // Disabling local color table

            Writer.Write((byte)header[10]); // LZW Min Code Size

            // Read/Write image data
            sourceGif.Position = SourceImageBlockPosition + SourceImageBlockHeaderLength;

            var dataLength = sourceGif.ReadByte();
            while (dataLength > 0)
            {
                var imgData = new byte[dataLength];
                sourceGif.Read(imgData, 0, dataLength);

                Writer.Write((byte)dataLength);
                Writer.Write(imgData, 0, dataLength);
                dataLength = sourceGif.ReadByte();
            }

            Writer.Write((byte)0); // Terminator
        }
        #endregion

        public void Dispose()
        {
            Frames.Clear();
            Frames = null;
        }

        public Image this[int Index] { get { return Frames[Index].Image; } }

        public IEnumerator<Image> GetEnumerator() { foreach (var Frame in Frames) yield return Frame.Image; }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }
}
