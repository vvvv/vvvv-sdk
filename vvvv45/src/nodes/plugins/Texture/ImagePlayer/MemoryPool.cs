using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace VVVV.Nodes.ImagePlayer
{
    public static class MemoryPool
    {
        public class Memory : IDisposable
        {
            private readonly Stack<Stream> FStack;
            
            internal Memory(Stack<Stream> stack, Stream stream)
            {
                FStack = stack;
                Stream = stream;
            }
            
            public Stream Stream
            {
                get;
                private set;
            }
            
            public void Dispose()
            {
                lock (MemoryPool.FPool)
                {
                    FStack.Push(Stream);
                }
            }
        }
        
        private static readonly Dictionary<int, Stack<Stream>> FPool = new Dictionary<int, Stack<Stream>>();
        
        public static Memory GetStream(int length)
        {
            lock (FPool)
            {
                Stack<Stream> stack = null;
                if (!FPool.TryGetValue(length, out stack))
                {
                    stack = new Stack<Stream>();
                    FPool[length] = stack;
                }
                
                if (stack.Count == 0)
                {
                    return new Memory(stack, new MemoryStream(length));
                }
                else
                {
                    var stream = stack.Pop();
                    stream.SetLength(length);
                    stream.Position = 0;
                    return new Memory(stack, stream);
                }
            }
        }
    }
}
