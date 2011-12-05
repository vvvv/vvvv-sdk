using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins
{
    /// <summary>
    /// base class for spread lists
    /// </summary>
    [ComVisible(false)]
    abstract class SpreadList<T> : Spread<ISpread<T>>, IDisposable
    {
        protected readonly IIOFactory FIOFactory;
        protected readonly IOAttribute FAttribute;
        protected IDiffSpread<int> FCountSpread;
        protected int FOffsetCounter;
        protected static int FInstanceCounter = 1;
        
        public SpreadList(IIOFactory ioFactory, IOAttribute attribute)
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
            
            FCountSpread = FIOFactory.CreateIO<IDiffSpread<int>>(att);
            FCountSpread.Changed += UpdatePins;
            
//            FCountSpread.Update();
        }
        
        public virtual void Dispose()
        {
            FCountSpread.Changed -= UpdatePins;
//            FCountSpread.Dispose();
            SliceCount = 0;
        }
        
        //pin management
        protected void UpdatePins(IDiffSpread<int> configSpread)
        {
            SliceCount = FCountSpread[0];
        }
        
        public override int SliceCount 
        {
            get 
            { 
                return base.SliceCount; 
            }
            set
            { 
                int oldSliceCount = SliceCount;
                int newSliceCount = value;
                
                if (newSliceCount < oldSliceCount)
                    SliceCountDecreasing(oldSliceCount, newSliceCount);
                
                base.SliceCount = value; 
                
                if (newSliceCount > oldSliceCount)
                    SliceCountIncreased(oldSliceCount, newSliceCount);
            }
        }
        
        private void SliceCountIncreased(int oldSliceCount, int newSliceCount)
        {
            for (int i = oldSliceCount; i < newSliceCount; i++)
            {
                this[i] = CreateSpread(i + 1);
            }
        }
        
        private void SliceCountDecreasing(int oldSliceCount, int newSliceCount)
        {
            for (int i = newSliceCount; i < oldSliceCount; i++)
            {
                var disposable = this[i] as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
        }
        
        protected abstract ISpread<T> CreateSpread(int pos);
    }
}
