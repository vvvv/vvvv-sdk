using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace VVVV.Utils.Streams
{
    [ComVisible(false)]
    public class BufferedStreamWriter<T> : IStreamWriter<T>
    {
        private readonly IStreamWriter<T> FStreamWriter;
        private readonly T[] FBuffer;
        private int FOffset;

        internal BufferedStreamWriter(IOutStream<T> stream)
        {
            FStreamWriter = stream.GetWriter();
            FBuffer = MemoryPool<T>.GetArray();
        }

        public void Write(T value, int stride = 1)
        {
            if (stride != 1) throw new NotSupportedException();
            Write(value);
        }

        public int Write(T[] buffer, int index, int length, int stride = 1)
        {
            if (stride != 1) throw new NotSupportedException();
            if (FOffset + length < FBuffer.Length)
            {
                Array.Copy(buffer, index, FBuffer, FOffset, length);
                FOffset += length;
            }
            else
            {
                for (int i = 0; i < length; i++)
                    Write(buffer[i]);
            }
            return length;
        }

        private void Write(T value)
        {
            FBuffer[FOffset] = value;
            FOffset++;
            FlushIfBufferIsFull();
        }

        private void FlushIfBufferIsFull()
        {
            if (FOffset >= FBuffer.Length)
                Flush();
        }

        private void Flush()
        {
            FStreamWriter.Write(FBuffer, 0, FOffset);
            FOffset = 0;
        }

        public void Reset()
        {
            Position = 0;
        }

        public bool Eos
        {
            get { return FStreamWriter.Eos; }
        }

        public int Position
        {
            get
            {
                return FStreamWriter.Position;
            }
            set
            {
                Flush();
                FStreamWriter.Position = value;
            }
        }

        public int Length
        {
            get { return FStreamWriter.Length; }
        }

        public void Dispose()
        {
            Flush();
            FStreamWriter.Dispose();
            MemoryPool<T>.PutArray(FBuffer);
        }
    }
}
