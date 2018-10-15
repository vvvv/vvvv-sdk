
using System;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;

namespace VVVV.Hosting.IO.Streams
{
    // Slow
    public abstract class PluginConfigStream<T> : MemoryIOStream<T>
    {  
        private readonly IPluginConfig FPluginConfig;
        private bool FIsFlushing;
        private int FPreviousChangeCount;
        
        public PluginConfigStream(IPluginConfig pluginConfig)
        {
            FPluginConfig = pluginConfig;
        }
        
        protected abstract T GetSlice(int index);
        
        protected abstract void SetSlice(int index, T value);
        
        public override sealed bool Sync()
        {
            if (FIsFlushing) 
                return IsChanged;
            IsChanged = FPluginConfig.PinIsChanged;
            if (IsChanged)
            {
                Length = FPluginConfig.SliceCount;
                var writer = GetWriter();
                for (int i = 0; i < Length; i++)
                {
                    writer.Write(GetSlice(i));
                }
                // Remember the change count in order to avoid cyclic flush/sync behaviour
                FPreviousChangeCount = FChangeCount;
            }
            return base.Sync();
        }

        public override sealed void Flush(bool force = false)
        {
            FIsFlushing = true;
            try 
            {
                if (FPreviousChangeCount < FChangeCount)
                {
                    FPluginConfig.SliceCount = Length;
                    var reader = GetReader();
                    for (int i = 0; i < Length; i++)
                    {
                        SetSlice(i, reader.Read());
                    }
                }
            } 
            finally 
            {
                base.Flush(force);
                FIsFlushing = false;
                FPreviousChangeCount = FChangeCount;
            }
        }
    }
    
    class StringConfigStream : PluginConfigStream<string>
    {
        private readonly IStringConfig FStringConfig;
        
        public StringConfigStream(IStringConfig stringConfig)
            : base(stringConfig)
        {
            FStringConfig = stringConfig;
        }
        
        protected override string GetSlice(int index)
        {
            string result;
            FStringConfig.GetString(index, out result);
            result = result ?? string.Empty;
            return result;
        }
        
        protected override void SetSlice(int index, string value)
        {
            FStringConfig.SetString(index, value);
        }
    }
    
    class ValueConfigStream<T> : PluginConfigStream<T> where T : struct
    {
        private readonly IValueConfig FValueConfig;
        private readonly TypeCode FTypeCode;
        
        public ValueConfigStream(IValueConfig valueConfig)
            : base(valueConfig)
        {
            FValueConfig = valueConfig;
            FTypeCode = Type.GetTypeCode(typeof(T));
        }
        
        protected override T GetSlice(int index)
        {
            double result;
            FValueConfig.GetValue(index, out result);
            switch (FTypeCode) 
            {
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                case TypeCode.Char:
                case TypeCode.DateTime:
                case TypeCode.String:
                    throw new NotSupportedException(string.Format("Type '{0}' is not supported by {1}.", typeof(T), this));
                case TypeCode.Boolean:
                    return (T) (object) (result >= 0.5);
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return (T) Convert.ChangeType(result, FTypeCode);
                default:
                    throw new Exception("Invalid value for TypeCode");
            }
        }
        
        protected override void SetSlice(int index, T value)
        {
            switch (FTypeCode) 
            {
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                case TypeCode.Char:
                case TypeCode.DateTime:
                case TypeCode.String:
                    throw new NotSupportedException(string.Format("Type '{0}' is not supported by {1}.", typeof(T), this));
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    FValueConfig.SetValue(index, (double) Convert.ChangeType(value, Type.GetTypeCode(typeof(double))));
                    break;
                default:
                    throw new Exception("Invalid value for TypeCode");
            }
        }
    }
    
    class ColorConfigStream : PluginConfigStream<RGBAColor>
    {
        private readonly IColorConfig FColorConfig;
        
        public ColorConfigStream(IColorConfig colorConfig)
            : base(colorConfig)
        {
            FColorConfig = colorConfig;
        }
        
        protected override RGBAColor GetSlice(int index)
        {
            RGBAColor result;
            FColorConfig.GetColor(index, out result);
            return result;
        }
        
        protected override void SetSlice(int index, RGBAColor value)
        {
            FColorConfig.SetColor(index, value);
        }
    }
    
    class SlimDXColorConfigStream : PluginConfigStream<Color4>
    {
        private readonly IColorConfig FColorConfig;
        
        public SlimDXColorConfigStream(IColorConfig colorConfig)
            : base(colorConfig)
        {
            FColorConfig = colorConfig;
        }
        
        protected override Color4 GetSlice(int index)
        {
            RGBAColor result;
            FColorConfig.GetColor(index, out result);
            return new Color4((float) result.R, (float) result.G, (float) result.B, (float) result.A);
        }
        
        protected override void SetSlice(int index, Color4 value)
        {
            var dst = new RGBAColor(value.Red, value.Green, value.Blue, value.Alpha);
            FColorConfig.SetColor(index, dst);
        }
    }
    
    class EnumConfigStream<T> : PluginConfigStream<T>
    {
        protected readonly IEnumConfig FEnumConfig;
        
        public EnumConfigStream(IEnumConfig enumConfig)
            : base(enumConfig)
        {
            FEnumConfig = enumConfig;
        }
        
        protected override void SetSlice(int index, T value)
        {
            FEnumConfig.SetString(index, value.ToString());
        }
        
        protected override T GetSlice(int index)
        {
            string entry;
            FEnumConfig.GetString(index, out entry);
            try
            {
                return (T)Enum.Parse(typeof(T), entry);
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
    
    class DynamicEnumConfigStream : EnumConfigStream<EnumEntry>
    {
        private string FEnumName;

        public DynamicEnumConfigStream(IEnumConfig enumConfig, string enumname)
            : base(enumConfig)
        {
            this.FEnumName = enumname;
        }
        
        protected override EnumEntry GetSlice(int index)
        {
            int ord;
            string name;
            FEnumConfig.GetOrd(index, out ord);
            FEnumConfig.GetString(index, out name);
            return new EnumEntry(FEnumName, ord, name);
        }
        
        protected override void SetSlice(int index, EnumEntry value)
        {
            FEnumConfig.SetOrd(index, value.Index);
        }
    }
}
