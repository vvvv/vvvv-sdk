using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
    public class InputAttribute : IOAttribute
    {
        public static readonly int DefaultBinSize = -1;
        public static readonly string DefaultBinName = " Bin Size";
        
        public InputAttribute(string name)
            :base(name)
        {
            BinSize = DefaultBinSize;
            BinName = DefaultBinName;
            BinVisibility = PinVisibility.True;
            BinOrder = 0;
            AutoValidate = true;
        }
        
        /// <summary>
        /// The bin size used in ISpread&lt;ISpread&lt;T&gt;&gt; implementations.
        /// </summary>
        public int BinSize
        {
            get;
            set;
        }
        
        /// <summary>
        /// The bin name used in ISpread&lt;ISpread&lt;T&gt;&gt; implementations.
        /// </summary>
        public string BinName
        {
            get;
            set;
        }
        
        /// <summary>
        /// The visibility of the bin size pin in the patch and inspektor.
        /// </summary>
        public PinVisibility BinVisibility
        {
            get;
            set;
        }
        
        /// <summary>
        /// Whether the pin is being validated on Evaluate or not.
        /// Validation triggers upstream node evaluation if upstream node was not
        /// evaluated yet in this frame.
        /// </summary>
        public bool AutoValidate
        {
            get;
            set;
        }
        
        /// <summary>
        /// Whether or not vvvv should check if the data changed from previous frame.
        /// This is by default disabled for numeric data as their spread counts are
        /// typically high and doing this check might have a negative impact on
        /// performance.
        /// </summary>
        public bool CheckIfChanged
        {
            get;
            set;
        }
        
        public override object Clone()
        {
            var clonedInstance = new InputAttribute(Name)
            {
                BinName = this.BinName,
                BinOrder = this.BinOrder,
                BinSize = this.BinSize,
                BinVisibility = this.BinVisibility,
                AutoValidate = this.AutoValidate,
                CheckIfChanged = this.CheckIfChanged
            };
            return base.Clone(clonedInstance);
        }
        
        public override string ToString()
        {
            return "Input";
        }

        public InputAttribute GetBinSizeInputAttribute(IIOContainer dataContainer)
        {
            return new InputAttribute(BinName == DefaultBinName ? GetBinSizeName(Name, dataContainer) : BinName)
            {
                DefaultValue = BinSize,
                // Don't do this, as spread max won't get computed for this pin
                // AutoValidate = false,
                Order = BinOrder,
                Visibility = BinVisibility,
                CheckIfChanged = true
            };
        }
    }
}
