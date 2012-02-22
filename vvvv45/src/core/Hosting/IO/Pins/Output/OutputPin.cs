
using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Output
{
	class OutputPin<T> : Pin<T>
	{
		public OutputPin(IPluginOut pluginOut, BufferedIOStream<T> stream)
			: base(pluginOut, stream)
		{
			SliceCount = 1;
		}
		
		public OutputPin(IPluginOut pluginOut, IOutStream<T> outStream)
			: this(pluginOut, new BufferedOutputIOStream<T>(outStream))
		{
		}
	}
	
	class BufferedOutputIOStream<T> : BufferedIOStream<T>
	{
	    private readonly IOutStream<T> FOutStream;
	    
	    public BufferedOutputIOStream(IOutStream<T> outStream)
	    {
	        FOutStream = outStream;
	    }
	    
        public override void Flush()
        {
            if (IsChanged)
            {
                // Write the buffered data to the out stream.
                FOutStream.AssignFrom(this);
                FOutStream.Flush();
            }
            base.Flush();
        }
	}
}
