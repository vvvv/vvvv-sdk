using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using VVVV.Hosting.Pins.Config;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins
{
    /// <summary>
    /// base class for spread lists
    /// </summary>
    [ComVisible(false)]
    public abstract class SpreadList<T> : Spread<ISpread<T>>, IDisposable
    {
        protected IPluginHost FHost;
        protected PinAttribute FAttribute;
        protected ConfigPin<int> FConfigPin;
        protected int FOffsetCounter;
        protected static int FInstanceCounter = 1;
        
        public SpreadList(IPluginHost host, PinAttribute attribute)
            : base(0)
        {
            //store fields
            FHost = host;
            FAttribute = attribute;
            
            //create config pin
            var att = new ConfigAttribute(FAttribute.Name + " Count");
            att.DefaultValue = 2;
            
            //increment instance Counter and store it as pin offset
            FOffsetCounter = FInstanceCounter++;
            
            FConfigPin = (ConfigPin<int>) PinFactory.CreatePin<int>(FHost, att);
            FConfigPin.Changed += UpdatePins;
            
            FConfigPin.Update();
        }
        
        public virtual void Dispose()
        {
            FConfigPin.Changed -= UpdatePins;
            FConfigPin.Dispose();
            SliceCount = 0;
        }
        
        //pin management
        protected void UpdatePins(IDiffSpread<int> configSpread)
        {
            SliceCount = FConfigPin[0];
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
