using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
    class DiffInputBinSpread<T> : InputBinSpread<T>, IDiffSpread<ISpread<T>>
    {
        class DiffInputBinSpreadStream : InputBinSpreadStream
        {
            public DiffInputBinSpreadStream(IIOFactory ioFactory, InputAttribute attribute)
                : base(ioFactory, attribute)
            {
                
            }
            
            protected override InputAttribute ManipulateAttribute(InputAttribute attribute)
            {
                attribute.CheckIfChanged = true;
                return attribute;
            }
        }
        
        public DiffInputBinSpread(IIOFactory ioFactory, InputAttribute attribute)
            : base(ioFactory, attribute, new DiffInputBinSpreadStream(ioFactory, attribute))
        {
        }
        
        public override bool Sync()
        {
            var isChanged = base.Sync();
            
            if (isChanged)
            {
                OnChanged();
            }
            
            return isChanged;
        }
        
        protected virtual void OnChanged()
        {
            if (Changed != null)
                Changed(this);
            if (FChanged != null)
                FChanged(this);
        }

        public event SpreadChangedEventHander<ISpread<T>> Changed;
        
        protected SpreadChangedEventHander FChanged;
        event SpreadChangedEventHander IDiffSpread.Changed
        {
            add
            {
                FChanged += value;
            }
            remove
            {
                FChanged -= value;
            }
        }
    }
}
