using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;

namespace VVVV.PluginInterfaces.V2
{
    /// <summary>
    /// Common class that represents all available pin attributes.
    /// Note that not all properties make sense for every pin data type.
    /// </summary>
    [ComVisible(false)]
    public abstract class IOAttribute : ImportAttribute, ICloneable
    {
        public static readonly double DefaultMinValue = double.MinValue;
        public static readonly double DefaultMaxValue = double.MaxValue;
        public static readonly double DefaultStepSize = 0.01;
        
        public IOAttribute(string name)
        {
            //pin
            Name = name;
            Visibility = PinVisibility.True;
            IsSingle = false;
            
            //string
            StringType = StringType.String;
            FileMask = "All Files (*.*)|*.*";
            MaxChars = -1;
            DefaultString = "";
            
            //value
            MinValue = DefaultMinValue;
            MaxValue = DefaultMaxValue;
            StepSize = DefaultStepSize;
            DefaultValues = new double[] { 0.0, 0.0, 0.0, 0.0 };
            
            //bool
            DefaultBoolean = false;
            
            //color
            HasAlpha = true;
            DefaultColor = new double[] { 0.0, 1.0, 0.0, 1.0 };
            
            //enum
            EnumName = "_ANY_";
            
            Dimension = 1;
        }
        
        //pin
        /// <summary>
        /// The name of this pin
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        
        /// <summary>
        /// The visibility of the pin in the patch and inspektor.
        /// </summary>
        public PinVisibility Visibility
        {
            get;
            set;
        }
        
        /// <summary>
        /// Determines whether the pin supports only one slice.
        /// </summary>
        public bool IsSingle
        {
            get;
            set;
        }
        
        /// <summary>
        /// Slice mode of the pin.
        /// </summary>
        public SliceMode SliceMode
        {
            get
            {
                if (IsSingle)
                    return SliceMode.Single;
                else
                    return SliceMode.Dynamic;
            }
        }
        
        //value
        /// <summary>
        /// Minimum value for this value pin in the gui.
        /// </summary>
        public double MinValue
        {
            get;
            set;
        }
        
        /// <summary>
        /// Maximum value for this value pin in the gui.
        /// </summary>
        public double MaxValue
        {
            get;
            set;
        }
        
        /// <summary>
        /// Step size when dragging the value of this pin in the gui.
        /// </summary>
        public double StepSize
        {
            get;
            set;
        }
        
        /// <summary>
        /// Default value for this value pin.
        /// </summary>
        public double DefaultValue
        {
            get;
            set;
        }
        
        /// <summary>
        /// Array of default values for vector pins.
        /// </summary>
        public double[] DefaultValues
        {
            get;
            set;
        }
        
        /// <summary>
        /// Array of names shown in the gui for each dimension of vector pins.
        /// </summary>
        public string[] DimensionNames
        {
            get;
            set;
        }
        
        /// <summary>
        /// Converts a bool pin from toggle to bang.
        /// </summary>
        public bool IsBang
        {
            get;
            set;
        }
        
        /// <summary>
        /// This is used for bool pins.
        /// </summary>
        public bool IsToggle
        {
            get;
            set;
        }
        
        /// <summary>
        /// Default value for this toggle pin.
        /// </summary>
        public bool DefaultBoolean
        {
            get;
            set;
        }
        
        /// <summary>
        /// Displays the value of this pin as an integer in the gui, regardless of the actual type.
        /// </summary>
        public bool AsInt
        {
            get;
            set;
        }
        
        //string
        /// <summary>
        /// Default string for this string pin.
        /// </summary>
        public string DefaultString
        {
            get;
            set;
        }
        
        /// <summary>
        /// Set a special string type for this string pin.
        /// </summary>
        public StringType StringType
        {
            get;
            set;
        }
        
        /// <summary>
        /// Filemask in the form of: "Audio File (*.wav, *.mp3)|*.wav;*.mp3".
        /// </summary>
        public string FileMask
        {
            get;
            set;
        }
        
        /// <summary>
        /// Maximum length of the string for this pin.
        /// </summary>
        public int MaxChars
        {
            get;
            set;
        }
        
        //color
        /// <summary>
        /// Use alpha channel for this color pin.
        /// </summary>
        public bool HasAlpha
        {
            get;
            set;
        }
        
