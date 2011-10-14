using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
    public class InputBinSpread<T> : BinSpread<T>, IDisposable
    {
        protected DiffInputPin<int> FBinSizePin;
        protected Pin<T> FSpreadPin;
        protected int FUpdateCount;
        protected int FBinSize;
        
        public InputBinSpread(IPluginHost host, InputAttribute attribute)
            : base(attribute)
        {
            //data pin
            FSpreadPin = CreateDataPin(host, attribute);
            FSpreadPin.Updated += AnyPin_Updated;
            
            //bin size pin
            var att = new InputAttribute(attribute.Name + " Bin Size");
            att.DefaultValue = attribute.BinSize;
            att.Order = attribute.Order + 1;
            FBinSizePin = PinFactory.CreateDiffPin<int>(host, att);
            FBinSizePin.Updated += AnyPin_Updated;
        }
        
        public Pin<T> SpreadPin
        {
            get
            {
                return this.FSpreadPin;
            }
        }
        
        public virtual void Dispose()
        {
            FSpreadPin.Updated -= AnyPin_Updated;
            FBinSizePin.Updated -= AnyPin_Updated;
            FSpreadPin.Dispose();
            FBinSizePin.Dispose();
        }

        protected virtual bool NeedToBuildSpread()
        {
            return true;
        }
        
        protected virtual Pin<T> CreateDataPin(IPluginHost host, InputAttribute attribute)
        {
            return PinFactory.CreatePin<T>(host, attribute);
        }
        
        void AnyPin_Updated(object sender, EventArgs args)
        {
            FUpdateCount++;
            if (FUpdateCount > 1)
            {
                FUpdateCount = 0;
                if (NeedToBuildSpread())
                    FSpreadPin.Raise(FBinSizePin, this);
            }
        }
    }
}
