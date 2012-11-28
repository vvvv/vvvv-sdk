
using System;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.IO.Streams
{
    class StringInStream : BufferedIOStream<string>
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
    
    class EnumInStream<T> : BufferedIOStream<T>
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
    
    class NodeInStream<T> : BufferedIOStream<T>
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

    class RawInStream : BufferedIOStream<System.IO.Stream>
    {
        private readonly IRawIn FRawIn;
        private readonly bool FAutoValidate;
        
        public RawInStream(IRawIn rawIn)
        {
            FRawIn = rawIn;
            FAutoValidate = rawIn.AutoValidate;
            Length = 0;
        }
        
        public unsafe override bool Sync()
        {
            IsChanged = FAutoValidate ? FRawIn.PinIsChanged : FRawIn.Validate();
            if (IsChanged)
            {
                foreach (var memoryStream in this)
                {
                    if (memoryStream != null)
                        memoryStream.Dispose();
                }
                Length = FRawIn.SliceCount;
                using (var writer = GetWriter())
                {
                    for (int i = 0; i < Length; i++)
                    {
                        byte* pData;
                        int length;
                        FRawIn.GetData(i, out pData, out length);
                        if (pData != null)
                            writer.Write(new System.IO.UnmanagedMemoryStream(pData, length));
                        else
                            writer.Write(new System.IO.MemoryStream());
                    }
                }
            }
            else
            {
                // Reset the streams
                foreach (var memoryStream in this)
                    memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            }
            return base.Sync();
        }
    }
}
