using System;
using System.Collections.Generic;
using System.IO;
using VL.Lib.Basics.Resources;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using com = System.Runtime.InteropServices.ComTypes;

namespace VVVV.VL.Hosting.IO.Streams
{
    class ResourceProviderInStream : MemoryIOStream<IResourceProvider<Stream>>
    {
        readonly IRawIn FRawIn;

        public ResourceProviderInStream(IRawIn rawIn)
        {
            FRawIn = rawIn;
        }

        protected override void BufferIncreased(IResourceProvider<Stream>[] oldBuffer, IResourceProvider<Stream>[] newBuffer)
        {
            Array.Copy(oldBuffer, newBuffer, oldBuffer.Length);
            for (int i = oldBuffer.Length; i < newBuffer.Length; i++)
                newBuffer[i] = global::VL.Lib.IO.StreamUtils.DefaultProvider;
        }

        public override bool Sync()
        {
            var isChanged = IsChanged = FRawIn.PinIsChanged;
            if (isChanged)
            {
                var length = Length = FRawIn.SliceCount;
                for (int i = 0; i < length; i++)
                {
                    com.IStream comStream;
                    FRawIn.GetData(i, out comStream);
                    this[i] = ResourceProvider.Return(Wrap(comStream)).ShareSerially().Do(s =>
                    {
                        if (s.CanSeek)
                            s.Position = 0;
                    });
                }
            }
            return isChanged;
        }

        Stream Wrap(com.IStream comStream)
        {
            if (comStream != null)
            {
                var netStream = comStream as Stream;
                if (netStream != null)
                    return netStream;
                return new ComAdapterStream(comStream);
            }
            return Stream.Null;
        }
    }

    class ResourceProviderOutStream : MemoryIOStream<IResourceProvider<Stream>>, IDisposable
    {
        readonly IRawOut FRawOut;
        List<IResourceHandle<Stream>> FHandles = new List<IResourceHandle<Stream>>();
        List<IResourceHandle<Stream>> FPreviousHandles = new List<IResourceHandle<Stream>>();

        public ResourceProviderOutStream(IRawOut rawOut)
        {
            FRawOut = rawOut;
        }

        public override bool Sync()
        {

            return base.Sync();
        }

        public override void Flush(bool force = false)
        {
            if (force || IsChanged)
            {
                try
                {
                    ReleaseHandles();
                    var handles = FHandles;
                    FRawOut.SliceCount = Length;
                    for (int i = 0; i < Length; i++)
                    {
                        var handle = (Buffer[i] ?? global::VL.Lib.IO.StreamUtils.DefaultProvider).GetHandle();
                        handles.Add(handle);
                        FRawOut.SetData(i, new AdapterComStream(handle.Resource));
                    }
                    FRawOut.MarkPinAsChanged();
                }
                finally
                {
                    SwapHandles();
                }
            }
            base.Flush(force);
        }

        void ReleaseHandles()
        {
            // Release all handles from previous frame
            var handles = FPreviousHandles;
            foreach (var handle in handles)
                handle.Dispose();
            handles.Clear();
        }

        void SwapHandles()
        {
            var handles = FHandles;
            FHandles = FPreviousHandles;
            FPreviousHandles = handles;
        }

        public void Dispose()
        {
            foreach (var handle in FHandles)
                handle?.Dispose();
        }
    }
}
