using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace VVVV.Utils.Streams
{
    // Base class with various utility functions.
    [ComVisible(false)]
    public static class StreamUtils
    {
        class EmptyStream<T> : IIOStream<T>
        {
            class EmptyStreamReader : IStreamReader<T>
            {
                public bool Eos { get { return true; } }
                public int Position { get { return 0; } set { } }
                public int Length { get { return 0; } }
                public T Current { get { return default(T); } }
                
                object System.Collections.IEnumerator.Current
                {
                    get
                    {
                        return Current;
                    }
                }
                
                public T Read(int stride)
                {
                    return default(T);
                }
                
                public int Read(T[] buffer, int index, int length, int stride)
                {
                    return 0;
                }
                
                public void Dispose()
                {
                    
                }
                
                public bool MoveNext()
                {
                    return false;
                }
                
                public void Reset()
                {
                    
                }
            }
            
            class EmptyStreamWriter : IStreamWriter<T>
            {
                
                public bool Eos { get { return true; } }
                public int Position { get { return 0; } set { } }
                public int Length { get { return 0; } }
                
                public void Reset()
                {
                    
                }
                
                public void Write(T value, int stride)
                {
                    
                }
                
                public int Write(T[] buffer, int index, int length, int stride)
                {
                    return 0;
                }
                
                public void Dispose()
                {
                    
                }
            }
            
            public int Length { get { return 0; } set { } }
            
            public object Clone()
            {
                return new EmptyStream<T>();
            }
            
            public bool Sync()
            {
                return IsChanged;
            }
            
            public bool IsChanged { get { return false; } }
            
            public void Flush(bool force = false)
            {
                
            }
            
            public IStreamReader<T> GetReader()
            {
                return new EmptyStreamReader();
            }
            
            public System.Collections.Generic.IEnumerator<T> GetEnumerator()
            {
                return GetReader();
            }
            
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            
            public IStreamWriter<T> GetWriter()
            {
                return new EmptyStreamWriter();
            }
        }
        
        public const int BUFFER_SIZE = 1024;
        public const string STREAM_IN_USE_MSG = "Stream is in use.";
        
        public static IIOStream<T> GetEmptyStream<T>()
        {
            return new EmptyStream<T>();
        }
        
        public static CyclicStreamReader<T> GetCyclicReader<T>(this IInStream<T> stream)
        {
            return new CyclicStreamReader<T>(stream);
        }

        public static BufferedStreamWriter<T> GetBufferedWriter<T>(this IOutStream<T> stream)
        {
            return new BufferedStreamWriter<T>(stream);
        }

        public static DynamicStreamWriter<T> GetDynamicWriter<T>(this IOutStream<T> stream)
        {
            return new DynamicStreamWriter<T>(stream);
        }
        
        public static int GetNumSlicesAhead(IStreamer streamer, int index, int length, int stride)
        {
            int slicesAhead = streamer.Length - streamer.Position;
            
            if (stride > 1)
            {
                int r = 0;
                slicesAhead = Math.DivRem(slicesAhead, stride, out r);
                if (r > 0)
                    slicesAhead++;
            }
            
            return Math.Max(Math.Min(length, slicesAhead), 0);
        }
        
        public static int Read<T>(IStreamReader<T> reader, T[] buffer, int index, int length, int stride = 1)
        {
            int slicesToRead = GetNumSlicesAhead(reader, index, length, stride);
            
            switch (stride)
            {
                case 0:
                    if (index == 0 && slicesToRead == buffer.Length)
                        buffer.Init(reader.Read(stride)); // Slightly faster
                    else
                        buffer.Fill(index, slicesToRead, reader.Read(stride));
                    break;
                default:
                    for (int i = index; i < index + slicesToRead; i++)
                    {
                        buffer[i] = reader.Read(stride);
                    }
                    break;
            }
            
            return slicesToRead;
        }
        
        public static int CombineStreams(this int c1, int c2)
        {
            if (c1 == 0 || c2 == 0)
                return 0;
            else
                return Math.Max(c1, c2);
        }
        
        public static int CombineWith<U, V>(this IInStream<U> stream1, IInStream<V> stream2)
        {
            return CombineStreams(stream1.Length, stream2.Length);
        }

        public static int GetSpreadMax(params IInStream[] streams)
        {
            var result = 1;
            for (int i = 0; i < streams.Length; i++)
            {
                if (streams[i].Length == 0)
                    return 0;
                else
                    result = Math.Max(result, streams[i].Length);
            }
            return result;
        }

        public static void SetLengthBy<T>(this IOutStream<T> outStream, IInStream<IInStream<T>> inputStreams)
        {
            outStream.Length = inputStreams.GetMaxLength() * inputStreams.Length;
        }
        
        public static void SetLengthBy<T>(this IInStream<IOutStream<T>> outputStreams, IInStream<T> inputStream)
        {
            int outputLength = outputStreams.Length;
            int remainder = 0;
            int lengthPerStream = outputLength > 0 ? Math.DivRem(inputStream.Length, outputLength, out remainder) : 0;
            if (remainder > 0) lengthPerStream++;
            
            foreach (var outputStream in outputStreams)
            {
                outputStream.Length = lengthPerStream;
            }
        }

        public static int GetMaxLength<T>(this IEnumerable<IInStream<T>> streams)
        {
            var result = 0;
            foreach (var stream in streams)
            {
                var streamLength = stream.Length;
                if (streamLength == 0) return 0;
                result = Math.Max(result, streamLength);
            }
            return result;
        }

        public static int GetMaxLength(params IInStream[] streams)
        {
            switch (streams.Length)
            {
                case 0:
                    return 0;
                default:
                    int maxLength = streams[0].Length;
                    for (int i = 1; i < streams.Length; i++)
                    {
                        maxLength = maxLength.CombineStreams(streams[i].Length);
                    }
                    return maxLength;
            }
        }
        
        public static int GetLengthSum<T>(this IInStream<IInStream<T>> streams)
        {
            int result = 0;
            foreach (var stream in streams)
            {
                result += stream.Length;
            }
            return result;
        }
        
        public static void Write<T>(this IStreamWriter<T> writer, IInStream<T> inStream, T[] buffer)
        {
            using (var reader = inStream.GetReader())
            {
                while (!reader.Eos)
                {
                    int numSlicesRead = reader.Read(buffer, 0, buffer.Length);
                    writer.Write(buffer, 0, numSlicesRead);
                }
            }
        }
        
        public static void AssignFrom<T>(this IOutStream<T> outStream, IInStream<T> inStream, T[] buffer)
        {
            outStream.Length = inStream.Length;
            
            using (var reader = inStream.GetReader())
            using (var writer = outStream.GetWriter())
            {
                while (!reader.Eos)
                {
                    int numSlicesRead = reader.Read(buffer, 0, buffer.Length);
                    writer.Write(buffer, 0, numSlicesRead);
                }
            }
        }
        
        public static void AssignFrom<T>(this IOutStream<T> outStream, IInStream<T> inStream)
        {
            if (inStream.Length != 1)
            {
                var buffer = MemoryPool<T>.GetArray();
                try
                {
                    outStream.AssignFrom(inStream, buffer);
                }
                finally
                {
                    MemoryPool<T>.PutArray(buffer);
                }
            }
            else
            {
                outStream.Length = 1;
                using (var reader = inStream.GetReader())
                using (var writer = outStream.GetWriter())
                    writer.Write(reader.Read());
            }
        }

        public static void AssignFrom<T>(this IOutStream<T> outStream, IEnumerable<T> source)
        {
            using (var writer = outStream.GetDynamicWriter())
                foreach (var entry in source)
                    writer.Write(entry);
        }

        public static void AssignFrom<T>(this IOutStream<T> outStream, ICollection<T> source)
        {
            outStream.Length = source.Count;
            using (var writer = outStream.GetWriter())
                foreach (var entry in source)
                    writer.Write(entry);
        }

        public static void AssignFrom<T>(this IOutStream<T> outStream, IReadOnlyCollection<T> source)
        {
            outStream.Length = source.Count;
            using (var writer = outStream.GetWriter())
                foreach (var entry in source)
                    writer.Write(entry);
        }

        public static void Append<T>(this IOutStream<T> outStream, IInStream<T> inStream, T[] buffer)
        {
            var initialOutLength = outStream.Length;
            outStream.Length += inStream.Length;
            using (var writer = outStream.GetWriter())
            {
                writer.Position = initialOutLength;
                writer.Write(inStream, buffer);
            }
        }

        public static void Append<T>(this IOutStream<T> outStream, IInStream<T> inStream)
        {
            var buffer = MemoryPool<T>.GetArray();
            try
            {
                outStream.Append(inStream, buffer);
            }
            finally
            {
                MemoryPool<T>.PutArray(buffer);
            }
        }
        
        // From: http://en.wikipedia.org/wiki/Power_of_two
        public static bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }
        
        // From: http://en.wikipedia.org/wiki/Power_of_two
        public static int NextHigher(int k) {
            if (k == 0) return 1;
            k--;
            for (int i = 1; i < sizeof(int) * 8; i <<= 1)
                k = k | k >> i;
            return k + 1;
        }
        
        public static void ResizeAndDismiss<T>(this IIOStream<T> stream, int length)
            where T : new()
        {
            stream.ResizeAndDismiss(length, () => new T());
        }
        
        public static void ResizeAndDismiss<T>(this IIOStream<T> stream, int length, Func<T> constructor)
        {
            var initialPosition = stream.Length;
            stream.Length = length;
            
            var buffer = MemoryPool<T>.GetArray();
            try
            {
                using (var writer = stream.GetWriter())
                {
                    writer.Position = initialPosition;
                    var numSlicesToWrite = writer.Length - writer.Position;
                    while (numSlicesToWrite > 0)
                    {
                        var blockSize = Math.Min(StreamUtils.BUFFER_SIZE, numSlicesToWrite);
                        for (int i = 0; i < blockSize; i++)
                        {
                            buffer[i] = constructor();
                        }
                        numSlicesToWrite -= writer.Write(buffer, 0, blockSize);
                    }
                }
            }
            finally
            {
                MemoryPool<T>.PutArray(buffer);
            }
        }
        
        public static void ResizeAndDismiss<T>(this IIOStream<T> stream, int length, Func<int, T> constructor)
        {
            var initialPosition = stream.Length;
            stream.Length = length;
            
            var buffer = MemoryPool<T>.GetArray();
            try
            {
                using (var writer = stream.GetWriter())
                {
                    writer.Position = initialPosition;
                    var numSlicesToWrite = writer.Length - writer.Position;
                    while (numSlicesToWrite > 0)
                    {
                        var blockSize = Math.Min(StreamUtils.BUFFER_SIZE, numSlicesToWrite);
                        initialPosition = writer.Position;
                        for (int i = 0; i < blockSize; i++)
                        {
                            buffer[i] = constructor(initialPosition + i);
                        }
                        numSlicesToWrite -= writer.Write(buffer, 0, blockSize);
                    }
                }
            }
            finally
            {
                MemoryPool<T>.PutArray(buffer);
            }
        }
        
        public static void ResizeAndDispose<T>(this IIOStream<T> stream, int length, Func<T> constructor)
            where T : IDisposable
        {
            stream.Resize(length, constructor, (t) => t.Dispose());
        }
        
        public static void ResizeAndDispose<T>(this IIOStream<T> stream, int length)
            where T : IDisposable, new()
        {
            stream.Resize(length, () => new T(), (t) => t.Dispose());
        }
        
        public static void Resize<T>(this IIOStream<T> stream, int length, Func<T> constructor, Action<T> destructor)
        {
            var buffer = MemoryPool<T>.GetArray();
            try
            {
                using (var reader = stream.GetReader())
                {
                    reader.Position = length;
                    var numSlicesToRead = reader.Length - reader.Position;
                    while (numSlicesToRead > 0)
                    {
                        var blockSize = reader.Read(buffer, 0, StreamUtils.BUFFER_SIZE);
                        for (int i = 0; i < blockSize; i++)
                        {
                        	if(buffer[i] != null)
                            	destructor(buffer[i]);
                        }
                        numSlicesToRead -= blockSize;
                    }
                }
                
                var initialPosition = stream.Length;
                stream.Length = length;
                
                using (var writer = stream.GetWriter())
                {
                    writer.Position = initialPosition;
                    var numSlicesToWrite = writer.Length - writer.Position;
                    while (numSlicesToWrite > 0)
                    {
                        var blockSize = Math.Min(StreamUtils.BUFFER_SIZE, numSlicesToWrite);
                        for (int i = 0; i < blockSize; i++)
                        {
                            buffer[i] = constructor();
                        }
                        numSlicesToWrite -= writer.Write(buffer, 0, blockSize);
                    }
                }
            }
            finally
            {
                MemoryPool<T>.PutArray(buffer);
            }
        }
        
        public static void Resize<T>(this IIOStream<T> stream, int length, Func<int, T> constructor, Action<T> destructor)
        {
            var buffer = MemoryPool<T>.GetArray();
            try
            {
                using (var reader = stream.GetReader())
                {
                    reader.Position = length;
                    var numSlicesToRead = reader.Length - reader.Position;
                    while (numSlicesToRead > 0)
                    {
                        var blockSize = reader.Read(buffer, 0, StreamUtils.BUFFER_SIZE);
                        for (int i = 0; i < blockSize; i++)
                        {
                            destructor(buffer[i]);
                        }
                        numSlicesToRead -= blockSize;
                    }
                }
                
                var initialPosition = stream.Length;
                stream.Length = length;
                
                using (var writer = stream.GetWriter())
                {
                    writer.Position = initialPosition;
                    var numSlicesToWrite = writer.Length - writer.Position;
                    while (numSlicesToWrite > 0)
                    {
                        var blockSize = Math.Min(StreamUtils.BUFFER_SIZE, numSlicesToWrite);
                        initialPosition = writer.Position;
                        for (int i = 0; i < blockSize; i++)
                        {
                            buffer[i] = constructor(initialPosition + i);
                        }
                        numSlicesToWrite -= writer.Write(buffer, 0, blockSize);
                    }
                }
            }
            finally
            {
                MemoryPool<T>.PutArray(buffer);
            }
        }

        public static RangeStream<T> GetRange<T>(this IInStream<T> source, int offset, int count)
        {
            return new RangeStream<T>(source, offset, count);
        }

        public static RangeStream<T> Take<T>(this IInStream<T> source, int count)
        {
            return new RangeStream<T>(source, 0, count);
        }

        public static CyclicStream<T> Cyclic<T>(this IInStream<T> source)
        {
            return new CyclicStream<T>(source);
        }

        public static ReverseStream<T> Reverse<T>(this IInStream<T> source)
        {
            return new ReverseStream<T>(source);
        }

        public static MemoryIOStream<T> ToStream<T>(this IEnumerable<T> source)
        {
            return new MemoryIOStream<T>(source.ToArray());
        }

        public static bool AnyChanged(params IInStream[] streams)
        {
            foreach (var stream in streams)
                if (stream.IsChanged) { return true; }
            return false;
        }

        public static bool AllChanged(params IInStream[] streams)
        {
            foreach (var stream in streams)
                if (!stream.IsChanged) { return false; }
            return true;
        }
    }
}
