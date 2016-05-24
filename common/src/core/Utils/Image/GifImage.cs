using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace VVVV.Utils.Imaging
{
    //adapted from:
    //http://stackoverflow.com/questions/1196322/how-to-create-an-animated-gif-in-net

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
            SourceGraphicControlExtensionPosition = 781,
            SourceGraphicControlExtensionLength = 8,
            SourceImageBlockPosition = 789,
            SourceImageBlockHeaderLength = 11,
            SourceColorBlockPosition = 13,
            SourceColorBlockLength = 768;

        const string ApplicationIdentification = "NETSCAPE2.0",
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

        public void Save(Stream OutStream)
        {
            using (var Writer = new BinaryWriter(OutStream))
            {
                for (int i = 0; i < Count; ++i)
                {
                    var Frame = Frames[i];

                    using (var gifStream = new MemoryStream())
                    {
                        Frame.Image.Save(gifStream, ImageFormat.Gif);

                        // Steal the global color table info
                        if (i == 0) InitHeader(gifStream, Writer, Frame.Image.Width, Frame.Image.Height);

                        WriteGraphicControlBlock(gifStream, Writer, Frame.Delay);
                        WriteImageBlock(gifStream, Writer, i != 0, Frame.XOffset, Frame.YOffset, Frame.Image.Width, Frame.Image.Height);
                    }
                }

                // Complete File
                Writer.Write(FileTrailer);
            }
        }

        #region Write
        void InitHeader(Stream sourceGif, BinaryWriter Writer, int w, int h)
        {
            // File Header
            Writer.Write(FileType.ToCharArray());
            Writer.Write(FileVersion.ToCharArray());

            Writer.Write((short)(DefaultWidth == 0 ? w : DefaultWidth)); // Initial Logical Width
            Writer.Write((short)(DefaultHeight == 0 ? h : DefaultHeight)); // Initial Logical Height

            sourceGif.Position = SourceGlobalColorInfoPosition;
            Writer.Write((byte)sourceGif.ReadByte()); // Global Color Table Info
            Writer.Write((byte)0); // Background Color Index
            Writer.Write((byte)0); // Pixel aspect ratio
            WriteColorTable(sourceGif, Writer);

            // App Extension Header
            unchecked { Writer.Write((short)ApplicationExtensionBlockIdentifier); };
            Writer.Write((byte)ApplicationBlockSize);
            Writer.Write(ApplicationIdentification.ToCharArray());
            Writer.Write((byte)3); // Application block length
            Writer.Write((byte)1);
            Writer.Write((short)Repeat); // Repeat count for images.
            Writer.Write((byte)0); // terminator
        }

        void WriteColorTable(Stream sourceGif, BinaryWriter Writer)
        {
            sourceGif.Position = SourceColorBlockPosition; // Locating the image color table
            var colorTable = new byte[SourceColorBlockLength];
            sourceGif.Read(colorTable, 0, colorTable.Length);
            Writer.Write(colorTable, 0, colorTable.Length);
        }

        void WriteGraphicControlBlock(Stream sourceGif, BinaryWriter Writer, double frameDelay)
        {
            sourceGif.Position = SourceGraphicControlExtensionPosition; // Locating the source GCE
            var blockhead = new byte[SourceGraphicControlExtensionLength];
            sourceGif.Read(blockhead, 0, blockhead.Length); // Reading source GCE

            unchecked { Writer.Write((short)GraphicControlExtensionBlockIdentifier); }; // Identifier
            Writer.Write((byte)GraphicControlExtensionBlockSize); // Block Size
            Writer.Write((byte)(blockhead[3] & 0xf7 | 0x08)); // Setting disposal flag
            Writer.Write((short)(frameDelay)); // Setting frame delay
            Writer.Write((byte)blockhead[6]); // Transparent color index
            Writer.Write((byte)0); // Terminator
        }

        void WriteImageBlock(Stream sourceGif, BinaryWriter Writer, bool includeColorTable, int x, int y, int w, int h)
        {
            sourceGif.Position = SourceImageBlockPosition; // Locating the image block
            var header = new byte[SourceImageBlockHeaderLength];
            sourceGif.Read(header, 0, header.Length);
            Writer.Write((byte)header[0]); // Separator
            Writer.Write((short)x); // Position X
            Writer.Write((short)y); // Position Y
            Writer.Write((short)w); // Width
            Writer.Write((short)h); // Height

            if (includeColorTable) // If first frame, use global color table - else use local
            {
                sourceGif.Position = SourceGlobalColorInfoPosition;
                Writer.Write((byte)(sourceGif.ReadByte() & 0x3f | 0x80)); // Enabling local color table
                WriteColorTable(sourceGif, Writer);
            }
            else Writer.Write((byte)(header[9] & 0x07 | 0x07)); // Disabling local color table

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
