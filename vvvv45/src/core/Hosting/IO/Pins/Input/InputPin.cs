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
		public InputPin(IPluginHost host, IPluginIO pluginIO, ManagedIOStream<T> stream)
			: base(host, pluginIO, stream)
		{
		}
	}
	
	class ManagedInputIOStream<T> : ManagedIOStream<T>
	{
	    private readonly IInStream<T> FInStream;
	    
	    public ManagedInputIOStream(IInStream<T> inStream)
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
		private readonly ManagedIOStream<T> FIOStream;
		private IInStream<T> FCurrentInStream;
		
		public InputIOStream(IInStream<T> inStream)
		{
			FInStream = inStream;
			FIOStream = new ManagedIOStream<T>();
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
