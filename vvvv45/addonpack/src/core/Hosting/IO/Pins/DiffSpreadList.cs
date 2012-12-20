using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace VVVV.Hosting.Pins
{
    /// <summary>
    /// base class for diff spread lists
    /// </summary>
    [ComVisible(false)]
    abstract class DiffSpreadList<T> : Spread<ISpread<T>>, IDiffSpread<ISpread<T>>, IDisposable
    {
        protected readonly IIOFactory FIOFactory;
        protected readonly IOAttribute FAttribute;
        protected IDiffSpread<int> FCountSpread;
        protected int FOffsetCounter;
        protected int FUpdateCounter;
        protected static int FInstanceCounter = 1;
        
        public DiffSpreadList(IIOFactory ioFactory, IOAttribute attribute)
            : base(0)
        {
            //store fields
            FIOFactory = ioFactory;
            FAttribute = attribute;
            
            //create config pin
            var att = new ConfigAttribute(FAttribute.Name + " Count");
            att.DefaultValue = 2;
            
            //increment instance Counter and store it as pin offset
            FOffsetCounter = FInstanceCounter++;
            
            FCountSpread = ioFactory.CreateIO<IDiffSpread<int>>(att);
            FCountSpread.Changed += UpdatePins;
        }

        public virtual void Dispose()
        {
            FCountSpread.Changed -= UpdatePins;
            SliceCount = 0;
        }
        
        //pin management
        protected void UpdatePins(IDiffSpread<int> spread)
        {
            SliceCount = FCountSpread[0];
        }
        
        protected override void SliceCountIncreased(int oldSliceCount, int newSliceCount)
        {
            for (int i = oldSliceCount; i < newSliceCount; i++)
            {
                var diffSpread = CreateDiffSpread(i + 1);
                diffSpread.Changed += HandleDiffSpreadChanged;
                this[i] = diffSpread;
            }
        }
        
        protected override void SliceCountDecreasing(int oldSliceCount, int newSliceCount)
        {
            for (int i = newSliceCount; i < oldSliceCount; i++)
            {
                var diffSpread = this[i] as IDiffSpread<T>;
                if (diffSpread != null)
                    diffSpread.Changed -= HandleDiffSpreadChanged;
                var disposable = this[i] as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
        }

        protected void HandleDiffSpreadChanged(IDiffSpread<T> spread)
        {
            // TODO: Fix this. May get called more than once in one frame.
            OnChanged();
        }
        
        //the actual pin creation
        protected abstract IDiffSpread<T> CreateDiffSpread(int pos);
        
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
        
        protected virtual void OnChanged()
        {
            if (Changed != null)
                Changed(this);
            if (FChanged != null)
                FChanged(this);
        }
    }
}
