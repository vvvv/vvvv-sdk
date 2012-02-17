
using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Output
{
	class OutputPin<T> : Pin<T>
	{
		public OutputPin(IPluginHost host, IPluginOut pluginOut, ManagedIOStream<T> stream)
			: base(host, pluginOut, stream)
		{
			SliceCount = 1;
		}
		
		public OutputPin(IPluginHost host, IPluginOut pluginOut, IOutStream<T> outStream)
			: this(host, pluginOut, new ManagedOutputIOStream<T>(outStream))
		{
		}
	}
	
	class ManagedOutputIOStream<T> : ManagedIOStream<T>
	{
	    private readonly IOutStream<T> FOutStream;
	    
	    public ManagedOutputIOStream(IOutStream<T> outStream)
	    {
	        FOutStream = outStream;
	    }
	    
        public override void Flush()
        {
            if (FChanged)
            {
                // Write the buffered data to the out stream.
                FOutStream.AssignFrom(this);
                FOutStream.Flush();
            }
            base.Flush();
        }
	}
	
	class OutputIOStream<T> : IIOStream<T>
	{
		private readonly ManagedIOStream<T> FIOStream;
		private readonly IOutStream<T> FOutStream;
		private bool FNeedsFlush;
		
		public OutputIOStream(IOutStream<T> outStream)
		{
			FIOStream = new ManagedIOStream<T>();
			FOutStream = outStream;
		}
		
		public int Length
		{
			get
			{
				return FIOStream.Length;
			}
			set
			{
				if (FIOStream.Length != value)
				{
					FNeedsFlush = true;
					FIOStream.Length = value;
				}
			}
		}
		
		public bool Sync()
		{
			// Nothing to do here.
			return true;
		}
		
		public object Clone()
		{
			return new OutputIOStream<T>(FOutStream.Clone() as IOutStream<T>);
		}
		
		public void Flush()
		{
			if (FNeedsFlush)
			{
				FNeedsFlush = false;
				
				// Write the buffered data to the out stream.
				FOutStream.AssignFrom(FIOStream);
				FOutStream.Flush();
			}
		}
		
		public IStreamReader<T> GetReader()
		{
			return FIOStream.GetReader();
		}
		
		public System.Collections.Generic.IEnumerator<T> GetEnumerator()
		{
			return FIOStream.GetEnumerator();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		public IStreamWriter<T> GetWriter()
		{
			FNeedsFlush = true;
			return FIOStream.GetWriter();
		}
	}
}
