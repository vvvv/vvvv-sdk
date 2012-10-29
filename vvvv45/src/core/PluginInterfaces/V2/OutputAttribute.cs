using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
    public sealed class OutputAttribute : IOAttribute
    {
        public static readonly string DefaultBinName = " Bin Size";
        
        public OutputAttribute(string name)
            :base(name)
        {
            BinName = DefaultBinName;
            BinVisibility = PinVisibility.True;
            BinOrder = 0;
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
        /// The position of the bin size used in ISpread&lt;ISpread&lt;T&gt;&gt; implementations.
        /// </summary>
        public int BinOrder
        {
            get;
            set;
        }
        
        public override object Clone()
        {
            var clonedInstance = new OutputAttribute(Name)
            {
                BinName = this.BinName,
                BinVisibility = this.BinVisibility,
                BinOrder = this.BinOrder
            };
            return base.Clone(clonedInstance);
        }
        
        public override string ToString()
        {
            return "Output";
        }

        public OutputAttribute GetBinSizeOutputAttribute()
        {
            return new OutputAttribute(BinName == DefaultBinName ? string.Format("{0} Bin Size", Name) : BinName)
            {
                Order = BinOrder,
                Visibility = BinVisibility
            };
        }
    }
}
