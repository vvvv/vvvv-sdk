using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
    public class DiffInputBinSpread<T> : InputBinSpread<T>, IDiffSpread<ISpread<T>>
    {
        public class DiffInputBinSpreadStream : InputBinSpreadStream
        {
            public DiffInputBinSpreadStream(IIOFactory ioFactory, InputAttribute attribute)
                : base(ioFactory, attribute, true)
            {
            }

            public DiffInputBinSpreadStream(IIOFactory ioFactory, InputAttribute attribute, Func<IIOContainer, IIOContainer<IInStream<int>>> binSizeIOContainerFactory)
                : base(ioFactory, attribute, true, binSizeIOContainerFactory)
            {
            }
        }
        
        public DiffInputBinSpread(IIOFactory ioFactory, InputAttribute attribute)
            : base(ioFactory, attribute, new DiffInputBinSpreadStream(ioFactory, attribute))
        {
        }
        
        public DiffInputBinSpread(IIOFactory ioFactory, InputAttribute attribute, IIOContainer<IInStream<int>> binSizeIOContainer)
        	: base(ioFactory, attribute, new DiffInputBinSpreadStream(ioFactory, attribute, _ => binSizeIOContainer))
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