        /// <summary>
        /// Array of rgba values in the range [0..1] to define the default color.
        /// </summary>
        public double[] DefaultColor
        {
            get;
            set;
        }
        
        /// <summary>
        /// Name of the dynamic enum, only needed for EnumEntry type.
        /// </summary>
        public string EnumName
        {
            get;
            set;
        }
        
        /// <summary>
        /// String representation of the default enum entry for this pin.
        /// </summary>
        public string DefaultEnumEntry
        {
            get;
            set;
        }

        /// <summary>
        /// The default value for this node pin.
        /// </summary>
        public object DefaultNodeValue
        {
            get;
            set;
        }
        
        /// <summary>
        /// Converts an ISpread&lt;ISpread&lt;T&gt;&gt; from a bin sized spread to a dynamic pin group.
        /// </summary>
        public bool IsPinGroup
        {
            get;
            set;	
        }
        
        /// <summary>
        /// The order property helps the node to arrange its pins visually. The higher the order, the more right the pin appears on the node.
        /// </summary>
        public int Order
        {
            get;
            set;
        }

        /// <summary>
        /// The pins dimension count. Valid values: 1, 2, 3 or 4.
        /// </summary>
        public int Dimension
        {
            get;
            set;
        }

        /// <summary>
        /// The position of the bin size used in ISpread&lt;ISpread&lt;T&gt;&gt; implementations.
        /// </summary>
        public int BinOrder
        {
            get;
            set;
        }

        /// <summary>
        /// Tells the hosting code how many dimensions from a multi dimensional spread should be wrapped with a bin size pin.
        /// By default all dimensions will be wrapped.
        /// </summary>
        public int BinSizeWrapCount { get; set; } = int.MaxValue;

        public abstract object Clone();
        
        protected object Clone(IOAttribute clonedInstance)
        {
            clonedInstance.AllowDefault = AllowDefault;
            clonedInstance.AllowRecomposition = AllowRecomposition;
            clonedInstance.AsInt = AsInt;
            clonedInstance.BinOrder = BinOrder;
            clonedInstance.BinSizeWrapCount = BinSizeWrapCount;
            clonedInstance.DefaultColor = DefaultColor;
            clonedInstance.DefaultEnumEntry = DefaultEnumEntry;
            clonedInstance.DefaultString = DefaultString;
            clonedInstance.DefaultValue = DefaultValue;
            clonedInstance.DefaultValues = DefaultValues;
            clonedInstance.DefaultNodeValue = DefaultNodeValue;
            clonedInstance.Dimension = Dimension;
            clonedInstance.DimensionNames = DimensionNames;
            clonedInstance.EnumName = EnumName;
            clonedInstance.FileMask = FileMask;
            clonedInstance.HasAlpha = HasAlpha;
            clonedInstance.IsBang = IsBang;
            clonedInstance.DefaultBoolean = DefaultBoolean;
            clonedInstance.IsPinGroup = IsPinGroup;
            clonedInstance.IsSingle = IsSingle;
            clonedInstance.MaxChars = MaxChars;
            clonedInstance.MaxValue = MaxValue;
            clonedInstance.MinValue = MinValue;
            clonedInstance.Name = Name;
            clonedInstance.Order = Order;
            clonedInstance.RequiredCreationPolicy = RequiredCreationPolicy;
            clonedInstance.StepSize = StepSize;
            clonedInstance.StringType = StringType;
            clonedInstance.Visibility = Visibility;
            
            return clonedInstance;
        }

        protected string GetBinSizeName(string name, IIOContainer dataContainer)
        {
            var result = $"{name} Bin Size";
            var binSizeCount = GetBinSizePinCount(dataContainer);
            for (int i = 0; i < binSizeCount; i++)
                result = $"Bin Size of {result}";
            return result;
        }

        int GetBinSizePinCount(IIOContainer container)
        {
            var multiPin = container.RawIOObject as IIOMultiPin;
            if (multiPin != null && multiPin.AssociatedContainers.Length == 1)
                return GetBinSizePinCount(multiPin.AssociatedContainers[0]) + 1;
            return 0;
        }

        public bool IsBinSizeEnabled => BinSizeWrapCount > 0;

        public IOAttribute DecreaseBinSizeWrapCount()
        {
            var result = Clone() as IOAttribute;
            result.BinSizeWrapCount--;
            return result;
        }
    }
}
