using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Utils.Streams
{
    // Sets the length of the underlying stream to the number of items
    // written to it on disposal.
    // Useful when the number of items to write is not known in advance.
    public class DynamicStreamWriter<T> : IStreamWriter<T>
    {
        private readonly IOutStream<T> FStream;
        private readonly int FStreamLength;
        private IStreamWriter<T> FStreamWriter;
        private MemoryIOStream<T> FMemoryStream;
        private MemoryIOStream<T>.StreamWriter FMemoryStreamWriter;
        private int FPosition;

        public DynamicStreamWriter(IOutStream<T> stream)
        {
            FStream = stream;
            FStreamLength = stream.Length;
            FStreamWriter = stream.GetWriter();
        }

        private MemoryIOStream<T>.StreamWriter MemoryStreamWriter
        {
            get
            {
                if (FMemoryStreamWriter == null)
                {
                    var length = FPosition - FStreamLength;
                    FMemoryStream = new MemoryIOStream<T>(length, length, true);
                    FMemoryStreamWriter = FMemoryStream.GetWriter();
                    FMemoryStreamWriter.Position = length;
                }
                return FMemoryStreamWriter;
            }
        }

        public void Write(T value, int stride = 1)
        {
            if (FPosition >= FStreamLength)
                MemoryStreamWriter.Write(value, stride);
            else
                FStreamWriter.Write(value, stride);
            FPosition += stride;
        }

        public int Write(T[] buffer, int index, int length, int stride = 1)
        {
            int slicesWritten;
            if (FPosition >= FStreamLength)
            {
                slicesWritten = MemoryStreamWriter.Write(buffer, index, length, stride);
                FPosition += slicesWritten * stride;
            }
            else
            {
                slicesWritten = FStreamWriter.Write(buffer, index, length, stride);
                FPosition += slicesWritten * stride;
                if (slicesWritten < length)
                {
                    // Corner case
                    index += slicesWritten;
                    length -= slicesWritten;
                    slicesWritten += Write(buffer, index, length, stride);
                }
            }
            return slicesWritten;
        }

        public void Reset()
        {
            FPosition = 0;
            if (FMemoryStream != null)
            {
                FMemoryStreamWriter.Dispose();
                FMemoryStreamWriter = null;
                FMemoryStream = null;
            }
        }

        public bool Eos
        {
            get { return false; }
        }

        public int Position
        {
            get
            {
                return FPosition;
            }
            set
            {
                FPosition = value;
            }
        }

        public int Length
        {
            get 
            {
                if (FMemoryStream != null)
                    return FStream.Length + FMemoryStream.Length;
                else
                    return FStream.Length;
            }
        }

        public void Dispose()
        {
            if (FStreamWriter != null)
            {
                FStreamWriter.Dispose();
                FStreamWriter = null;

                if (FPosition > FStreamLength)
                {
                    // Stream is larger now
                    FStream.Append(FMemoryStream);
                    FMemoryStreamWriter.Dispose();
                    FMemoryStreamWriter = null;
                    FMemoryStream = null;
                }
                else
                {
                    // Stream is smaller now
                    FStream.Length = FPosition;
                }
            }
        }
    }
}
