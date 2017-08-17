using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
    public class OutputAttribute : IOAttribute
    {
        public static readonly string DefaultBinName = " Bin Size";
        
        public OutputAttribute(string name)
            :base(name)
        {
            BinName = DefaultBinName;
            BinVisibility = PinVisibility.True;
            BinOrder = 0;
            AutoFlush = true;
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
        /// Whether the pin is flushed after Evaluate or not.
        /// </summary>
        public bool AutoFlush
        {
            get;
            set;
        }

        /// <summary>
        /// Whether or not feedback loops are allowed on this pin.
        /// By default disabled.
        /// </summary>
        [Obsolete("You may want to implement IPluginFeedbackLoop. It allows you to specify in detail on which inputs this output depends on.", false)]
        public bool AllowFeedback
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
                AutoFlush = this.AutoFlush,
                AllowFeedback = this.AllowFeedback
            };
            return base.Clone(clonedInstance);
        }
        
        public override string ToString()
        {
            return "Output";
        }

        public OutputAttribute GetBinSizeOutputAttribute(IIOContainer dataContainer)
        {
            return new OutputAttribute(BinName == DefaultBinName ? GetBinSizeName(Name, dataContainer) : BinName)
            {
                Order = BinOrder,
                Visibility = BinVisibility
            };
        }
    }
}
