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
    
    class TextureOutStream<T, TMetadata> : BufferedIOStream<T>, IDXTexturePin
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

    class MeshOutStream<T, TMetadata> : BufferedIOStream<T>, IDXMeshPin
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
