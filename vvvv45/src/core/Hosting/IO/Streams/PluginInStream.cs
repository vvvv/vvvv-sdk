
using System;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using System.IO;

namespace VVVV.Hosting.IO.Streams
{
    class StringInStream : MemoryIOStream<string>
    {
        private readonly IStringIn FStringIn;
        private readonly bool FAutoValidate;
        
        public StringInStream(IStringIn stringIn)
        {
            FStringIn = stringIn;
            FAutoValidate = stringIn.AutoValidate;
        }
        
        public override bool Sync()
        {
            IsChanged = FAutoValidate ? FStringIn.PinIsChanged : FStringIn.Validate();
            if (IsChanged)
            {
                Length = FStringIn.SliceCount;
                using (var writer = GetWriter())
                {
                    for (int i = 0; i < Length; i++)
                    {
                        string result;
                        FStringIn.GetString(i, out result);
                        writer.Write(result ?? string.Empty);
                    }
                }
            }
            return base.Sync();
        }
    }
    
    class EnumInStream<T> : MemoryIOStream<T>
    {
        protected readonly IEnumIn FEnumIn;
        protected readonly bool FAutoValidate;
        
        public EnumInStream(IEnumIn enumIn)
        {
            FEnumIn = enumIn;
            FAutoValidate = enumIn.AutoValidate;
        }
        
        public override bool Sync()
        {
            IsChanged = FAutoValidate ? FEnumIn.PinIsChanged : FEnumIn.Validate();
            if (IsChanged)
            {
                Length = FEnumIn.SliceCount;
                using (var writer = GetWriter())
                {
                    for (int i = 0; i < Length; i++)
                    {
                        string entry;
                        FEnumIn.GetString(i, out entry);
                        try
                        {
                            writer.Write((T)Enum.Parse(typeof(T), entry));
                        }
                        catch (Exception)
                        {
                            writer.Write(default(T));
                        }
                    }
                }
            }
            return base.Sync();
        }
    }
    
    class DynamicEnumInStream : EnumInStream<EnumEntry>
    {
        private readonly string FEnumName;
        
        public DynamicEnumInStream(IEnumIn enumIn, string enumName)
            : base(enumIn)
        {
            FEnumName = enumName;
        }
        
        public override bool Sync()
        {
            IsChanged = FAutoValidate ? FEnumIn.PinIsChanged : FEnumIn.Validate();
            if (IsChanged)
            {
                Length = FEnumIn.SliceCount;
                using (var writer = GetWriter())
                {
                    for (int i = 0; i < Length; i++)
                    {
                        int ord;
                        FEnumIn.GetOrd(i, out ord);
                        writer.Write(new EnumEntry(FEnumName, ord));
                    }
                }
            }
            return IsChanged;
        }
    }
    
    class NodeInStream<T> : MemoryIOStream<T>
    {
        private readonly INodeIn FNodeIn;
        private readonly bool FAutoValidate;
        
        public NodeInStream(INodeIn nodeIn, IConnectionHandler handler)
        {
            FNodeIn = nodeIn;
            FNodeIn.SetConnectionHandler(handler, this);
            FAutoValidate = nodeIn.AutoValidate;
        }
        
        public NodeInStream(INodeIn nodeIn)
            : this(nodeIn, new DefaultConnectionHandler())
        {
        }
        
        public override bool Sync()
        {
            IsChanged = FAutoValidate ? FNodeIn.PinIsChanged : FNodeIn.Validate();
            if (IsChanged)
            {
                Length = FNodeIn.SliceCount;
                using (var writer = GetWriter())
                {
                    object usI;
                    FNodeIn.GetUpstreamInterface(out usI);
                    var upstreamInterface = usI as IGenericIO;
                    
                    for (int i = 0; i < Length; i++)
                    {
                        int usS;
                        var result = default(T);
                        if (upstreamInterface != null)
                        {
                            FNodeIn.GetUpsreamSlice(i, out usS);
                            result = (T) upstreamInterface.GetSlice(usS);
                        }
                        writer.Write(result);
                    }
                }
            }
            return base.Sync();
        }
    }

    class RawInStream : IInStream<System.IO.Stream>
    {
        class RawInStreamReader : IStreamReader<System.IO.Stream>
        {
            private readonly RawInStream FRawInStream;

            public RawInStreamReader(RawInStream stream)
            {
                FRawInStream = stream;
            }

            public Stream Read(int stride = 1)
            {
                VVVV.Utils.Win32.IStream stream;
                FRawInStream.FRawIn.GetData(Position, out stream);
                Position += stride;
                if (stream != null)
                {
                    var result = new ComStream(stream);
                    result.Position = 0;
                    return result;
                }
                else
                    return new MemoryStream(0);
            }

            public int Read(Stream[] buffer, int offset, int length, int stride = 1)
            {
                var numSlicesToRead = StreamUtils.GetNumSlicesAhead(this, offset, length, stride);
                for (int i = offset; i < numSlicesToRead; i++)
                    buffer[i] = Read(stride);
                return numSlicesToRead;
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
                get { return FRawInStream.Length; }
            }

            public void Dispose()
            {
                
            }

            public Stream Current
            {
                get;
                private set;
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                var result = !Eos;
                if (result)
                {
                    Current = Read();
                }
                return result;
            }

            public void Reset()
            {
                Position = 0;
            }
        }

        private readonly IRawIn FRawIn;
        private readonly bool FAutoValidate;
        private int FLength;
        
        public RawInStream(IRawIn rawIn)
        {
            this.FRawIn = rawIn;
            this.FAutoValidate = rawIn.AutoValidate;
            this.Length = rawIn.SliceCount;
        }

        public bool IsChanged { get; private set; }
        public int Length { get; private set; }
        
        public bool Sync()
        {
            IsChanged = FAutoValidate ? FRawIn.PinIsChanged : FRawIn.Validate();
            if (IsChanged)
            {
                Length = FRawIn.SliceCount;
            }
            return IsChanged;
        }

        public IStreamReader<Stream> GetReader()
        {
            return new RawInStreamReader(this);
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IEnumerator<Stream> GetEnumerator()
        {
            return GetReader();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
