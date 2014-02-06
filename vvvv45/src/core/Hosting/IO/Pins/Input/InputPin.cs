using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VMath;
using SlimDX;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
    class InputPin<T> : Pin<T>
    {
        public InputPin(IIOFactory factory, IPluginIn pluginIn, MemoryIOStream<T> stream)
            : base(factory, pluginIn, stream)
        {
        }
        
        public InputPin(IIOFactory factory, IPluginIn pluginIn, IInStream<T> stream)
            : this(factory, pluginIn, new BufferedInputIOStream<T>(stream))
        {
            
        }
    }
    
    class BufferedInputIOStream<T> : MemoryIOStream<T>
    {
        private readonly IInStream<T> FInStream;
        
        public BufferedInputIOStream(IInStream<T> inStream)
        {
            FInStream = inStream;
        }
        
        public override bool Sync()
        {
            IsChanged = FInStream.Sync();
            if (IsChanged)
            {
                this.AssignFrom(FInStream);
            }
            return base.Sync();
        }
    }
    
    class BufferedInputMatrix4x4IOStream : MemoryIOStream<Matrix4x4>
    {
        private readonly IInStream<Matrix4x4> FInStream;
        
        public BufferedInputMatrix4x4IOStream(IInStream<Matrix4x4> inStream)
        {
            FInStream = inStream;
        }
        
        public override bool Sync()
        {
            IsChanged = FInStream.Sync();
            if (IsChanged)
            {
            	var buffer = MemoryPool<Matrix4x4>.GetArray();
	            try
	            {
	            	var anySliceChanged = this.Length != FInStream.Length;
	                this.Length = FInStream.Length;
            
		            using (var reader = FInStream.GetReader())
	                using (var writer = this.GetWriter())
	                {
	                    while (!reader.Eos)
	                    {
	                    	var offset = reader.Position;
	                        int numSlicesRead = reader.Read(buffer, 0, buffer.Length);
	                        if (!anySliceChanged)
	                        {
		                        for (int i = 0; i < numSlicesRead; i++)
		                        {
		                        	var previousSlice = this.Buffer[offset + i];
		                        	var newSlice = buffer[i];
		                        	if (newSlice != previousSlice)
		                        	{
		                        		anySliceChanged = true;
		                        		break;
		                        	}
		                        }
	                        }
	                        writer.Write(buffer, 0, numSlicesRead);
	                    }
	                }
		            
		            IsChanged = anySliceChanged;
	            }
	            finally
	            {
	                MemoryPool<Matrix4x4>.PutArray(buffer);
	            }
            }
            return base.Sync();
        }
    }
    
    class BufferedInputMatrixIOStream : MemoryIOStream<Matrix>
    {
        private readonly IInStream<Matrix> FInStream;
        
        public BufferedInputMatrixIOStream(IInStream<Matrix> inStream)
        {
            FInStream = inStream;
        }
        
        public override bool Sync()
        {
            IsChanged = FInStream.Sync();
            if (IsChanged)
            {
            	var buffer = MemoryPool<Matrix>.GetArray();
	            try
	            {
	            	var anySliceChanged = this.Length != FInStream.Length;
	                this.Length = FInStream.Length;
            
		            using (var reader = FInStream.GetReader())
	                using (var writer = this.GetWriter())
	                {
	                    while (!reader.Eos)
	                    {
	                    	var offset = reader.Position;
	                        int numSlicesRead = reader.Read(buffer, 0, buffer.Length);
	                        if (!anySliceChanged)
	                        {
		                        for (int i = 0; i < numSlicesRead; i++)
		                        {
		                        	var previousSlice = this.Buffer[offset + i];
		                        	var newSlice = buffer[i];
		                        	if (newSlice != previousSlice)
		                        	{
		                        		anySliceChanged = true;
		                        		break;
		                        	}
		                        }
	                        }
	                        writer.Write(buffer, 0, numSlicesRead);
	                    }
	                }
		            
		            IsChanged = anySliceChanged;
	            }
	            finally
	            {
	                MemoryPool<Matrix>.PutArray(buffer);
	            }
            }
            return base.Sync();
        }
    }
    
    [ComVisible(false)]
    class InputIOStream<T> : IIOStream<T>
    {
        private readonly IInStream<T> FInStream;
        private readonly MemoryIOStream<T> FIOStream;
        private IInStream<T> FCurrentInStream;
        
        public InputIOStream(IInStream<T> inStream)
        {
            FInStream = inStream;
            FIOStream = new MemoryIOStream<T>();
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
        
        public bool IsChanged
        {
            get
            {
                return FCurrentInStream.IsChanged;
            }
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

        public void Flush(bool force = false)
        {
            FIOStream.Flush(force);
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
