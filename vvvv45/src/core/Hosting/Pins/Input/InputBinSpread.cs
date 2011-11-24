using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using VVVV.Hosting.Streams;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
    public class InputBinSpread<T> : BinSpread<T>, IDisposable
    {
        protected IDiffSpread<int> FBinSizeSpread;
        protected ISpread<T> FDataSpread;
        protected int FUpdateCount;
        protected int FBinSize;
        
        public InputBinSpread(IOFactory ioFactory, InputAttribute attribute)
            : base(ioFactory, attribute)
        {
            //data pin
            FDataSpread = CreateDataSpread(attribute);
//            FDataSpread.Updated += AnyPin_Updated;
            
            //bin size pin
            var att = new InputAttribute(attribute.Name + " Bin Size");
            att.DefaultValue = attribute.BinSize;
            att.Order = attribute.Order + 1;
            FBinSizeSpread = FIOFactory.CreateIO<IDiffSpread<int>>(att);
//            FBinSizeSpread.Updated += AnyPin_Updated;
        }
        
//        public Pin<T> SpreadPin
//        {
//            get
//            {
//                return this.FDataSpread;
//            }
//        }
        
        public virtual void Dispose()
        {
//            FDataSpread.Updated -= AnyPin_Updated;
//            FBinSizeSpread.Updated -= AnyPin_Updated;
//            FDataSpread.Dispose();
//            FBinSizeSpread.Dispose();
        }

        protected virtual bool NeedToBuildSpread()
        {
            return true;
        }
        
        protected virtual ISpread<T> CreateDataSpread(InputAttribute attribute)
        {
            return FIOFactory.CreateIO<ISpread<T>>(attribute);
        }
        
        void AnyPin_Updated(object sender, EventArgs args)
        {
            FUpdateCount++;
            if (FUpdateCount > 1)
            {
                FUpdateCount = 0;
                if (NeedToBuildSpread())
                    FDataSpread.Raise(FBinSizeSpread, this);
            }
        }
    }
}
