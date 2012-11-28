using System;
using SlimDX.Direct3D9;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.Interfaces.EX9;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.Streams;
using VVVV.Utils.VMath;
using SlimDX;
using System.Runtime.InteropServices;

namespace VVVV.Hosting.IO.Streams
{
    class StringOutStream : BufferedIOStream<string>
    {
        private readonly IStringOut FStringOut;
        
        public StringOutStream(IStringOut stringOut)
        {
            FStringOut = stringOut;
        }
        
        public override void Flush()
        {
            if (IsChanged)
            {
                FStringOut.SliceCount = Length;
                for (int i = 0; i < Length; i++)
                {
                    FStringOut.SetString(i, this[i]);
                }
            }
            base.Flush();
        }
    }
    
    class EnumOutStream<T> : BufferedIOStream<T>
    {
        protected readonly IEnumOut FEnumOut;
        
        public EnumOutStream(IEnumOut enumOut)
        {
            FEnumOut = enumOut;
        }
        
        public override void Flush()
        {
            if (IsChanged)
            {
                FEnumOut.SliceCount = Length;
                for (int i = 0; i < Length; i++)
                {
                    SetSlice(i, this[i]);
                }
            }
            base.Flush();
        }
        
        protected virtual void SetSlice(int index, T value)
        {
            FEnumOut.SetString(index, value.ToString());
        }
    }
    
    class DynamicEnumOutStream : EnumOutStream<EnumEntry>
    {
        public DynamicEnumOutStream(IEnumOut enumOut)
            : base(enumOut)
        {
        }
        
        protected override void SetSlice(int index, EnumEntry value)
        {
            FEnumOut.SetOrd(index, value.Index);
        }
    }
    
    class NodeOutStream<T> : BufferedIOStream<T>, IGenericIO
    {
        private readonly INodeOut FNodeOut;
        
        public NodeOutStream(INodeOut nodeOut)
            : this(nodeOut, new DefaultConnectionHandler())
        {
            
        }
        
        public NodeOutStream(INodeOut nodeOut, IConnectionHandler handler)
        {
            FNodeOut = nodeOut;
            FNodeOut.SetInterface(this);
            FNodeOut.SetConnectionHandler(handler, this);
        }
        
        object IGenericIO.GetSlice(int index)
        {
            return this[VMath.Zmod(index, Length)];
        }
        
        public override void Flush()
        {
            if (IsChanged)
            {
                FNodeOut.SliceCount = Length;
                FNodeOut.MarkPinAsChanged();
            }
            base.Flush();
        }
    }

    class RawOutStream : IOutStream<System.IO.Stream>
    {
        private readonly IRawOut FRawOut;
        private int length;
        private bool markPinAsChanged;

        public RawOutStream(IRawOut rawOut)
        {
            FRawOut = rawOut;
        }

        public void Flush()
        {
            if (markPinAsChanged)
            {
                this.FRawOut.MarkPinAsChanged();
            }
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public int Length
        {
            get { return this.length; }
            set
            {
                if (value != this.length)
                {
                    this.FRawOut.SliceCount = value;
                    this.length = value;
                }
            }
        }

        public IStreamWriter<System.IO.Stream> GetWriter()
        {
            markPinAsChanged = true;
            return new Writer(this);
        }

        class Writer : IStreamWriter<System.IO.Stream>
        {
            private RawOutStream rawOutStream;

            public Writer(RawOutStream rawOutStream)
            {
                this.rawOutStream = rawOutStream;
            }

            public unsafe void Write(System.IO.Stream value, int stride = 1)
            {
                var buffer = MemoryPool<byte>.GetArray();
                try
                {
                    byte* pByte;
                    var itemsToRead = (int)value.Length;
                    value.Position = 0;
                    this.rawOutStream.FRawOut.SetDataLength(this.Position, itemsToRead, out pByte);
                    while (itemsToRead > 0)
                    {
                        var destination = new IntPtr(pByte + value.Position);
                        var itemsRead = value.Read(buffer, 0, buffer.Length);
                        if (itemsRead > 0)
                        {
                            Marshal.Copy(buffer, 0, destination, itemsRead);
                        }
                        itemsToRead -= itemsRead;
                    }
                }
                finally
                {
                    MemoryPool<byte>.PutArray(buffer);
                }
                this.Position += stride;
            }

            public int Write(System.IO.Stream[] buffer, int index, int length, int stride = 1)
            {
                var numSlicesToWrite = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
                for (int i = index; i < numSlicesToWrite; i++)
                {
                    Write(buffer[i]);
                }
                return numSlicesToWrite;
            }

            public void Reset()
            {
                Position = 0;
            }

            public bool Eos
            {
                get { return Position >= Length; }
            }

            public int Position
            {
                get;
                set;
            }

            public int Length
            {
                get { return this.rawOutStream.Length; }
            }

            public void Dispose()
            {
                
            }
        }
    }

    abstract class ResourceOutStream<T, TResource, TMetadata> : BufferedIOStream<T>, IDXResourcePin
        where T : DXResource<TResource, TMetadata>
        where TResource : ComObject
    {
        void IDXResourcePin.UpdateResources(Device device)
        {
            foreach (var resource in this)
            {
                resource.UpdateResource(device);
            }
        }

        void IDXResourcePin.DestroyResources(Device device, bool onlyUnmanaged)
        {
            var isDx9ExDevice = device is DeviceEx;
            foreach (var resource in this)
            {
                // If we should destroy only unmanaged resources (those in the default pool)
                // do so only if we're on DirectX9 and the resource is in the default pool.
                // In case of DirectX9Ex where all resources are in the default pool we don't
                // need to do anything.
                if (!onlyUnmanaged || (resource.IsDefaultPool && !isDx9ExDevice))
                {
                    resource.DestroyResource(device);
                }
            }
        }
    }

    class TextureOutStream<T, TMetadata> : ResourceOutStream<T, Texture, TMetadata>, IDXTexturePin
        where T : DXResource<Texture, TMetadata>
    {
        private readonly IDXTextureOut FInternalTextureOut;
        
        public TextureOutStream(IInternalPluginHost host, OutputAttribute attribute)
        {
            FInternalTextureOut = host.CreateTextureOutput2(
                this,
                attribute.Name,
                (TSliceMode) attribute.SliceMode,
                (TPinVisibility) attribute.Visibility
               );
        }
        
        public override void Flush()
        {
            if (IsChanged)
            {
                FInternalTextureOut.SliceCount = Length;
                FInternalTextureOut.MarkPinAsChanged();
            }
            base.Flush();
        }
        
        Texture IDXTexturePin.this[Device device, int slice]
        {
            get
            {
                return this[slice][device];
            }
        }
    }

    class MeshOutStream<T, TMetadata> : ResourceOutStream<T, Mesh, TMetadata>, IDXMeshPin
        where T : DXResource<Mesh, TMetadata>
    {
        private readonly IDXMeshOut FInternalMeshOut;

        public MeshOutStream(IInternalPluginHost host, OutputAttribute attribute)
        {
            FInternalMeshOut = host.CreateMeshOutput2(
                this,
                attribute.Name,
                (TSliceMode)attribute.SliceMode,
                (TPinVisibility)attribute.Visibility
               );
        }

        public override void Flush()
        {
            if (IsChanged)
            {
                FInternalMeshOut.SliceCount = Length;
                FInternalMeshOut.MarkPinAsChanged();
            }
            base.Flush();
        }

        Mesh IDXMeshPin.this[Device device, int slice]
        {
            get
            {
                return this[slice][device];
            }
        }
    }
}
