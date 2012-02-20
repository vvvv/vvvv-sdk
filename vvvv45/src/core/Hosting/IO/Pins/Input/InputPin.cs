using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
    class InputPin<T> : Pin<T>
    {
        private readonly IPluginIn FPluginIn;
        private readonly bool FAutoValidate;
        private readonly bool FManagesChanges;
        
        public InputPin(IPluginIn pluginIn, BufferedIOStream<T> stream)
            : base(pluginIn, stream)
        {
            FPluginIn = pluginIn;
            FAutoValidate = pluginIn.AutoValidate;
            FManagesChanges = !(FPluginIn is IPluginFastIn);
        }
        
        public InputPin(IPluginIn pluginIn, IInStream<T> stream)
            : this(pluginIn, new BufferedInputIOStream<T>(stream))
        {
            
        }
        
        public override bool Sync()
        {
            if (FAutoValidate)
            {
                if (FManagesChanges && !FPluginIn.PinIsChanged)
                {
                    return false;
                }
            }
            return base.Sync();
        }
    }
    
    class BufferedInputIOStream<T> : BufferedIOStream<T>
    {
        private readonly IInStream<T> FInStream;
        
        public BufferedInputIOStream(IInStream<T> inStream)
        {
            FInStream = inStream;
        }
        
        public override bool Sync()
        {
            if (FInStream.Sync())
            {
                this.AssignFrom(FInStream);
                return true;
            }
            return false;
        }
    }
    
    [ComVisible(false)]
    class InputIOStream<T> : IIOStream<T>
    {
        private readonly IInStream<T> FInStream;
        private readonly BufferedIOStream<T> FIOStream;
        private IInStream<T> FCurrentInStream;
        
        public InputIOStream(IInStream<T> inStream)
        {
            FInStream = inStream;
            FIOStream = new BufferedIOStream<T>();
            FCurrentInStream = FInStream;
        }
        
        public int Length
        {
            get
            {
                return FCurrentInStream.Length;
            }
            set
            {
                if (Length != value)
                {
                    CopyOnWrite();
                    FIOStream.Length = value;
                }
            }
        }
        
        public bool Sync()
        {
            var changed = FInStream.Sync();
            FCurrentInStream = FInStream;
            return changed;
        }
        
        public object Clone()
        {
            return new InputIOStream<T>(FInStream.Clone() as IInStream<T>);
        }
        
        private void CopyOnWrite()
        {
            if (FCurrentInStream == FInStream)
            {
                // Copy data
                FIOStream.AssignFrom(FInStream);
                
                // Set current inStream to ioStream
                FCurrentInStream = FIOStream;
            }
        }
        
        public void Flush()
        {
            FIOStream.Flush();
        }
        
        public IStreamReader<T> GetReader()
        {
            return FCurrentInStream.GetReader();
        }
        
        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            return FCurrentInStream.GetEnumerator();
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public IStreamWriter<T> GetWriter()
        {
            CopyOnWrite();
            return FIOStream.GetWriter();
        }
    }
}
