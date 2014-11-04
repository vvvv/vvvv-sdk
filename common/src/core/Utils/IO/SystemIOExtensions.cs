using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Utils;
using VVVV.Utils.Streams;

namespace System.IO
{
    public static partial class SystemIOExtensions
    {
        public static bool StreamEquals(this Stream stream1, Stream stream2)
        {
            if (stream1 == null && stream2 != null)
                return false;
            if (stream1 != null && stream2 == null)
                return false;
            if (stream1.Equals(stream2))
                return true;

            stream1.Position = 0;
            stream2.Position = 0;
            try
            {
                using (var buffer1 = MemoryPool<byte>.GetBuffer())
                using (var buffer2 = MemoryPool<byte>.GetBuffer())
                {
                    var bufferSize = buffer1.Length;
                    while (true)
                    {
                        var count1 = stream1.Read(buffer1, 0, bufferSize);
                        // Check the case where stream1 and stream2 use the same
                        // underlying stream.
                        if (stream1.Position == stream2.Position)
                            return true;
                        var count2 = stream2.Read(buffer2, 0, bufferSize);
                        if (count1 != count2)
                            return false;
                        if (count1 == 0)
                            return true;
                        if (count1 != bufferSize)
                        {
                            for (int i = 0; i < count1; i++)
                                if (buffer1[i] != buffer2[i])
                                    return false;
                        }
                        else
                        {
                            if (!ArrayExtensions.ContentEquals(buffer1, buffer2))
                                return false;
                        }
                    }
                }
            }
            finally
            {
                stream1.Position = 0;
                stream2.Position = 0;
            }
        }

        public static void CopyTo(this Stream source, Stream destination, byte[] buffer)
        {
            int count;
            while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
                destination.Write(buffer, 0, count);
        }
    }
}
