
using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Output
{
	class OutputPin<T> : Pin<T>
	{
		public OutputPin(IIOFactory factory, IPluginOut pluginOut, MemoryIOStream<T> stream)
			: base(factory, pluginOut, stream)
		{
			SliceCount = 1;
		}
		
		public OutputPin(IIOFactory factory, IPluginOut pluginOut, IOutStream<T> outStream)
			: this(factory, pluginOut, new BufferedOutputIOStream<T>(outStream))
		{
		}
	}
	
	class BufferedOutputIOStream<T> : MemoryIOStream<T>
	{
	    private readonly IOutStream<T> FOutStream;
	    
	    public BufferedOutputIOStream(IOutStream<T> outStream)
	    {
	        FOutStream = outStream;
	    }

        public override void Flush(bool force = false)
        {
            if (force || IsChanged)
            {
                // Write the buffered data to the out stream.
                FOutStream.AssignFrom(this);
                FOutStream.Flush(force);
            }
            base.Flush(force);
        }
	}
}
