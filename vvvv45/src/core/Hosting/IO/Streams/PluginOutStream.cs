using System;
using SlimDX.Direct3D9;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.Interfaces.EX9;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.IO.Streams
{
    // Slow
    abstract class PluginOutStream<T> : IOutStream<T>
    {
        class PluginOutWriter : IStreamWriter<T>
        {
            private readonly PluginOutStream<T> FStream;
            
            public PluginOutWriter(PluginOutStream<T> stream)
            {
                FStream = stream;
            }
            
            public bool Eos
            {
                get
                {
                    return Position >= Length;
                }
            }
            
            public int Position
            {
                get;
                set;
            }
            
            public int Length
            {
                get
                {
                    return FStream.Length;
                }
                set
                {
                    FStream.Length = value;
                }
            }
            
            public void Write(T value, int stride)
            {
                FStream.SetSlice(Position, value);
                Position += stride;
            }
            
            public int Write(T[] buffer, int index, int length, int stride)
            {
                var numSlicesToWrite = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
                for (int i = index; i < index + numSlicesToWrite; i++)
                {
                    Write(buffer[i], stride);
                }
                return numSlicesToWrite;
            }
            
            public void Dispose()
            {
                // Nothing to do
            }
            
            public void Reset()
            {
                Position = 0;
            }
        }
        
        public object Clone()
        {
            throw new NotImplementedException();
        }
        
        public abstract int Length
        {
            get;
            set;
        }
        
        protected abstract void SetSlice(int index, T value);
        
        public void Flush()
        {
            // Nothing to do
        }
        
        public IStreamWriter<T> GetWriter()
        {
            return new PluginOutWriter(this);
        }
    }
    
    class StringOutStream : ManagedIOStream<string>
    {
        private readonly IStringOut FStringOut;
        
        public StringOutStream(IStringOut stringOut)
        {
            FStringOut = stringOut;
        }
        
        public override void Flush()
        {
            if (FChanged)
            {
                FStringOut.SliceCount = Length;
                
                int i = 0;
                using (var reader = GetReader())
                {
                    while (!reader.Eos)
                    {
                        FStringOut.SetString(i++, reader.Read());
                    }
                }
            }
            
            base.Flush();
        }
    }
    
    class EnumOutStream<T> : PluginOutStream<T>
    {
        protected readonly IEnumOut FEnumOut;
        
        public EnumOutStream(IEnumOut enumOut)
        {
            FEnumOut = enumOut;
        }
        
        protected override void SetSlice(int index, T value)
        {
            FEnumOut.SetString(index, value.ToString());
        }
        
        public override int Length
        {
            get
            {
                return FEnumOut.SliceCount;
            }
            set
            {
                FEnumOut.SliceCount = value;
            }
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
    
    class NodeOutStream<T> : PluginOutStream<T>, IGenericIO
    {
        private readonly INodeOut FNodeOut;
        private readonly ISpread<T> FDataStore;
        
        public NodeOutStream(INodeOut nodeOut)
            : this(nodeOut, new DefaultConnectionHandler())
        {
            
        }
        
        public NodeOutStream(INodeOut nodeOut, IConnectionHandler handler)
            : this(nodeOut, new Spread<T>(), handler)
        {
            
        }
        
        private NodeOutStream(INodeOut nodeOut, ISpread<T> dataStore, IConnectionHandler handler)
        {
            FNodeOut = nodeOut;
            FNodeOut.SetInterface(this);
            FNodeOut.SetConnectionHandler(handler, this);
            FDataStore = dataStore;
        }
        
        object IGenericIO.GetSlice(int index)
        {
            return FDataStore[index];
        }
        
        protected override void SetSlice(int index, T value)
        {
            FDataStore[index] = value;
        }
        
        public override int Length
        {
            get
            {
                return FNodeOut.SliceCount;
            }
            set
            {
                FNodeOut.SliceCount = value;
                FDataStore.SliceCount = value;
            }
        }
    }
    
    class TextureOutStream<T, TMetadata> : ManagedIOStream<T>, IDXTexturePin
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
            if (FChanged)
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
                using (var reader = GetReader())
                {
                    reader.Position = slice;
                    return reader.Read()[device];
                }
            }
        }
        
        void IDXResourcePin.UpdateResources(Device device)
        {
            foreach (var resource in this)
            {
                resource.UpdateResource(device);
            }
        }
        
        void IDXResourcePin.DestroyResources(Device device, bool onlyUnmanaged)
        {
            foreach (var resource in this)
            {
                resource.DestroyResource(device);
            }
        }
    }
}
